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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.Commands;
using ActiveQueryBuilder.Core.PropertiesEditors;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.DatabaseSchemaView;
using ActiveQueryBuilder.View.EventHandlers.MetadataStructureItems;
using ActiveQueryBuilder.View.MetadataEditor;
using ActiveQueryBuilder.View.QueryView;
using ActiveQueryBuilder.View.WPF;
using ActiveQueryBuilder.View.WPF.Commands;
using Helpers = ActiveQueryBuilder.View.WPF.Helpers;

namespace MetadataEditorDemo.Common
{
    /// <summary>
    /// Interaction logic for MetadataEditorControl.xaml
    /// </summary>
    [ToolboxItem(false), DesignTimeVisible(false)]
    public sealed partial class MetadataEditorControl : IMetadataEditorView
    {
        #region private members
        private readonly MetadataEditorController _controller;
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();
        private CompositeDisposable _contextMenuSubscriptions;
        private SQLContext _sqlContext;
        private bool _structureTreeVisible = true;
        private MetadataEditorOptions _metadataEditorOptions = 0;
        private bool _isVisibleMiddleColumn = true;
        private Command _fillStructurePopupOpenCommand;

        private double _minWidthColumnStructure;
        private GridLength _widthColumnStructure;
        private GridLength _widthColumnSchema;
        private ContextMenu _separatorContextMenu;
        #endregion

        #region public event

        public event EventHandler LoadStart
        {
            add { _controller.LoadStart += value; }
            remove { _controller.LoadStart -= value; }
        }

        public event EventHandler LoadStep
        {
            add { _controller.LoadStep += value; }
            remove { _controller.LoadStep -= value; }
        }

        public event EventHandler LoadFinish
        {
            add { _controller.LoadFinish += value; }
            remove { _controller.LoadFinish -= value; }
        }

        #endregion

        #region Interface implemetations
        public IDatabaseSchemaView ContainerView => DatabaseSchemaTree;
        public IDatabaseSchemaView StructureView => MetadataStructureTree;
        public IPropertiesBar PropertiesBar => PropertiesBarControl;
        public IControlFactory ControlFactory => ActiveQueryBuilder.View.WPF.ControlFactory.Instance;
        string IMetadataEditorView.Language => Helpers.ConvertLanguageFromNative(Language);

        #endregion

        #region public property
        public bool OpenContainerLoadFormIfNotConnected { get; set; }

        public static readonly DependencyProperty ContainerViewReadOnlyProperty = DependencyProperty.Register(
            "ContainerViewReadOnly", typeof(bool), typeof(MetadataEditorControl), new PropertyMetadata(false, ContainerViewPropertyChanged));

        private static void ContainerViewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (MetadataEditorControl)d;
            self.MenuItemDatabaseAddRoot.IsEnabled = !(bool)e.NewValue;
        }

        public bool ContainerViewReadOnly
        {
            get { return (bool)GetValue(ContainerViewReadOnlyProperty); }
            set { SetValue(ContainerViewReadOnlyProperty, value); }
        }
        
        public bool IsChanged
        {
            get { return _controller.IsChanged; }
            set { _controller.IsChanged = value; }
        }

        public bool HideVirtualObjects
        {
            get { return _controller.HideVirtualObjects; }
            set { _controller.HideVirtualObjects = value; }
        }

        public bool StructureTreeVisible
        {
            get { return _structureTreeVisible; }
            set
            {
                if (_structureTreeVisible == value)
                {
                    return;
                }

                _structureTreeVisible = value;
                if (_structureTreeVisible) //show
                {
                    ShowMetadataStructureTree();
                }
                else //hide
                {
                    HideMetadataStructureTree();
                }
            }
        }

        public MetadataEditorOptions MetadataEditorOptions
        {
            get { return _metadataEditorOptions; }
            set
            {
                if ((value & MetadataEditorOptions.DisableStructurePane) == MetadataEditorOptions.DisableStructurePane
                    && StructureTreeVisible == false)
                {
                    StructureTreeVisible = false;

                }
                else if ((value & MetadataEditorOptions.DisableStructurePane) == 0
                         && StructureTreeVisible)
                {
                    StructureTreeVisible = true;
                }

                _metadataEditorOptions = value;
            }
        }
        #endregion

