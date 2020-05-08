//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2019 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Collections.Generic;
using System.Linq;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.Commands;
using ActiveQueryBuilder.Core.PropertiesEditors;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.DatabaseSchemaView;
using ActiveQueryBuilder.View.MetadataEditor;
using ActiveQueryBuilder.View.MetadataStructureView;
using ActiveQueryBuilder.View.PropertiesEditors;
using ActiveQueryBuilder.View.PropertiesEditors.ObjectsProperties;
using Helpers = ActiveQueryBuilder.View.Helpers;

namespace MetadataEditorDemo.Common
{
    public class MetadataEditorController : IDisposable
    {
        public bool IsChanged { get; set; }

        private readonly IMetadataEditorView _view;

        private SQLContext _sqlContext;
        private MetadataContainer _container;
        private MetadataStructure _containerStructure;
        private MetadataStructure _structure;

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();
        private readonly ContainerLoadFormStorage _containerLoadFormStorage = new ContainerLoadFormStorage();

        private readonly MetadataEditorPropertiesBarController _propertiesBarController;
        private readonly MetadataEditorLoader _metadataLoader;
        private readonly MetadataEditorItemsManager _itemsManager;
        private bool _turnOffAlwaysAutomaticLoadChild;
        private bool _turnOnAlwaysAutomaticLoadChild;

        #region Commands

        private Command[] _addNamespaceCommands;
        private Command[] _addObjectCommands;
        private Command[] _addFieldCommands;

        public Command AddStructureItemCommand { get; private set; }
        public Command DeleteStructureItemCommand { get; private set; }
        public Command LoadContainerCommand { get; private set; }
        public Command DeleteMetadataItemCommand { get; private set; }

        public Command AddLinkedServerCommand { get; private set; }
        public Command AddDatabaseCommand { get; private set; }
        public Command AddSchemaCommand { get; private set; }
        public Command AddPackageCommand { get; private set; }
        public Command AddTableCommand { get; private set; }
        public Command AddViewCommand { get; private set; }
        public Command AddProcedureCommand { get; private set; }
        public Command AddSynonymCommand { get; private set; }
        public Command AddFieldCommand { get; private set; }
        public Command AddForeignKeyCommand { get; private set; }

        public Command SortSchemaByNameCommand { get; private set; }
        public Command SortSchemaByTypeCommand { get; private set; }
        public Command MoveUpSchemaCommand { get; private set; }
        public Command MoveDownSchemaCommand { get; private set; }

        public Command SortStructureByNameCommand { get; private set; }
        public Command SortStructureByTypeCommand { get; private set; }
        public Command MoveUpStructureCommand { get; private set; }
        public Command MoveDownStructureCommand { get; private set; }

        public Command ClearSchemaCommand { get; private set; }
        public Command ClearStructureCommand { get; private set; }

        public Command LoadSchemaChildsCommand { get; private set; }
        public Command ConnectAndLoadCommand { get; private set; }
        public Command CreateStructureChildsCommand { get; private set; }

        private void InitializeCommands()
        {
            AddStructureItemCommand = new Command(AddStructureItemToFocused);
            DeleteStructureItemCommand = new Command(GetConfirmedDeletionAction(DeleteSelectedStructureItems));
            LoadContainerCommand = new Command(LoadContainer);
            DeleteMetadataItemCommand = new Command(GetConfirmedDeletionAction(DeleteSelectedContainerItems));
            
            AddLinkedServerCommand = new Command(GetCreateMetadataItemAction(MetadataType.Server));
            AddDatabaseCommand = new Command(GetCreateMetadataItemAction(MetadataType.Database));
            AddSchemaCommand = new Command(GetCreateMetadataItemAction(MetadataType.Schema));
            AddPackageCommand = new Command(GetCreateMetadataItemAction(MetadataType.Package));
            AddTableCommand = new Command(GetCreateMetadataItemAction(MetadataType.Table));
            AddViewCommand = new Command(GetCreateMetadataItemAction(MetadataType.View));
            AddProcedureCommand = new Command(GetCreateMetadataItemAction(MetadataType.Procedure));
            AddSynonymCommand = new Command(GetCreateMetadataItemAction(MetadataType.Synonym));
            AddFieldCommand = new Command(GetCreateMetadataItemAction(MetadataType.Field));
            AddForeignKeyCommand = new Command(GetCreateMetadataItemAction(MetadataType.ForeignKey));

            SortSchemaByNameCommand = new Command(GetSortAction(SortingType.Name, TargetView.Container));
            SortSchemaByTypeCommand = new Command(GetSortAction(SortingType.Type, TargetView.Container));
            SortStructureByNameCommand = new Command(GetSortAction(SortingType.Name, TargetView.Structure));
            SortStructureByTypeCommand = new Command(GetSortAction(SortingType.Type, TargetView.Structure));

            MoveUpSchemaCommand = new Command(GetMoveAction(MoveDirection.Up, TargetView.Container));
            MoveDownSchemaCommand = new Command(GetMoveAction(MoveDirection.Down, TargetView.Container));
            MoveUpStructureCommand = new Command(GetMoveAction(MoveDirection.Up, TargetView.Structure));
            MoveDownStructureCommand = new Command(GetMoveAction(MoveDirection.Down, TargetView.Structure));

            ClearSchemaCommand = new Command(ClearCurrentContainerItem);
            ClearStructureCommand = new Command(ClearCurrentStructureItem);
            LoadSchemaChildsCommand = new Command(LoadCurrentSchemaChildItems);
            ConnectAndLoadCommand = new Command(ShowContainerLoadForm);
            CreateStructureChildsCommand = new Command(LoadCurrentStructureChildItems);

            _addNamespaceCommands = new[] { AddDatabaseCommand, AddLinkedServerCommand, AddSchemaCommand };
            _addObjectCommands = new[] { AddTableCommand, AddViewCommand, AddProcedureCommand, AddSynonymCommand };
            _addFieldCommands = new[] { AddFieldCommand, AddForeignKeyCommand };
        }

        private Action GetConfirmedDeletionAction(Action action)
        {
            return delegate
            {
                if (_view.ConfirmItemDeletion())
                    action.Invoke();
            };
        }

        private Action GetCreateMetadataItemAction(MetadataType type)
        {
            return () => AddMetadataItemToFocused(type);
        }

        private enum TargetView
        {
            Container,
            Structure
        };

        private Action GetSortAction(SortingType sortingType, TargetView targetView)
        {
            if (targetView == TargetView.Structure)
                return () => SortCurrentStructureItems(sortingType);

            if (targetView == TargetView.Container)
                return () => SortCurrentContainerItems(sortingType);

            return null;
        }

        private Action GetMoveAction(MoveDirection direction, TargetView targetView)
        {
            if (targetView == TargetView.Structure)
                return () => MoveCurrentStructureItems(direction);

            if (targetView == TargetView.Container)
                return () => MoveCurrentContainerItems(direction);

            return null;
        }

        private void UpdateLoadContainerCommand()
        {
            LoadContainerCommand.CanExecute.Value = _sqlContext.MetadataProvider != null && _sqlContext.MetadataProvider.Connected;
        }

        public void UpdateContainerCommands()
        {
            var focusedItem = _view.ContainerView.FocusedItem;
            if (focusedItem == null)
            {
                DisableContainerCommands();
                return;
            }
            
            DeleteMetadataItemCommand.CanExecute.Value = focusedItem != _containerStructure && !IsGroupingFolder(focusedItem);

            if (IsGroupingFolder(focusedItem))
            {
                EnableCommandsForGroupingFolder(focusedItem);
                return;
            }

            if (focusedItem.MetadataItem == null)
            {
                DisableContainerCommands();
            }
            else
            {
                EnableCommandsForItemType(focusedItem.MetadataItem.Type);
            }
        }

