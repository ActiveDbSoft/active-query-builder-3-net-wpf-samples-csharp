//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2021 Active Database Software              //
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
using System.Windows.Input;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.DatabaseSchemaView;
using ActiveQueryBuilder.View.EventHandlers.MetadataStructureItems;
using ActiveQueryBuilder.View.ExpressionEditor;
using ActiveQueryBuilder.View.WPF;
using ActiveQueryBuilder.View.WPF.Annotations;
using ActiveQueryBuilder.View.WPF.ExpressionEditor;
using ExpressionEditorDemo.Common;
using DragEventArgs = ActiveQueryBuilder.View.EventHandlers.DragEventArgs;

namespace ExpressionEditorDemo
{
    /// <summary>
    /// Interaction logic for ExpressionEditorControl.xaml
    /// </summary>
    public partial class ExpressionEditorControl
    {
        private BaseSyntaxProvider _syntaxProvider;
        private readonly CompositeDisposable _querySubscriptions = new CompositeDisposable();
        private SQLContext _sqlContext;
        private readonly CompositeDisposable _contextSubscriptions = new CompositeDisposable();
        private readonly Dictionary<MetadataStructureItem, object> _mapTreeItems =
            new Dictionary<MetadataStructureItem, object>();
        private readonly ExpressionEditorOptions _options = new ExpressionEditorOptions();
        private readonly SqlTextEditorOptions _textEditorSqlOptions = new SingleLineSqlTextEditorOptions();
        private readonly TextEditorOptions _textEditorOptions = new TextEditorOptions();
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        private Rect _dragBoxFromMouseDown;
        private ImageList _imageList;

        public event AddCustomKeywordsEventHandler CustomizeKeywords
        {
            add { SqlEditor.CustomizeKeywords += value; }
            remove { SqlEditor.CustomizeKeywords -= value; }
        }
        public event AddCustomFunctionsEventHandler CustomizeFunctions
        {
            add { SqlEditor.CustomizeFunctions += value; }
            remove { SqlEditor.CustomizeFunctions -= value; }
        }

        public BaseSyntaxProvider SyntaxProvider
        {
            get { return _syntaxProvider; }
            private set
            {
                if (_syntaxProvider == value)
                    return;

                _syntaxProvider = value;

                if (_syntaxProvider == null)
                    return;

                if ( Query != null && !Query.IsDisposing)
                {
                    ReloadOperators();
                    ReloadFunctions();
                }
            }
        }

