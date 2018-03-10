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
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.DatabaseSchemaView;
using ActiveQueryBuilder.View.EventHandlers;
using ActiveQueryBuilder.View.EventHandlers.MetadataStructureItems;
using ActiveQueryBuilder.View.ExpressionEditor;
using ActiveQueryBuilder.View.WPF;
using ActiveQueryBuilder.View.WPF.ExpressionEditor;
using ExpressionEditorDemo.Common;
using NodeData = ActiveQueryBuilder.View.ExpressionEditor.NodeData;

namespace ExpressionEditorDemo
{
    public class Controller : IDisposable
    {
        private IQueryController _query;
        private UnionSubQuery _activeUnionSubQuery;
        private IView _view;

        private readonly ISqlTextEditor _sqlTextEditor;

        private bool _loadFieldsOnSearching = true;

        private readonly Dictionary<MetadataStructureItem, object> _mapTreeItems;

        private IView View
        {
            set { SetViewParams(value); }
            get { return _view; }
        }

        private void SetViewParams(IView view)
        {
            _view = view;

            if(_view == null) return;

            View.OperatorButtonClick += View_OperatorButtonClick;
            View.FilterFunctionsChanged += View_FilterFunctionsChanged;
            View.FilterTextForFunctionsChanged += View_FilterTextForFunctionsChanged;
            View.ObjectTreeDoubleClick += View_ObjectTreeDoubleClick;
            View.FunctionTreeDoubleClick += View_FunctionTreeDoubleClick;
            View.FunctionTreeMouseUp += View_FunctionTreeMouseUp;
            View.QueryObjectTreeDoubleClick += View_QueryObjectTreeDoubleClick;
            View.FunctionTreeMouseMove += View_FunctionTreeMouseMove;

            View.AddTextEditor(_sqlTextEditor);

            View.ObjectsTree.SortingType = ObjectsSortingType.NameExceptFields;
            View.QueryObjectsTree.SortingType = ObjectsSortingType.NameExceptFields;

            View.QueryObjectsTree.UseAltNames = true;

            View.ObjectsTree.DefaultExpandLevel = 0;
            View.QueryObjectsTree.DefaultExpandLevel = 0;

            if (Query != null)
            {
                View.ObjectsTree.SQLContext = Query.SQLContext;
                View.QueryObjectsTree.SQLContext = Query.SQLContext;
            }

            View.ObjectTreeValidateItemContextMenu += View_TreeObjectsValidateItemContextMenu;
            View.QueryObjectsTree.ValidateItemContextMenu += QueryObjectsTree_ValidateItemContextMenu;
        }

        private void View_FilterTextForFunctionsChanged(object sender, EventArgs e)
        {
            ReloadFunctions();
        }

        public event ValidateContextMenuEventHandler ValidateContextMenu;

        public IQueryController Query
        {
            get { return _query; }
            set
            {
                if (_query != null)
                {
                    _query.SQLContextChanged -= Query_SQLContextChanged;

                    if (_query.SQLContext != null)
                    {
                        _query.SQLContext.SyntaxProviderChanged -= SQLContext_SyntaxProviderChanged;

                        if (_query.SQLContext.MetadataContainer != null)
                            _query.SQLContext.MetadataContainer.Updated -= MetadataContainer_Updated;
                    }
                }

                _query = value;

                _sqlTextEditor.Query = _query;

                if (View != null)
                {
                    _view.ObjectsTree.SQLContext = _query.SQLContext;
                    _view.QueryObjectsTree.SQLContext = _query.SQLContext;

                    ReloadMetadata();
                    ReloadQueryObjects();
                    ReloadOperators();
                    ReloadFunctions();
                }

                if (_query != null)
                {
                    _query.SQLContextChanged += Query_SQLContextChanged;

                    if (_query.SQLContext != null)
                    {
                        _query.SQLContext.SyntaxProviderChanged += SQLContext_SyntaxProviderChanged;
                        _query.SQLContext.Disposing += SQLContext_Disposing;

                        if (_query.SQLContext.MetadataContainer != null)
                        {
                            _query.SQLContext.MetadataContainer.Updated += MetadataContainer_Updated;
                            _query.SQLContext.MetadataContainer.Disposing += MetadataContainer_Disposing;
                        }

                        if (_query.SQLContext.SyntaxProvider != null)
                        {
                            AutoSyntaxProvider autoSyntaxProvider = _query.SQLContext.SyntaxProvider as AutoSyntaxProvider;
                            SyntaxProvider = autoSyntaxProvider != null ? autoSyntaxProvider.DetectedSyntaxProvider : _query.SQLContext.SyntaxProvider;
                        }
                        else
                        {
                            SyntaxProvider = null;
                        }
                    }
                }
            }
        }

