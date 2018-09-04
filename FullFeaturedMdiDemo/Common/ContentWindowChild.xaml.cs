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
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.QueryTransformer;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.WPF;
using ActiveQueryBuilder.View.WPF.ExpressionEditor;
using ActiveQueryBuilder.View.WPF.QueryView;

namespace FullFeaturedMdiDemo.Common
{
    /// <summary>
    /// Interaction logic for ContentWindowChild.xaml
    /// </summary>
    public partial class ContentWindowChild
    {
        public event EventHandler SaveQueryEvent;
        public event EventHandler SaveAsInFileEvent;
        public event EventHandler SaveAsNewUserQueryEvent;

        private readonly QueryTransformer _transformerSql;
        private readonly Timer _timerStartingExecuteSql;

        private string _sql;

        public BehaviorOptions BehaviorOptions
        {
            get { return SqlQuery.BehaviorOptions; }
            set { SqlQuery.BehaviorOptions = value; }
        }

        public DataSourceOptions DataSourceOptions
        {
            get { return (DataSourceOptions)DPaneControl.DataSourceOptions; }
            set { DPaneControl.DataSourceOptions = value; }
        }

        public DesignPaneOptions DesignPaneOptions
        {
            get { return DPaneControl.Options; }
            set { DPaneControl.Options = value; }
        }

        public QueryNavBarOptions QueryNavBarOptions
        {
            get { return NavigationBar.Options; }
            set { NavigationBar.Options = value; }
        }

        public MetadataLoadingOptions MetadataLoadingOptions
        {
            get { return SqlContext.LoadingOptions; }
            set { SqlContext.LoadingOptions = value; }
        }

        public MetadataStructureOptions MetadataStructureOptions
        {
            get { return SqlContext.MetadataStructureOptions; }
            set { SqlContext.MetadataStructureOptions = value; }
        }

        /*
        public AddObjectDialogOptions AddObjectDialogOptions
        {
            get { return QueryView; }
            set { QueryView.AddObjectDialog.Options = value; }
        }*/

        public UserInterfaceOptions UserInterfaceOptions
        {
            get { return QView.UserInterfaceOptions; }
            set { QView.UserInterfaceOptions = value; }
        }

        public ExpressionEditorOptions ExpressionEditorOptions
        {
            get { return ExpressionEditor.Options; }
            set { ExpressionEditor.Options = value; }
        }

        public TextEditorOptions TextEditorOptions
        {
            get { return BoxSql.Options; }
            set
            {
                ExpressionEditor.TextEditorOptions = value;
                BoxSql.Options = value;
                BoxSqlCurrentSubQuery.Options = value;
            }
        }

        public SqlTextEditorOptions TextEditorSqlOptions
        {
            get { return BoxSql.SqlOptions; }
            set
            {
                ExpressionEditor.TextEditorSqlOptions = value;
                BoxSql.SqlOptions = value;
                BoxSqlCurrentSubQuery.SqlOptions = value;
            }
        }

        public VisualOptions VisualOptions
        {
            get { return DockManager.Options; }
            set { DockManager.Options = value; }
        }

        public QueryColumnListOptions QueryColumnListOptions
        {
            get { return ColumnListControl.Options; }
            set { ColumnListControl.Options = value; }
        }

        public bool IsModified
        {
            set;
            get;
        }

        public MetadataStructureItem UserMetadataStructureItem { set; get; }
        public string FileSourceUrl { set; get; }

        public SQLContext SqlContext { get; private set; }


        public string QueryText
        {
            get { return SqlQuery.SQL; }
            set
            {
                SqlQuery.SQL = value;
                _sql = value;
                IsModified = false;
                UpdateStateButtons();
            }
        }

        private SQLFormattingOptions _sqlFormattingOptions;
        public SQLFormattingOptions SqlFormattingOptions
        {
            get { return _sqlFormattingOptions; }
            set
            {
                if (_sqlFormattingOptions != null)
                    _sqlFormattingOptions.Updated -= SqlFormattingOptionsOnUpdated;

                _sqlFormattingOptions = value;

                if (_sqlFormattingOptions != null)
                    _sqlFormattingOptions.Updated += SqlFormattingOptionsOnUpdated;
                CBuilder.QueryTransformer.SQLGenerationOptions = value;
            }
        }