        private void DisableContainerCommands()
        {
            DeleteMetadataItemCommand.CanExecute.Value = false;
            SetCommandGroupCanExecute(_addNamespaceCommands, false);
            SetCommandGroupCanExecute(_addObjectCommands, false);
            SetCommandGroupCanExecute(_addFieldCommands, false);
        }

        private bool IsItemFolderOf(MetadataStructureItem item, MetadataType type)
        {
            if (item == null || item.MetadataItem != null || item.MetadataFilter.Count == 0)
            {
                return false;
            }

            var filter = item.MetadataFilter[0];
            return filter.ObjectTypes.Contains(type);
        }

        private bool IsGroupingFolder(MetadataStructureItem item)
        {
            return item.MetadataItem == null && item.MetadataFilter.Count != 0;
        }

        private void EnableCommandsForGroupingFolder(MetadataStructureItem item)
        {
            SetCommandGroupCanExecute(_addNamespaceCommands, false);
            SetCommandGroupCanExecute(_addObjectCommands, false);
            SetCommandGroupCanExecute(_addFieldCommands, false);

            if (IsItemFolderOf(item, MetadataType.Field))
            {
                AddFieldCommand.CanExecute.Value = true;
            }
            else if (IsItemFolderOf(item, MetadataType.ForeignKey))
            {
                AddForeignKeyCommand.CanExecute.Value = true;
            }
        }

        private void EnableCommandsForItemType(MetadataType type)
        {
            LoadSchemaChildsCommand.CanExecute.Value = true;
            ConnectAndLoadCommand.CanExecute.Value = true;

            var syntax = _sqlContext.SyntaxProvider;
            var supportServers = syntax != null && syntax.IsSupportServers();
            var supportDatabases = syntax != null && syntax.IsSupportDatabases();
            var supportSchemas = syntax != null && syntax.IsSupportSchemas();
            var supportPackages = syntax != null && syntax.IsSupportPackages();

            AddLinkedServerCommand.CanExecute.Value = supportServers && type.IsA(MetadataType.Root);
            AddDatabaseCommand.CanExecute.Value = supportDatabases && type.IsA(MetadataType.Root) || type.IsA(MetadataType.Server);
            AddSchemaCommand.CanExecute.Value = supportSchemas && (type.IsA(MetadataType.Database) || type.IsA(MetadataType.Root));
            AddPackageCommand.CanExecute.Value = supportPackages && type.IsNamespace();

            if (supportDatabases || supportSchemas)
                SetCommandGroupCanExecute(_addObjectCommands, (supportDatabases && !supportSchemas && type.IsA(MetadataType.Database)) || type.IsA(MetadataType.Schema));
            else if (type == MetadataType.Root)
                SetCommandGroupCanExecute(_addObjectCommands, true);

            SetCommandGroupCanExecute(_addFieldCommands, type.IsObject());
        }

        private void SetCommandGroupCanExecute(Command[] group, bool canExecute)
        {
            Array.ForEach(group, x => x.CanExecute.Value = canExecute);
        }

        #endregion

        public event EventHandler LoadStart
        {
            add { _metadataLoader.LoadStart += value; }
            remove { _metadataLoader.LoadStart -= value; }
        }

        public event EventHandler LoadStep
        {
            add { _metadataLoader.LoadStep += value; }
            remove { _metadataLoader.LoadStep -= value; }
        }

        public event EventHandler LoadFinish
        {
            add { _metadataLoader.LoadFinish += value; }
            remove { _metadataLoader.LoadFinish -= value; }
        }

        public bool HideVirtualObjects
        {
            get { return _propertiesBarController.HideVirtualObjects;}
            set { _propertiesBarController.HideVirtualObjects = value; }
        }

        public MetadataEditorController(IMetadataEditorView view)
        {
            InitializeCommands();

            _view = view;

            _propertiesBarController = new MetadataEditorPropertiesBarController(_view);
            _metadataLoader = new MetadataEditorLoader();
            _itemsManager = new MetadataEditorItemsManager(_view);

            _view.StructureView.FocusedItemChanged += StructureItemChanged;
            _subscriptions.Add(Disposable.Create(() => _view.StructureView.FocusedItemChanged -= StructureItemChanged));

            _view.ContainerView.FocusedItemChanged += SchemaItemChanged;
            _subscriptions.Add(Disposable.Create(() => _view.StructureView.FocusedItemChanged -= SchemaItemChanged));
        }

        private void StructureViewMOnItemAdded(MetadataStructureItem item)
        {
            if (item == null || item.MetadataItem != null || item.Parent == null) return;

            if (!item.Parent.AllowChildAutoItems) return;

            if (_turnOffAlwaysAutomaticLoadChild && !_turnOnAlwaysAutomaticLoadChild)
            {
                item.Parent.AllowChildAutoItems = false;
                return;
            }

            if (_turnOnAlwaysAutomaticLoadChild) return;

            var window = _view.ControlFactory.ConfirmTurnOffAutoLoadChildWindow();

            window.Message = Helpers.Localizer.GetString("strWouldTurnOffAutoLoadChildItems",
                _view.Language,
                LocalizableConstantsUI.strWouldTurnOffAutoLoadChildItems);

            window.CheckBoxString = Helpers.Localizer.GetString("strDoNotAskAlwaysTurnOff",
                _view.Language,
                LocalizableConstantsUI.strDoNotAskAlwaysTurnOff);

            var result = window.Show(_view);
            if (!result)
            {
                _turnOnAlwaysAutomaticLoadChild = window.IsAlwaysTurn;
                return;
            }

            item.Parent.AllowChildAutoItems = false;

            _turnOffAlwaysAutomaticLoadChild = window.IsAlwaysTurn;
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }

        public void SetCurrentError(PropertyErrorDescription error)
        {
            _propertiesBarController.SetCurrentError(error);
        }

        public void Init(SQLContext sqlContext)
        {
            _propertiesBarController.SqlContext = sqlContext;

            _sqlContext = sqlContext;
            _container = _sqlContext.MetadataContainer;

            _structure = _sqlContext.MetadataStructure;
            _structure.LoadDynamicNodes = false;

            _metadataLoader.MetadataContainer = _container;
            _metadataLoader.MetadataStructure = _structure;

            _containerStructure = new MetadataStructure { MetadataItem = _container};
            _subscriptions.Add(_containerStructure);
            InitContainerStructure(_containerStructure);

            InitSchemaTree(_view.ContainerView);
            InitStructureTree(_view.StructureView);

            SubscribeToUpdated(_structure);
            SubscribeToUpdated(_containerStructure);

            UpdateLoadContainerCommand();
        }

        public void OnFixIssue(PropertyErrorDescription errorDescription)
        {
            if (errorDescription.Properties != null)
            {
                var structureItemProps = errorDescription.Properties as MetadataStructureItemProperties;
                if (structureItemProps != null)
                {
                    _view.StructureView.FocusedItem = structureItemProps.MetadataStructureItem;
                }

                var metadataItemProps = errorDescription.Properties as MetadataItemProperties;
                if (metadataItemProps != null)
                {
                    var structureItem = FindStructureItem(_containerStructure, metadataItemProps.MetadataItem);
                    if (structureItem != null)
                    {
                        _view.ContainerView.FocusedItem = structureItem;
                    }
                }
            }

            _view.PropertiesBar.ThrowError(errorDescription);
        }

        public MetadataStructureItem FindStructureItem(MetadataStructureItem structureItem, MetadataItem metadataItem)
        {
            MetadataStructureItem found = null;

            foreach (var item in structureItem.Items)
            {
                if (item.MetadataItem == metadataItem)
                {
                    return item;
                }

                found = FindStructureItem(item, metadataItem);
                if (found != null)
                {
                    break;
                }
            }

            return found;
        }

        public void AddMetadataItemToFocused(MetadataType type)
        {
            AddMetadataItem(_view.ContainerView.FocusedItem, type);
        }