        public BaseSyntaxProvider SyntaxProvider { get; private set; }

        public string Expression
        {
            get { return _sqlTextEditor.Text; }
            set
            { _sqlTextEditor.Text = value; }
        }

        public AstNode ExpressionAST
        {
            set
            {
                var formattingOptions = SQLFormattingOptions ??
                    new SQLFormattingOptions()
                    {
                        UseAltNames = SQLGenerationOptions.UseAltNames,
                        QuoteIdentifiers = SQLGenerationOptions.QuoteIdentifiers,
                    };
                Expression = value != null && _query != null ? value.GetSQL(formattingOptions) : string.Empty;
            }
        }

        public UnionSubQuery ActiveUnionSubQuery
        {
            get { return _activeUnionSubQuery; }
            set
            {
                _activeUnionSubQuery = value; 
                ReloadQueryObjects();
            }
        }

        public object TextEditorFont
        {
            get { return _sqlTextEditor.Font; }
            set { _sqlTextEditor.Font = value; }
        }

        public bool TextEditorAutoIndent
        {
            get { return _sqlTextEditor.AutoIndent; }
            set { _sqlTextEditor.AutoIndent = value; }
        }

        public bool TextEditorScrollBarsAlwaysVisible
        {
            get { return _sqlTextEditor.ScrollBarsAlwaysVisible; }
            set { _sqlTextEditor.ScrollBarsAlwaysVisible = value; }
        }

        public int TextEditorTabSize
        {
            get { return _sqlTextEditor.TabSize; }
            set { _sqlTextEditor.TabSize = value; }
        }

        public ParenthesesHighlighting TextEditorHighlightMatchingParentheses
        {
            get { return _sqlTextEditor.HighlightMatchingParentheses; }
            set { _sqlTextEditor.HighlightMatchingParentheses = value; }
        }

        public bool TextEditorAutoInsertPairs
        {
            get { return _sqlTextEditor.AutoInsertPairs; }
            set { _sqlTextEditor.AutoInsertPairs = value; }
        }

        public AutocompletedKeywordsCasing TextEditorKeywordsCasing
        {
            get { return _sqlTextEditor.KeywordsCasing; }
            set { _sqlTextEditor.KeywordsCasing = value; }
        }

        public bool TextEditorDisableCodeCompletion
        {
            get { return _sqlTextEditor.DisableCodeCompletion; }
            set { _sqlTextEditor.DisableCodeCompletion = value; }
        }

        public bool TextEditorWordWrap
        {
            get { return _sqlTextEditor.WordWrap; }
            set { _sqlTextEditor.WordWrap = value; }
        }

        public object TextEditorBackColor
        {
            get { return _sqlTextEditor.BackColor; }
            set { _sqlTextEditor.BackColor = value; }
        }

        public object TextEditorTextColor
        {
            get { return _sqlTextEditor.TextColor; }
            set { _sqlTextEditor.TextColor = value; }
        }

        public object TextEditorSelectionBackColor
        {
            get { return _sqlTextEditor.SelectionBackColor; }
            set { _sqlTextEditor.SelectionBackColor = value; }
        }

        public object TextEditorSelectionTextColor
        {
            get { return _sqlTextEditor.SelectionTextColor; }
            set { _sqlTextEditor.SelectionTextColor = value; }
        }