        private void SqlFormattingOptionsOnUpdated(object sender, EventArgs eventArgs)
        {
            BoxSql.Text = FormattedQueryText;
        }

        public SQLGenerationOptions SqlGenerationOptions
        {
            get { return QueryView.SQLGenerationOptions; }
            set { QueryView.SQLGenerationOptions = value; }
        }

        public QueryView QueryView
        {
            get { return QView; }
        }

        public bool IsNeedClose { set; get; }

        public SQLQuery SqlQuery { private set; get; }

        public Helpers.SourceType SqlSourceType { set; get; }

        public string FormattedQueryText
        {
            get
            {
                return !SqlQuery.QueryRoot.SleepMode ?
                    FormattedSQLBuilder.GetSQL(SqlQuery.QueryRoot, SqlFormattingOptions) :
                    SqlQuery.SQL;
            }
        }

        public ContentWindowChild(SQLContext sqlContext)
        {
            Init();

            SqlContext = sqlContext;

            SqlSourceType = Helpers.SourceType.New;

            SqlQuery = new SQLQuery(SqlContext);

            SqlQuery.SQLUpdated += SqlQuery_SQLUpdated;
            SqlQuery.QueryRoot.AllowSleepMode = true;
            SqlQuery.QueryAwake += SqlQueryOnQueryAwake;
            SqlQuery.SleepModeChanged += SqlQuery_SleepModeChanged;
            NavigationBar.QueryView = QueryView;
            QueryView.Query = SqlQuery;

            QueryView.ActiveUnionSubQueryChanged += QueryView_ActiveUnionSubQueryChanged;

            BoxSql.Query = SqlQuery;
            BoxSqlCurrentSubQuery.Query = SqlQuery;
            BoxSqlCurrentSubQuery.ActiveUnionSubQuery = QueryView.ActiveUnionSubQuery;

            QueryView.ActiveUnionSubQueryChanged += delegate
            {
                BoxSqlCurrentSubQuery.ActiveUnionSubQuery = QueryView.ActiveUnionSubQuery;
            };

            _transformerSql = new QueryTransformer();

            _timerStartingExecuteSql = new Timer(TimerStartingExecuteSql_Elapsed);

            CBuilder.QueryTransformer = new QueryTransformer
            {
                Query = SqlQuery
            };

            // Options to present the formatted SQL query text to end-user
            // Use names of virtual objects, do not replace them with appropriate derived tables
            SqlFormattingOptions = new SQLFormattingOptions { ExpandVirtualObjects = false };

            // Options to generate the SQL query text for execution against a database server
            // Replace virtual objects with derived tables
            SqlGenerationOptions = new SQLGenerationOptions { ExpandVirtualObjects = true };

            NavigationBar.QueryView = QueryView;
            NavigationBar.Query = SqlQuery;

            CBuilder.QueryTransformer.SQLUpdated += QueryTransformer_SQLUpdated;

            DataGridResult.QueryTransformer = CBuilder.QueryTransformer;
            DataGridResult.SqlQuery = SqlQuery;

            // The pagination panel is displayed if the current SyntaxProvider has support for pagination
            PaginationPanel.Visibility = (CBuilder.QueryTransformer.IsSupportLimitCount ||
                                          CBuilder.QueryTransformer.IsSupportLimitOffset) && SqlContext.SyntaxProvider != null
                ? Visibility.Visible
                : Visibility.Collapsed;


            PaginationPanel.IsSupportLimitCount = CBuilder.QueryTransformer.IsSupportLimitCount;
            PaginationPanel.IsSupportLimitOffset = CBuilder.QueryTransformer.IsSupportLimitOffset;

            UpdateStateButtons();
        }

        private void QueryView_ActiveUnionSubQueryChanged(object sender, EventArgs e)
        {
            SetSqlTextCurrentSubQuery();
        }

        private void SqlQuery_SQLUpdated(object sender, EventArgs e)
        {
            IsModified = _sql != QueryText;

            BoxSql.Text = FormattedQueryText;

            SetSqlTextCurrentSubQuery();

            UpdateStateButtons();
            CheckParameters();

            if (!TabItemFastResult.IsSelected || CheckBoxAutoRefreash.IsChecked == false) return;

            _timerStartingExecuteSql.Change(600, Timeout.Infinite);            
        }