        public void AddMetadataItem(MetadataStructureItem parent, MetadataType type)
        {
            if (type.IsNamespace())
                _itemsManager.AddNamespace(parent, type);
            else if (type.IsObject())
                _itemsManager.AddObject(parent, type);
            else if (type.IsA(MetadataType.Field))
                _itemsManager.AddField(parent);
            else if (type.IsA(MetadataType.ForeignKey))
                _itemsManager.AddForeignKey(parent);
        }

        public void AddStructureItemToFocused()
        {
            AddStructureItem(_view.StructureView.FocusedItem);
        }

        public void AddStructureItem(MetadataStructureItem parent)
        {
            _itemsManager.AddStructureItem(parent);
        }

        public void DeleteSelectedStructureItems()
        {
            _itemsManager.DeleteStructureItems(_view.StructureView.SelectedItems.ToList());
        }

        public void DeleteSelectedContainerItems()
        {
            _itemsManager.DeleteContainerItems(_view.ContainerView.SelectedItems.ToList());
        }

        public void SortCurrentStructureItems(SortingType sortingType)
        {
            var item = _view.StructureView.FocusedItem.Parent;
            if (item != null)
            {
                _itemsManager.SortStructureItems(item, sortingType);
            }
        }

        public void SortCurrentContainerItems(SortingType sortingType)
        {
            var item = _view.ContainerView.FocusedItem.Parent;
            if (item != null)
            {
                _itemsManager.SortContainerItems(item, sortingType);
            }
        }

        public void MoveCurrentStructureItems(MoveDirection direction)
        {
            var selectedItems = _view.StructureView.SelectedItems.ToList();
            _itemsManager.MoveStructureItems(selectedItems, direction);
        }

        public void MoveCurrentContainerItems(MoveDirection direction)
        {
            var selectedItems = _view.ContainerView.SelectedItems.ToList();
            _itemsManager.MoveContainerItems(selectedItems, direction);
        }

        public void ClearCurrentContainerItem()
        {
            var item = _view.ContainerView.FocusedItem;
            if (item != null)
              _itemsManager.ClearContainerItem(item.MetadataItem);
        }

        public void ClearCurrentStructureItem()
        {
            var item = _view.StructureView.FocusedItem;
            if (item != null)
                _itemsManager.ClearStructureItem(item);
        }

        public void GenerateStructure(bool groupByServers, bool groupByDatabases, bool groupBySchemas,
            bool groupByTypes, bool populateObjects)
        {
            _itemsManager.GenerateStructure(groupByServers, groupByDatabases, groupBySchemas, groupByTypes, populateObjects);
        }

        public void LoadCurrentStructureChildItems()
        {
            _itemsManager.CreateChildStructureItems(_view.StructureView.FocusedItem);
        }

        public void LoadContainer()
        {
            _metadataLoader.LoadEntireContainer();
            IsChanged = false;
        }

        public void LoadCurrentSchemaChildItems()
        { 
            _metadataLoader.LoadChilds(_view.ContainerView.FocusedItem);
        }

        public void ShowContainerLoadForm()
        {
            using (var f = new MetadataContainerLoadForm(_view, _sqlContext.MetadataContainer, _containerLoadFormStorage.Connection))
            {
                f.LoadFields = _containerLoadFormStorage.LoadFields;
                if (f.ShowDialog() != true)
                    return;

                var connection = f.Connection;

                _containerLoadFormStorage.Connection = connection;
                _containerLoadFormStorage.LoadFields = f.LoadFields;

                if (connection != null)
                {
                    _sqlContext.MetadataProvider = connection.MetadataProvider;
                    _sqlContext.SyntaxProvider = connection.SyntaxProvider;
                }

                UpdateLoadContainerCommand();
            }

            _view.ContainerView.InitializeDatabaseSchemaTree();

            IsChanged = false;

            _view.ContainerView.ExpandTypes(MetadataType.Namespaces | MetadataType.Objects | MetadataType.Aggregate);
        }

        public void StopLoading()
        {
            _metadataLoader.StopLoading = true;
        }

        public void InsertDefaultContainerItem()
        {
            var focusedItem = _view.ContainerView.FocusedItem;
            if (focusedItem == null)
                return;

            BaseSyntaxProvider syntax = _container.SQLContext.SyntaxProvider;
            bool supportDatabases = syntax != null && syntax.IsSupportDatabases();
            bool supportSchemas = syntax != null && syntax.IsSupportSchemas();

            if (syntax == null)
                return;

            MetadataItem metadataItem = null;
            if (focusedItem.Parent != null)
            {
                if (focusedItem.Parent.MetadataItem != null)
                    metadataItem = focusedItem.Parent.MetadataItem;
                else if (focusedItem.Parent.Parent != null)
                    metadataItem = focusedItem.Parent.Parent.MetadataItem;
            }
            else
                metadataItem = focusedItem.MetadataItem;

            if (metadataItem == null)
                return;

            switch (metadataItem.Type)
            {
                case MetadataType.Root:
                case MetadataType.Server:
                    if (supportDatabases)
                        AddMetadataItemToFocused(MetadataType.Database);
                    else if (supportSchemas)
                        AddMetadataItemToFocused(MetadataType.Schema);
                    else
                        AddMetadataItemToFocused(MetadataType.Table);
                    break;

                case MetadataType.Database:
                    if (supportSchemas)
                        AddMetadataItemToFocused(MetadataType.Schema);
                    else
                        AddMetadataItemToFocused(MetadataType.Table);
                    break;

                case MetadataType.Schema:
                    AddMetadataItemToFocused(MetadataType.Table);
                    break;

                case MetadataType.Table:
                case MetadataType.View:
                case MetadataType.Procedure:
                case MetadataType.Synonym:
                    AddMetadataItemToFocused(MetadataType.Field);
                    break;

                case MetadataType.Field:
                    AddMetadataItemToFocused(MetadataType.Field);
                    break;

                case MetadataType.ForeignKey:
                    AddMetadataItemToFocused(MetadataType.ForeignKey);
                    break;
            }
        }

        public List<string> ValidateBeforeApply()
        {
            var errorList = new List<string>();
            CollectContainerErrors(_container, errorList);
            CollectStructureErrors(_structure, errorList);
            return errorList;
        }

        public ICustomContextMenu GetStructureContainerContextMenu()
        {
            var menu = _view.ControlFactory.GetCustomContextMenu();
            return menu;
        }

        public ICustomContextMenu GetSchemaContainerContextMenu()
        {
            var menu = _view.ControlFactory.GetCustomContextMenu();

            return menu;
        }

        public bool IsStructureEmpty(MetadataStructure structure)
        {
            if (structure.Items.Count == 0)
                return true;
            return structure.Items.Count == 1 && structure.Items[0].IsFavoritesFolder();
        }