        public object TextEditorInactiveSelectionBackColor
        {
            get { return _sqlTextEditor.InactiveSelectionBackColor; }
            set { _sqlTextEditor.InactiveSelectionBackColor = value; }
        }

        public object TextEditorCommentColor
        {
            get { return _sqlTextEditor.CommentColor; }
            set { _sqlTextEditor.CommentColor = value; }
        }

        public object TextEditorStringColor
        {
            get { return _sqlTextEditor.StringColor; }
            set { _sqlTextEditor.StringColor = value; }
        }

        public object TextEditorNumberColor
        {
            get { return _sqlTextEditor.NumberColor; }
            set { _sqlTextEditor.NumberColor = value; }
        }

        public object TextEditorKeywordColor
        {
            get { return _sqlTextEditor.KeywordColor; }
            set { _sqlTextEditor.KeywordColor = value; }
        }

        public object TextEditorFunctionColor
        {
            get { return _sqlTextEditor.FunctionColor; }
            set { _sqlTextEditor.FunctionColor = value; }
        }

        public int TextEditorShowSuggestionAfterCharCount
        {
            get { return _sqlTextEditor.ShowSuggestionAfterCharCount; }
            set { _sqlTextEditor.ShowSuggestionAfterCharCount = value; }
        }

        public SuggestionObjectType TextEditorSuggestionListContent
        {
            get { return _sqlTextEditor.SuggestionListContent; }
            set { _sqlTextEditor.SuggestionListContent = value; }
        }

        public bool TextEditorKeepMetadataObjectsOnTopOfSuggestionList
        {
            get { return _sqlTextEditor.KeepMetadataObjectsOnTopOfSuggestionList; }
            set { _sqlTextEditor.KeepMetadataObjectsOnTopOfSuggestionList = value; }
        }

        public bool TextEditorLoadMetadataOnCodeCompletion
        {
            get { return _sqlTextEditor.LoadMetadataOnCodeCompletion; }
            set { _sqlTextEditor.LoadMetadataOnCodeCompletion = value; }
        }

        public event AddCustomKeywordsEventHandler TextEditorCustomizeKeywords
        {
            add { _sqlTextEditor.CustomizeKeywords += value; }
            remove { _sqlTextEditor.CustomizeKeywords -= value; }
        }

        public event AddCustomFunctionsEventHandler TextEditorCustomizeFunctions
        {
            add
            {
                _sqlTextEditor.CustomizeFunctions += value;

                if (View != null && View.Visible)
                    ReloadFunctions();

                _sqlTextEditor.CustomizeFunctions -= UpdateFunctionList;
                _sqlTextEditor.CustomizeFunctions += UpdateFunctionList;
            }
            remove { _sqlTextEditor.CustomizeFunctions -= value; }
        }

        public bool LoadFieldsOnSearching
        {
            get { return _loadFieldsOnSearching; }
            set { _loadFieldsOnSearching = value; }
        }

        public SQLGenerationOptions SQLGenerationOptions
        {
            get { return _sqlTextEditor.SQLGenerationOptions; }
            set { _sqlTextEditor.SQLGenerationOptions = value; }
        }

        public SQLFormattingOptions SQLFormattingOptions { get; set; }

        public Controller(IView expressionEditor)
        {
            _sqlTextEditor = new SqlTextEditor();

            View = expressionEditor;
            
            _sqlTextEditor.Dock = CDockStyle.Fill;
            _sqlTextEditor.Query = Query;
            _sqlTextEditor.AcceptTabs = true;
            _sqlTextEditor.TabIndex = 0;
            _sqlTextEditor.HighlightMatchingParentheses = ParenthesesHighlighting.HighlightWithColor;
            _sqlTextEditor.ValidateContextMenu += TextEditor_ValidateContextMenu;

            _sqlTextEditor.PreProcessDragDrop += _sqlTextEditor_PreProcessDragDrop;

            _mapTreeItems = new Dictionary<MetadataStructureItem, object>();
        }