        public MetadataEditorControl()
        {
            InitializeComponent();

            _controller = new MetadataEditorController(this);

            HideVirtualObjects = false;

            InformationMessageControl.FixIssueEvent += MessageControl_FixIssueEvent;
            InformationMessageControl.Closing += MessageControl_Closing;

            PropertiesBarControl.InformationMessageHost = this;

            DatabaseSchemaTree.Options.SortingType = ObjectsSortingType.None;
            MetadataStructureTree.Options.SortingType = ObjectsSortingType.None;

            DatabaseSchemaTree.Options.DefaultExpandMetadataType |= MetadataType.Root;
            MetadataStructureTree.Options.DefaultExpandMetadataType |= MetadataType.Root;

            DatabaseSchemaTree.Options.DefaultExpandFolderNodes = true;
            MetadataStructureTree.Options.DefaultExpandFolderNodes = true;

            CreateAndBindCommands();
        }

        private void MessageControl_FixIssueEvent(object sender, EventArgs eventArgs)
        {
            _controller.OnFixIssue(InformationMessageControl.ErrorDescription);
        }

        private void MessageControl_Closing(object sender, EventArgs eventArgs)
        {
            _controller.SetCurrentError(null);

            var element = InformationMessageControl.Owner as ISupportFocusOnError;
            element?.Revert();
        }

