//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Data.Odbc;
using System.Data.OleDb;
using Oracle.ManagedDataAccess.Client;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View;
using FullFeaturedMdiDemo.Common;
using FullFeaturedMdiDemo.MdiControl;
using FullFeaturedMdiDemo.PropertiesForm;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using Npgsql;
using Helpers = ActiveQueryBuilder.Core.Helpers;
using ActiveQueryBuilder.View.EventHandlers.MetadataStructureItems;
using BuildInfo = ActiveQueryBuilder.Core.BuildInfo;

namespace FullFeaturedMdiDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private ConnectionInfo _selectedConnection;
        private SQLContext _sqlContext;
        private readonly SQLFormattingOptions _sqlFormattingOptions;
        private readonly SQLGenerationOptions _sqlGenerationOptions;
        private bool _showHintConnection = true;

        public MainWindow()
        {
            // Options to present the formatted SQL query text to end-user
            // Use names of virtual objects, do not replace them with appropriate derived tables
            _sqlFormattingOptions = new SQLFormattingOptions { ExpandVirtualObjects = false };

            // Options to generate the SQL query text for execution against a database server
            // Replace virtual objects with derived tables
            _sqlGenerationOptions = new SQLGenerationOptions { ExpandVirtualObjects = true };

            InitializeComponent();

            Closing += MainWindow_Closing;
            MdiContainer1.ActiveWindowChanged += MdiContainer1_ActiveWindowChanged;
            Dispatcher.CurrentDispatcher.Hooks.DispatcherInactive += Hooks_DispatcherInactive;

            var currentLang = System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            LoadLanguage();

            var defLang = "en";

            if (Helpers.Localizer.Languages.Contains(currentLang.ToLower()))
            {
                Language = XmlLanguage.GetLanguage(currentLang);
                defLang = currentLang.ToLower();
            }

            var menuItem = MenuItemLanguage.Items.Cast<MenuItem>().First(item => (string)item.Tag == defLang);
            menuItem.IsChecked = true;

            // DEMO WARNING
            if (BuildInfo.GetEdition() == BuildInfo.Edition.Trial)
            {
                var trialNoticePanel = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Background = Brushes.LightGreen,
                    Padding = new Thickness(5),
                    Margin = new Thickness(0, 0, 0, 2)
                };
                trialNoticePanel.SetValue(Grid.RowProperty, 1);

                var label = new TextBlock
                {
                    Text =
                        @"Generation of random aliases for the query output columns is the limitation of the trial version. The full version is free from this behavior.",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };

                var button = new Button
                {
                    Background = Brushes.Transparent,
                    Padding = new Thickness(0),
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Margin = new Thickness(0, 0, 5, 0),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Content = new Image
                    {
                        Source = ActiveQueryBuilder.View.WPF.Helpers.GetImageSource(Properties.Resources.cancel),
                        Stretch = Stretch.None
                    }
                };

                button.Click += delegate { GridRoot.Visibility = Visibility.Collapsed; };

                trialNoticePanel.Child = label;
                GridRoot.Children.Add(trialNoticePanel);
                GridRoot.Children.Add(button);
            }

        }

        bool _shown;

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (_shown)
                return;

            _shown = true;

            CommandNew_OnExecuted(this, null);
        }

        private void MdiContainer1_ActiveWindowChanged(object sender, EventArgs args)
        {
            var window = MdiContainer1.ActiveChild as ChildWindow;

            if (window != null)
            { 
                QueriesView.QueryView = window.QueryView;
                QueriesView.SQLQuery = window.SqlQuery;
            }
        }

        private void LoadLanguage()
        {
            foreach (var language in Helpers.Localizer.Languages)
            {
                if (language.ToLower() == "auto" || language.ToLower() == "default") continue;

                var culture = new CultureInfo(language);

                var stroke = string.Format("{0}", culture.DisplayName);

                var menuItem = new MenuItem
                {
                    Header = stroke,
                    Tag = language,
                    IsCheckable = true
                };

                MenuItemLanguage.Items.Add(menuItem);
                menuItem.SetValue(MenuBehavior.OptionGroupNameProperty, "group");
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= MainWindow_Closing;
            Dispatcher.CurrentDispatcher.Hooks.DispatcherInactive -= Hooks_DispatcherInactive;
        }

        private void Hooks_DispatcherInactive(object sender, EventArgs e)
        {
            MenuItemSave.IsEnabled = MdiContainer1.Children.Count > 0;
            MenuItemQueryStatistics.IsEnabled = MdiContainer1.Children.Count > 0;
            MenuItemSaveIco.IsEnabled = MdiContainer1.Children.Count > 0;

            MenuItemUndo.IsEnabled = ((ChildWindow)MdiContainer1.ActiveChild != null &&
                                      ((ChildWindow)MdiContainer1.ActiveChild).CanUndo());
            MenuItemRedo.IsEnabled = ((ChildWindow)MdiContainer1.ActiveChild != null &&
                                      ((ChildWindow)MdiContainer1.ActiveChild).CanRedo());
            MenuItemCopyIco.IsEnabled = MenuItemCopy.IsEnabled = ((ChildWindow)MdiContainer1.ActiveChild != null &&
                                                                  ((ChildWindow)MdiContainer1.ActiveChild).CanCopy());
            MenuItemPasteIco.IsEnabled = MenuItemPaste.IsEnabled = ((ChildWindow)MdiContainer1.ActiveChild != null &&
                                                                    ((ChildWindow)MdiContainer1.ActiveChild).CanPaste());
            MenuItemCutIco.IsEnabled = MenuItemCut.IsEnabled = ((ChildWindow)MdiContainer1.ActiveChild != null &&
                                                                ((ChildWindow)MdiContainer1.ActiveChild).CanCut());
            MenuItemSelectAll.IsEnabled = ((ChildWindow)MdiContainer1.ActiveChild != null &&
                                           ((ChildWindow)MdiContainer1.ActiveChild).CanSelectAll());
            MenuItemNewQueryToolBar.IsEnabled = MenuItemNewQuery.IsEnabled = _sqlContext != null;

            MenuItemQueryAddDerived.IsEnabled = MdiContainer1.ActiveChild != null &&
                                                ((ChildWindow)MdiContainer1.ActiveChild).CanAddDerivedTable();

            MenuItemCopyUnionSq.IsEnabled = MdiContainer1.ActiveChild != null &&
                                            ((ChildWindow)MdiContainer1.ActiveChild).CanCopyUnionSubQuery();
            MenuItemAddUnionSq.IsEnabled = MdiContainer1.ActiveChild != null &&
                                           ((ChildWindow)MdiContainer1.ActiveChild).CanAddUnionSubQuery();
            MenuItemProp.IsEnabled = MdiContainer1.ActiveChild != null &&
                                     ((ChildWindow)MdiContainer1.ActiveChild).CanShowProperties();
            MenuItemAddObject.IsEnabled = MdiContainer1.ActiveChild != null &&
                                          ((ChildWindow)MdiContainer1.ActiveChild).CanAddObject();
            MenuItemProperties.IsEnabled = MdiContainer1.ActiveChild != null;

            foreach (var item in MetadataItemMenu.Items.Cast<FrameworkElement>().Where(x => x is MenuItem).ToList())
            {
                item.IsEnabled = _sqlContext != null;
            }
        }

        private void MenuItemNewQuery_OnClick(object sender, RoutedEventArgs e)
        {
            MdiContainer1.Children.Add(CreateChildWindow());
        }

        private ChildWindow CreateChildWindow(string caption = "")
        {
            if (_sqlContext == null) return null;

            var title = string.IsNullOrEmpty(caption) ? "New Query" : caption;
            if (MdiContainer1.Children.Any(x => x.Title == title))
            {
                for (var i = 1; i < 1000; i++)
                {
                    if (MdiContainer1.Children.Any(x => x.Title == title + " (" + i + ")"))
                        continue;
                    title += " (" + i + ")";
                    break;
                }
            }

            var window = new ChildWindow(_sqlContext)
            {
                State = StateWindow.Maximized,
                Title = title,
                SqlFormattingOptions = _sqlFormattingOptions,
                SqlGenerationOptions = _sqlGenerationOptions
            };

            window.Closing += Window_Closing;
            window.SaveQueryEvent += Window_SaveQueryEvent;
            window.SaveAsInFileEvent += Window_SaveAsInFileEvent;
            window.SaveAsNewUserQueryEvent += Window_SaveAsNewUserQueryEvent;

            return window;
        }

        private void Window_Closing(object sender, EventArgs e)
        {
            var window = sender as ChildWindow;

            if (window == null) return;

            window.Closing -= Window_Closing;
            window.SaveQueryEvent -= Window_SaveQueryEvent;
            window.SaveAsInFileEvent -= Window_SaveAsInFileEvent;
            window.SaveAsNewUserQueryEvent -= Window_SaveAsNewUserQueryEvent;
        } 

        private bool InitializeSqlContext()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                // setup the query builder with metadata and syntax providers
                if (_selectedConnection.IsXmlFile)
                {
                    _sqlContext = new SQLContext
                    {
                        SyntaxProvider = _selectedConnection.ConnectionDescriptor.SyntaxProvider,
                        LoadingOptions = { OfflineMode = true },
                        MetadataStructureOptions = {AllowFavourites = true}
                    };
					
                    try
                    {
                        _sqlContext.MetadataContainer.ImportFromXML(_selectedConnection.XMLPath);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
                else
                {
                    try
                    {
                        _sqlContext = _selectedConnection.ConnectionDescriptor.GetSqlContext();
                        _sqlContext.MetadataStructureOptions.AllowFavourites = true;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }

                TextBlockConnectionName.Text = _selectedConnection.Name;

                DatabaseSchemaView1.SQLContext = _sqlContext;
                DatabaseSchemaView1.InitializeDatabaseSchemaTree();

                QueriesView.SQLContext = _sqlContext;
                QueriesView.SQLQuery = new SQLQuery(_sqlContext);
		QueriesView.Initialize();

                if (MdiContainer1.Children.Count > 0)
                {
                    foreach (var mdiChildWindow in MdiContainer1.Children.ToList())
                        mdiChildWindow.Close();
                }
            }
            finally
            {
                if (_sqlContext.MetadataContainer.LoadingOptions.OfflineMode)
                {
                    TsmiOfflineMode.IsChecked = true;
                }
                Mouse.OverrideCursor = null;
            }

            return true;
        }

        #region Executed commands
        private void CommandOpen_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog
            {
                DefaultExt = "sql",
                Filter = "SQL query files (*.sql)|*.sql|All files|*.*"
            };

            if (openFileDialog1.ShowDialog() != true) return;

            var sb = new StringBuilder();

            using (var sr = new StreamReader(openFileDialog1.FileName))
            {
                string s;

                while ((s = sr.ReadLine()) != null)
                {
                    sb.AppendLine(s);
                }
            }

            if (_sqlContext == null)
                CommandNew_OnExecuted(null, null);

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                var window = CreateChildWindow(Path.GetFileName(openFileDialog1.FileName));

                window.FileSourceUrl = openFileDialog1.FileName;
                window.QueryText = sb.ToString();
                window.SqlFormattingOptions = _sqlFormattingOptions;
                window.SqlSourceType = Common.Helpers.SourceType.File;

                MdiContainer1.Children.Add(window);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void CommandSave_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (MdiContainer1.ActiveChild == null) return;

            SaveInFile((ChildWindow)MdiContainer1.ActiveChild);
        }

        private void CommandExit_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void CommandNew_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var cf = new DatabaseConnectionWindow(_showHintConnection) {Owner = this};

            _showHintConnection = false;

            if (cf.ShowDialog() != true) return;
            _selectedConnection = cf.SelectedConnection;

            if (!InitializeSqlContext())
                return;

            if (string.IsNullOrEmpty(_selectedConnection.UserQueries)) return;

            var bytes = Encoding.UTF8.GetBytes(_selectedConnection.UserQueries);

            using (var reader = new MemoryStream(bytes))
            {
                QueriesView.ImportFromXML(reader);
            }
        }

        private void CommandUndo_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var child = MdiContainer1.ActiveChild as ChildWindow;

            if (child == null) return;

            child.Undo();
        }

        private void CommandRedo_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var child = MdiContainer1.ActiveChild as ChildWindow;

            if (child == null) return;

            child.Redo();
        }

        private void CommandCopy_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var child = MdiContainer1.ActiveChild as ChildWindow;

            if (child == null) return;

            child.Copy();
        }

        private void CommandPaste_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var child = MdiContainer1.ActiveChild as ChildWindow;

            if (child == null) return;

            child.Paste();
        }

        private void CommandCut_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var child = MdiContainer1.ActiveChild as ChildWindow;

            if (child == null) return;

            child.Cut();
        }

        private void CommandSelectAll_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var child = MdiContainer1.ActiveChild as ChildWindow;

            if (child == null) return;

            child.SelectAll();
        }
        #endregion

        #region MenuItem ckick
        private void MenuItemQueryStatistics_OnClick(object sender, RoutedEventArgs e)
        {
            var child = MdiContainer1.ActiveChild as ChildWindow;
            if (child != null) child.ShowQueryStatistics();
        }

        private void MenuItemCascade_OnClick(object sender, RoutedEventArgs e)
        {
            MdiContainer1.LayoutMdi(MdiLayout.Cascade);
        }

        private void MenuItemVertical_OnClick(object sender, RoutedEventArgs e)
        {
            MdiContainer1.LayoutMdi(MdiLayout.TileVertical);
        }

        private void MenuItemHorizontal_OnClick(object sender, RoutedEventArgs e)
        {
            MdiContainer1.LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void MenuItemQueryAddDerived_OnClick(object sender, RoutedEventArgs e)
        {
            if (MdiContainer1.ActiveChild == null) return;
            var window = (ChildWindow)MdiContainer1.ActiveChild;
            window.AddDerivedTable();
        }

        private void MenuItemCopyUnionSq_OnClick(object sender, RoutedEventArgs e)
        {
            if (MdiContainer1.ActiveChild == null) return;
            var window = (ChildWindow)MdiContainer1.ActiveChild;
            window.CopyUnionSubQuery();
        }

        private void MenuItemAddUnionSq_OnClick(object sender, RoutedEventArgs e)
        {
            if (MdiContainer1.ActiveChild == null) return;
            var window = (ChildWindow)MdiContainer1.ActiveChild;
            window.AddUnionSubQuery();
        }

        private void MenuItemProp_OnClick(object sender, RoutedEventArgs e)
        {
            if (MdiContainer1.ActiveChild == null) return;
            var window = (ChildWindow)MdiContainer1.ActiveChild;
            window.PropertiesQuery();
        }

        private void MenuItemAddObject_OnClick(object sender, RoutedEventArgs e)
        {
            if (MdiContainer1.ActiveChild == null) return;
            var window = (ChildWindow)MdiContainer1.ActiveChild;
            window.AddObject();
        }

        private void MenuItemProperties_OnClick(object sender, RoutedEventArgs e)
        {
            if (MdiContainer1.ActiveChild == null) return;
            var window = (ChildWindow)MdiContainer1.ActiveChild;
            var propWindow = new QueryPropertiesWindow(window, DatabaseSchemaView1.Options);
            propWindow.ShowDialog();
        }

        private void MenuItem_OfflineMode_OnChecked(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;

            if (menuItem.IsChecked)
            {
                try
                {
                    Cursor = Cursors.Wait;

                    _sqlContext.MetadataContainer.LoadAll(true);
                }
                finally
                {
                    Cursor = Cursors.Arrow;
                }
            }

            _sqlContext.MetadataContainer.LoadingOptions.OfflineMode = menuItem.IsChecked;

        }

        private void MenuItem_RefreashMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            if (_sqlContext.MetadataProvider != null && _sqlContext.MetadataProvider.Connected)
            {
                // to refresh metadata, just clear already loaded items
                _sqlContext.MetadataContainer.Clear();
            }
        }

        private void MenuItem_ClearMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            // to refresh metadata, just clear already loaded items
            _sqlContext.MetadataContainer.Clear();
        }

        private void MenuItem_LoadMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog { Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*" };

            if (fileDialog.ShowDialog() != true) return;

            _sqlContext.MetadataContainer.LoadingOptions.OfflineMode = true;
            _sqlContext.MetadataContainer.ImportFromXML(fileDialog.FileName);

            DatabaseSchemaView1.InitializeDatabaseSchemaTree();
        }

        private void MenuItem_SaveMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new SaveFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                FileName = "Metadata.xml"
            };

            if (fileDialog.ShowDialog() != true) return;

            _sqlContext.MetadataContainer.LoadAll(true);
            _sqlContext.MetadataContainer.ExportToXML(fileDialog.FileName);
        }

        private void MenuItem_About_OnClick(object sender, RoutedEventArgs e)
        {
            var f = new AboutForm { Owner = this };

            f.ShowDialog();
        }
        #endregion

        private void DatabaseSchemaView_OnItemDoubleClick(object sender, MetadataStructureItem clickeditem)
        {
            if (clickeditem.MetadataItem == null)
                return;

            // Adding a table to the currently active query.
            var objectMetadata = (MetadataType.ObjectMetadata & clickeditem.MetadataItem.Type) != 0;
            var obj = (MetadataType.Objects & clickeditem.MetadataItem.Type) != 0;

            if (!obj && !objectMetadata) return;

            if (MdiContainer1.ActiveChild == null && MdiContainer1.Children.Count == 0)
            {
                var childWindow = CreateChildWindow();
                if (childWindow == null) return;

                MdiContainer1.Children.Add(childWindow);
                MdiContainer1.ActiveChild = childWindow;
            }

            var window = (ChildWindow)MdiContainer1.ActiveChild;

            if (window == null) return;

            var metadataItem = clickeditem.MetadataItem;

            if (metadataItem == null) return;

            if ((metadataItem.Type & MetadataType.Objects) <= 0 && metadataItem.Type != MetadataType.Field) return;

            using (var qualifiedName = metadataItem.GetSQLQualifiedName(null, true))
            {
                window.QueryView.AddObjectToActiveUnionSubQuery(qualifiedName.GetSQL());
            }
        }

        private void LanguageMenuItemChecked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            var menuItem = (MenuItem)sender;
            var lng = menuItem.Tag.ToString();
            Language = XmlLanguage.GetLanguage(lng);

        }

        private void QueriesView_OnEditUserQuery(object sender, MetadataStructureItemCancelEventArgs e)
        {
            // Opening the user query in a new query window.
            if (e.MetadataStructureItem == null) return;

            var window = CreateChildWindow(e.MetadataStructureItem.MetadataItem.Name);

            window.UserMetadataStructureItem = e.MetadataStructureItem;
            window.SqlSourceType = Common.Helpers.SourceType.UserQueries;
            MdiContainer1.Children.Add(window);
            MdiContainer1.ActiveChild = window;

            window.QueryText = ((MetadataObject)e.MetadataStructureItem.MetadataItem).Expression.Trim('(', ')');
        }

        private void Window_SaveQueryEvent(object sender, EventArgs e)
        {
            // Saving the current query
            var windowChild = MdiContainer1.ActiveChild as ChildWindow;

            if (windowChild == null) return;

            switch (windowChild.SqlSourceType)
            {
                // as a new user query
                case Common.Helpers.SourceType.New:
                    SaveNewUserQuery(windowChild);
                    break;
                // in a text file
                case Common.Helpers.SourceType.File:
                    SaveInFile(windowChild);
                    break;
                // replacing an exising user query 
                case Common.Helpers.SourceType.UserQueries:
                    SaveUserQuery(windowChild);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (windowChild.IsNeedClose) windowChild.Close();
        }

        private void Window_SaveAsInFileEvent(object sender, EventArgs e)
        {
            var windowChild = MdiContainer1.ActiveChild as ChildWindow;

            if (windowChild == null) return;

            SaveInFile(windowChild);

            if (windowChild.IsNeedClose) windowChild.Close();
        }

        private void Window_SaveAsNewUserQueryEvent(object sender, EventArgs e)
        {
            var windowChild = MdiContainer1.ActiveChild as ChildWindow;

            if (windowChild == null) return;

            SaveNewUserQuery(windowChild);

            if (windowChild.IsNeedClose) windowChild.Close();
        }

        private static void SaveInFile(ChildWindow windowChild)
        {
            if (string.IsNullOrEmpty(windowChild.FileSourceUrl) || !File.Exists(windowChild.FileSourceUrl))
            {
                var saveFileDialog1 = new SaveFileDialog()
                {
                    DefaultExt = "sql",
                    FileName = "query",
                    Filter = "SQL query files (*.sql)|*.sql|All files|*.*"
                };

                if (saveFileDialog1.ShowDialog() != true) return;

                windowChild.SqlSourceType = Common.Helpers.SourceType.File;
                windowChild.FileSourceUrl = saveFileDialog1.FileName;
            }

            using (var sw = new StreamWriter(windowChild.FileSourceUrl))
            {
                sw.Write(windowChild.FormattedQueryText);
            }
            windowChild.IsModified = false;
        }

        private void SaveUserQuery(ChildWindow childWindow)
        {
            if(!childWindow.IsModified) return;
            if (childWindow.UserMetadataStructureItem == null) return;

            if (!UserQueries.IsUserQueryExist(childWindow.SqlQuery.SQLContext.MetadataContainer,
                childWindow.UserMetadataStructureItem.MetadataName)) return;

            UserQueries.SaveUserQuery(childWindow.SqlQuery.SQLContext.MetadataContainer,
                childWindow.UserMetadataStructureItem, "(" + childWindow.QueryText + ")", ActiveQueryBuilder.View.Helpers.GetLayout(childWindow.SqlQuery.QueryRoot));

            childWindow.IsModified = false;
            SaveSettings();
        }

        private void SaveNewUserQuery(ChildWindow childWindow)
        {
            string title;
            MetadataStructureItem newItem;
            if(string.IsNullOrEmpty(childWindow.QueryText)) return;

            do
            {
                var window = new WindowNameQuery {Owner = this};
                var answer = window.ShowDialog();
                if (answer != true) return;

                title = window.NameQuery;

                if (UserQueries.IsUserQueryExist(childWindow.SqlQuery.SQLContext.MetadataContainer, title))
                {
                    var path = QueriesView.GetPathAtUserQuery(title);
                    var message = string.IsNullOrEmpty(path)
                        ? @"The same-named query already exists in the root folder."
                        : string.Format("The same-named query already exists in the \"{0}\" folder.", path);

                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                var atItem = QueriesView.FocusedItem ?? QueriesView.MetadataStructure;

                if (!UserQueries.IsFolder(atItem))
                    atItem = atItem.Parent;

                newItem = UserQueries.AddUserQuery(childWindow.SqlQuery.SQLContext.MetadataContainer, atItem, title,
                    "(" + childWindow.SqlQuery.SQL + ")", (int) DefaultImageListImageIndices.VirtualObject, ActiveQueryBuilder.View.Helpers.GetLayout(childWindow.SqlQuery.QueryRoot));

                break;

            } while (true);

            childWindow.Title = title;
            childWindow.UserMetadataStructureItem = newItem;
            childWindow.SqlSourceType = Common.Helpers.SourceType.UserQueries;
            childWindow.IsModified = false;

            SaveSettings();
        }

        // Saving user queries to the connection settings
        private void SaveSettings()
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    QueriesView.ExportToXML(stream);
                    stream.Position = 0;

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        _selectedConnection.UserQueries = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception exception)
            {
                throw new QueryBuilderException(exception.Message, exception);
            }

            Properties.Settings.Default.XmlFiles = App.XmlFiles;
            Properties.Settings.Default.Connections = App.Connections;
            Properties.Settings.Default.Save();

            QueriesView.Refresh();
        }

        // Closing the current query window on deleting the corresponding user query.
        private void QueriesView_OnUserQueryItemRemoved(object sender, MetadataStructureItem item)
        {
            var childWindow =
                MdiContainer1.Children.Cast<ChildWindow>().FirstOrDefault(x => x.UserMetadataStructureItem == item);

            if (childWindow != null) childWindow.ForceClose();

            SaveSettings();
        }

        private void QueriesView_OnErrorMessage(object sender, MetadataStructureItemErrorEventArgs eventArgs)
        {
            var wMessage = new WindowMessage
            {
                Owner = this,
                Text = eventArgs.Message,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Title = "UserQueries error"
            };

            var buttonOk = new Button { Content = "OK", HorizontalAlignment = HorizontalAlignment.Center, Width = 75 };
            buttonOk.Click += delegate { wMessage.Close(); };

            wMessage.Buttons.Add(buttonOk);
            wMessage.ShowDialog();
        }

        private void QueriesView_OnUserQueryItemRenamed(object sender, MetadataStructureItemTextChangedEventArgs e)
        {
            SaveSettings();
        }

        private void QueriesView_OnValidateItemContextMenu(object sender, MetadataStructureItemMenuEventArgs e)
        {
            e.Menu.AddItem("Copy SQL", Execute_SqlExpression, false, true, null,
                ((MetadataObject) e.MetadataStructureItem.MetadataItem).Expression);
        }

        private static void Execute_SqlExpression(object sender, EventArgs eventArgs)
        {
            var item = (ICustomMenuItem) sender;

            Clipboard.SetText(item.Tag.ToString(), TextDataFormat.UnicodeText);

            Debug.WriteLine("SQL: {0}", item.Tag);
        }

        private void MenuItemExecuteUserQuery_OnClick(object sender, RoutedEventArgs e)
        {
            if (QueriesView.FocusedItem == null) return;

            var window = CreateChildWindow(QueriesView.FocusedItem.MetadataItem.Name);

            window.UserMetadataStructureItem = QueriesView.FocusedItem;
            window.SqlSourceType = Common.Helpers.SourceType.UserQueries;
            MdiContainer1.Children.Add(window);
            MdiContainer1.ActiveChild = window;

            window.QueryText = ((MetadataObject)QueriesView.FocusedItem.MetadataItem).Expression.Trim('(', ')');
            window.OpenExecuteTab();
        }

        private void QueriesView_OnSelectedItemChanged(object sender, EventArgs e)
        {
            MenuItemExecuteUserQuery.IsEnabled = QueriesView.FocusedItem != null && !QueriesView.FocusedItem.IsFolder();
        }
    }
}