        private void _sqlTextEditor_PreProcessDragDrop(object source, DragEventArgs eventArgs, ref bool handled)
        {
            var metadataDragObject = View.GetDragObject<MetadataDragObject>(eventArgs.Data);

            if (metadataDragObject != null)
            {
                foreach (var metadataStructureItem in metadataDragObject.MetadataStructureItems)
                    InsertTextIntoEditor(GetNameFromMetadataStructureItem(metadataStructureItem));
            }
            else
            {
                var text = View.GetDragObject<string>(eventArgs.Data);

                if (!string.IsNullOrEmpty(text))
                    InsertTextIntoEditor(text);
            }

            handled = true;
        }

        public void ReloadMetadata()
        {
            View.ShowWaitCursor();

            try
            {
                var metadataStructure = new MetadataStructure
                {
                    Options =
                    {
                        GroupBySchemas = true,
                        GroupByTypes = true
                    },
                    MetadataItem = Query.SQLContext.MetadataContainer
                };
                metadataStructure.LoadChildItems();

                View.FillObjectTree(metadataStructure);
            }
            finally
            {
                View.ResetCursor();
            }
        }

        public void ReloadQueryObjects()
        {
            _mapTreeItems.Clear();

            if(Query == null || ActiveUnionSubQuery == null) return;

            var metadataStructure = new MetadataStructure { AllowChildAutoItems = false };

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

            View.FillQueryObjectsTree(metadataStructure);
        }

        public void ReloadOperators()
        {
            List<string> operatorList = new List<string>();

            if (SyntaxProvider != null)
            {
                SyntaxProvider.GetComparisonOperators(operatorList);
            }

            View.FillOperators(operatorList);
        }