        private void CreateAndBindCommands()
        {
            _subscriptions.Add(
                ActiveQueryBuilder.Core.Localization.MetadataEditor.DatabaseSchemaCaption.Subscribe(x => LabelHeaderDatabase.Text = x));
            _subscriptions.Add(
                ActiveQueryBuilder.Core.Localization.MetadataEditor.MetadataStructureCaption.Subscribe(x =>
                    LabelHeaderMeatdaStructure.Text = x));
            _subscriptions.Add(
                ActiveQueryBuilder.Core.Localization.MetadataEditor.PropertiesBarCaption.Subscribe(x => LabelHeaderProperties.Text = x));

            _subscriptions.Add(
                ActiveQueryBuilder.Core.Localization.MetadataEditor.GroupByServer.Subscribe(x => CheckBoxByServer.Content = x));
            _subscriptions.Add(
                ActiveQueryBuilder.Core.Localization.MetadataEditor.GroupByDatabase.Subscribe(x => CheckBoxByDatabase.Content = x));
            _subscriptions.Add(
                ActiveQueryBuilder.Core.Localization.MetadataEditor.GroupBySchema.Subscribe(x => CheckBoxBySchema.Content = x));
            _subscriptions.Add(
                ActiveQueryBuilder.Core.Localization.MetadataEditor.GroupByTypes.Subscribe(x => CheckBoxByTypes.Content = x));
            _subscriptions.Add(
                ActiveQueryBuilder.Core.Localization.MetadataEditor.GenerateObjects.Subscribe(x => CheckBoxGenerateObjects.Content = x));
            _subscriptions.Add(
                ActiveQueryBuilder.Core.Localization.MetadataEditor.StructureWillBeCleared.Subscribe(x => TextBlockWarning.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.Common.Proceed.Subscribe(x => ButtonProceed.Content = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.Common.Cancel.Subscribe(x => ButtonCancel.Content = x));

            _subscriptions.Add(CommandBinder.Bind(MenuItemMetadataLoadAll, _controller.LoadContainerCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.LoadEntireContainer, x => MenuItemMetadataLoadAll.IsEnabled = x,
                false));

            _subscriptions.Add(CommandBinder.Bind(MenuItemDatabaseDelete, _controller.DeleteMetadataItemCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.DeleteMetadataItem, x => MenuItemDatabaseDelete.IsEnabled = x,
                false));

            _subscriptions.Add(CommandBinder.Bind(MenuItemMetadataStructureAdd, _controller.AddStructureItemCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddStructureItem,
                x => MenuItemMetadataStructureAdd.IsEnabled = x, false));

            _subscriptions.Add(CommandBinder.Bind(MenuItemMetadataStructureDelete,
                _controller.DeleteStructureItemCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.DeleteMetadataItem,
                x => MenuItemMetadataStructureDelete.IsEnabled = x, false));

            _fillStructurePopupOpenCommand = new Command(MenuItemMetadataStructureFillFromSchema_OnClick);

            _subscriptions.Add(CommandBinder.Bind(MenuItemAddLinkedServer, _controller.AddLinkedServerCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddServer, x => MenuItemAddLinkedServer.IsEnabled = x));

            _subscriptions.Add(CommandBinder.Bind(MenuItemAddDatabase, _controller.AddDatabaseCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddDatabase, x => MenuItemAddDatabase.IsEnabled = x));

            _subscriptions.Add(CommandBinder.Bind(MenuItemAddSchema, _controller.AddSchemaCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddSchema, x => MenuItemAddSchema.IsEnabled = x));

            _subscriptions.Add(CommandBinder.Bind(MenuItemAddPackage, _controller.AddPackageCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddPackage, x => MenuItemAddPackage.IsEnabled = x));

            _subscriptions.Add(CommandBinder.Bind(MenuItemAddTable, _controller.AddTableCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddTable, x => MenuItemAddTable.IsEnabled = x));

            _subscriptions.Add(CommandBinder.Bind(MenuItemAddView, _controller.AddViewCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddView, x => MenuItemAddView.IsEnabled = x));

            _subscriptions.Add(CommandBinder.Bind(MenuItemAddProcedure, _controller.AddProcedureCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddProcedure, x => MenuItemAddProcedure.IsEnabled = x));

            _subscriptions.Add(CommandBinder.Bind(MenuItemAddSynonym, _controller.AddSynonymCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddSynonym, x => MenuItemAddSynonym.IsEnabled = x));

            _subscriptions.Add(CommandBinder.Bind(MenuItemAddField, _controller.AddFieldCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddField, x => MenuItemAddField.IsEnabled = x));

            _subscriptions.Add(CommandBinder.Bind(MenuItemAddForeignKey, _controller.AddForeignKeyCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddForeignKey, x => MenuItemAddForeignKey.IsEnabled = x));

            _subscriptions.Add(CommandBinder.Bind(MenuItemMetadataStructureFillFromSchema,
                _fillStructurePopupOpenCommand, ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.FillStructure,
                x => MenuItemMetadataStructureFillFromSchema.IsEnabled = x));

            Loaded += (sender, args) =>
            {
                _controller.UpdateStructureCommands();
                _controller.UpdateContainerCommands();
            };

           
        }

        #region Interface methods
        public void ShowInformationMessage(object sender, PropertyErrorDescription description)
        {
            _controller.SetCurrentError(description);

            if (description == null || !description.IsError)
            {
                InformationMessageControl.Hide();
                return;
            }

            InformationMessageControl.Owner = sender;
            InformationMessageControl.Show(description);
            InformationMessageControl.HorizontalAlignment = HorizontalAlignment.Center;
            InformationMessageControl.BringIntoView();
        }

        public void SetCustomControlToPropertiesBar(object control)
        {
            var c = control as FrameworkElement;
            if (c == null)
                return;

            c.VerticalAlignment = VerticalAlignment.Stretch;
            c.HorizontalAlignment = HorizontalAlignment.Stretch;

            ((Grid)PropertiesBar).RowDefinitions.Clear();

            var row = 0;

            foreach (FrameworkElement child in ((Grid)PropertiesBar).Children)
            {
                child.SetValue(Grid.RowProperty, row);
                ((Grid)PropertiesBar).RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                row++;
            }
            c.SetValue(Grid.RowProperty, row);
            ((Grid)PropertiesBar).RowDefinitions.Add(new RowDefinition());
            ((Grid) PropertiesBar).Children.Add(c);
        }

        public void AppendStructureFilterControl(object control, bool isExpanded)
        {
            var c = control as FrameworkElement;
            if (c == null)
                return;

            var filterControl = new GroupContainer
            {
                CanCollapse = true,
                Text = Helpers.Localizer.GetString("strMetadataStructureFilter",Helpers.ConvertLanguageFromNative(Language), 
                    LocalizableConstantsInternal.strMetadataStructureFilter),
                VerticalAlignment = VerticalAlignment.Top,
                IsExpanded = isExpanded
            };

            var row = 0;

            ((Grid)PropertiesBar).RowDefinitions.Clear();

            foreach (FrameworkElement child in ((Grid) PropertiesBar).Children)
            {
                child.SetValue(Grid.RowProperty, row);
                ((Grid) PropertiesBar).RowDefinitions.Add(new RowDefinition {Height = GridLength.Auto});
                row++;
            }

            ((Grid) PropertiesBar).RowDefinitions.Add(new RowDefinition());

            filterControl.AddSecondaryControl(c);

            filterControl.SetValue(Grid.RowProperty, row);

            ((Grid)PropertiesBar).Children.Add(filterControl);
        }

        public bool ConfirmItemDeletion()
        {
            return MessageBox.Show(
                       Helpers.Localizer.GetString("strDeleteItemConfirmation",
                           LocalizableConstantsUI.strDeleteItemConfirmation),
                       Helpers.Localizer.GetString("strConfirmation", LocalizableConstantsUI.strConfirmation),
                       MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes;
        }
        #endregion

        #region public methods
        public void RefreshContainerTree()
        {
            DatabaseSchemaTree.MetadataStructure.Refresh();
        }

        public List<string> ValidateBeforeApply()
        {
            return _controller.ValidateBeforeApply();
        }

        public void ShowContainerLoadForm()
        {
            _controller.ShowContainerLoadForm();
        }

        public void StopLoading()
        {
            _controller.StopLoading();
        }

        public void Init(SQLContext sqlContext)
        {
            _sqlContext = sqlContext;

            using (Disposable.Create(() => Cursor = Cursors.Arrow))
            {
                Cursor = Cursors.Wait;

                _controller.Init(sqlContext);
            }
           
            ButtonProceed.Click += btnGenerateStructure_Click;
            _subscriptions.Add(Disposable.Create(() => ButtonProceed.Click -= btnGenerateStructure_Click));

            ButtonCancel.Click += btnCancelGenerate_Click;
            _subscriptions.Add(Disposable.Create(() => ButtonCancel.Click -= btnCancelGenerate_Click));
            
        }

        private void btnGenerateStructure_Click(object sender, EventArgs e)
        {
            _controller.GenerateStructure(CheckBoxByServer.IsChecked == true, CheckBoxByDatabase.IsChecked == true, CheckBoxBySchema.IsChecked == true,
                CheckBoxByTypes.IsChecked == true, CheckBoxGenerateObjects.IsChecked == true);
        }

        private void btnCancelGenerate_Click(object sender, EventArgs e)
        {
            FillStructureFromSchemaPopup.IsOpen = false;

            CheckBoxByServer.IsChecked = false;
            CheckBoxByDatabase.IsChecked = false;
            CheckBoxBySchema.IsChecked = false;
            CheckBoxByTypes.IsChecked = false;
            CheckBoxGenerateObjects.IsChecked = false;
        }

        #endregion

        #region private methods

        private void HideMetadataStructureTree()
        {
            if (!_isVisibleMiddleColumn) return;

            _isVisibleMiddleColumn = false;

            var leftControls = GridRoot.Children.Cast<FrameworkElement>()
                .Where(x => Grid.GetColumn(x) == 0 && !(x is GridSplitter)).ToList();

            var middleControls = GridRoot.Children.Cast<FrameworkElement>()
                .Where(x => Grid.GetColumn(x) == 1 && !(x is GridSplitter)).ToList();

            foreach (var frameworkElement in middleControls)
                frameworkElement.Visibility = Visibility.Collapsed;

            foreach (var frameworkElement in leftControls)
                frameworkElement.SetValue(Grid.ColumnSpanProperty, 2);

            GridSplitterLeft.Visibility = Visibility.Collapsed;
        }

        private void ShowMetadataStructureTree()
        {
            if (_isVisibleMiddleColumn) return;

            _isVisibleMiddleColumn = true;

            var leftControls = GridRoot.Children.Cast<FrameworkElement>()
                .Where(x => Grid.GetColumn(x) == 0 && !(x is GridSplitter)).ToList();

            var middleControls = GridRoot.Children.Cast<FrameworkElement>()
                .Where(x => Grid.GetColumn(x) == 1 && !(x is GridSplitter)).ToList();

            foreach (var frameworkElement in middleControls)
                frameworkElement.Visibility = Visibility.Visible;

            foreach (var frameworkElement in leftControls)
                frameworkElement.SetValue(Grid.ColumnSpanProperty, 1);

            GridSplitterLeft.Visibility = Visibility.Visible;
        }

        #endregion

        private void MenuItemMetadataStructureFillFromSchema_OnClick()
        {
            FillStructureFromSchemaPopup.IsOpen = true;

            TextBlockWarning.Visibility= !_controller.IsStructureEmpty(MetadataStructureTree.MetadataStructure) ? Visibility.Visible:Visibility.Collapsed;
        }


        internal class ListLog : ILog
        {
            private readonly List<string> _errorList;

            public ListLog(List<string> errorList)
            {
                _errorList = errorList;
            }

            public void Trace(string msg)
            {
                _errorList.Add("Trace: " + msg);
            }

            public void Warning(string msg)
            {
                _errorList.Add("Warn : " + msg);
            }

            public void Error(string msg)
            {
                _errorList.Add("Error: " + msg);
            }

            public void Error(string msg, Exception e)
            {
                Error(msg);
                _errorList.Add(e.ToString());
            }
        }

        private void MetadataStructureTree_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    _controller.DeleteSelectedStructureItems();
                    break;
                case Key.Insert:
                    _controller.AddStructureItemToFocused();
                    break;
            }
        }

        private void DatabaseSchemaTree_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (ContainerViewReadOnly) return;
            
            switch (e.Key)
            {
                case Key.Delete:
                    _controller.DeleteSelectedContainerItems();
                    break;
                case Key.Insert:
                    _controller.InsertDefaultContainerItem();
                    break;
            }
        }

