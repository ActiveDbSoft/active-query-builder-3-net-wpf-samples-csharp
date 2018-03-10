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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.DatabaseSchemaView;
using ActiveQueryBuilder.View.EventHandlers.MetadataStructureItems;
using ActiveQueryBuilder.View.ExpressionEditor;
using ActiveQueryBuilder.View.MetadataStructureView;
using ActiveQueryBuilder.View.WPF;
using ActiveQueryBuilder.View.WPF.Annotations;
using ExpressionEditorDemo.Common;
using Helpers = ActiveQueryBuilder.View.WPF.Helpers;

namespace ExpressionEditorDemo
{
    /// <summary>
    /// Interaction logic for View.xaml
    /// </summary>
    internal partial class View : IView
    {
        private Rect _dragBoxFromMouseDown;
        private ImageList _imageList;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler OperatorButtonClick;

        public event MetadataStructureItemMenuEventHandler ObjectTreeValidateItemContextMenu;
        public event MetadataStructureItemMenuEventHandler QueryObjectTreeValidateItemContextMenu;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        event MetadataStructureItemEventHandler IView.ObjectTreeDoubleClick
        {
            add { TreeObjects.ItemDoubleClick += value;}
            remove { TreeObjects.ItemDoubleClick -= value;}
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        event EventHandler IView.FilterFunctionsChanged
        {
            add
            {
                cbFilterFunctions.Checked += new RoutedEventHandler(value);
                cbFilterFunctions.Unchecked += new RoutedEventHandler(value);
            }
            remove
            {
                cbFilterFunctions.Checked -= new RoutedEventHandler(value);
                cbFilterFunctions.Unchecked -= new RoutedEventHandler(value);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler FilterTextForFunctionsChanged;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event CKeyEventHandler FilterForFunctionsKeyDown;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event CKeyEventHandler FunctionTreeKeyDown;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler FunctionTreeDoubleClick;
      

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event CMouseEventHandler FunctionTreeMouseUp;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event CMouseEventHandler FunctionTreeMouseMove;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        event MetadataStructureItemEventHandler IView.QueryObjectTreeDoubleClick
        {
            add { TreeQueryObjects.ItemDoubleClick += value; }
            remove { TreeQueryObjects.ItemDoubleClick -= value; }
        }

        private readonly Controller _controller;

        [Browsable(true)]
        [DefaultValue(false)]
        [Category("Editor")]
        [Description("Disables code completion in editor.")]
        public bool DisableCodeCompletion
        {
            get { return _controller.TextEditorDisableCodeCompletion; }
            set { _controller.TextEditorDisableCodeCompletion = value; }
        }

        [Browsable(true)]
        [DefaultValue(false)]
        [Category("Editor")]
        [Description("Enables word wrapping in editor.")]
        public bool WordWrap
        {
            get { return _controller.TextEditorWordWrap; }
            set { _controller.TextEditorWordWrap = value; }
        }

        [Browsable(true)]
        [Category("Editor")]
        public string Expression
        {
            get { return _controller.Expression; }
            set { _controller.Expression = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public AstNode ExpressionAST
        {
            set { _controller.ExpressionAST = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public UnionSubQuery ActiveUnionSubQuery
        {
            get { return _controller.ActiveUnionSubQuery; }
            set { _controller.ActiveUnionSubQuery = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public IQueryController Query
        {
            get { return _controller.Query; }
            set { _controller.Query = value; }
        }

        [Browsable(true)]
        [DefaultValue(true)]
        [Category("Editor")]
        [Description("When and you press ENTER, the new line of text is automatically indented to the same tab stop as the line preceding it. ")]
        public bool AutoIndent
        {
            get { return _controller.TextEditorAutoIndent; }
            set { _controller.TextEditorAutoIndent = value; }
        }

        [Browsable(true)]
        [DefaultValue(false)]
        [Category("Editor")]
        [Description("Always show scroll bars.")]
        public bool ScrollBarsAlwaysVisible
        {
            get { return _controller.TextEditorScrollBarsAlwaysVisible; }
            set { _controller.TextEditorScrollBarsAlwaysVisible = value; }
        }

        [Browsable(true)]
        [DefaultValue(4)]
        [Category("Editor")]
        public int TabSize
        {
            get { return _controller.TextEditorTabSize; }
            set { _controller.TextEditorTabSize = value; }
        }

        [Browsable(true)]
        [DefaultValue(ParenthesesHighlighting.HighlightWithColor)]
        [Category("Editor")]
        [Description("Highlight matching parentheses with color or outline.")]
        public ParenthesesHighlighting HighlightMatchingParentheses
        {
            get { return _controller.TextEditorHighlightMatchingParentheses; }
            set { _controller.TextEditorHighlightMatchingParentheses = value; }
        }

        [Browsable(true)]
        [DefaultValue(true)]
        [Category("Editor")]
        [Description("Auto-insert closing parentheses and quotes.")]
        public bool AutoInsertPairs
        {
            get { return _controller.TextEditorAutoInsertPairs; }
            set { _controller.TextEditorAutoInsertPairs = value; }
        }


        [Browsable(true)]
        [DefaultValue(AutocompletedKeywordsCasing.Uppercase)]
        [Category("Editor")]
        [Description("Case of auto-completed SQL keywords.")]
        public AutocompletedKeywordsCasing AutocompletedKeywordsCasing
        {
            get { return _controller.TextEditorKeywordsCasing; }
            set { _controller.TextEditorKeywordsCasing = value; }
        }

        [Browsable(true)]
        [Category("Editor Colors")]
        [Editor("System.Drawing.Design.ColorEditor, System.Drawing.Design", typeof(UITypeEditor))]
        public object BackColor
        {
            get { return (Color)_controller.TextEditorBackColor; }
            set { _controller.TextEditorBackColor = value; }
        }

        [Browsable(true)]
        [DefaultValue(true)]
        [Description("Load object fields on demand when searching through database objects.")]
        public bool LoadFieldsOnSearching
        {
            get { return _controller.LoadFieldsOnSearching; }
            set { _controller.LoadFieldsOnSearching = value; }
        }

        [Browsable(true)]
        [DefaultValue(1)]
        [Category("Editor")]
        [Description("Show suggestion list after count of character typed.")]
        public int ShowSuggestionAfterCharCount
        {
            get { return _controller.TextEditorShowSuggestionAfterCharCount; }
            set { _controller.TextEditorShowSuggestionAfterCharCount = value; }
        }

        [Browsable(true)]
        [TypeConverter(typeof(FlagsEnumConverter))]
        [Category("Editor")]
        [DefaultValue(SuggestionObjectType.All)]
        [Description("Defines entities contained in the editor's suggestion list.")]
        public SuggestionObjectType SuggestionListContent
        {
            get { return _controller.TextEditorSuggestionListContent; }
            set { _controller.TextEditorSuggestionListContent = value; }
        }

        [Browsable(true)]
        [Category("Editor")]
        [DefaultValue(false)]
        [Description("Show database objects on the top of suggestion list before keywords and functions.")]
        public bool KeepMetadataObjectsOnTopOfSuggestionList
        {
            get { return _controller.TextEditorKeepMetadataObjectsOnTopOfSuggestionList; }
            set { _controller.TextEditorKeepMetadataObjectsOnTopOfSuggestionList = value; }
        }

        [Browsable(true)]
        [Category("Editor")]
        [DefaultValue(false)]
        [Description("Load database objects from underlying server before displaying suggestions.")]
        public bool LoadMetadataOnCodeCompletion
        {
            get { return _controller.TextEditorLoadMetadataOnCodeCompletion; }
            set { _controller.TextEditorLoadMetadataOnCodeCompletion = value; }
        }

        [Browsable(true)]
        [Category("Editor")]
        [Editor("System.Drawing.Design.FontEditor, System.Drawing.Design", typeof(UITypeEditor))]
        public object TextEditorFont
        {
            get { return _controller.TextEditorFont; }
            set { _controller.TextEditorFont = value; }
        }

        [Browsable(true)]
        [Category("Editor")]
        public SQLFormattingOptions SQLFormattingOptions
        {
            get { return _controller.SQLFormattingOptions; }
            set { _controller.SQLFormattingOptions = value; }
        }

        [Browsable(true)]
        [Category("Editor")]
        [DefaultValue(IdentQuotation.IfNeed)]
        public IdentQuotation QuoteIdentifiers
        {
            get { return SQLGenerationOptions.QuoteIdentifiers; }
            set
            {
                SQLGenerationOptions.QuoteIdentifiers = value;
                SQLFormattingOptions.QuoteIdentifiers = value;
            }
        }

        [Browsable(true)]
        [Category("Editor")]
        [DefaultValue(true)]
        [Description("If metadata objects has the alt name specified, use it in the suggesion list instead of the real object name.")]
        public bool UseAltNames
        {
            get { return SQLGenerationOptions.UseAltNames; }
            set
            {
                SQLGenerationOptions.UseAltNames = value;
                SQLFormattingOptions.UseAltNames = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public SQLGenerationOptions SQLGenerationOptions
        {
            get { return _controller.SQLGenerationOptions; }
            set { _controller.SQLGenerationOptions = value; }
        }

        [Browsable(true)]
        [Category("Editor Colors")]
        [Description("Normal text color")]
        [Editor("System.Drawing.Design.ColorEditor, System.Drawing.Design", typeof(UITypeEditor))]
        public object TextColor
        {
            get { return _controller.TextEditorTextColor; }
            set { _controller.TextEditorTextColor = value; }
        }

        [Browsable(true)]
        [Category("Editor Colors")]
        [Description("Background color of selected text")]
        [Editor("System.Drawing.Design.ColorEditor, System.Drawing.Design", typeof(UITypeEditor))]
        public object SelectionBackColor
        {
            get { return _controller.TextEditorSelectionBackColor; }
            set { _controller.TextEditorSelectionBackColor = value; }
        }

        [Browsable(true)]
        [Category("Editor Colors")]
        [Description("Selected text color")]
        [Editor("System.Drawing.Design.ColorEditor, System.Drawing.Design", typeof(UITypeEditor))]
        public object SelectionTextColor
        {
            get { return _controller.TextEditorSelectionTextColor; }
            set { _controller.TextEditorSelectionTextColor = value; }
        }

        [Browsable(true)]
        [Category("Editor Colors")]
        [Description("Background color of inactive selected text")]
        [Editor("System.Drawing.Design.ColorEditor, System.Drawing.Design", typeof(UITypeEditor))]
        public object InactiveSelectionBackColor
        {
            get { return _controller.TextEditorInactiveSelectionBackColor; }
            set { _controller.TextEditorInactiveSelectionBackColor = value; }
        }

        [Browsable(true)]
        [Category("Editor Colors")]
        [Description("Color of commentaries")]
        [Editor("System.Drawing.Design.ColorEditor, System.Drawing.Design", typeof(UITypeEditor))]
        public object CommentColor
        {
            get { return _controller.TextEditorCommentColor; }
            set { _controller.TextEditorCommentColor = value; }
        }

        [Browsable(true)]
        [Category("Editor Colors")]
        [Description("Color of quoted string constants")]
        [Editor("System.Drawing.Design.ColorEditor, System.Drawing.Design", typeof(UITypeEditor))]
        public object StringColor
        {
            get { return _controller.TextEditorStringColor; }
            set { _controller.TextEditorStringColor = value; }
        }

        [Browsable(true)]
        [Category("Editor Colors")]
        [Description("Color of numbers")]
        [Editor("System.Drawing.Design.ColorEditor, System.Drawing.Design", typeof(UITypeEditor))]
        public object NumberColor
        {
            get { return _controller.TextEditorNumberColor; }
            set { _controller.TextEditorNumberColor = value; }
        }

        [Browsable(true)]
        [Category("Editor Colors")]
        [Description("Color of keywords")]
        [Editor("System.Drawing.Design.ColorEditor, System.Drawing.Design", typeof(UITypeEditor))]
        public object KeywordColor
        {
            get { return _controller.TextEditorKeywordColor; }
            set { _controller.TextEditorKeywordColor = value; }
        }

        [Browsable(true)]
        [Category("Editor Colors")]
        [Description("Color of functions")]
        [Editor("System.Drawing.Design.ColorEditor, System.Drawing.Design", typeof(UITypeEditor))]
        public object FunctionColor
        {
            get { return _controller.TextEditorFunctionColor; }
            set { _controller.TextEditorFunctionColor = value; }
        }

        public bool FilterFunctions
        {
            get { return cbFilterFunctions.IsChecked.HasValue && cbFilterFunctions.IsChecked.Value; }
        }

        public string FilterTextForFunctions
        {
            get { return TbFilterForFunctions.Text; }
        }

        public IDatabaseSchemaView ObjectsTree
        {
            get { return TreeObjects; }
        }

        public IDatabaseSchemaView QueryObjectsTree
        {
            get { return TreeQueryObjects; }
        }

        public ITreeViewMod FunctionsTree
        {
            get { return TreeFunctions; }
        }

        public bool Visible
        {
            get { return Visibility == Visibility.Visible; }
            set { Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        public View()
        {
            InitializeComponent();

            _controller = new Controller(this);

            var il = new ImageList();

            il.Images.Add(Helpers.GetImageSource(Properties.Resources.field, ImageFormat.Png));		// 0
            il.Images.Add(Helpers.GetImageSource(Properties.Resources.table, ImageFormat.Png));		// 1
            il.Images.Add(Helpers.GetImageSource(Properties.Resources.view, ImageFormat.Png));		// 2
            il.Images.Add(Helpers.GetImageSource(Properties.Resources.procedure, ImageFormat.Png));	// 3
            il.Images.Add(Helpers.GetImageSource(Properties.Resources.synonym, ImageFormat.Png));	// 4
            il.Images.Add(Helpers.GetImageSource(Properties.Resources.function, ImageFormat.Png));	// 5

            SetImageList(il);

            TreeFunctions.MouseDown += TreeFunctions_MouseDown;
            TreeFunctions.MouseUp += TreeFunctions_MouseUp;
            TreeFunctions.MouseMove += TreeFunctions_MouseMove;
            TreeFunctions.KeyDown += TreeFunctions_KeyDown;

            TreeObjects.ValidateItemContextMenu += OnTreeObjectsValidateItemContextMenu;
            TreeQueryObjects.ValidateItemContextMenu += OnQueryObjectTreeValidateItemContextMenu;

            Localize();

            var langProperty = DependencyPropertyDescriptor.FromProperty(LanguageProperty, typeof(View));
            langProperty.AddValueChanged(this, LanguaheChanged);
        }

        private void Localize()
        {
            DockPanelDatabaseShema.Title = Helpers.Localizer.GetString("strDatabaseSchemaPanelTitle",
                Helpers.ConvertLanguageFromNative(Language),
                Constants.strDatabaseSchemaPanelTitle);

            DockPanelSqlContext.Title = Helpers.Localizer.GetString("strSqlContextPanelTitle",
                Helpers.ConvertLanguageFromNative(Language),
                Constants.strSqlContextPanelTitle);
        }

        private void LanguaheChanged(object sender, EventArgs e)
        {
            Localize();
        }

        private void TreeFunctions_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(TreeFunctions);
            if (!(sender is TreeViewItem))
            {
                _dragBoxFromMouseDown = Rect.Empty;
                return;
            }
            var item = (TreeViewItem)sender;

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

            var mousePosition = e.GetPosition(TreeFunctions);

            if (FunctionTreeMouseUp != null)
                FunctionTreeMouseUp(sender,
                    new CMouseEventArgs(Helpers.GetMouseButton(e.ChangedButton), e.ClickCount, (int)mousePosition.X,
                        (int)mousePosition.Y, 0));
        }

        private void TreeFunctions_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(TreeFunctions);
            if (FunctionTreeMouseMove != null)
                FunctionTreeMouseMove(sender, new CMouseEventArgs(Mouse.LeftButton == MouseButtonState.Pressed ? CMouseButtons.Left : CMouseButtons.None, 0, (int)mousePosition.X, (int)mousePosition.Y, 0));
        }

        private void TreeFunctions_KeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(FilterTextForFunctions)) return;

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
                }

                break;
            }
        }

        public void SetImageList(object imageList)
        {
            _imageList = (ImageList)imageList;
        }

        public void AddTextEditor(object textEditor)
        {
            var element = (FrameworkElement)textEditor;
            if (element.Parent is Grid)
                ((Grid)element.Parent).Children.Remove(element);

            PART_content.Children.Add(element);
        }

        public void FillObjectTree(MetadataStructure metadataStructure)
        {
            TreeObjects.MetadataStructure = metadataStructure;
            TreeObjects.InitializeDatabaseSchemaTree();
        }

        public void FillQueryObjectsTree(MetadataStructure metadataStructure)
        {
            TreeQueryObjects.MetadataStructure = metadataStructure;
            TreeQueryObjects.InitializeDatabaseSchemaTree();
        }

        public void FillFunctionsTree(List<NodeData> nodes)
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

        public void FillOperators(List<string> operatorList)
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
            if (OperatorButtonClick != null)
            {
                OperatorButtonClick(sender, e);
            }
        }

        public object GetNodeTag(object node)
        {
            var nodeTree = node as ListBoxItem;
            if (nodeTree == null || nodeTree.DataContext == null) return null;

            return ((TreeViewItemData)nodeTree.DataContext).Tag;
        }

        public string GetNodeText(object node)
        {
            var nodeTree = node as TreeViewItem;
            if (nodeTree == null) return "";

            var template = nodeTree.Header as TreeViewItemData;

            return template != null ? template.Text : "";
        }

        public void LocalizeForm()
        {
            //Subscribed to Language property changed.
        }

        public bool ShouldBeginDrag(int x, int y)
        {
            return (!_dragBoxFromMouseDown.IsEmpty && !_dragBoxFromMouseDown.Contains(x, y));
        }

        public void ShowWaitCursor()
        {
            Cursor = Cursors.Wait;
        }

        public void ResetCursor()
        {
            Cursor = null;
        }

        public CPoint GetScreenPoint(object sender, CPoint point)
        {
            return point;
        }

        public T GetDragObject<T>(object dragObject) where T : class
        {
            var dr = dragObject as DataObject;
            if (dr == null) return null;

            return dr.GetData(typeof(T)) as T;
        }

        private void CheckTreeFilter([NotNull] ListBox tree, string filter)
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

        private void TbFilterForFunctions_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var filter = TbFilterForFunctions.Text;

            if (FilterTextForFunctionsChanged != null)
                FilterTextForFunctionsChanged(TreeFunctions, EventArgs.Empty);

            CheckTreeFilter(TreeFunctions, filter);
        }

        private void OnTreeObjectsValidateItemContextMenu(object sender, MetadataStructureItemMenuEventArgs e)
        {
            var handler = ObjectTreeValidateItemContextMenu;
            if (handler != null) handler(sender, e);
        }

        private void OnQueryObjectTreeValidateItemContextMenu(object sender, MetadataStructureItemMenuEventArgs e)
        {
            var handler = QueryObjectTreeValidateItemContextMenu;
            if (handler != null) handler(this, e);
        }

        private void TreeFunctions_OnSuperMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FunctionTreeDoubleClick != null) FunctionTreeDoubleClick(sender, e);
        }
    }
}