        public bool IsAllChildSameType(List<MetadataItem> items)
        {
            if (items == null || items.Count == 0)
            {
                return true;
            }

            var type = items[0].Type;
            foreach (var child in items)
            {
                if (child == null)
                {
                    continue;
                }

                if (child.Type != type)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsItemsHaveSameParent(List<MetadataStructureItem> items)
        {
            if (items == null || items.Count == 0)
            {
                return false;
            }

            var parent = items[0].Parent;
            foreach (var item in items)
            {
                if (item.Parent != parent)
                {
                    return false;
                }
            }

            return true;
        }

        public void UpdateStructureCommands()
        {
            var focusedItem = _view.StructureView.FocusedItem;
            var structure = _view.StructureView.MetadataStructure;
            var itemsEnabled = focusedItem != null;

            CreateStructureChildsCommand.CanExecute.Value = itemsEnabled;
            ClearStructureCommand.CanExecute.Value = itemsEnabled;
            SortStructureByTypeCommand.CanExecute.Value = itemsEnabled;
            SortStructureByNameCommand.CanExecute.Value = itemsEnabled;
            DeleteStructureItemCommand.CanExecute.Value = itemsEnabled;
            AddStructureItemCommand.CanExecute.Value = itemsEnabled;

            if (focusedItem == null)
            {
                return;
            }

            var moveActionsEnabled = IsItemsHaveSameParent(_view.StructureView.SelectedItems.ToList());
            MoveDownStructureCommand.CanExecute.Value = moveActionsEnabled;
            MoveUpStructureCommand.CanExecute.Value = moveActionsEnabled;

            if (focusedItem.Parent != null)
            {
                var metadataItems = focusedItem.Parent.Items.Select(x => x.MetadataItem).ToList();
                if (!metadataItems.Contains(null))
                {
                    SortStructureByTypeCommand.CanExecute.Value = IsAllChildSameType(metadataItems);
                }
            }

            
            DeleteStructureItemCommand.CanExecute.Value = focusedItem != structure
                && !focusedItem.IsFavoritesFolder()
                ;
        }

        private static void CollectContainerErrors(MetadataItem item, List<string> errorList)
        {
            var metadataObject = item as MetadataObject;
            if (metadataObject != null)
            {
                if (!metadataObject.IsExpressionReferencedObjectsCorrect())
                {
                    errorList.Add("Invalid expression reference on " + metadataObject.NameFull);
                }
            }

            var metadataField = item as MetadataField;
            if (metadataField != null)
            {
                if (!metadataField.IsExpressionReferencedObjectsCorrect())
                {
                    errorList.Add("Invalid expression reference on " + metadataField.NameFull);
                }
            }

            foreach (var child in item.Items)
            {
                CollectContainerErrors(child, errorList);
            }
        }

        private static void CollectStructureErrors(MetadataStructureItem item, ICollection<string> errorList)
        {
            if (item.IsMetadataNameIncorrect())
            {
                errorList.Add("Incorrect metadata name on " + item.Caption);
            }

            foreach (var child in item.Items)
            {
                CollectStructureErrors(child, errorList);
            }
        }

        private static void InitContainerStructure(MetadataStructure structure)
        {
            structure.Options.ShowForeignKeys = true;
            structure.Options.ShowParameters = true;
            structure.Options.GroupFieldsByTypes = true;
        }

        private void InitSchemaTree(IDatabaseSchemaView tree)
        {
            var treeView = tree.DatabaseStructureView as IMetadataStructureTree;
            if (treeView != null)
                EnableTreeRoot(treeView);

            tree.SQLContext = _sqlContext;
            tree.MetadataStructure = _containerStructure;
            tree.InitializeDatabaseSchemaTree();
        }

        private void EnableTreeRoot(IMetadataStructureTree tree)
        {
            tree.Controller.ShowMetadataStructureItem = true;
            tree.Controller.MetadataStructureItemCaption = "Root";
        }

        private void InitStructureTree(IDatabaseSchemaView tree)
        {
            var treeView = tree.DatabaseStructureView as IMetadataStructureTree;
            if (treeView != null)
                EnableTreeRoot(treeView);

            tree.MetadataStructure = _structure;
            tree.SQLContext = _sqlContext;
            tree.Options.AllowDrop = true;

            if (treeView != null)
                treeView.Controller.DragController = new MetadataStructureTreeDragController();

            tree.InitializeDatabaseSchemaTree();

            tree.MetadataStructure.ItemAdded -= StructureViewMOnItemAdded;

            tree.MetadataStructure.ItemAdded += StructureViewMOnItemAdded;
            _subscriptions.Add(Disposable.Create(() => tree.MetadataStructure.ItemAdded -= StructureViewMOnItemAdded));
        }

        private void SubscribeToUpdated(IUpdatable updatable)
        {
            updatable.Updated += OnUpdated;
            _subscriptions.Add(Disposable.Create(() => updatable.Updated -= OnUpdated));
        }

        private void OnUpdated(object sender, EventArgs eventArgs)
        {
            IsChanged = true;
        }

        public void OnContainerItemChanged()
        {
            UpdateContainerProps(_view.ContainerView.FocusedItem);
        }

        public void OnStructureItemChanged()
        {
            UpdateStructureProps(_view.StructureView.FocusedItem);
        }

        private void SchemaItemChanged(object sender, MetadataStructureItem item)
        {
            UpdateContainerProps(item);
        }

        private void StructureItemChanged(object sender, MetadataStructureItem item)
        {
            UpdateStructureProps(item);
        }

        private void UpdateContainerProps(MetadataStructureItem item)
        {
            _propertiesBarController.MetadataItemChanged(item);
            UpdateContainerCommands();
        }

        private void UpdateStructureProps(MetadataStructureItem item)
        {
            _propertiesBarController.StructureItemChanged(item);
            UpdateStructureCommands();
        }

        public enum SortingType
        {
            Name,
            Type
        }

        public enum MoveDirection
        {
            Up,
            Down
        }

        private class MetadataEditorPropertiesBarController
        {
            private readonly IMetadataEditorView _view;

            private MetadataItem _metadataItem;
            private MetadataStructureItem _metadataStructureItem;

            private IPropertiesContainer _currentContainer;
            private PropertyErrorDescription _currentError;

            private MetadataStructureItem _subscribedStructureItem;
            private MetadataItem _subscribedMetadataItem;
            private object _issueObject;

            public bool HideVirtualObjects { get; set; }

            public SQLContext SqlContext { get; set; }

            public void SetCurrentError(PropertyErrorDescription error)
            {
                if (_currentError == error)
                    return;

                if (error != null && !error.IsError)
                {
                    if (error.Properties != null)
                        UnsubscribeItemDispose(error.Properties);

                    _currentError = null;
                    return;
                }

                if (_currentError?.Properties != null)
                    UnsubscribeItemDispose(_currentError.Properties);

                _currentError = error;

                if (_currentError?.Properties != null)
                    SubscribeItemDispose(_currentError.Properties);
            }

            //private ObjectProperties ErrorProperties
            //{
            //    get
            //    {
            //        if (_currentError == null)
            //            return null;

            //        return _currentError.Properties;
            //    }
            //}

            public MetadataEditorPropertiesBarController(IMetadataEditorView view)
            {
                _view = view;
            }

            public void MetadataItemChanged(MetadataStructureItem structureItem)
            {
                if (structureItem == null)
                {
                    ClearPropertiesBar();
                    return;
                }

                if (structureItem == _metadataStructureItem)
                    return;

                _metadataItem = structureItem.MetadataItem;
                _metadataStructureItem = structureItem;

                UpdateSchemaTree(_metadataItem, _metadataStructureItem);
            }

            public void StructureItemChanged(MetadataStructureItem structureItem)
            {
                if (structureItem == null)
                {
                    ClearPropertiesBar();
                    return;
                }

                if (structureItem.IsFavoritesFolder())
                {
                    ClearPropertiesBar();
                    return;
                }

                var props = new MetadataStructureItemProperties(structureItem, SqlContext);
                PutPropertiesOnBar(props);

                var filterControl =
                    _view.ControlFactory.GetMetadataFilterPropertiesFrame(structureItem.MetadataFilter,
                        structureItem.Structure.MetadataItem as MetadataContainer);

                _view.AppendStructureFilterControl(filterControl, structureItem.MetadataFilter.Count > 0);

                if (structureItem == _issueObject)
                {
                    _view.PropertiesBar.ThrowError(_currentError);
                }
            }

            private void SubscribeItemDispose(ObjectProperties properties)
            {
                var structureItemProps = properties as MetadataStructureItemProperties;
                if (structureItemProps != null)
                {
                    if (_subscribedStructureItem != null)
                    {
                        _subscribedStructureItem.Disposing -= OnIssueItemDispose;
                    }

                    _subscribedStructureItem = structureItemProps.MetadataStructureItem;
                    _issueObject = _subscribedStructureItem;
                    structureItemProps.MetadataStructureItem.Disposing += OnIssueItemDispose;
                }

                var metadataItemProps = properties as MetadataItemProperties;
                if (metadataItemProps != null)
                {
                    if (_subscribedMetadataItem != null)
                    {
                        _subscribedMetadataItem.Disposing -= OnIssueItemDispose;
                    }

                    _subscribedMetadataItem = metadataItemProps.MetadataItem;
                    _issueObject = _subscribedMetadataItem;
                    metadataItemProps.MetadataItem.Disposing += OnIssueItemDispose;
                }
            }

            private void UnsubscribeItemDispose(ObjectProperties properties)
            {
                var structureItemProps = properties as MetadataStructureItemProperties;
                if (structureItemProps != null)
                {
                    if (_subscribedStructureItem != null)
                    {
                        _subscribedStructureItem.Disposing -= OnIssueItemDispose;
                        _subscribedStructureItem = null;
                    }
                }

                var metadataItemProps = properties as MetadataItemProperties;
                if (metadataItemProps != null)
                {
                    if (_subscribedMetadataItem != null)
                    {
                        _subscribedMetadataItem.Disposing -= OnIssueItemDispose;
                        _subscribedMetadataItem = null;
                    }
                }

                _issueObject = null;
            }

            private void OnIssueItemDispose(object sender, EventArgs e)
            {
                _view.ShowInformationMessage(this, null);
            }

            private void UpdateSchemaTree(MetadataItem metadataItem, MetadataStructureItem structureItem)
            {
                if (metadataItem is MetadataForeignKey)
                {
                    SetupForeignKeyEditor(metadataItem as MetadataForeignKey);
                    return;
                }

                var properties = GetSchemaObjectProperties(metadataItem, structureItem);
                if (properties == null)
                {
                    ClearPropertiesBar();
                    return;
                }

                var containerRecreated = PutPropertiesOnBar(properties);
                if (_currentContainer != null)
                {
                    CustomizeSchemaPropertiesContainer(_currentContainer, metadataItem, containerRecreated);
                }

                if (metadataItem == _issueObject)
                {
                    _view.PropertiesBar.ThrowError(_currentError);
                    SetVirtualGroupExpanded(_currentContainer, true);
                }
            }

            private void CustomizeSchemaPropertiesContainer(IPropertiesContainer container, MetadataItem metadataItem,
                bool appendVirtualInfo)
            {
                if (IsCanBeVirtual(metadataItem))
                {
                    SetVirtualGroupExpanded(container, IsVirtualObject(metadataItem));
                }

                if (appendVirtualInfo)
                {
                    AppendVirtualInfo(container, metadataItem);
                }
            }

            private void SetVirtualGroupExpanded(IPropertiesContainer container, bool expanded)
            {
                var group = GetVirtualGroupContainer(container);
                if (group == null)
                    return;

                group.IsExpanded = expanded;
            }

            private void AppendVirtualInfo(IPropertiesContainer container, MetadataItem metadataItem)
            {
                var group = GetVirtualGroupContainer(container);
                if (group == null)
                    return;

                var infoPanel = _view.ControlFactory.GetInformationPanel();
                infoPanel.IconLocation = InfoIconLocation.Left;

                if (metadataItem.Type == MetadataType.Field)
                    infoPanel.InfoText = Helpers.Localizer.GetString("strVirtualFieldDescription",
                        LocalizableConstantsUI.strVirtualFieldDescription);
                else if (metadataItem.Type.IsA(MetadataType.Objects))
                    infoPanel.InfoText = Helpers.Localizer.GetString("strVirtualObjectDescription",
                        LocalizableConstantsUI.strVirtualObjectDescription);

                group.InsertHeader(infoPanel);
            }

            private IGroupContainer GetVirtualGroupContainer(IPropertiesContainer container)
            {
                var captionObj = Helpers.Localizer.GetString("strMetadataVirtualObjectProperties",
                    LocalizableConstantsInternal.strMetadataVirtualObjectProperties);
                var captionFields = Helpers.Localizer.GetString("strMetadataVirtualFieldProperties",
                    LocalizableConstantsInternal.strMetadataVirtualFieldProperties);

                return container.GroupContainers.FirstOrDefault(x => x.Text == captionObj || x.Text == captionFields);
            }

            private bool IsCanBeVirtual(MetadataItem item)
            {
                return item is MetadataObject || item is MetadataField;
            }

            private bool IsVirtualObject(MetadataItem item)
            {
                var obj = item as MetadataObject;
                if (obj != null)
                    return obj.IsVirtual();

                var field = item as MetadataField;
                if (field != null)
                    return field.IsVirtual();

                return false;
            }

            private void SetupForeignKeyEditor(MetadataForeignKey foreignKey)
            {
                _currentContainer = null;
                ClearPropertiesBar();

                _view.SetCustomControlToPropertiesBar(
                    _view.ControlFactory.GetMetadataForeignKeyPropertiesFrame(foreignKey));
            }

            private bool PutPropertiesOnBar(ObjectProperties properties)
            {
                if (!NeedToRecreateProperties(properties))
                {
                    _currentContainer.ObjectProperties = properties;
                    return false;
                }

                _view.PropertiesBar.ClearProperties();

                _currentContainer = PropertiesFactory.GetPropertiesContainer(properties);
                _view.PropertiesBar.SetProperties(_currentContainer);

                return true;
            }

            private bool NeedToRecreateProperties(ObjectProperties properties)
            {
                return _currentContainer == null || (properties is MetadataStructureItemProperties) ||
                       _currentContainer.ObjectProperties.GetType() != properties.GetType();

            }

            private void ClearPropertiesBar()
            {
                _view.PropertiesBar.ClearProperties();
                _currentContainer = null;
            }

            private ObjectProperties GetSchemaObjectProperties(MetadataItem metadataItem,
                MetadataStructureItem structureItem)
            {
                var metadataNamespace = metadataItem as MetadataNamespace;
                if (metadataNamespace != null)
                {
                    if (metadataNamespace.Type == MetadataType.Database)
                    {
                        return new MetadataDatabaseProperties(metadataNamespace);
                    }

                    if (metadataNamespace.Type == MetadataType.Schema)
                    {
                        return new MetadataSchemaProperties(metadataNamespace);
                    }

                    return new MetadataNamespaceProperties(metadataNamespace);

                }

                var metadataObject = metadataItem as MetadataObject;
                if (metadataObject != null)
                {
                    return new MetadataObjectProperties(metadataObject, structureItem, HideVirtualObjects);
                }

                var metadataField = metadataItem as MetadataField;
                if (metadataField != null)
                {
                    return new MetadataFieldProperties(metadataField, HideVirtualObjects);
                }

                return null;
            }
        }

        private class MetadataEditorLoader
        {
            public event EventHandler LoadStart;
            public event EventHandler LoadStep;
            public event EventHandler LoadFinish;

            public MetadataContainer MetadataContainer { get; set; }
            public MetadataStructure MetadataStructure { get; set; }

            public bool StopLoading { get; set; }

            public void LoadEntireContainer()
            {
                LoadMetadataItemsInternal(MetadataContainer, true, true);

                MetadataStructure.Refresh();
            }

            public void LoadChilds(MetadataStructureItem item)
            {
                if (item.MetadataItem == null)
                {
                    return;
                }

                LoadMetadataItemsInternal(item.MetadataItem, true, false);
                item.Refresh();

                if (MetadataStructure.Options.GroupFieldsByTypes)
                {
                    foreach (var child in item.Items)
                    {
                        child.Refresh();
                    }
                }
            }

            private void LoadMetadataItemsInternal(MetadataItem item, bool withFields, bool recursive)
            {
                StopLoading = false;

                DoLoadStart();

                item.Items.Clear();
                LoadItems(item, withFields, recursive);

                DoLoadFinish();
            }

            private void LoadItems(MetadataItem item, bool withFields, bool recursive)
            {
                DoLoadStep();
                
                if (StopLoading)
                {
                    return;
                }

                item.Items.Load(withFields ? MetadataType.All : MetadataType.Namespaces | MetadataType.Objects, false);

                if (!recursive) return;

                foreach (var it in item.Items)
                {
                    LoadItems(it, withFields, true);
                }
            }

            private void DoLoadStart()
            {
                LoadStart?.Invoke(this, EventArgs.Empty);
            }

            private void DoLoadStep()
            {
                LoadStep?.Invoke(this, EventArgs.Empty);
            }

            private void DoLoadFinish()
            {
                LoadFinish?.Invoke(this, EventArgs.Empty);
            }
        }

        private class MetadataEditorItemsManager
        {
            private readonly IMetadataEditorView _view;

            public MetadataContainer Container => _view.ContainerView.MetadataStructure.MetadataItem as MetadataContainer;

            public MetadataStructure ContainerStructure => _view.ContainerView.MetadataStructure;

            public MetadataStructure Structure => _view.StructureView.MetadataStructure;

            public MetadataEditorItemsManager(IMetadataEditorView view)
            {
                _view = view;
            }

            public void AddNamespace(MetadataStructureItem parent, MetadataType type)
            {
                if (parent == null)
                    return;

                var syntax = Container.SQLContext.SyntaxProvider;
                if (syntax == null)
                {
                    return;
                }

                MetadataStructureItem parentItem = null;
                MetadataItem parentMetadataItem, createdItem = null;

                if (type == MetadataType.Database && syntax.IsSupportDatabases())
                {
                    parentItem = FindMetadataItemOfType(parent, MetadataType.Server) ?? ContainerStructure;
                    parentMetadataItem = parentItem.MetadataItem;
                    createdItem = CreateNamespaceItem(parentMetadataItem, type);
                }
                else if (type == MetadataType.Schema && syntax.IsSupportSchemas())
                {
                    parentItem = FindMetadataItemOfType(parent, MetadataType.Database) ?? ContainerStructure;
                    parentMetadataItem = parentItem.MetadataItem;
                    createdItem = CreateNamespaceItem(parentMetadataItem, type);
                }
                else if (type == MetadataType.Package && syntax.IsSupportPackages())
                {
                    parentItem = FindMetadataItemOfType(parent, MetadataType.Namespaces) ?? ContainerStructure;
                    parentMetadataItem = parentItem.MetadataItem;
                    createdItem = CreateNamespaceItem(parentMetadataItem, type);
                }
                else if (type == MetadataType.Server && syntax.IsSupportServers())
                {
                    parentItem = ContainerStructure;
                    parentMetadataItem = parentItem.MetadataItem;
                    createdItem = CreateNamespaceItem(parentMetadataItem, type);
                }

                AddToDatabaseTreeSchema(createdItem, parentItem);
            }

            public void AddObject(MetadataStructureItem parent, MetadataType type)
            {
                if (parent == null)
                    return;

                var syntax = Container.SQLContext.SyntaxProvider;
                bool supportDatabases = syntax != null && syntax.IsSupportDatabases();
                bool supportSchemas = syntax != null && syntax.IsSupportSchemas();

                var databaseItem = FindMetadataItemOfType(parent, MetadataType.Database);
                var schemaItem = FindMetadataItemOfType(parent, MetadataType.Schema);
                MetadataItem parentMetadataItem;
                MetadataStructureItem parentItem;

                if (supportDatabases && supportSchemas)
                {
                    parentMetadataItem = schemaItem.MetadataItem;
                    parentItem = schemaItem;
                }
                else if (!supportDatabases && supportSchemas)
                {
                    parentMetadataItem = schemaItem.MetadataItem;
                    parentItem = schemaItem;
                }
                else if (supportDatabases && !supportSchemas)
                {
                    parentMetadataItem = databaseItem.MetadataItem;
                    parentItem = databaseItem;
                }
                else
                {
                    parentMetadataItem = Container;
                    parentItem = ContainerStructure;
                }

                var metadataItem = CreateObjectItem(parentMetadataItem, type);
                AddToDatabaseTreeSchema(metadataItem, parentItem);
            }

            public void AddField(MetadataStructureItem parent)
            {
                if (parent == null)
                    return;

                var parentItem = FindMetadataItemOfType(parent, MetadataType.Objects, false);
                if (parentItem == null)
                    return;

                var parentMetadataItem = parentItem.MetadataItem;
                MetadataItem field = new MetadataField(parentMetadataItem.Items)
                {
                    Name = GetObjectName(MetadataType.Field, parentMetadataItem)
                };

                parentMetadataItem.Items.Add(field);
                ExpandTreeNode(_view.ContainerView, parentItem);

                var folder = GetFieldsFolder(parentItem);
                if (folder == null)
                    folder = CreateFieldsFolder(parentItem);

                AddToDatabaseTreeSchema(field, folder);
            }

            public void AddForeignKey(MetadataStructureItem parent)
            {
                if (parent == null)
                    return;

                var parentItem = FindMetadataItemOfType(parent, MetadataType.Objects);
                if (parentItem == null)
                    return;

                var parentMetadataItem = parentItem.MetadataItem;

                var foreignKey = new MetadataForeignKey(parentMetadataItem.Items);
                parentMetadataItem.Items.Add(foreignKey);
                ExpandTreeNode(_view.ContainerView, parentItem);

                var folder = GetForeignKeyFolder(parentItem);
                if (folder == null)
                    folder = CreateForeignKeyFolder(parentItem);

                AddToDatabaseTreeSchema(foreignKey, folder);
            }

            public void DeleteContainerItems(List<MetadataStructureItem> items)
            {
                if (items == null || items.Count == 0)
                    return;

                foreach (var item in items)
                {
                    if (item != null && item.Parent != null)
                    {
                        var metadataItem = item.MetadataItem;
                        if (metadataItem == null)
                            continue;
                        var metadataParent = metadataItem.Parent;
                        if (metadataParent == null)
                            continue;
                        metadataParent.Items.Remove(metadataItem);
                    }
                }

                // todo: Uncommenting this line raise null ref exception in tree, need to fix it
                //_view.ContainerView.SelectedItems = null;
            }

            public void AddStructureItem(MetadataStructureItem parent)
            {
                var structureItem = parent;
                if (structureItem == null)
                {
                    structureItem = Structure;
                }

                structureItem.LoadChildItems();

                MetadataStructureItem newStructureItem = new MetadataStructureItem();
                var caption = Helpers.Localizer.GetString("strNewItem", LocalizableConstantsInternal.strNewItem);
                caption = structureItem.GetUniqueFolderName(caption);

                newStructureItem.Caption = caption;
                structureItem.Items.Add(newStructureItem);
                _view.StructureView.FocusedItem = newStructureItem;
            }

            public void DeleteStructureItems(List<MetadataStructureItem> items)
            {
                foreach (var item in items)
                {
                    var parentItem = item.Parent;
                    if (parentItem == null)
                    {
                        return;
                    }
                    parentItem.Items.Remove(item);
                }
            }

            public void SortContainerItems(MetadataStructureItem item, SortingType sortingType)
            {
                if (item == null || item.MetadataItem == null)
                {
                    return;
                }

                item.MetadataItem.Items.Sort(new MetadataItemComparer(sortingType));
                item.Refresh();
                ExpandTreeNode(_view.ContainerView, item);
            }

            public void SortStructureItems(MetadataStructureItem item, SortingType sortingType)
            {
                if (item == null)
                {
                    return;
                }

                item.Items.Sort(new MetadataStructureComparer(sortingType, _view.StructureView.SQLGenerationOptions));
                item.Refresh();
                ExpandTreeNode(_view.StructureView, item);
            }

            public void MoveContainerItems(List<MetadataStructureItem> items, MoveDirection direction)
            {
                if (items == null || items.Count == 0)
                {
                    return;
                }

                var metadataItems = items.Select(x => x.MetadataItem).ToList();
                if (metadataItems.Count == 0)
                {
                    return;
                }

                var structureParent = items[0].Parent;
                var parent = metadataItems[0].Parent;
                var sortedItems = metadataItems.OrderBy(x => parent.Items.IndexOf(x)).ToList();
                if (direction == MoveDirection.Up)
                {
                    for (int i = 0; i < sortedItems.Count; i++)
                    {
                        parent.Items.Move(parent.Items.IndexOf(sortedItems[i]), i);
                    }
                }
                else if (direction == MoveDirection.Down)
                {
                    int k = parent.Items.Count - 1;
                    for (int i = sortedItems.Count - 1; i >= 0; i--)
                    {
                        parent.Items.Move(parent.Items.IndexOf(sortedItems[i]), k);
                        k--;
                    }
                }

                structureParent.Refresh();
                ExpandTreeNode(_view.ContainerView, structureParent);
                var newStructureItems = structureParent.Items.Where(x => metadataItems.Contains(x.MetadataItem));

                _view.ContainerView.SelectedItems = null;
                _view.ContainerView.SelectedItems = newStructureItems.ToArray();
            }

            public void MoveStructureItems(List<MetadataStructureItem> items, MoveDirection direction)
            {
                if (items == null || items.Count == 0)
                {
                    return;
                }

                var parent = items[0].Parent;
                var sortedItems = items.OrderBy(x => parent.Items.IndexOf(x)).ToList();
                if (direction == MoveDirection.Up)
                {
                    for (int i = 0; i < sortedItems.Count; i++)
                    {
                        parent.Items.Move(parent.Items.IndexOf(sortedItems[i]), i);
                    }
                }
                else if (direction == MoveDirection.Down)
                {
                    int k = parent.Items.Count - 1;
                    for (int i = sortedItems.Count - 1; i >= 0; i--)
                    {
                        parent.Items.Move(parent.Items.IndexOf(sortedItems[i]), k);
                        k--;
                    }
                }

                var captions = items.Select(x => x.GetDisplayCaption(_view.StructureView.SQLGenerationOptions)).ToList();
                parent.Refresh();
                ExpandTreeNode(_view.StructureView, parent);
                var newStructureItems =
                    parent.Items.Where(x => captions.Contains(x.GetDisplayCaption(_view.StructureView.SQLGenerationOptions)));

                _view.StructureView.SelectedItems = null;
                _view.StructureView.SelectedItems = newStructureItems.ToArray();
            }

            public void ClearContainerItem(MetadataItem metadataItem)
            {
                if (metadataItem != null)
                    metadataItem.Items.Clear();
            }

            public void ClearStructureItem(MetadataStructureItem structureItem)
            {
                if (structureItem != null)
                    structureItem.Items.Clear();
            }

            public void CreateChildStructureItems(MetadataStructureItem item)
            {
                if (item == null)
                    return;

                item.Items.Clear();

                item.LoadChildItems();
                List<MetadataItem> metadataItems = item.GetChildMetadataItems();

                foreach (var metadataItem in metadataItems)
                {
                    if (metadataItem.Type.IsObject() && Structure.Options.GroupByTypes) continue;

                    if (metadataItem.Type.IsA(MetadataType.ObjectMetadata) && Structure.Options.GroupFieldsByTypes) continue;

                    var newItem = new MetadataStructureItem(false);
                    item.Items.Add(newItem);
                    newItem.MetadataName = metadataItem.NameFull;
                    newItem.AllowChildAutoItems = false;
                }

                ExpandTreeNode(_view.StructureView, item);
            }

            public void GenerateStructure(bool groupByServers, bool groupByDatabases, bool groupBySchemas,
                bool groupByTypes, bool populateObjects)
            {
                var gServers = Structure.Options.GroupByServers;
                var gDatabases = Structure.Options.GroupByDatabases;
                var gSchemas = Structure.Options.GroupBySchemas;
                var gTypes = Structure.Options.GroupByTypes;

                Structure.LoadDynamicNodes = true;
                Structure.AllowChildAutoItems = true;
                try
                {
                    ClearStructure(Structure);

                    Structure.Options.GroupByServers = groupByServers;
                    Structure.Options.GroupByDatabases = groupByDatabases;
                    Structure.Options.GroupBySchemas = groupBySchemas;
                    Structure.Options.GroupByTypes = groupByTypes;

                    Structure.Refresh();
                    PopulateGeneratedStructure(Structure, populateObjects);
                    ProcessMetadataNamesAndFlags(Structure);
                }
                finally
                {
                    Structure.LoadDynamicNodes = false;
                    Structure.AllowChildAutoItems = false;

                    Structure.Options.GroupByServers = gServers;
                    Structure.Options.GroupByDatabases = gDatabases;
                    Structure.Options.GroupBySchemas = gSchemas;
                    Structure.Options.GroupByTypes = gTypes;
                }

                var focused = _view.StructureView.FocusedItem;
                focused?.Refresh();
                MetadataType tp = MetadataType.Root;

                if (groupByServers)
                {
                    tp |= MetadataType.Server;
                }

                if (groupByDatabases)
                {
                    tp |= MetadataType.Database;
                }

                if (groupBySchemas)
                {
                    tp |= MetadataType.Schema;
                }

                if (groupByTypes)
                {
                    tp |= MetadataType.ObjectMetadata;
                }

                if (populateObjects)
                {
                    tp |= MetadataType.Objects;
                }

                _view.StructureView.Options.DefaultExpandMetadataType = tp;
                _view.StructureView.InitializeDatabaseSchemaTree();
            }

            private void PopulateGeneratedStructure(MetadataStructureItem parentItem, bool generateObjectNodes)
            {
                foreach (MetadataStructureItem item in parentItem.Items)
                {
                    if (item.MetadataItem == null || !item.MetadataItem.Type.IsObject() || generateObjectNodes)
                        item.IsDynamic = false;
                    
                    item.LoadChildItems();

                    if (item.MetadataItem == null || !item.MetadataItem.Type.IsObject())
                        PopulateGeneratedStructure(item, generateObjectNodes);

                    if (item.MetadataItem == null && generateObjectNodes)
                        item.MetadataFilter.Clear();
                }
            }

            private void ProcessMetadataNamesAndFlags(MetadataStructureItem item)
            {
                if (item.MetadataItem != null && !(item is MetadataStructure))
                {
                    var metadataName = item.MetadataItem.NameFull;
                    item.MetadataItem = null;
                    item.MetadataName = metadataName;
                    item.AllowChildAutoItems = false;
                }

                foreach (var it in item.Items)
                {
                    ProcessMetadataNamesAndFlags(it);
                }
            }

            private MetadataStructureItem GetFieldsFolder(MetadataStructureItem item)
            {
                return item.Items.FirstOrDefault(x => x.Caption == ContainerStructure.Options.FieldsFolderText);
            }

            private MetadataStructureItem GetForeignKeyFolder(MetadataStructureItem item)
            {
                return item.Items.FirstOrDefault(x => x.Caption == ContainerStructure.Options.ForeignKeysFolderText);
            }

            private MetadataStructureItem CreateFieldsFolder(MetadataStructureItem parent)
            {
                var item = new MetadataStructureItem(true)
                {
                    Caption = ContainerStructure.Options.FieldsFolderText
                };

                var filterItem = item.MetadataFilter.Add();
                filterItem.ObjectTypes = MetadataType.Field | MetadataType.Objects;

                parent.Items.Add(item);

                return item;
            }

            private MetadataStructureItem CreateForeignKeyFolder(MetadataStructureItem parent)
            {
                var item = new MetadataStructureItem(true)
                {
                    Caption = ContainerStructure.Options.ForeignKeysFolderText
                };

                var filterItem = item.MetadataFilter.Add();
                filterItem.ObjectTypes = MetadataType.ForeignKey | MetadataType.Objects;

                parent.Items.Add(item);

                return item;
            }

            private void ClearStructure(MetadataStructure structure)
            {
                var itemsToDelete = new List<MetadataStructureItem>();
                foreach (var item in structure.Items)
                {
                    if (!item.IsFavoritesFolder())
                        itemsToDelete.Add(item);
                }

                itemsToDelete.ForEach(x => structure.Items.Remove(x));
            }

            private MetadataItem CreateNamespaceItem(MetadataItem parentItem, MetadataType type)
            {
                MetadataItem namepaceItem = new MetadataNamespace(parentItem.Items, type);
                namepaceItem.Name = GetObjectName(namepaceItem.Type, parentItem);
                parentItem.Items.Add(namepaceItem);
                return namepaceItem;
            }

            private MetadataItem CreateObjectItem(MetadataItem parentItem, MetadataType type)
            {
                MetadataItem item = new MetadataObject(parentItem.Items, type);
                item.Name = GetObjectName(item.Type, parentItem);
                parentItem.Items.Add(item);

                return item;
            }

            private void AddToDatabaseTreeSchema(MetadataItem item, MetadataStructureItem parentStructureItem)
            {
                if (item == null || parentStructureItem == null)
                    return;

                ExpandTreeNode(_view.ContainerView, parentStructureItem);

                var structureItem = parentStructureItem.Items.FirstOrDefault(x => x.MetadataItem == item);
                if (structureItem == null)
                {
                    structureItem = new MetadataStructureItem(true)
                    {
                        MetadataItem = item
                    };

                    parentStructureItem.Items.Add(structureItem);
                }

                structureItem.LoadChildItems();
                _view.ContainerView.FocusedItem = structureItem;
            }

            private static void ExpandTreeNode(IDatabaseSchemaView tree, MetadataStructureItem item)
            {
                var treeView = (tree.DatabaseStructureView as IMetadataStructureTree);
                treeView?.ExpandItem(item);
            }

            private string GetObjectName(MetadataType type, MetadataItem parentItem)
            {
                string result = string.Empty;
                switch (type)
                {
                    case MetadataType.Table:
                        result = Helpers.Localizer.GetString("strNewTable", LocalizableConstantsInternal.strNewTable);
                        break;

                    case MetadataType.View:
                        result = Helpers.Localizer.GetString("strNewView", LocalizableConstantsInternal.strNewView);
                        break;

                    case MetadataType.Procedure:
                        result = Helpers.Localizer.GetString("strNewProcedure", LocalizableConstantsInternal.strNewProcedure);
                        break;

                    case MetadataType.Synonym:
                        result = Helpers.Localizer.GetString("strNewSynonym", LocalizableConstantsInternal.strNewSynonym);
                        break;

                    case MetadataType.Package:
                        result = Helpers.Localizer.GetString("strNewPackage", LocalizableConstantsInternal.strNewPackage);
                        break;

                    case MetadataType.Server:
                        result = Helpers.Localizer.GetString("strNewServer", LocalizableConstantsInternal.strNewServer);
                        break;

                    case MetadataType.Schema:
                        result = Helpers.Localizer.GetString("strNewSchema", LocalizableConstantsInternal.strNewSchema);
                        break;

                    case MetadataType.Database:
                        result = Helpers.Localizer.GetString("strNewDatabase", LocalizableConstantsInternal.strNewDatabase);
                        break;

                    case MetadataType.Field:
                        result = Helpers.Localizer.GetString("strNewField", LocalizableConstantsInternal.strNewField);
                        break;
                }

                return parentItem.GetUniqueItemName(type, result);
            }

            private static MetadataStructureItem FindMetadataItemOfType(MetadataStructureItem structureItem, MetadataType type, bool bidirectional = true)
            {
                var item = structureItem;
                while (item != null)
                {
                    if (item.MetadataItem != null && item.MetadataItem.Type.IsA(type))
                        break;

                    item = item.Parent;
                }

                if (bidirectional && item == null)
                {
                    item = structureItem;
                    while (item != null && (item.MetadataItem != null && (item.MetadataItem.Type & type) == 0))
                        item = (item.Items.Count > 0) ? item.Items[0] : null;
                }

                if (item == null)
                    return null;

                if (item.MetadataItem != null && item.MetadataItem.Type.IsA(type))
                    return item;
                else
                    return null;
            }

            private class MetadataItemComparer : IComparer<MetadataItem>
            {
                public SortingType Type { get; set; }

                public MetadataItemComparer(SortingType type)
                {
                    Type = type;
                }

                public int Compare(MetadataItem x, MetadataItem y)
                {
                    if (x == y)
                    {
                        return 0;
                    }

                    if (x == null)
                    {
                        return 1;
                    }

                    if (y == null)
                    {
                        return -1;
                    }

                    if (Type == SortingType.Name)
                    {
                        return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
                    }
                    else if (Type == SortingType.Type)
                    {
                        var result = ActiveQueryBuilder.Core.Helpers.CompareMetadataItemsTypes(x, y);
                        if (result == 0)
                        {
                            return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            return result;
                        }
                    }

                    return 0;
                }
            }

            private class MetadataStructureComparer : IComparer<MetadataStructureItem>
            {
                public SortingType Type { get; set; }

                public SQLGenerationOptions SqlGenerationOptions { get; set; }

                public MetadataStructureComparer(SortingType type, SQLGenerationOptions sqlGenerationOptions)
                {
                    Type = type;
                    SqlGenerationOptions = sqlGenerationOptions;
                }

                private int CompareByName(MetadataStructureItem x, MetadataStructureItem y)
                {
                    if (string.IsNullOrEmpty(x.Caption) && !string.IsNullOrEmpty(y.Caption))
                    {
                        return string.Compare(x.GetDisplayCaption(SqlGenerationOptions), y.Caption, StringComparison.OrdinalIgnoreCase);
                    }
                    else if (!string.IsNullOrEmpty(x.Caption) && string.IsNullOrEmpty(y.Caption))
                    {
                        return string.Compare(x.Caption, y.GetDisplayCaption(SqlGenerationOptions), StringComparison.OrdinalIgnoreCase);
                    }
                    else if (string.IsNullOrEmpty(x.Caption) && string.IsNullOrEmpty(y.Caption))
                    {
                        return string.Compare(x.GetDisplayCaption(SqlGenerationOptions), y.GetDisplayCaption(SqlGenerationOptions), StringComparison.OrdinalIgnoreCase);
                    }

                    return string.Compare(x.Caption, y.Caption, StringComparison.OrdinalIgnoreCase);
                }

                public int Compare(MetadataStructureItem x, MetadataStructureItem y)
                {
                    if (x == y)
                    {
                        return 0;
                    }

                    if (x == null)
                    {
                        return 1;
                    }

                    if (y == null)
                    {
                        return -1;
                    }

                    if (Type == SortingType.Name)
                    {
                        return CompareByName(x, y);
                    }
                    else if (Type == SortingType.Type)
                    {
                        if (x.MetadataItem == y.MetadataItem)
                        {
                            return 0;
                        }
                        if (y.MetadataItem == null)
                        {
                            return -1;
                        }
                        if (x.MetadataItem == null)
                        {
                            return 1;
                        }

                        var result = ActiveQueryBuilder.Core.Helpers.CompareMetadataItemsTypes(x.MetadataItem, y.MetadataItem);
                        if (result == 0)
                        {
                            return CompareByName(x, y);
                        }

                        return result;
                    }

                    return 0;
                }
            }
        }

        private class ContainerLoadFormStorage
        {
            public BaseConnectionDescriptor Connection { get; set; }
            public bool LoadFields { get; set; }
        }
    }
}