        [Browsable(true)]
        [TypeConverter(typeof(ExpandableObjectConverter)),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ExpressionEditorOptions Options
        {
            get { return _options; }
            set { _options.Assign(value); }
        }

        [Browsable(true)]
        [TypeConverter(typeof(ExpandableObjectConverter)), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public SqlTextEditorOptions TextEditorSqlOptions
        {
            get { return _textEditorSqlOptions; }
            set { _textEditorSqlOptions.Assign(value); }
        }

        [Browsable(true)]
        [TypeConverter(typeof(ExpandableObjectConverter)), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TextEditorOptions TextEditorOptions
        {
            get { return _textEditorOptions; }
            set { _textEditorOptions.Assign(value); }
        }

        [Browsable(true)]
        [DefaultValue(null)]
        public SQLContext SQLContext
        {
            get { return _sqlContext; }
            set
            {
                if (_sqlContext == value)
                    return;

                UnsubscribeFromContext();

                _sqlContext = value;
                SqlEditor.SQLContext = value;
                TreeObjects.SQLContext = SQLContext;
                TreeQueryObjects.SQLContext = SQLContext;

                if (_sqlContext == null) return;

                SubscribeToContext(_sqlContext);
                SyntaxProvider = GetSyntaxFromContext(_sqlContext);
                ReloadMetadata();
            }
        }

        [Browsable(false)]
        [Category("Editor")]
        public string Expression
        {
            get { return SqlEditor.Text; }
            set { SqlEditor.Text = value; }
        }

        [Browsable(false)]
        public AstNode ExpressionAST
        {
            set
            {
                var formattingOptions = SQLFormattingOptions ??
                                        new SQLFormattingOptions
                                        {
                                            UseAltNames = SQLGenerationOptions.UseAltNames,
                                            QuoteIdentifiers = SQLGenerationOptions.QuoteIdentifiers,
                                        };
                Expression = value != null && Query != null ? value.GetSQL(formattingOptions) : string.Empty;
            }
        }

        public static readonly DependencyProperty QueryProperty = DependencyProperty.Register(
            "Query", typeof(IQueryController), typeof(ExpressionEditorControl), new PropertyMetadata(default(IQueryController), QueryOnChangedCallback));

        private static void QueryOnChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as ExpressionEditorControl;
            if(self == null) return;
            var value = e.NewValue as IQueryController;

            self._querySubscriptions.Clear();

            self.SqlEditor.Query = value;

            if (value == null) return;

            value.SQLContextChanged += self.Query_SQLContextChanged;
            self._querySubscriptions.Add(Disposable.Create(() => value.SQLContextChanged -= self.Query_SQLContextChanged));

            if (value.SQLContext != null)
                self.SQLContext = value.SQLContext;
        }

        public IQueryController Query
        {
            get { return (IQueryController) GetValue(QueryProperty); }
            set { SetValue(QueryProperty, value); }
        }

        [Browsable(false)]
        public SQLGenerationOptions SQLGenerationOptions
        {
            get { return SqlEditor.SQLGenerationOptions; }
            set { SqlEditor.SQLGenerationOptions = value; }
        }

        [Browsable(true)]
        [Category("Editor")]
        public SQLFormattingOptions SQLFormattingOptions { get; set; }

        public static readonly DependencyProperty ActiveUnionSubQueryProperty = DependencyProperty.Register(
            "ActiveUnionSubQuery", typeof(UnionSubQuery), typeof(ExpressionEditorControl), new PropertyMetadata(null, ActiveUnionSubQueryPropertyChanged));

        private static void ActiveUnionSubQueryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as ExpressionEditorControl;

            if (self == null) return;

            if (e.NewValue != null)
                self.ReloadQueryObjects();
        }

        [Browsable(false)]
        public UnionSubQuery ActiveUnionSubQuery
        {
            get { return (UnionSubQuery)GetValue(ActiveUnionSubQueryProperty); }
            set { SetValue(ActiveUnionSubQueryProperty, value); }
        }

        public static readonly DependencyProperty ObjectsTreePinnedProperty = DependencyProperty.Register(
            "DatabaseSchemaViewPanelPinned", typeof(bool), typeof(ExpressionEditorControl), new PropertyMetadata(false, DsvPanelPinnedChanged));

        private static void DsvPanelPinnedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (ExpressionEditorControl)d;
            self.DockPanelDatabaseShema.AutoHide = !(bool)e.NewValue;
        }

        public bool ObjectsTreePinned
        {
            get { return (bool)GetValue(ObjectsTreePinnedProperty); }
            set { SetValue(ObjectsTreePinnedProperty, value); }
        }

        public static readonly DependencyProperty QueryObjectsTreePinnedProperty = DependencyProperty.Register(
            "SqlContexPanelPinned", typeof(bool), typeof(ExpressionEditorControl), new PropertyMetadata(true, ScPanelPinnedChanged));

        private static void ScPanelPinnedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (ExpressionEditorControl)d;
            self.DockPanelSqlContext.AutoHide = !(bool)e.NewValue;
        }

        public bool QueryObjectsTreePinned
        {
            get { return (bool)GetValue(QueryObjectsTreePinnedProperty); }
            set { SetValue(QueryObjectsTreePinnedProperty, value); }
        }

