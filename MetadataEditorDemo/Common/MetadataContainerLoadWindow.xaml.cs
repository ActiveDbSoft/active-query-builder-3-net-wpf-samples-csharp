//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2023 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.PropertiesEditors;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.PropertiesEditors;

using ActiveQueryBuilder.View.WPF.Annotations;
using ActiveQueryBuilder.View.WPF.QueryView;
using MetadataEditorDemo.Common.LoadingWizardPages;
using Helpers = ActiveQueryBuilder.View.WPF.Helpers;

namespace MetadataEditorDemo.Common
{
    delegate void ShowProc();
    delegate void CheckProc();
    delegate void PrepareProc();

    internal partial class MetadataContainerLoadWindow: IMetadataContainerLoadForm
    {
        private bool _isSaveChanges;
        private readonly List<WizardPageInfo> _pages;
        private readonly object _owner;

        private int _currentPage;
        private FrameworkElement _usedConnectionProps;
        private MetadataProviderObjectProperties _metadataProperties;
        private MetadataSelection<MetadataNamespace> _databases;

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        private readonly SQLContext _temporarySqlContext;

        private readonly WelcomeWizardPage _wizardPageWelcome;
        private readonly ConnectionTypeWizardPage _wizardPageConnectionType;
        private readonly MetadataOptsWizardPage _wizardPageMetadataOpts;
        private readonly LoadOptsWizardPage _wizardPageLoadOpts;
        private readonly FilterWizardPage _wizardPageFilter;
        private readonly LoadingWizardPage _wizardPageLoading;

        private readonly BaseConnectionDescriptor _initialConnection;
        private int _connectionIndex = -1;

        public bool LoadFields { get; set; }

        public BaseConnectionDescriptor Connection { get; private set; }

        bool IMetadataContainerLoadForm.ShowDialog()
        {
            return ShowDialog() == true;
        }

        public string DefaultDatabase { get; private set; }

        public MetadataContainer EditedMetadataContainer { get; }

        public MetadataContainer TemporaryMetadataContainer { get; }

        public MetadataContainerLoadWindow(object owner, MetadataContainer metadataContainer, BaseConnectionDescriptor connection = null)
        {
            Debug.Assert(metadataContainer != null);

            _owner = owner;

            _pages = new List<WizardPageInfo>();

            // store reference to edited object
            EditedMetadataContainer = metadataContainer;

            _initialConnection = connection;
            Connection = connection;

            // create new SQLContext for loading
            _temporarySqlContext = new SQLContext();
            _temporarySqlContext.Assign(EditedMetadataContainer.SQLContext);

            // create temporary MetadataContainer
            TemporaryMetadataContainer = new MetadataContainer(_temporarySqlContext);
            _temporarySqlContext.MetadataContainer = TemporaryMetadataContainer;

            TemporaryMetadataContainer.Assign(EditedMetadataContainer);
            TemporaryMetadataContainer.LoadingOptions = new MetadataLoadingOptions();
            TemporaryMetadataContainer.LoadingOptions.Assign(EditedMetadataContainer.LoadingOptions);

            InitializeComponent();

            // set up pages

            _wizardPageWelcome = new WelcomeWizardPage {Visibility = Visibility.Collapsed};
            GridRoot.Children.Add(_wizardPageWelcome);

            _wizardPageConnectionType = new ConnectionTypeWizardPage {Visibility = Visibility.Collapsed};
            GridRoot.Children.Add(_wizardPageConnectionType);

            _wizardPageMetadataOpts = new MetadataOptsWizardPage {Visibility = Visibility.Collapsed};
            GridRoot.Children.Add(_wizardPageMetadataOpts);

            _wizardPageLoadOpts = new LoadOptsWizardPage {Visibility = Visibility.Collapsed};
            GridRoot.Children.Add(_wizardPageLoadOpts);

            _wizardPageFilter = new FilterWizardPage {Visibility = Visibility.Collapsed};
            GridRoot.Children.Add(_wizardPageFilter);

            _wizardPageLoading = new LoadingWizardPage {Visibility = Visibility.Collapsed};
            GridRoot.Children.Add(_wizardPageLoading);

            _pages.Add(new WizardPageInfo(ShowWelcome));
            _pages.Add(new WizardPageInfo(ShowConnection, CheckConnectionSelected));
            _pages.Add(new WizardPageInfo(ShowMetadataOpts, CheckShowMetadataOpts, true, BeforeMetadataOpts));
            _pages.Add(new WizardPageInfo(ShowLoadOpts, CheckLoadOpts, (_temporarySqlContext.SyntaxProvider != null && _temporarySqlContext.SyntaxProvider.IsSupportDatabases()), BeforeLoadOpts));
            _pages.Add(new WizardPageInfo(ShowFilter, CheckFilter));
            _pages.Add(new WizardPageInfo(ShowLoading));

            _currentPage = 0;

            _pages[_currentPage].ShowProc();

            _wizardPageMetadataOpts.bConnectionTest.Click += buttonConnectionTest_Click;
            _wizardPageConnectionType.ComboBoxConnectionType.SelectionChanged += cbConnectionType_SelectedIndexChanged;
            _wizardPageConnectionType.ComboBoxSyntaxProvider.SelectionChanged += ComboBoxSyntaxProvider_SelectedIndexChanged;

            bBack.Click += buttonBack_Click;
            bNext.Click += buttonNext_Click;

            Loaded += MetadataContainerLoadForm_Load;

            Localize();

            Loaded += MetadataContainerLoadForm_Loaded;

            var propertyLanguage =
                DependencyPropertyDescriptor.FromProperty(LanguageProperty, typeof(MetadataContainerLoadWindow));
            propertyLanguage.AddValueChanged(this, LanguagePropertyChanged);


        }