        private void CheckParameters()
        {
            if (Helpers.CheckParameters(SqlContext.MetadataProvider, SqlContext.SyntaxProvider, SqlQuery.QueryParameters))
                HideParametersErrorPanel();
            else
            {
                var acceptableFormats =
                    Helpers.GetAcceptableParametersFormats(SqlContext.MetadataProvider, SqlContext.SyntaxProvider);
                ShowParametersErrorPanel(acceptableFormats);
            }
        }

        private void ShowParametersErrorPanel(List<string> acceptableFormats)
        {
            var formats = acceptableFormats.Select(x =>
            {
                var s = x.Replace("n", "<number>");
                return s.Replace("s", "<name>");
            });

            lbParamsError.Text = "Unsupported parameter notation detected. For this type of connection and database server use " + string.Join(", ", formats);
            pnlParamsError.Visibility = Visibility.Visible;
        }

        private void HideParametersErrorPanel()
        {
            pnlParamsError.Visibility = Visibility.Collapsed;
        }

        public void OpenExecuteTab()
        {
            TabItemData.IsSelected = true;
        }

        public void ShowQueryStatistics()
        {
            var qs = QueryView.Query.QueryStatistics;

            var stats = "Used Objects (" + qs.UsedDatabaseObjects.Count + "):\r\n";
            stats = qs.UsedDatabaseObjects.Aggregate(stats,
                (current, t) => current + ("\r\n" + t.ObjectName.QualifiedName));

            stats += "\r\n\r\n" + "Used Columns (" + qs.UsedDatabaseObjectFields.Count + "):\r\n";
            stats = qs.UsedDatabaseObjectFields.Aggregate(stats,
                (current, t) => current + ("\r\n" + t.ObjectName.QualifiedName));

            stats += "\r\n\r\n" + "Output Expressions (" + qs.OutputColumns.Count + "):\r\n";
            stats = qs.OutputColumns.Aggregate(stats, (current, t) => current + ("\r\n" + t.Expression));

            var f = new QueryStatisticsWindow { textBox = { Text = stats } };

            f.ShowDialog();
        }

        public bool CanAddObject()
        {
            return QueryView.AddObjectDialog != null;
        }

        public bool CanShowProperties()
        {
            return QueryView.ActiveUnionSubQuery != null;
        }

        public bool CanAddUnionSubQuery()
        {
            if (SqlContext.SyntaxProvider == null) return false;

            if (QueryView.ActiveUnionSubQuery != null && QueryView.ActiveUnionSubQuery.QueryRoot.IsSubQuery)
            {
                return SqlContext.SyntaxProvider.IsSupportSubQueryUnions();
            }

            return SqlContext.SyntaxProvider.IsSupportUnions();
        }

        public bool CanCopyUnionSubQuery()
        {
            return CanAddUnionSubQuery();
        }

        public bool CanAddDerivedTable()
        {
            if (SqlContext.SyntaxProvider == null) return false;

            if (QueryView.ActiveUnionSubQuery != null && QueryView.ActiveUnionSubQuery.QueryRoot.IsMainQuery)
            {
                return SqlContext.SyntaxProvider.IsSupportDerivedTables();
            }

            return SqlContext.SyntaxProvider.IsSupportSubQueryDerivedTables();
        }

        public bool CanCopy()
        {
            if (!GetCurrentEditor().IsFocused) return false;
            return GetCurrentEditor().SelectionLength > 0;
        }

        public bool CanCut()
        {
            if (!GetCurrentEditor().IsFocused) return false;

            return !string.IsNullOrEmpty(GetCurrentEditor().SelectedText);
        }

        public bool CanPaste()
        {
            return GetCurrentEditor().IsFocused && Clipboard.ContainsText();
        }

        public bool CanUndo()
        {
            return GetCurrentEditor().IsFocused && GetCurrentEditor().CanUndo;
        }

        public bool CanRedo()
        {
            return GetCurrentEditor().IsFocused && GetCurrentEditor().CanRedo;
        }

        public bool CanSelectAll()
        {
            return GetCurrentEditor().IsFocused && GetCurrentEditor().Text.Length > 0;
        }

        public void Undo()
        {
            GetCurrentEditor().Undo();
        }