        public ExpressionEditorControl()
        {
            InitializeComponent();

            _subscriptions.Add(_options.SubscribeToUpdated(() => Options.Assign(_options)));
            _subscriptions.Add(_textEditorOptions.SubscribeToUpdated(() => TextEditorOptions.Assign(_textEditorOptions)));
            _subscriptions.Add(_textEditorSqlOptions.SubscribeToUpdated(() => TextEditorSqlOptions.Assign(_textEditorSqlOptions)));

            var il = new ImageList();

            il.Images.Add(ActiveQueryBuilder.View.WPF.Images.Metadata.Field.Value);            // 0
            il.Images.Add(ActiveQueryBuilder.View.WPF.Images.Metadata.UserTable.Value);        // 1
            il.Images.Add(ActiveQueryBuilder.View.WPF.Images.Metadata.UserView.Value);         // 2
            il.Images.Add(ActiveQueryBuilder.View.WPF.Images.Metadata.UserProcedure.Value);    // 3
            il.Images.Add(ActiveQueryBuilder.View.WPF.Images.Metadata.UserSynonym.Value);      // 4
            il.Images.Add(ActiveQueryBuilder.View.WPF.Images.TextEditor.Function.Value);       // 5

            SetImageList(il);

            Localize();

            var langProperty = DependencyPropertyDescriptor.FromProperty(LanguageProperty, GetType());
            langProperty.AddValueChanged(this, LanguaheChanged);

            Assign(_options);

            SqlEditor.Options = _textEditorOptions;

            TreeObjects.Options.SortingType = ObjectsSortingType.NameExceptFields;
            TreeQueryObjects.Options.SortingType = ObjectsSortingType.NameExceptFields;

            TreeQueryObjects.SQLGenerationOptions.UseAltNames = true;

            TreeObjects.SQLContext = SQLContext;
            TreeQueryObjects.SQLContext = SQLContext;

            TreeObjects.ValidateItemContextMenu += TreeObjects_ValidateItemContextMenu;
            TreeQueryObjects.ValidateItemContextMenu += TreeQueryObjects_ValidateItemContextMenu;

            Assign(_options);
            AssignSqlTextEditorOptions(_textEditorSqlOptions);
            AssignTextEditorOptions(_textEditorOptions);

            DockPanelSqlContext.AutoHide = !QueryObjectsTreePinned;
            DockPanelDatabaseShema.AutoHide = !ObjectsTreePinned;
        }

        public void InsertMetadataItemNameIntoEditor(MetadataItem metadataItem)
        {
            InsertTextIntoEditor(GetNameMetadataItem(metadataItem, false));
        }

        public void InsertTextIntoEditor(string text)
        {
            int cursor = text.IndexOf("%CURSOR%", StringComparison.Ordinal);

            if (cursor != -1)
                text = text.Replace("%CURSOR%", "");

            int start = SqlEditor.SelectionStart;

            SqlEditor.ReplaceSelection(text);

            if (cursor != -1)
                SqlEditor.SelectionStart = start + cursor;

            SqlEditor.Focus();
        }

        public void ShowInformationMessage(string message)
        {
            LbNotification.Text = message;
            PnlNotification.Visibility = Visibility.Visible;
        }

        public void HideInformationMessage()
        {
            PnlNotification.Visibility = Visibility.Collapsed;
        }