        private void LanguagePropertyChanged(object sender, EventArgs e)
        {
            Localize();
        }

        private void Localize()
        {
            Title = Helpers.Localizer.GetString("strLoadMetadataWizardCaption", LocalizableConstantsUI.strLoadMetadataWizardCaption);

            bBack.Content = Helpers.Localizer.GetString("bBack", LocalizableConstantsUI.bBack);
            bNext.Content = Helpers.Localizer.GetString("bNext", LocalizableConstantsUI.bNext);
            bCancel.Content = Helpers.Localizer.GetString("bCancel", LocalizableConstantsUI.bCancel);
        }

        private void MetadataContainerLoadForm_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MetadataContainerLoadForm_Loaded;
        }

        private void ReplaceSyntaxEditControl(FrameworkElement control)
        {
            if (control == null)
            {
                _wizardPageConnectionType.PanelSyntaxOpts.Children.Clear();
                return;
            }

            _wizardPageConnectionType.PanelSyntaxOpts.Children.Clear();
            _wizardPageConnectionType.PanelSyntaxOpts.Children.Add(control);
        }

        private void ComboBoxSyntaxProvider_SelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            if (Connection == null) return;
            Connection.SyntaxProvider = _wizardPageConnectionType.ComboBoxSyntaxProvider.SelectedItem as BaseSyntaxProvider;
            var syntaxEditControl = CreateSyntaxEditControl(Connection);
            ReplaceSyntaxEditControl(syntaxEditControl);
        }

        private void cbConnectionType_SelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            SetShown(ShowLoadOpts, false);
            SetShown(ShowMetadataOpts, false);

            var connectionCombo = _wizardPageConnectionType.ComboBoxConnectionType;
            if (connectionCombo.SelectedIndex != -1)
            {
                var connectionName = connectionCombo.SelectedItem as string;
                if (_initialConnection != null && connectionName == _initialConnection.GetDescription())
                {
                    Connection = _initialConnection;
                }
                else
                {
                    var type = ActiveQueryBuilder.Core.Helpers.ConnectionDescriptorList[connectionCombo.SelectedIndex];
                    try
                    {
                        Connection = Activator.CreateInstance(type) as BaseConnectionDescriptor;
                    }
                    catch (Exception e)
                    {
                        var message = GetMostInnerException(e).Message;
                        MessageBox.Show(message + "\r\n \r\n" +
                                        "To fix this error you may need to install the appropriate database client software or re-compile the project from sources and add the needed assemblies to the References section.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        connectionCombo.SelectedIndex = _connectionIndex;
                        return;
                    }
                }

                _wizardPageConnectionType.SyntaxVisible = Connection != null && Connection.IsGenericSyntax() ? Visibility.Visible:Visibility.Collapsed;
                SelectSyntax();
            }

            _connectionIndex = connectionCombo.SelectedIndex;
        }

        private static Exception GetMostInnerException(Exception exception)
        {
            var e = exception;
            while (e.InnerException != null)
                e = e.InnerException;

            return e;
        }

        private void SelectSyntax()
        {
            if (Connection?.SyntaxProvider == null)
            {
                return;
            }

            foreach (var item in _wizardPageConnectionType.ComboBoxSyntaxProvider.Items)
            {
                if (item.GetType() != Connection.SyntaxProvider.GetType()) continue;

                _wizardPageConnectionType.ComboBoxSyntaxProvider.SelectedItem = item;
                return;
            }
        }

        private void MetadataContainerLoadForm_Load(object sender, EventArgs e)
        {
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataContainerLoadForm.LoadMetadataWizardCaption.Subscribe(x => Title = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.Common.ButtonCancel.Subscribe(x => bCancel.Content = x));

            bBack.Content = "< " + ActiveQueryBuilder.Core.Localization.Common.ButtonBack.Value;
            bNext.Content = ActiveQueryBuilder.Core.Localization.Common.ButtonNext.Value + " >";

            var uiElement = _owner as FrameworkElement;

            if (uiElement == null) return;
            {
                var ownerPointToScreen = uiElement.PointToScreen(new Point(0,0));
                var sizeOwner = new Size(uiElement.ActualWidth, uiElement.ActualHeight);

                var x = ownerPointToScreen.X + (sizeOwner.Width - ActualWidth) / 2;
                var y = ownerPointToScreen.Y + (sizeOwner.Height - ActualHeight) / 2;

                Left = x;
                Top = y;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _subscriptions.Dispose();

            _wizardPageMetadataOpts.bConnectionTest.Click -= buttonConnectionTest_Click;
            _wizardPageConnectionType.ComboBoxConnectionType.SelectionChanged -= cbConnectionType_SelectedIndexChanged;
            _wizardPageConnectionType.ComboBoxConnectionType.SelectionChanged -= ComboBoxSyntaxProvider_SelectedIndexChanged;
            bBack.Click -= buttonBack_Click;
            bNext.Click -= buttonNext_Click;

            base.OnClosed(e);
        }
      
        private void LoadDatabaseList()
        {
            DefaultDatabase = null;
            _databases = null;

            if (Connection.MetadataProvider != null && Connection.SyntaxProvider.IsSupportDatabases())
            {
                _temporarySqlContext.MetadataProvider = Connection.MetadataProvider;
                _temporarySqlContext.SyntaxProvider = Connection.SyntaxProvider;

                MetadataLoadingOptions oldOptions = new MetadataLoadingOptions();
                oldOptions.Assign(TemporaryMetadataContainer.LoadingOptions);

                MetadataLoadingOptions tempOptions = new MetadataLoadingOptions
                {
                    LoadDefaultDatabaseOnly = false,
                    LoadSystemObjects = true
                };

                try
                {
                    TemporaryMetadataContainer.LoadingOptions = tempOptions;
                    MetadataList list = new MetadataList(TemporaryMetadataContainer);
                    list.Load(MetadataType.Database, false);
                    _databases = list.Databases;
                }
                finally
                {
                    TemporaryMetadataContainer.LoadingOptions = oldOptions;
                    tempOptions.Dispose();
                }
            }

            _wizardPageLoadOpts.ChecklistDatabases.Items.Clear();

            if (_databases == null) return;

            foreach (MetadataNamespace database in _databases)
            {
                if (database.Default)
                    DefaultDatabase = database.Name;

                _wizardPageLoadOpts.ChecklistDatabases.Items.Add(new DatabaseObjectForListbox(database.Name, database.Default));
            }
        }

        private void SetActivePage(UserControl page)
        {
            _wizardPageWelcome.Visibility = (page is WelcomeWizardPage) ? Visibility.Visible : Visibility.Collapsed;
            _wizardPageConnectionType.Visibility = (page is ConnectionTypeWizardPage) ? Visibility.Visible : Visibility.Collapsed;
            _wizardPageMetadataOpts.Visibility = (page is MetadataOptsWizardPage) ? Visibility.Visible : Visibility.Collapsed;
            _wizardPageLoadOpts.Visibility = (page is LoadOptsWizardPage) ? Visibility.Visible : Visibility.Collapsed;
            _wizardPageFilter.Visibility = (page is FilterWizardPage) ? Visibility.Visible : Visibility.Collapsed;
            _wizardPageLoading.Visibility = (page is LoadingWizardPage) ? Visibility.Visible : Visibility.Collapsed;
            //panelBottom.SendToBack();
        }

        private void ShowWelcome()
        {
            SetActivePage(_wizardPageWelcome);

            var check = TemporaryMetadataContainer.Items.IsLoaded(MetadataType.Namespaces)
                        || TemporaryMetadataContainer.Items.IsLoaded(MetadataType.Objects)
                        || TemporaryMetadataContainer.Items.IsLoaded(MetadataType.ObjectMetadata);

            _wizardPageWelcome.cbClearBeforeLoading.IsChecked = check;
            _wizardPageWelcome.cbClearBeforeLoading.IsEnabled = check;
        }

        private void CheckConnectionSelected()
        {
            if (Connection == null)
            {
                throw new QueryBuilderException(Helpers.Localizer.GetString("strSelectConnectionType", LocalizableConstantsInternal.strSelectConnectionType));
            }

            if (Connection.IsGenericSyntax() && _wizardPageConnectionType.ComboBoxSyntaxProvider.SelectedItem == null)
            {
                throw new QueryBuilderException(Helpers.Localizer.GetString("strSelectSyntaxProvider", LocalizableConstantsInternal.strSelectSyntaxProvider));
            }

            SetPageEnabled(ShowLoadOpts, Connection.SyntaxProvider.IsSupportDatabases());
        }

        private void ShowConnection()
        {
            SetActivePage(_wizardPageConnectionType);

            if (FirstShow(ShowConnection))
            {
                SetShown(ShowConnection, true);

                for (int i = 0; i < ActiveQueryBuilder.Core.Helpers.ConnectionDescriptorList.Count; i++)
                {
                    var name = ActiveQueryBuilder.Core.Helpers.ConnectionDescriptorList.Names[i];
                    _wizardPageConnectionType.ComboBoxConnectionType.Items.Add(name);
                }

                foreach (Type type in ActiveQueryBuilder.Core.Helpers.SyntaxProviderList)
                {
                    if (Connection != null && Connection.IsGenericSyntax() && Connection.SyntaxProvider != null &&
                        Connection.SyntaxProvider.GetType() == type)
                    {
                        _wizardPageConnectionType.ComboBoxSyntaxProvider.Items.Add(Connection.SyntaxProvider);
                        continue;
                    }

                    var syntax = Activator.CreateInstance(type) as BaseSyntaxProvider;
                    if (syntax != null)
                    {
                        _wizardPageConnectionType.ComboBoxSyntaxProvider.Items.Add(syntax);
                    }
                }

                if (Connection != null)
                {
                    _wizardPageConnectionType.ComboBoxConnectionType.SelectedItem = Connection.GetDescription();

                    if (Connection.IsGenericSyntax() && Connection.SyntaxProvider != null)
                    {
                        _wizardPageConnectionType.ComboBoxSyntaxProvider.SelectedItem = Connection.SyntaxProvider;
                    }
                }
            }
        }

        private FrameworkElement CreateConnectionEditControl(BaseConnectionDescriptor connection)
        {
            _metadataProperties = connection.MetadataProperties;

            if (_metadataProperties == null) return null;

            _metadataProperties.PropertiesEditors.Clear();
            var propsContainer = PropertiesFactory.GetPropertiesContainer(_metadataProperties);
            var propertiesBar = new PropertiesBar
            {
                EditorsOptions =
                {
                    NarrowEditControlsMinWidth = 160,
                    MultiLineEditorsMaxWidth = 570
                }
            };
            var propertiesControl = (IPropertiesControl)propertiesBar;
            propertiesControl.SetProperties(propsContainer);
            return propertiesBar;

        }

        private FrameworkElement CreateSyntaxEditControl(BaseConnectionDescriptor connection)
        {
            var properties = connection.SyntaxProperties;
            if (properties == null) return null;
            properties.PropertiesEditors.Clear();
            var propsContainer = PropertiesFactory.GetPropertiesContainer(properties);
            var propertiesBar = new PropertiesBar();
            var propertiesControl = (IPropertiesControl)propertiesBar;
            propertiesControl.SetProperties(propsContainer);
            return propertiesBar;

        }

        private void BeforeMetadataOpts()
        {
            if (Connection != null)
            {
                ClearPropsPage();

                if (!Connection.MetadataProvider.ConnectionObjectsCreated)
                {
                    Connection.MetadataProvider.CreateAndBindInternalConnectionObj();
                }

                if (_usedConnectionProps == null)
                {
                    _usedConnectionProps = CreateConnectionEditControl(Connection);
                }

                ReplaceConnectionEditControl(_usedConnectionProps);
            }
            else
            {
                _usedConnectionProps = null;
            }

            SetPageEnabled(ShowMetadataOpts, (_usedConnectionProps != null));
        }

        private void ReplaceConnectionEditControl(FrameworkElement control)
        {
            _wizardPageMetadataOpts.panelMetadataOpts.Children.Clear();

            if (control != null)
            {
                _wizardPageMetadataOpts.panelMetadataOpts.Children.Add(control);
            }
        }

        private void ClearPropsPage()
        {
            if (_usedConnectionProps != null)
            {
                _wizardPageMetadataOpts.panelMetadataOpts.Children.Remove(_usedConnectionProps);
                _usedConnectionProps = null;
            }

            SetShown(ShowLoadOpts, false);
        }

        private void ShowMetadataOpts()
        {
            SetActivePage(_wizardPageMetadataOpts);

            if (!FirstShow(ShowMetadataOpts)) return;
            SetShown(ShowMetadataOpts, true);

            if (Connection == null) return;
            if (!Connection.MetadataProvider.ConnectionObjectsCreated)
            {
                Connection.MetadataProvider.CreateAndBindInternalConnectionObj();
            }
        }

        private void CheckShowMetadataOpts()
        {
            try
            {
                if (Connection.MetadataProvider.Connected)
                    Connection.MetadataProvider.Disconnect();

                _metadataProperties.ApplyConnectionString();
                Connection.MetadataProvider.Connect();
            }
            catch (Exception e)
            {
                throw new QueryBuilderException(Helpers.Localizer.GetString("strFailedToConnectDatabase", LocalizableConstantsInternal.strFailedToConnectDatabase) + "\n" + e.Message);
            }
        }

        private void BeforeLoadOpts()
        {
            LoadDatabaseList();

            if (Connection.SyntaxProvider.IsSupportDatabases())
            {
                SetPageEnabled(ShowLoadOpts, (_databases != null && _databases.Count != 0));
            }
        }

        private void ShowLoadOpts()
        {
            SetActivePage(_wizardPageLoadOpts);

            if (FirstShow(ShowLoadOpts))
            {
                SetShown(ShowLoadOpts, true);
            }
        }

        private void CheckLoadOpts()
        {
            if (_wizardPageLoadOpts.CheckedItems.Count == 0)
            {
                throw new QueryBuilderException(Helpers.Localizer.GetString("strLoadingWizardPageDatabaseWelcom", LocalizableConstantsInternal.strLoadingWizardPageDatabaseWelcom));
            }
        }

        private void ShowFilter()
        {
            _wizardPageFilter.LoadFileds = LoadFields;

            SetActivePage(_wizardPageFilter);

            _wizardPageFilter.MetadataFilter = TemporaryMetadataContainer.LoadingOptions.MetadataFilter;
            _wizardPageFilter.ShowPackage = Connection.SyntaxProvider.IsSupportPackages();
            _wizardPageFilter.ShowServer = Connection.SyntaxProvider.IsSupportServers();

            if (Connection.SyntaxProvider.IsSupportDatabases())
            {
                _wizardPageFilter.ShowDatabase = true;
                _wizardPageFilter.DatabaseList = LoadMetadata(MetadataType.Database);
            }

            if (Connection.SyntaxProvider.IsSupportSchemas())
            {
                _wizardPageFilter.ShowSchema = true;
                _wizardPageFilter.SchemaList = LoadMetadata(MetadataType.Schema);
            }
        }

        private void CheckFilter()
        {
            var itemsToDelete = new List<MetadataFilterItem>();
            var filters = TemporaryMetadataContainer.LoadingOptions.MetadataFilter;

            foreach (var filter in filters)
            {
                if (filter.IsEmpty)
                    itemsToDelete.Add(filter);
            }

            foreach (var filter in itemsToDelete)
            {
                filters.Remove(filter);
            }

            LoadFields = _wizardPageFilter.LoadFileds;
        }

        private List<string> LoadMetadata(MetadataType type)
        {
            var loadingOptions = new MetadataLoadingOptions
            {
                LoadDefaultDatabaseOnly = type != MetadataType.Database,
                LoadSystemObjects = true
            };

            var container = new MetadataContainer(TemporaryMetadataContainer.SQLContext)
            {
                LoadingOptions = loadingOptions
            };

            var list = new MetadataList(container);
            list.Load(type, false);
            return list.Select(x => x.Name).ToList();
        }

        private MetadataLoader _loader;

        private void ShowLoading()
        {
            SetActivePage(_wizardPageLoading);

            bBack.IsEnabled = false;
            bNext.IsEnabled = false;

            _temporarySqlContext.MetadataProvider = Connection.MetadataProvider;
            _temporarySqlContext.SyntaxProvider = Connection.SyntaxProvider;

            TemporaryMetadataContainer.Items.SetLoaded(MetadataType.All, false);

            if (_wizardPageWelcome.cbClearBeforeLoading.IsChecked == true)
            {
                EditedMetadataContainer.Items.Clear();
                TemporaryMetadataContainer.Items.Clear();
            }

            TemporaryMetadataContainer.LoadingOptions.LoadDefaultDatabaseOnly = false;
            TemporaryMetadataContainer.LoadingOptions.LoadSystemObjects = false;

            var databasesToLoad = new List<string>();
            if ((!Connection.SyntaxProvider.IsSupportDatabases() || _databases.Count == 0) && DefaultDatabase != null)
                databasesToLoad.Add(DefaultDatabase);
            else
            {
                foreach (DatabaseObjectForListbox checkedItem in _wizardPageLoadOpts.CheckedItems)
                {
                    databasesToLoad.Add(checkedItem.Database);
                }
            }

            _loader = new MetadataLoader(TemporaryMetadataContainer, databasesToLoad)
            {
                LoadFields = LoadFields
            };

            _loader.DatabaseLoadingStart += LoaderOnDatabaseLoadingStart;
            _loader.LoadingFinished += LoadingFinished;
            _loader.LoadingFailed += LoadingFailed;
            _loader.EntitiesLoaded += EntitiesLoaded;
            _loader.Start();
        }

        private void EntitiesLoaded(object sender, MetadataItem metadataItem, int databases, int schemas, int packages, int objects, int fields, int foreignkeys)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                _wizardPageLoading.lblLoaded.Text = string.Format(Helpers.Localizer.GetString("strContainerLoadingStatistics", 
                        LocalizableConstantsInternal.strContainerLoadingStatistics),
                    databases, schemas, packages, objects, fields, foreignkeys);

                WriteMetadataLoadingLog($"Loading {metadataItem.Type} {metadataItem.Name}");
            }));
        }

        private void LoadingFailed(object sender, string message)
        {
            WriteMetadataLoadingLog(Helpers.Localizer.GetString("strContainerLoadingFailed",
                                        LocalizableConstantsInternal.strContainerLoadingFailed) + " " + message);
        }

        private void LoaderOnDatabaseLoadingStart(object sender, string database)
        {
            WriteMetadataLoadingLog(string.Format(Helpers.Localizer.GetString("strContainerLoadingFromDatabase", 
                LocalizableConstantsInternal.strContainerLoadingFromDatabase), database));
        }

        private void LoadingFinished(object sender, EventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                bCancel.Content = ActiveQueryBuilder.Core.Localization.Common.ButtonClose.Value;
                bCancel.IsEnabled = true;
            }));

            WriteMetadataLoadingLog(Helpers.Localizer.GetString("strContainerLoadingSuccess", 
                LocalizableConstantsInternal.strContainerLoadingSuccess));

            WriteMetadataLoadingLog(string.Format(
                                        Helpers.Localizer.GetString("strContainerLoadedEntities", 
                                            LocalizableConstantsInternal.strContainerLoadedEntities),
                                        (_loader.LoadedDatabases + _loader.LoadedSchemas + _loader.LoadedPackages + 
                                         _loader.LoadedObjects + _loader.LoadedFields + _loader.LoadedForeignKeys)
                                        .ToString(CultureInfo.InvariantCulture)));

            _loader.DatabaseLoadingStart -= LoaderOnDatabaseLoadingStart;
            _loader.LoadingFinished -= LoadingFinished;
            _loader.LoadingFailed -= LoadingFailed;
            _loader.EntitiesLoaded -= EntitiesLoaded;
            _loader = null;

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                _wizardPageLoading.ShowSuccess();
                _isSaveChanges = true;
            }));
        }

        private int IndexOfPage(ShowProc APageProc)
        {
            for (int i = 0; i < _pages.Count; i++)
            {
                if (_pages[i].ShowProc == APageProc)
                {
                    return i;
                }
            }

            return -1;
        }

        private bool FirstShow(ShowProc APageProc)
        {
            int i = IndexOfPage(APageProc);

            if (i != -1)
            {
                return !_pages[i].PageShown;
            }

            return false;
        }

        private void SetShown(ShowProc APageProc, bool AShown /*= true*/)
        {
            int i = IndexOfPage(APageProc);

            if (i != -1)
            {
                _pages[i].PageShown = AShown;
            }
        }

        private void WriteMetadataLoadingLog(String message)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                _wizardPageLoading.textLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
                _wizardPageLoading.textLog.ScrollToEnd();
            }));
        }

        private void SetPageEnabled(ShowProc APageProc, bool AEnabled)
        {
            int i = IndexOfPage(APageProc);

            if (i != -1)
            {
                _pages[i].PageEnabled = AEnabled;
            }
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            if (_pages[_currentPage].CheckProc != null)
            {
                try
                {
                    _pages[_currentPage].CheckProc();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Helpers.Localizer.GetString("strError", LocalizableConstantsUI.strError));
                    return;
                }
            }

            if (_currentPage + 1 < _pages.Count)
            {
                do
                {
                    _currentPage++;

                    if (_pages[_currentPage].PrepareProc != null)
                    {
                        try
                        {
                            _pages[_currentPage].PrepareProc();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, Helpers.Localizer.GetString("strError", LocalizableConstantsUI.strError));
                            _currentPage--;
                            return;
                        }
                    }
                }
                while (!_pages[_currentPage].PageEnabled);

                bBack.IsEnabled = true;
                bNext.IsEnabled = (_currentPage + 1 < _pages.Count);

                if (_currentPage + 2 == _pages.Count)
                {
                    bNext.Content = ActiveQueryBuilder.Core.Localization.Common.ButtonLoad.Value;
                }
                else
                {
                    bNext.Content = ActiveQueryBuilder.Core.Localization.Common.ButtonNext.Value + " >";
                }

                _pages[_currentPage].ShowProc();
            }
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            if (_currentPage > 0)
            {
                _currentPage--;

                while (!_pages[_currentPage].PageEnabled)
                {
                    _currentPage--;
                }

                bBack.IsEnabled = (_currentPage > 0);
                bNext.IsEnabled = true;
                bNext.Content = ActiveQueryBuilder.Core.Localization.Common.ButtonNext.Value + " >";

                _pages[_currentPage].ShowProc();
            }
        }

        void buttonConnectionTest_Click(object sender, EventArgs e)
        {
            if (Connection != null)
            {
                _metadataProperties.ApplyConnectionString();
                Connection.Disconnect();

                try
                {
                    Connection.Connect();
                    Connection.Disconnect();

                    MessageBox.Show(Helpers.Localizer.GetString("strTestConnectionSuccess", LocalizableConstantsInternal.strTestConnectionSuccess));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Helpers.Localizer.GetString("strTestConnectionFail", LocalizableConstantsInternal.strTestConnectionFail) + "\n" + ex.Message);
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _loader?.Stop();

            if (_isSaveChanges)
            {
                MetadataLoadingOptions loadingOptions = EditedMetadataContainer.LoadingOptions;
                EditedMetadataContainer.Assign(TemporaryMetadataContainer);
                EditedMetadataContainer.LoadingOptions = loadingOptions;
            }

            base.OnClosing(e);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = _isSaveChanges;
        }

        public void Dispose() {}
    }

    internal class WizardPageInfo
    {
        public ShowProc ShowProc;
        public CheckProc CheckProc;
        public bool PageShown;
        public bool PageEnabled;
        public PrepareProc PrepareProc;

        public WizardPageInfo(ShowProc AShowProc)
        {
            ShowProc = AShowProc;
            CheckProc = null;
            PageShown = false;
            PageEnabled = true;
            PrepareProc = null;
        }

        public WizardPageInfo(ShowProc AShowProc, CheckProc ACheckProc)
        {
            ShowProc = AShowProc;
            CheckProc = ACheckProc;
            PageShown = false;
            PageEnabled = true;
            PrepareProc = null;
        }

        public WizardPageInfo(ShowProc AShowProc, CheckProc ACheckProc, bool AEnabled)
        {
            ShowProc = AShowProc;
            CheckProc = ACheckProc;
            PageShown = false;
            PageEnabled = AEnabled;
            PrepareProc = null;
        }

        public WizardPageInfo(ShowProc AShowProc, CheckProc ACheckProc, bool AEnabled, PrepareProc APrepareProc)
        {
            ShowProc = AShowProc;
            CheckProc = ACheckProc;
            PageShown = false;
            PageEnabled = AEnabled;
            PrepareProc = APrepareProc;
        }
    }

    internal class DatabaseObjectForListbox : object, INotifyPropertyChanged
    {
        private bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        public string Database;
        public bool IsDefaultDatabase;

        public DatabaseObjectForListbox(string database, bool isDefaultDatabase)
        {
            Database = database;
            IsDefaultDatabase = isDefaultDatabase;
            IsChecked = isDefaultDatabase;
        }

        public override string ToString()
        {
            Debug.Assert(Database != null);

            if (!IsDefaultDatabase)
            {
                return Database;
            }

            return Database + " " +
                   $"({Helpers.Localizer.GetString("strMetadataTreeCurrentDatabase", LocalizableConstantsInternal.strMetadataTreeCurrentDatabase)})";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class MetadataLoader
    {
        public delegate void DatabaseLoadingStartEventHandler(object sender, string database);
        public delegate void DatabaseLoadingFailedEventHandler(object sender, string message);
        public delegate void DatabaseEntitiesLoadedEventHandler(object sender, MetadataItem metadataItem, int databases, int schemas, int packages, int objects, int fields, int foreignKeys);

        private Thread _thread;
        private readonly List<string> _databases;
        private readonly MetadataContainer _container;
        private bool _loadingAborted;

        public int LoadedDatabases { get; private set; } = 0;
        public int LoadedSchemas { get; private set; } = 0;
        public int LoadedPackages { get; private set; } = 0;
        public int LoadedObjects { get; private set; } = 0;
        public int LoadedFields { get; private set; } = 0;
        public int LoadedForeignKeys { get; private set; } = 0;

        public bool LoadFields { get; set; }

        public event DatabaseLoadingStartEventHandler DatabaseLoadingStart;
        public event DatabaseLoadingFailedEventHandler LoadingFailed;
        public event EventHandler LoadingFinished;
        public event DatabaseEntitiesLoadedEventHandler EntitiesLoaded;

        public MetadataLoader(MetadataContainer container, List<string> databases = null)
        {
            _container = container;
            _databases = databases;
        }

        public void Start()
        {
            _thread = new Thread(LoadMetadata);
            _thread.Start();
        }

        public void Stop()
        {
            _loadingAborted = true;
            _container.AbortMetadataLoading();
        }

        private void LoadMetadata()
        {
            _container.MetadataChildItemAdded += ContainerOnMetadataChildItemAdded;
            try
            {
                using (new UpdateRegion(_container))
                {
                    if (_databases != null && _databases.Count != 0)
                        LoadForDatabases();
                    else
                        LoadEntireContainer();
                }
            }
            catch (Exception e)
            {
                LoadingFailed?.Invoke(this, e.Message);
            }
            finally
            {
                _container.MetadataChildItemAdded -= ContainerOnMetadataChildItemAdded;
                LoadingFinished?.Invoke(this, EventArgs.Empty);
            }
        }

        private void LoadForDatabases()
        {
            foreach (var database in _databases)
            {
                if (_loadingAborted)
                    break;

                DatabaseLoadingStart?.Invoke(this, database);
                MetadataNamespace db = _container.AddDatabase(database);
                db.Items.Load(LoadFields ? MetadataType.All : MetadataType.All & ~MetadataType.ObjectMetadata, true);
            }
        }

        private void LoadEntireContainer()
        {
            _container.Items.Load(LoadFields ? MetadataType.All : MetadataType.All & ~MetadataType.ObjectMetadata, true);
        }

        private void ContainerOnMetadataChildItemAdded(MetadataItem sender, MetadataItem item)
        {
            UpdateCounters(item);
            EntitiesLoaded?.Invoke(this, item, LoadedDatabases, LoadedSchemas, LoadedPackages, LoadedObjects,
                LoadedFields, LoadedForeignKeys);
        }

        private void UpdateCounters(MetadataItem item)
        {
            switch (item.Type)
            {
                case MetadataType.Database:
                    LoadedDatabases++;
                    break;
                case MetadataType.Schema:
                    LoadedSchemas++;
                    break;
                case MetadataType.Package:
                    LoadedPackages++;
                    break;
                case MetadataType.Table:
                case MetadataType.View:
                case MetadataType.Procedure:
                case MetadataType.Synonym:
                case MetadataType.Aggregate:
                    LoadedObjects++;
                    break;
                case MetadataType.Parameter:
                case MetadataType.Field:
                    LoadedFields++;
                    break;
                case MetadataType.ForeignKey:
                    LoadedForeignKeys++;
                    break;
            }
        }
    }
}