        public void Redo()
        {
            GetCurrentEditor().Undo();
        }

        public void Cut()
        {
            GetCurrentEditor().Cut();
        }

        public void Copy()
        {
            GetCurrentEditor().Copy();
        }

        public void Paste()
        {
            GetCurrentEditor().Paste();
        }

        public void SelectAll()
        {
            GetCurrentEditor().SelectAll();
        }

        private SqlTextEditor GetCurrentEditor()
        {
            if (TabItemQueryText.IsSelected) return BoxSql;

            if (TabItemQueryText.IsSelected) return BoxSqlCurrentSubQuery;


            return BoxSql;
        }

        public void AddDerivedTable()
        {
            using (new UpdateRegion(QueryView.ActiveUnionSubQuery.FromClause))
            {
                var sqlContext = QueryView.ActiveUnionSubQuery.SQLContext;

                var fq = new SQLFromQuery(sqlContext)
                {
                    Alias = new SQLAliasObjectAlias(sqlContext)
                    {
                        Alias = QueryView.ActiveUnionSubQuery.QueryRoot.CreateUniqueSubQueryName()
                    },
                    SubQuery = new SQLSubSelectStatement(sqlContext)
                };

                var sqse = new SQLSubQuerySelectExpression(sqlContext);
                fq.SubQuery.Add(sqse);
                sqse.SelectItems = new SQLSelectItems(sqlContext);
                sqse.From = new SQLFromClause(sqlContext);

                NavigationBar.Query.AddObject(QueryView.ActiveUnionSubQuery, fq, typeof(DataSourceQuery));
            }
        }

        public void CopyUnionSubQuery()
        {
            // add an empty UnionSubQuery
            var usq = QueryView.ActiveUnionSubQuery.ParentGroup.Add();

            // copy the content of existing union sub-query to a new one
            var usqAst = QueryView.ActiveUnionSubQuery.ResultQueryAST;
            usqAst.RestoreColumnPrefixRecursive(true);

            var lCte = new List<SQLWithClauseItem>();
            var lFromObj = new List<SQLFromSource>();
            QueryView.ActiveUnionSubQuery.GatherPrepareAndFixupContext(lCte, lFromObj, false);
            usqAst.PrepareAndFixupRecursive(lCte, lFromObj);

            usq.LoadFromAST(usqAst);
            QueryView.ActiveUnionSubQuery = usq;
        }

        public void AddUnionSubQuery()
        {
            QueryView.ActiveUnionSubQuery = QueryView.ActiveUnionSubQuery.ParentGroup.Add();
        }

        public void PropertiesQuery()
        {
            QueryView.ShowActiveUnionSubQueryProperties();
        }

        public void AddObject()
        {
            if (QueryView.AddObjectDialog != null)
                QueryView.AddObjectDialog.ShowModal();
        }

        private void BoxSql_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                // Update the query builder with manually edited query text:
                SqlQuery.SQL = BoxSql.Text;