        public void Assign(IExpressionEditorOptions options)
        {
            switch (options.DatabaseSchemaViewPanelDocking)
            {
                case SidePanelDockStyle.Left:
                    DockPanelDatabaseShema.Position = Docking.Left;
                    break;
                case SidePanelDockStyle.Right:
                    DockPanelDatabaseShema.Position = Docking.Right;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (options.SqlContextPanelDocking)
            {
                case SidePanelDockStyle.Left:
                    DockPanelSqlContext.Position = Docking.Left;
                    break;
                case SidePanelDockStyle.Right:
                    DockPanelSqlContext.Position = Docking.Right;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            ObjectsTreePinned = options.DatabaseSchemaViewPanelPinned;
            QueryObjectsTreePinned = options.SqlContextPanelPinned;
            Options.SearchFields = options.SearchFields;
            MinWidth = options.MinimumSize.Width;
            MinHeight = options.MinimumSize.Height;
        }

        public void AssignTextEditorOptions(ITextEditorOptions options)
        {
            SqlEditor.Options.Assign(options);
        }

        public void AssignSqlTextEditorOptions(ISqlTextEditorOptions options)
        {
            SqlEditor.SqlOptions.Assign(options);
        }

        private void Query_SQLContextChanged(object sender, EventArgs eventArgs)
        {
            QueryRoot query = (QueryRoot)sender;

            SQLContext = query.SQLContext;
        }

        private static BaseSyntaxProvider GetSyntaxFromContext(SQLContext context)
        {
            if (context?.SyntaxProvider == null)
                return null;

            var autoSyntaxProvider = context.SyntaxProvider as AutoSyntaxProvider;
            return autoSyntaxProvider != null ? autoSyntaxProvider.DetectedSyntaxProvider : context.SyntaxProvider;

        }

        private void SQLContext_SyntaxProviderChanged(object sender, EventArgs e)
        {
            SyntaxProvider = GetSyntaxFromContext(SQLContext);
        }

        private void SQLContext_Disposing(object sender, EventArgs e)
        {
            SQLContext = null;
        }

        private void MetadataContainer_Updated(object sender, EventArgs e)
        {
            if (Query != null && !Query.IsDisposing)
            {
                ReloadMetadata();
            }
        }

        private void MetadataContainer_Disposing(object sender, EventArgs e)
        {
            ((MetadataContainer)sender).Updated -= MetadataContainer_Updated;
            ((MetadataContainer)sender).Disposing -= MetadataContainer_Disposing;
        }

        private void UnsubscribeFromContext()
        {
            _contextSubscriptions.Clear();
        }

        private void ReloadMetadata()
        {
            try
            {
                ShowWaitCursor();

                var metadataStructure = new MetadataStructure
                {
                    Options =
                    {
                        GroupBySchemas = true,
                        GroupByTypes = true
                    },
                    MetadataItem = SQLContext.MetadataContainer
                };
                metadataStructure.LoadChildItems();

                FillObjectTree(metadataStructure);
            }
            finally
            {
                ResetCursor();
            }
        }

        private void SubscribeToContext(SQLContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.SyntaxProviderChanged += SQLContext_SyntaxProviderChanged;
            _contextSubscriptions.Add(Disposable.Create(() =>
                context.SyntaxProviderChanged -= SQLContext_SyntaxProviderChanged));

            context.Disposing += SQLContext_Disposing;
            _contextSubscriptions.Add(Disposable.Create(() => context.Disposing -= SQLContext_Disposing));

            var metadataContainer = context.MetadataContainer;
            if (metadataContainer != null)
            {
                metadataContainer.Updated += MetadataContainer_Updated;
                _contextSubscriptions.Add(Disposable.Create(() => metadataContainer.Updated -= MetadataContainer_Updated));

                metadataContainer.Disposing += MetadataContainer_Disposing;
                _contextSubscriptions.Add(Disposable.Create(() => metadataContainer.Disposing -= MetadataContainer_Disposing));
            }
        }

        private void TreeQueryObjects_ValidateItemContextMenu(object sender, MetadataStructureItemMenuEventArgs e)
        {
            var metadataItem = e.MetadataStructureItem.MetadataItem;
            var mappedObject = _mapTreeItems.ContainsKey(e.MetadataStructureItem) ? _mapTreeItems[e.MetadataStructureItem] : null;
            var lng = ActiveQueryBuilder.View.WPF.Helpers.ConvertLanguageFromNative(Language);

            if (metadataItem == null)
            {
                var dataSource = mappedObject as DataSource;
                if (dataSource != null)
                {
                    if (dataSource.AliasAST != null)
                    {
                        e.Menu.AddItem(ActiveQueryBuilder.View.WPF.Helpers.Localizer.GetString("strInsertAlias", lng, LocalizableConstantsUI.strInsertAlias), MenuItem_Clicked, false, true, null, dataSource.NameInQuery);
                        e.Menu.AddItem(ActiveQueryBuilder.View.WPF.Helpers.Localizer.GetString("strInsertName", lng, LocalizableConstantsUI.strInsertName), MenuItem_Clicked, false, true, null,
                            dataSource.GetObjectNameInQuery(SQLGenerationOptions));
                    }
                    else
                    {
                        e.Menu.AddItem(ActiveQueryBuilder.View.WPF.Helpers.Localizer.GetString("strInsertName", lng, LocalizableConstantsUI.strInsertName), MenuItem_Clicked, false, true, null, dataSource.NameInQuery);
                    }
                }

                var field = mappedObject as MetadataField;

                if (field != null)
                {
                    e.Menu.AddItem(ActiveQueryBuilder.View.WPF.Helpers.Localizer.GetString("strInsertName", lng, LocalizableConstantsUI.strInsertName), MenuItem_Clicked, false, true, null, field.Name);
                }
            }
            else
            {
                if (mappedObject == null)
                {
                    e.Menu.AddItem(ActiveQueryBuilder.View.WPF.Helpers.Localizer.GetString("strInsertName", lng, LocalizableConstantsUI.strInsertName), MenuItem_Clicked, false, true, null, GetNameMetadataItem(metadataItem, false));
                }
                else
                {
                    var dataSource = mappedObject as DataSource;
                    if (dataSource != null)
                    {
                        if (dataSource.AliasAST != null)
                        {
                            e.Menu.AddItem(ActiveQueryBuilder.View.WPF.Helpers.Localizer.GetString("strInsertAlias", lng, LocalizableConstantsUI.strInsertAlias), MenuItem_Clicked, false, true, null, dataSource.NameInQuery);
                        }

                        e.Menu.AddItem(ActiveQueryBuilder.View.WPF.Helpers.Localizer.GetString("strInsertName", lng, LocalizableConstantsUI.strInsertName), MenuItem_Clicked, false, true, null, GetNameMetadataItem(metadataItem, false));
                    }

                    var field = mappedObject as MetadataField;

                    if (field != null)
                    {
                        e.Menu.AddItem(ActiveQueryBuilder.View.WPF.Helpers.Localizer.GetString("strInsertName", lng, LocalizableConstantsUI.strInsertName), MenuItem_Clicked, false, true, null, GetNameMetadataItem(field, false));
                    }
                }
            }
        }

        private void TreeObjects_ValidateItemContextMenu(object sender, MetadataStructureItemMenuEventArgs e)
        {
            var metadataItem = e.MetadataStructureItem.MetadataItem;

            if (metadataItem == null) return;

            var lng = ActiveQueryBuilder.View.WPF.Helpers.ConvertLanguageFromNative(Language);

            e.Menu.AddItem(ActiveQueryBuilder.View.WPF.Helpers.Localizer.GetString("strInsertName", lng, LocalizableConstantsUI.strInsertName), MenuItem_Clicked, false, true, null, GetNameMetadataItem(metadataItem, false));
            e.Menu.AddItem(ActiveQueryBuilder.View.WPF.Helpers.Localizer.GetString("strInsertFullName", lng, LocalizableConstantsUI.strInsertFullName), MenuItem_Clicked, false, true, null, GetNameMetadataItem(metadataItem, true));

        }

        private string GetNameMetadataItem(MetadataItem metadataItem, bool isFullName)
        {
            if (metadataItem == null)
                return "";

            return metadataItem.GetQualifiedNameSQL(isFullName ? null : metadataItem.Parent, SQLGenerationOptions, ObjectPrefixSkipping.SkipAll);
        }

        private void TextEditor_PreProcessDragDrop(object source, DragEventArgs dragEventArgs)
        {
            var metadataDragObject = GetDragObject<MetadataDragObject>(dragEventArgs.Data);

            if (metadataDragObject != null)
            {
                foreach (var metadataStructureItem in metadataDragObject.MetadataStructureItems)
                    InsertTextIntoEditor(GetNameFromMetadataStructureItem(metadataStructureItem));
            }
            else
            {
                var text = GetDragObject<string>(dragEventArgs.Data);

                if (!string.IsNullOrEmpty(text))
                    InsertTextIntoEditor(text);
            }

            dragEventArgs.Handled = true;
        }

        private string GetNameFromMetadataStructureItem(MetadataStructureItem structureItem)
        {
            if (structureItem.MetadataItem == null)
            {
                return structureItem.Caption;
            }

            string insertString;

            if (structureItem.MetadataItem is MetadataField)
            {
                var nameField = GetNameMetadataItem(structureItem.MetadataItem, false);

                var mapItem =
                    (_mapTreeItems.ContainsKey(structureItem.Parent) ? _mapTreeItems[structureItem.Parent] : null) as DataSource;

                var nameObject = mapItem?.AliasAST != null ?
                    mapItem.NameInQuery :
                    GetNameMetadataItem(structureItem.MetadataItem.Parent, mapItem == null);

                insertString = $"{nameObject}.{nameField}";
            }
            else
            {
                var mapItem =
                    (_mapTreeItems.ContainsKey(structureItem) ? _mapTreeItems[structureItem] : null) as DataSource;

                insertString = mapItem?.AliasAST != null
                    ? mapItem.NameInQuery
                    : GetNameMetadataItem(structureItem.MetadataItem, mapItem == null);
            }

            return insertString;
        }

        private void MenuItem_Clicked(object sender, EventArgs e)
        {
            InsertTextIntoEditor((string)((ICustomMenuItem)sender).Tag);
        }

        private void LanguaheChanged(object sender, EventArgs e)
        {
            Localize();
        }

        private void Localize()
        {
            DockPanelDatabaseShema.Title = ActiveQueryBuilder.View.WPF.Helpers.Localizer.GetString("strDatabaseSchemaPanelTitle",
                ActiveQueryBuilder.View.WPF.Helpers.ConvertLanguageFromNative(Language),
                LocalizableConstantsUI.strDatabaseSchemaPanelTitle);

            DockPanelSqlContext.Title = ActiveQueryBuilder.View.WPF.Helpers.Localizer.GetString("strSqlContextPanelTitle",
                ActiveQueryBuilder.View.WPF.Helpers.ConvertLanguageFromNative(Language),
                LocalizableConstantsUI.strSqlContextPanelTitle);
        }

        private void TreeFunctions_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(TreeFunctions);
            if (!(sender is ListBoxItem))
            {
                _dragBoxFromMouseDown = Rect.Empty;
                return;
            }
            var item = (ListBoxItem)sender;

            item.IsSelected = true;
            var dragSize = new Size(SystemParameters.MinimumHorizontalDragDistance,
                SystemParameters.MinimumVerticalDragDistance);

            _dragBoxFromMouseDown =
                new Rect(new Point(mousePosition.X - (dragSize.Width / 2), mousePosition.Y - (dragSize.Height / 2)),
                    dragSize);
        }

        private void TreeFunctions_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _dragBoxFromMouseDown = Rect.Empty;
            var p = e.GetPosition((FrameworkElement) sender);
            var mousePosition = ((FrameworkElement) sender).PointToScreen(p);

            if (e.ChangedButton != MouseButton.Right) return;

            var lng = ActiveQueryBuilder.View.WPF.Helpers.ConvertLanguageFromNative(Language);
            ICustomContextMenu menu = ControlFactory.Instance.GetCustomContextMenu();

            object tag = GetNodeTag(sender);

            var aki = tag as AdvancedKeywordInfo;

            if (aki != null)
                menu.AddItem(
                    ActiveQueryBuilder.View.WPF.Helpers.Localizer.GetString("strInsertFunction", lng,
                        LocalizableConstantsUI.strInsertFunction), MenuItem_Clicked, false, true, null, aki.Template);

            if (menu.ItemCount > 0)
                menu.Show(null, mousePosition.X, mousePosition.Y);
        }