        private void DatabaseSchemaTree_OnValidateItemContextMenu(object sender, MetadataStructureItemMenuEventArgs e)
        {
            if (DatabaseSchemaTree.FocusedItem == null || ContainerViewReadOnly) return;
            
            _contextMenuSubscriptions = new CompositeDisposable();

            var menu = e.Menu;
            menu.ClearItems();

            if (OpenContainerLoadFormIfNotConnected &&
                (_sqlContext.MetadataProvider == null || !_sqlContext.MetadataProvider.Connected))
            {
                var contextMenuItemSchemaConnectAndLoad = (MenuItem) menu.AddItem("", null, false, false, null, null);

                _subscriptions.Add(CommandBinder.Bind(contextMenuItemSchemaConnectAndLoad,
                    _controller.ConnectAndLoadCommand,
                    ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.ConnectAndLoadChilds,
                    x => contextMenuItemSchemaConnectAndLoad.IsEnabled = x));
            }

            if (!(OpenContainerLoadFormIfNotConnected &&
                  (_sqlContext.MetadataProvider == null || !_sqlContext.MetadataProvider.Connected)))
            {
                var contextMenuItemSchemaLoadChildDatabase =
                    (MenuItem) menu.AddItem("", null, false, false, null, null);

                _subscriptions.Add(CommandBinder.Bind(contextMenuItemSchemaLoadChildDatabase,
                    _controller.LoadSchemaChildsCommand, ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.LoadChildItems,
                    x => contextMenuItemSchemaLoadChildDatabase.IsEnabled = x));
            }

            var contextMenuItemSchemaDelete = (MenuItem) menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemSchemaDelete, _controller.DeleteMetadataItemCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.DeleteMetadataItem,
                x => contextMenuItemSchemaDelete.IsEnabled = x));

            var contextMenuItemSchemaClearChild = (MenuItem) menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemSchemaClearChild, _controller.ClearSchemaCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.ClearChildItems,
                x => contextMenuItemSchemaClearChild.IsEnabled = x));

            menu.AddSeparator();

            var contextMenuItemSchemaAddLinkedServer = (MenuItem) menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemSchemaAddLinkedServer,
                _controller.AddLinkedServerCommand, ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddServer,
                x => contextMenuItemSchemaAddLinkedServer.Visibility = x ? Visibility.Visible : Visibility.Collapsed));

            var contextMenuItemSchemaAddDatabase = (MenuItem) menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemSchemaAddDatabase, _controller.AddDatabaseCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddDatabase,
                x => contextMenuItemSchemaAddDatabase.Visibility = x ? Visibility.Visible : Visibility.Collapsed));

            var contextMenuItemSchemaAddSchema = (MenuItem) menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemSchemaAddSchema, _controller.AddSchemaCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddSchema,
                x => contextMenuItemSchemaAddSchema.Visibility = x ? Visibility.Visible : Visibility.Collapsed));

            var contextMenuItemSchemaAddPackage = (MenuItem) menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemSchemaAddPackage, _controller.AddPackageCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddPackage,
                x => contextMenuItemSchemaAddPackage.Visibility = x ? Visibility.Visible : Visibility.Collapsed));

            var contextMenuItemSchemaAddTable = (MenuItem) menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemSchemaAddTable, _controller.AddTableCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddTable,
                x => contextMenuItemSchemaAddTable.Visibility = x ? Visibility.Visible : Visibility.Collapsed));

            var contextMenuItemSchemaAddView = (MenuItem) menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemSchemaAddView, _controller.AddViewCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddView,
                x => contextMenuItemSchemaAddView.Visibility = x ? Visibility.Visible : Visibility.Collapsed));

            var contextMenuItemSchemaAddProcedure = (MenuItem) menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemSchemaAddProcedure, _controller.AddProcedureCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddProcedure,
                x => contextMenuItemSchemaAddProcedure.Visibility = x ? Visibility.Visible : Visibility.Collapsed));

            var contextMenuItemSchemaAddSynonym = (MenuItem) menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemSchemaAddSynonym, _controller.AddSynonymCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddSynonym,
                x => contextMenuItemSchemaAddSynonym.Visibility = x ? Visibility.Visible : Visibility.Collapsed));

            var contextMenuItemSchemaAddField = (MenuItem) menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemSchemaAddField, _controller.AddFieldCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddField,
                x => contextMenuItemSchemaAddField.Visibility = x ? Visibility.Visible : Visibility.Collapsed));

            var contextMenuItemSchemaAddForeignKey = (MenuItem) menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemSchemaAddForeignKey, _controller.AddForeignKeyCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddForeignKey,
                x => contextMenuItemSchemaAddForeignKey.Visibility = x ? Visibility.Visible : Visibility.Collapsed));

            var contextSubMenuItemSchemaSort = (MenuItem) menu.AddSubMenu("");
            _subscriptions.Add(
                ActiveQueryBuilder.Core.Localization.MetadataEditor.Sort.Subscribe(x => contextSubMenuItemSchemaSort.Header = x));

            var contextSubMenuItemSchemaSortByName =
                (MenuItem) ((ICustomSubMenu) contextSubMenuItemSchemaSort).AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextSubMenuItemSchemaSortByName,
                _controller.SortSchemaByNameCommand, ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.SortByName,
                x => contextSubMenuItemSchemaSortByName.Visibility = x ? Visibility.Visible : Visibility.Collapsed));

            var contextSubMenuItemSchemaSortByTypeAndName =
                (MenuItem) ((ICustomSubMenu) contextSubMenuItemSchemaSort).AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextSubMenuItemSchemaSortByTypeAndName,
                _controller.SortSchemaByTypeCommand, ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.SortByType,
                x => contextSubMenuItemSchemaSortByTypeAndName.Visibility =
                    x ? Visibility.Visible : Visibility.Collapsed));

            var contextMenuItemSchemaMoveToTop = (MenuItem) menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemSchemaMoveToTop, _controller.MoveUpSchemaCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.MoveToTop,
                x => contextMenuItemSchemaMoveToTop.Visibility = x ? Visibility.Visible : Visibility.Collapsed));

            var contextMenuItemSchemaMoveToBottom = (MenuItem) menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemSchemaMoveToBottom,
                _controller.MoveDownSchemaCommand, ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.MoveToBottom,
                x => contextMenuItemSchemaMoveToBottom.Visibility = x ? Visibility.Visible : Visibility.Collapsed));

            _controller.UpdateContainerCommands();
            menu.Closing += Menu_Closing;
        }

        private void Menu_Closing(object sender, EventArgs e)
        {
            if (_contextMenuSubscriptions == null) return;

            _contextMenuSubscriptions.Dispose();
            _contextMenuSubscriptions = null;
        }

        private void MetadataStructureTree_OnValidateItemContextMenu(object sender, MetadataStructureItemMenuEventArgs e)
        {
            _contextMenuSubscriptions = new CompositeDisposable();

            var menu = e.Menu;
            menu.ClearItems();

            var contextMenuItemStructureAddItem = (MenuItem)menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemStructureAddItem, _controller.AddStructureItemCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.AddStructureItem,
                x => contextMenuItemStructureAddItem.IsEnabled = x));

            var contextMenuItemStructureDelete= (MenuItem)menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemStructureDelete,
                _controller.DeleteStructureItemCommand, ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.DeleteStructureItem,
                x => contextMenuItemStructureDelete.IsEnabled = x));

            var contextMenuItemStructureCreateChild= (MenuItem)menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemStructureCreateChild,
                _controller.CreateStructureChildsCommand, ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.CreateChildStructure,
                x => contextMenuItemStructureCreateChild.IsEnabled = x));

            var contextMenuItemStructureClearChild= (MenuItem)menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemStructureClearChild, _controller.ClearStructureCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.ClearChildItems,
                x => contextMenuItemStructureClearChild.IsEnabled = x));

            var contextSubMenuItemStructureSort = (MenuItem)menu.AddSubMenu("");
            _subscriptions.Add(
                ActiveQueryBuilder.Core.Localization.MetadataEditor.Sort.Subscribe(x => contextSubMenuItemStructureSort.Header = x));

            var contextSubMenuItemStructureSortByName =
                (MenuItem)((ICustomSubMenu)contextSubMenuItemStructureSort).AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextSubMenuItemStructureSortByName,
                _controller.SortStructureByNameCommand, ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.SortByName,
                x => contextSubMenuItemStructureSortByName.Visibility = x ? Visibility.Visible : Visibility.Collapsed));

            var contextSubMenuItemStructureSortByTypeAndName =
                (MenuItem)((ICustomSubMenu)contextSubMenuItemStructureSort).AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextSubMenuItemStructureSortByTypeAndName,
                _controller.SortStructureByTypeCommand, ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.SortByType,
                x => contextSubMenuItemStructureSortByTypeAndName.Visibility =
                    x ? Visibility.Visible : Visibility.Collapsed));

            var contextMenuItemStructureMoveToTop = (MenuItem)menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemStructureMoveToTop, _controller.MoveUpStructureCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.MoveToTop,
                x => contextMenuItemStructureMoveToTop.Visibility = x ? Visibility.Visible : Visibility.Collapsed));

            var contextMenuItemStructureMoveToBottom = (MenuItem)menu.AddItem("", null, false, false, null, null);
            _subscriptions.Add(CommandBinder.Bind(contextMenuItemStructureMoveToBottom,
                _controller.MoveDownStructureCommand, ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.MoveToBottom,
                x => contextMenuItemStructureMoveToBottom.Visibility = x ? Visibility.Visible : Visibility.Collapsed));

            menu.Closing += Menu_Closing;
        }

        private void Container_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
                e.Handled = true;
        }

        private void ColumnStructureExpander_OnExpanded(object sender, RoutedEventArgs e)
        {
            var fe = (FrameworkElement) sender;
            if (!fe.IsLoaded) return;

            ColumnDefinitionStructure.MinWidth = _minWidthColumnStructure;
            ColumnDefinitionStructure.Width = _widthColumnStructure;
            ColumnDefinitionSchema.Width = _widthColumnSchema;
        }

        private void ColumnStructureExpander_OnCollapsed(object sender, RoutedEventArgs e)
        {
            _minWidthColumnStructure = ColumnDefinitionStructure.MinWidth;
            _widthColumnStructure = ColumnDefinitionStructure.Width;
            _widthColumnSchema = ColumnDefinitionSchema.Width;

            ColumnDefinitionSchema.Width =
                new GridLength(ColumnDefinitionSchema.Width.Value + ColumnDefinitionStructure.Width.Value,
                    GridUnitType.Star);
            ColumnDefinitionStructure.MinWidth = 0;
            ColumnDefinitionStructure.Width = new GridLength(0);
        }

        private void Separator_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_separatorContextMenu != null)
            {
                _separatorContextMenu.IsOpen = false;
                _separatorContextMenu = null;
            }

            var point = e.GetPosition(this);
            _separatorContextMenu = new ContextMenu()
            {
                Placement = PlacementMode.RelativePoint,
                PlacementTarget = this,
                HorizontalOffset = point.X,
                VerticalOffset = point.Y
            };
            var menuItem = new MenuItem()
            {
                Header = Helpers.Localizer.GetString("strMetadataStructureTreeVisibility",
                    Helpers.ConvertLanguageFromNative(Language),
                    LocalizableConstantsUI.strMetadataStructureTreeVisibility),
                IsChecked = StructureTreeVisible
            };

            menuItem.Click += MenuItem_Click;
            _separatorContextMenu.IsOpen = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            StructureTreeVisible = !StructureTreeVisible;
        }
    }
}