                // Hide error banner if any
                ErrorBox.Message = string.Empty;
            }
            catch (SQLParsingException ex)
            {
                // Show banner with error text
                ErrorBox.Message = ex.Message;
            }
        }


        private void TimerStartingExecuteSql_Elapsed(object state)
        {
            Dispatcher.BeginInvoke((Action)FillFastResult);
        }

        private static void SqlQueryOnQueryAwake(QueryRoot sender, ref bool abort)
        {
            if (MessageBox.Show(
                    "You had typed something that is not a SELECT statement in the text editor and continued with visual query building." +
                    "Whatever the text in the editor is, it will be replaced with the SQL generated by the component. Is it right?",
                    "Active Query Builder .NET Demo", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                abort = true;
            }
        }

        private void SqlQuery_SleepModeChanged(object sender, EventArgs e)
        {
            BorderSleepMode.Visibility = SqlQuery.SleepMode ? Visibility.Visible : Visibility.Collapsed;
            TabItemFastResult.Visibility = SqlQuery.SleepMode ? Visibility.Collapsed : Visibility.Visible;
            TabItemCurrentSubQuery.Visibility = SqlQuery.SleepMode ? Visibility.Collapsed : Visibility.Visible;
            TabItemData.IsEnabled = !SqlQuery.SleepMode;
        }

        private void UpdateStateButtons()
        {
            var value = SqlQuery != null && SqlQuery.SQL.Length > 0;

            ButtonSave.IsEnabled = value && IsModified;
            ButtonSaveCurrentSubQuery.IsEnabled = value && IsModified;
            ButtonSaveToFileAs.IsEnabled = value;
        }

        public ContentWindowChild()
        {
            Init();
        }

        private void Init()
        {
            InitializeComponent();

            Loaded += ContentWindowChild_Loaded;
            var langProperty = DependencyPropertyDescriptor.FromProperty(LanguageProperty, typeof(ContentWindowChild));
            langProperty.AddValueChanged(this, LanguageChanged);
        }

        private void QueryTransformer_SQLUpdated(object sender, EventArgs e)
        {
            // Handle the event raised by Query Transformer object that the text of SQL query is changed
            // update the text box
            if (DataGridResult.QueryTransformer == null || !TabItemData.IsSelected ||
                BoxSqlTransformer.Text == DataGridResult.QueryTransformer.SQL) return;

            BoxSqlTransformer.Text = DataGridResult.QueryTransformer.SQL;

            DataGridResult.FillDataGrid(DataGridResult.QueryTransformer.SQL);
            
        }

        private void LanguageChanged(object sender, EventArgs e)
        {
            DockPanelPropertiesBar.Title = Helpers.GetLocalizedText("strPropertiesBarCaption", Language);
            DockPanelSubQueryNavBar.Title = Helpers.GetLocalizedText("strSubQueryStructureBarCaption", Language);
        }

        private void ContentWindowChild_Loaded(object sender, RoutedEventArgs e)
        {
            DockPanelPropertiesBar.Title = Helpers.GetLocalizedText("strPropertiesBarCaption", Language);
            DockPanelSubQueryNavBar.Title = Helpers.GetLocalizedText("strSubQueryStructureBarCaption", Language);
        }

        #region Menu buttons event method

        private void ButtonPropertise_OnClick(object sender, RoutedEventArgs e)
        {
            PropertiesQuery();
        }
        
        private void ButtonAddObject_OnClick(object sender, RoutedEventArgs e)
        {
            AddObject();
        }

        private void ButtonAddDerivedTable_OnClick(object sender, RoutedEventArgs e)
        {
            AddDerivedTable();
        }

        private void ButtonNewUnionSubQuery_OnClick(object sender, RoutedEventArgs e)
        {
            AddUnionSubQuery();
        }

        private void ButtonCopyUnionSubQuery_OnClick(object sender, RoutedEventArgs e)
        {
            CopyUnionSubQuery();
        }

        private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
           OnSaveQueryEvent();
           UpdateStateButtons();
        }

        private void ButtonSaveToFileAs_OnClick(object sender, RoutedEventArgs e)
        {
            OnSaveAsInFileEvent();
            UpdateStateButtons();
        }

        private void ButtonSaveCurrentSubQuery_OnClick(object sender, RoutedEventArgs e)
        {
            OnSaveAsNewUserQueryEvent();
            UpdateStateButtons();
        }
        #endregion

        private void BoxSql_OnTextChanged(object sender, EventArgs eventArgs)
        {
            ErrorBox.Message = string.Empty;
        }

        private void ResetPagination()
        {
            PaginationPanel.Reset();
            CBuilder.QueryTransformer.Skip("");
            CBuilder.QueryTransformer.Take("");
        }

        private void PaginationPanel_OnCurrentPageChanged(object sender, RoutedEventArgs e)
        {
            if (PaginationPanel.CurrentPage == 1)
            {
                CBuilder.QueryTransformer.Skip("");
                return;
            }

            // Select next n records
            CBuilder.QueryTransformer.Skip(
                (PaginationPanel.PageSize * (PaginationPanel.CurrentPage - 1)).ToString());
        }

        private void PaginationPanel_OnEnabledPaginationChanged(object sender, RoutedEventArgs e)
        {
            // Turn paging on and off
            if (PaginationPanel.IsEnabled)
            {
                CBuilder.QueryTransformer.Take(PaginationPanel.PageSize.ToString());
            }
            else
            {
                ResetPagination();
            }
        }

        private void PaginationPanel_OnPageSizeChanged(object sender, RoutedEventArgs e)
        {
            CBuilder.QueryTransformer.Take(PaginationPanel.PageSize.ToString());
        }

        private void SetSqlTextCurrentSubQuery()
        {
            BorderErrorFast.Visibility = Visibility.Collapsed;
            if (_transformerSql == null) return;

            if (QueryView.ActiveUnionSubQuery == null || SqlQuery.SleepMode)
            {
                 BoxSqlCurrentSubQuery.Text = string.Empty;
                _transformerSql.Query = null;
                return;
            }


            var sqlForDataPreview = QueryView.ActiveUnionSubQuery.ParentSubQuery.GetSqlForDataPreview();
            _transformerSql.Query = new SQLQuery(QueryView.ActiveUnionSubQuery.SQLContext) { SQL = sqlForDataPreview };

            var sql = QueryView.ActiveUnionSubQuery.ParentSubQuery.GetResultSQL(SqlFormattingOptions);
            BoxSqlCurrentSubQuery.Text = sql;
        }

        private void FillFastResult()
        {
            var result = _transformerSql.Take("10");

            try
            {
                var dv = Helpers.ExecuteSql(result.SQL, (SQLQuery)_transformerSql.Query);

                ListViewFastResultSql.ItemsSource = dv;
                BorderError.Visibility = Visibility.Collapsed;
            }
            catch (Exception exception)
            {
                BorderError.Visibility = Visibility.Visible;
                LabelError.Text = exception.Message;

                ListViewFastResultSql.ItemsSource = null;
            }
        }

        private void ButtonRefreashFastResult_OnClick(object sender, RoutedEventArgs e)
        {
            FillFastResult();
        }

        private void CheckBoxAutoRefreash_OnChecked(object sender, RoutedEventArgs e)
        {
            if (ButtonRefreashFastResult == null || CheckBoxAutoRefreash == null) return;
            ButtonRefreashFastResult.IsEnabled = CheckBoxAutoRefreash.IsChecked == false;
        }

        private void CloseImage_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            BorderError.Visibility = Visibility.Collapsed;
        }

        private void DataGridResult_OnRowsLoaded(object sender, EventArgs e)
        {
            if (!PaginationPanel.IsEnabled)
                PaginationPanel.CountRows = DataGridResult.CountRows;
            BorderBlockPagination.Visibility = Visibility.Collapsed;
        }

        private void BoxSqlCurrentSubQuery_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (QueryView.ActiveUnionSubQuery == null) return;
            try
            {
                BorderErrorFast.Visibility = Visibility.Collapsed;

                QView.ActiveUnionSubQuery.ParentSubQuery.SQL = ((SqlTextEditor)sender).Text;

                var sql = QueryView.ActiveUnionSubQuery.ParentSubQuery.GetResultSQL(SqlFormattingOptions);

                _transformerSql.Query = new SQLQuery(QueryView.ActiveUnionSubQuery.SQLContext) { SQL = sql };
            }
            catch (Exception ex)
            {
                LabelErrorFast.Text = ex.Message;
                BorderErrorFast.Visibility = Visibility.Visible;
            }
        }

        protected internal virtual void OnSaveQueryEvent()
        {
            var handler = SaveQueryEvent;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected internal virtual void OnSaveAsInFileEvent()
        {
            var handler = SaveAsInFileEvent;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected internal virtual void OnSaveAsNewUserQueryEvent()
        {
            var handler = SaveAsNewUserQueryEvent;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void TabControlMain_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Execute a query on switching to the Data tab
            if (SqlContext == null ||
                (SqlContext.SyntaxProvider == null || !TabItemData.IsSelected))
            {
                DataGridResult.StopFill();
                return;
            }

            ResetPagination();
            BoxSqlTransformer.Text = BoxSql.Text;

            if (!TabItemData.IsSelected) return;

            BorderBlockPagination.Visibility = Visibility.Visible;
            DataGridResult.FillDataGrid(BoxSql.Text);
        }

        private void TabControlSqlText_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ErrorBox.Visibility == Visibility.Visible)
                ErrorBox.Visibility = Visibility.Collapsed;
            if (!e.AddedItems.Contains(TabItemFastResult) || _transformerSql.Query == null ||
                CheckBoxAutoRefreash.IsChecked == false) return;

            FillFastResult();
        }
    }
}