        private void TreeFunctions_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            
            var tag = GetNodeTag(sender);

            if (tag == null) return;

            var mousePosition = e.GetPosition(TreeFunctions);

            if (!ShouldBeginDrag(mousePosition.X, mousePosition.Y)) return;

            var text = ((AdvancedKeywordInfo)tag).Template;

            if (string.IsNullOrEmpty(text)) return;

            TreeFunctions.DoDragDrop(text, CDragDropEffects.Copy | CDragDropEffects.Move);
        }

        private void TreeFunctions_KeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(TbFilterForFunctions.Text)) return;

            if (e.Key != Key.F3) return;

            var index = Keyboard.Modifiers != ModifierKeys.Shift
                ? TreeFunctions.SelectedIndex + 1
                : TreeFunctions.SelectedIndex - 1;

            var source = ((IEnumerable<TreeViewItemData>)TreeFunctions.ItemsSource).ToList();

            while (index < source.Count && index > 0)
            {
                if (string.IsNullOrEmpty(source[index].StringToUnderscore))
                {
                    if (Keyboard.Modifiers != ModifierKeys.Shift)
                        index++;
                    else
                        index--;
                    continue;
                }

                if (TreeFunctions.SelectedItem != source[index])
                {
                    TreeFunctions.SelectedItem = source[index];
                    TreeFunctions.ScrollIntoView(source[index]);

                    var item = TreeFunctions.ItemContainerGenerator.ContainerFromItem(source[index]) as ListBoxItem;
                    item?.Focus();
                }

                break;
            }
        }

        private void SetImageList(object imageList)
        {
            _imageList = (ImageList)imageList;
        }

        private void FillObjectTree(MetadataStructure metadataStructure)
        {
            TreeObjects.MetadataStructure = metadataStructure;
            TreeObjects.InitializeDatabaseSchemaTree();
        }

        private void FillQueryObjectsTree(MetadataStructure metadataStructure)
        {
            TreeQueryObjects.MetadataStructure = metadataStructure;
            TreeQueryObjects.InitializeDatabaseSchemaTree();
        }

        private void FillFunctionsTree(List<NodeData> nodes)
        {
            var source = new List<TreeViewItemData>();

            foreach (var nodeData in nodes)
            {
                var node = new TreeViewItemData
                {
                    ImageSource = nodeData.ImageIndex == -1 ? null : _imageList.Images[nodeData.ImageIndex],
                    Text = nodeData.Name,
                    ToolTip = nodeData.ToolTipText,
                    Tag = nodeData.Tag
                };

                source.Add(node);

                if (nodeData.Children == null) continue;

                foreach (var fieldNodeData in nodeData.Children)
                {
                    var nodeChild = new TreeViewItemData
                    {
                        ImageSource = _imageList.Images[fieldNodeData.ImageIndex],
                        Text = fieldNodeData.Name,
                        ToolTip = fieldNodeData.ToolTipText,
                        Tag = fieldNodeData.Tag
                    };

                    node.Items.Add(nodeChild);
                }
            }

            TreeFunctions.ItemsSource = source.OrderBy(x => x.Text);

            if (!string.IsNullOrEmpty(TbFilterForFunctions.Text))
            {
                CheckTreeFilter(TreeFunctions, TbFilterForFunctions.Text.ToLower());
            }
        }

        private void FillOperators(List<string> operatorList)
        {
            foreach (var item in PanelOperators.Children.Cast<Button>())
                item.Click -= OperatorButton_Click;

            PanelOperators.Children.Clear();

            if (operatorList.Count <= 0) return;

            foreach (var item in operatorList)
            {
                var b = ControlFactory.Instance.GetOperatorButton();
                b.Text = item;
                b.Click += OperatorButton_Click;
                PanelOperators.Children.Add((Button)b);
            }
        }

        private void OperatorButton_Click(object sender, EventArgs e)
        {
            InsertTextIntoEditor(((ISimpleButton)sender).Text);
        }

        private object GetNodeTag(object node)
        {
            var nodeTree = node as ListBoxItem;

            return ((TreeViewItemData)nodeTree?.DataContext)?.Tag;
        }

        private bool ShouldBeginDrag(double x, double y)
        {
            return (!_dragBoxFromMouseDown.IsEmpty && !_dragBoxFromMouseDown.Contains(x, y));
        }

        private void ShowWaitCursor()
        {
            Cursor = Cursors.Wait;
        }

        private void ResetCursor()
        {
            Cursor = null;
        }

        private static T GetDragObject<T>(object dragObject) where T : class
        {
            var dr = dragObject as DataObject;

            return dr?.GetData(typeof(T)) as T;
        }

        private void CheckTreeFilter([NotNull] ItemsControl tree, string filter)
        {
            var source = tree.ItemsSource as IEnumerable<TreeViewItemData>;

            if (source == null) return;

            var treeViewItemsData = source as TreeViewItemData[] ?? source.ToArray();
            var firstmatch = treeViewItemsData.FirstOrDefault(x => x.Text.ToLowerInvariant().Contains(filter.ToLowerInvariant()));

            if (firstmatch != null)
            {
                TreeFunctions.ScrollIntoView(firstmatch);
                TreeFunctions.SelectedItem = firstmatch;
            }

            foreach (var treeViewItemData in treeViewItemsData)
            {
                treeViewItemData.StringToUnderscore =
                    treeViewItemData.Text.ToLowerInvariant().Contains(filter.ToLowerInvariant()) ? filter : "";
            }
        }

        private void TreeFunctions_OnSuperMouseDoubleClick(object sender, EventArgs e)
        {
            var tag = GetNodeTag(sender);

            if (tag != null)
            {
                InsertTextIntoEditor(((AdvancedKeywordInfo)tag).Template);
            }
        }

        private void TbFilterForFunctions_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var filter = TbFilterForFunctions.Text;

            ReloadFunctions();

            CheckTreeFilter(TreeFunctions, filter);
        }

        private void TreeQueryObjects_OnItemDoubleClick(object sender, MetadataStructureItem clickeditem)
        {
            InsertTextIntoEditor(GetNameFromMetadataStructureItem(clickeditem));
        }

        private void TreeObjects_OnItemDoubleClick(object sender, MetadataStructureItem clickeditem)
        {
            var name = GetNameMetadataItem(clickeditem.MetadataItem, true);
            InsertTextIntoEditor(name);
        }

        private void TbFilterForFunctions_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.F3) return;

            TreeFunctions_KeyDown(TreeFunctions, e);
        }

        private void ReloadQueryObjects()
        {
            if (ActiveUnionSubQuery == null)
                return;

            _mapTreeItems.Clear();

            var metadataStructure = new MetadataStructure() { AllowChildAutoItems = false };

            List<DataSource> listDataSource = ActiveUnionSubQuery.GetVisibleDataSources();

            foreach (var dataSource in listDataSource)
            {
                var itemStructure = new MetadataStructureItem { MetadataItem = dataSource.MetadataObject };

                _mapTreeItems.Add(itemStructure, dataSource);

                if (dataSource.AliasAST != null && !(dataSource is DataSourceQuery))
                {
                    itemStructure.Caption = dataSource.NameInQuery + " (" +
                                            dataSource.GetObjectNameInQuery(SQLGenerationOptions) + ")";
                }

                if (dataSource.MetadataObject == null)
                {
                    if (string.IsNullOrEmpty(itemStructure.Caption))
                        itemStructure.Caption = dataSource.NameInQuery;

                    itemStructure.ImageIndex = 5;

                    foreach (var field in dataSource.Metadata.Fields)
                    {
                        var metadataItemField = new MetadataStructureItem { ImageIndex = 11 };
                        _mapTreeItems.Add(metadataItemField, field);

                        if (SQLGenerationOptions.UseAltNames && !string.IsNullOrEmpty(field.AltName))
                            metadataItemField.Caption = field.AltName;
                        else
                            metadataItemField.Caption = field.Name;

                        itemStructure.Items.Add(metadataItemField);
                    }
                }

                metadataStructure.Items.Add(itemStructure);
            }

            FillQueryObjectsTree(metadataStructure);
        }

        private void ReloadOperators()
        {
            List<string> operatorList = new List<string>();

            SyntaxProvider?.GetComparisonOperators(operatorList);

            FillOperators(operatorList);
        }

        private void ReloadFunctions()
        {
            bool filtering = CbFilterFunctions.IsChecked.HasValue && CbFilterFunctions.IsChecked.Value;
            string filter = TbFilterForFunctions.Text;
            List<NodeData> newNodes = new List<NodeData>();

            if (Query != null)
            {
                foreach (KeyValuePair<string, AdvancedKeywordInfo> kvp in SqlEditor.SuggestionObjects.Functions)
                {
                    if (filtering)
                    {
                        if (filter.Length <= 0) continue;

                        if (kvp.Key.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) == -1) continue;

                        var node = new NodeData(kvp.Value.Name)
                        {
                            Tag = kvp.Value,
                            ImageIndex = 5,
                            ToolTipText = TextHelpers.Wrap(kvp.Value.Description, 120)
                        };

                        newNodes.Add(node);
                    }
                    else
                    {
                        NodeData node = new NodeData(kvp.Value.Name)
                        {
                            Tag = kvp.Value,
                            ImageIndex = 5,
                            ToolTipText = TextHelpers.Wrap(kvp.Value.Description, 120)
                        };

                        newNodes.Add(node);
                    }
                }
            }

            FillFunctionsTree(newNodes);
        }

        private void CbFilterFunctions_OnChange(object sender, RoutedEventArgs e)
        {
            ReloadFunctions();
        }
    }
}