        public void ReloadFunctions()
        {
            bool filtering = View.FilterFunctions;
            string filter = View.FilterTextForFunctions;
            List<NodeData> newNodes = new List<NodeData>();

            if (Query != null)
            {
                foreach (KeyValuePair<string, AdvancedKeywordInfo> kvp in _sqlTextEditor.SuggestionObjects.Functions)
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

            View.FillFunctionsTree(newNodes);
        }

        public string GetNameMetadataItem(MetadataItem metadataItem, bool isFullName)
        {
            if (metadataItem == null)
                return "";

            return metadataItem.GetQualifiedNameSQL(isFullName ? null : metadataItem.Parent, SQLGenerationOptions, ObjectPrefixSkipping.SkipAll);

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

            int start = _sqlTextEditor.SelectionStart;

            _sqlTextEditor.ReplaceSelection(text);

            if (cursor != -1)
                _sqlTextEditor.SelectionStart = start + cursor;
        }

        public void OnValidateContextMenu(object source, ICustomContextMenu menu)
        {
            if (ValidateContextMenu != null)
            {
                ValidateContextMenu(this, menu);
            }
        }

        public void Dispose()
        {
            SyntaxProvider = null;

            _sqlTextEditor.ValidateContextMenu -= TextEditor_ValidateContextMenu;
            _sqlTextEditor.CustomizeFunctions -= UpdateFunctionList;

            if (View != null)
            {
                DisposeView();
                _sqlTextEditor.Dispose();
            }

            if (_query != null)
            {
                _query.SQLContextChanged -= Query_SQLContextChanged;

                if (_query.SQLContext != null)
                {
                    _query.SQLContext.SyntaxProviderChanged -= SQLContext_SyntaxProviderChanged;

                    if (_query.SQLContext.MetadataContainer != null)
                        _query.SQLContext.MetadataContainer.Updated -= MetadataContainer_Updated;
                }
            }

            _activeUnionSubQuery = null;
        }

        private void DisposeView()
        {

            View.OperatorButtonClick -= View_OperatorButtonClick;
            View.ObjectTreeDoubleClick -= View_ObjectTreeDoubleClick;
            View.FunctionTreeDoubleClick -= View_FunctionTreeDoubleClick;
            View.FunctionTreeMouseUp -= View_FunctionTreeMouseUp;
            View.QueryObjectTreeDoubleClick -= View_QueryObjectTreeDoubleClick;
            View.FunctionTreeMouseMove -= View_FunctionTreeMouseMove;
            View.FilterFunctionsChanged -= View_FilterFunctionsChanged;
        }

        void QueryObjectsTree_ValidateItemContextMenu(object sender, MetadataStructureItemMenuEventArgs e)
        {
            var metadataItem = e.MetadataStructureItem.MetadataItem;
            var mappedObject = _mapTreeItems.ContainsKey(e.MetadataStructureItem) ? _mapTreeItems[e.MetadataStructureItem] : null;

            if (metadataItem == null)
            {
                var dataSource = mappedObject as DataSource;
                if (dataSource != null)
                {
                    if (dataSource.AliasAST != null)
                    {
                        e.Menu.AddItem("Insert Alias", MenuItem_Clicked, false, true, null, dataSource.NameInQuery);
                        e.Menu.AddItem("Insert Name", MenuItem_Clicked, false, true, null,
                            dataSource.GetObjectNameInQuery(SQLGenerationOptions));
                    }
                    else
                    {
                        e.Menu.AddItem("Insert Name", MenuItem_Clicked, false, true, null, dataSource.NameInQuery);
                    }
                }

                var field = mappedObject as MetadataField;

                if (field != null)
                {
                    e.Menu.AddItem("Insert Name", MenuItem_Clicked, false, true, null, field.Name);
                }
            }
            else
            {
                if (mappedObject == null)
                {
                    e.Menu.AddItem("Insert Name", MenuItem_Clicked, false, true, null, GetNameMetadataItem(metadataItem, false));
                }
                else
                {
                    var dataSource = mappedObject as DataSource;
                    if (dataSource != null)
                    {
                        if (dataSource.AliasAST != null)
                        {
                            e.Menu.AddItem("Insert Alias", MenuItem_Clicked, false, true, null, dataSource.NameInQuery);
                        }

                        e.Menu.AddItem("Insert Name", MenuItem_Clicked, false, true, null, GetNameMetadataItem(metadataItem, false));
                    }

                    var field = mappedObject as MetadataField;

                    if (field != null)
                    {
                        e.Menu.AddItem("Insert Name", MenuItem_Clicked, false, true, null, GetNameMetadataItem(field, false));
                    }
                }
            }

            OnValidateContextMenu(this, e.Menu);
        }

        private void View_TreeObjectsValidateItemContextMenu(object sender, MetadataStructureItemMenuEventArgs e)
        {
            var metadataItem = e.MetadataStructureItem.MetadataItem;

            if (metadataItem == null) return;

            e.Menu.AddItem("Insert Name", MenuItem_Clicked, false, true, null, GetNameMetadataItem(metadataItem, false));
            e.Menu.AddItem("Insert Full Name", MenuItem_Clicked, false, true, null, GetNameMetadataItem(metadataItem, true));

            OnValidateContextMenu(this, e.Menu);
        }

        private void View_FilterFunctionsChanged(object sender, EventArgs e)
        {
            ReloadFunctions();
        }

        private void Query_SQLContextChanged(object sender, EventArgs eventArgs)
        {
            QueryRoot query = (QueryRoot)sender;

            ReloadMetadata();
            ReloadQueryObjects();
            ReloadOperators();
            ReloadFunctions();

            if (query.SQLContext != null)
            {
                query.SQLContext.MetadataContainer.Updated += MetadataContainer_Updated;
                query.SQLContext.MetadataContainer.Disposing += MetadataContainer_Disposing;
                query.SQLContext.SyntaxProviderChanged += SQLContext_SyntaxProviderChanged;
                query.SQLContext.Disposing += SQLContext_Disposing;
            }
        }

        private void TextEditor_ValidateContextMenu(object source, ICustomContextMenu menu)
        {
            OnValidateContextMenu(source, menu);
        }

        private void SQLContext_SyntaxProviderChanged(object sender, EventArgs e)
        {
            if (Query != null && !Query.IsDisposing)
            {
                AutoSyntaxProvider autoSyntaxProvider = _query.SQLContext.SyntaxProvider as AutoSyntaxProvider;

                SyntaxProvider = autoSyntaxProvider != null ? autoSyntaxProvider.DetectedSyntaxProvider : _query.SQLContext.SyntaxProvider;

                if (View == null || !View.Visible) return;

                ReloadOperators();
                ReloadFunctions();
            }
            else
            {
                SyntaxProvider = null;
            }
        }

        private void SQLContext_Disposing(object sender, EventArgs e)
        {
            ((SQLContext)sender).SyntaxProviderChanged -= SQLContext_SyntaxProviderChanged;
            ((SQLContext)sender).Disposing -= SQLContext_Disposing;
        }

        private void MetadataContainer_Updated(object sender, EventArgs e)
        {
            if (Query != null && !Query.IsDisposing)
            {
                if (View != null && View.Visible)
                    ReloadMetadata();
            }
        }

        private void MetadataContainer_Disposing(object sender, EventArgs e)
        {
            ((MetadataContainer)sender).Updated -= MetadataContainer_Updated;
            ((MetadataContainer)sender).Disposing -= MetadataContainer_Disposing;
        }

        private void View_OperatorButtonClick(object sender, EventArgs e)
        {
            InsertTextIntoEditor(((ISimpleButton)sender).Text);
        }

        private void View_ObjectTreeDoubleClick(object sender, MetadataStructureItem metadataStructureItem)
        {
            var name = GetNameMetadataItem(metadataStructureItem.MetadataItem, true);
            InsertTextIntoEditor(name);
        }

        private void View_FunctionTreeDoubleClick(object sender, EventArgs e)
        {
            var tag = View.GetNodeTag(sender);

            if (tag != null)
            {
                InsertTextIntoEditor(((AdvancedKeywordInfo)tag).Template);
            }
        }

        private void View_FunctionTreeMouseUp(object sender, CMouseEventArgs e)
        {
            if (e.Button != CMouseButtons.Right) return;

            ICustomContextMenu menu = ControlFactory.Instance.GetCustomContextMenu(); ;

            object tag = View.GetNodeTag(sender);

            var aki = tag as AdvancedKeywordInfo;

            if (aki != null)
                menu.AddItem("Insert Function", MenuItem_Clicked, false, true, null, aki.Template);

            OnValidateContextMenu(this, menu);

            CPoint point = View.GetScreenPoint(sender, e.Location);

            if (menu.ItemCount > 0)
                menu.Show(point.X, point.Y);
        }

        private void MenuItem_Clicked(object sender, EventArgs e)
        {
            InsertTextIntoEditor((string)((ICustomMenuItem)sender).Tag);
        }

        private void View_QueryObjectTreeDoubleClick(object sender, MetadataStructureItem structureItem)
        {
            InsertTextIntoEditor(GetNameFromMetadataStructureItem(structureItem));
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

                var nameObject = mapItem != null && mapItem.AliasAST != null
                    ? mapItem.NameInQuery
                    : GetNameMetadataItem(structureItem.MetadataItem.Parent, mapItem == null);

                insertString = string.Format("{0}.{1}", nameObject, nameField);
            }
            else
            {
                var mapItem =
                    (_mapTreeItems.ContainsKey(structureItem) ? _mapTreeItems[structureItem] : null) as DataSource;

                insertString = mapItem != null && mapItem.AliasAST != null
                    ? mapItem.NameInQuery
                    : GetNameMetadataItem(structureItem.MetadataItem, mapItem == null);
            }

            return insertString;
        }

        private void View_FunctionTreeMouseMove(object sender, CMouseEventArgs e)
        {
            var tag = View.GetNodeTag(sender);

            if (tag == null) return;

            if ((e.Button & CMouseButtons.Left) != CMouseButtons.Left) return;

            if (!View.ShouldBeginDrag(e.X, e.Y)) return;

            var text = ((AdvancedKeywordInfo)tag).Template;

            if (string.IsNullOrEmpty(text)) return;

            View.FunctionsTree.DoDragDrop(text, CDragDropEffects.Copy);
        }

        private void UpdateFunctionList(Dictionary<string, AdvancedKeywordInfo> functions)
        {
            if (View != null && View.Visible)
                ReloadFunctions();
        }
    }
}
