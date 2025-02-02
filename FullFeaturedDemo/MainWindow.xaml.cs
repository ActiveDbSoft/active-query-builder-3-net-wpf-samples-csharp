//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2025 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.QueryTransformer;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ActiveQueryBuilder.View.WPF;
using GeneralAssembly;
using GeneralAssembly.Common;
using GeneralAssembly.QueryBuilderProperties;
using GeneralAssembly.Windows;
using GeneralAssembly.Windows.QueryInformationWindows;
using Helpers = ActiveQueryBuilder.Core.Helpers;
using BuildInfo = ActiveQueryBuilder.Core.BuildInfo;

namespace FullFeaturedDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private ConnectionInfo _selectedConnection;
        //private readonly SQLFormattingOptions _sqlFormattingOptions;
        //private readonly SQLGenerationOptions _sqlGenerationOptions;
        private bool _showHintConnection = true;
        private readonly QueryTransformer _transformerSql;
        private readonly Timer _timerStartingExecuteSql;

        public MainWindow()
        {
            InitializeComponent();
            // Options to present the formatted SQL query text to end-user
            // Use names of virtual objects, do not replace them with appropriate derived tables
            QBuilder.SQLFormattingOptions = new SQLFormattingOptions { ExpandVirtualObjects = false };

            // Options to generate the SQL query text for execution against a database server
            // Replace virtual objects with derived tables
            QBuilder.SQLGenerationOptions = new SQLGenerationOptions { ExpandVirtualObjects = true };



            Closing += MainWindow_Closing;
            Dispatcher.Hooks.DispatcherInactive += Hooks_DispatcherInactive;

            var currentLang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            LoadLanguage();

            var defLang = "en";

            if (Helpers.Localizer.Languages.Contains(currentLang.ToLower()))
            {
                Language = XmlLanguage.GetLanguage(currentLang);
                defLang = currentLang.ToLower();
            }

            var menuItem = MenuItemLanguage.Items.Cast<MenuItem>().First(item => (string)item.Tag == defLang);
            menuItem.IsChecked = true;

            QBuilder.SyntaxProvider = new GenericSyntaxProvider();

            _transformerSql = new QueryTransformer();

            _timerStartingExecuteSql = new Timer(TimerStartingExecuteSql_Elapsed);

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
                        Source = ActiveQueryBuilder.View.WPF.Helpers.GetImageSource(Properties.Resources.cancel,
                            ImageFormat.Png),
                        Stretch = Stretch.None
                    }
                };

                button.Click += delegate { GridRoot.Visibility = Visibility.Collapsed; };

                trialNoticePanel.Child = label;
                GridRoot.Children.Add(trialNoticePanel);
                GridRoot.Children.Add(button);
            }

            QBuilder.SQLQuery.QueryRoot.AllowSleepMode = true;

            QBuilder.SleepModeChanged += SqlQuery_SleepModeChanged;
            QBuilder.QueryAwake += SqlQuery_QueryAwake;
        }

        bool _shown;
        private int _errorPosition = -1;
        private string _lastValidText;
        private string _lastValidText1;
        private int _errorPosition1 = -1;

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (_shown)
                return;

            _shown = true;

            CommandNew_OnExecuted(this, null);
        }

        private static void SqlQuery_QueryAwake(QueryRoot sender, ref bool abort)
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
            BorderSleepMode.Visibility = QBuilder.SleepMode ? Visibility.Visible : Visibility.Collapsed;
            TabItemFastResult.Visibility = QBuilder.SleepMode ? Visibility.Collapsed : Visibility.Visible;
            TabItemCurrentSubQuery.Visibility = QBuilder.SleepMode ? Visibility.Collapsed : Visibility.Visible;
            TabItemData.IsEnabled = !QBuilder.SleepMode;
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
                menuItem.SetValue(GroupedMenuBehavior.OptionGroupNameProperty, "group");
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= MainWindow_Closing;
            Dispatcher.Hooks.DispatcherInactive -= Hooks_DispatcherInactive;
        }

        private void Hooks_DispatcherInactive(object sender, EventArgs e)
        {
            MenuItemQueryProperties.IsEnabled = _selectedConnection != null;
            MenuItemNewObject.IsEnabled = _selectedConnection != null;
            MenuItemAddDerivedTable.IsEnabled = _selectedConnection != null;
            MenuItemAddUnionSubquery.IsEnabled = _selectedConnection != null;
            MenuItemCopyUnionSubquery.IsEnabled = _selectedConnection != null;

            MenuItemSave.IsEnabled = _selectedConnection != null;
            MenuItemQueryStatistics.IsEnabled = _selectedConnection != null;
            MenuItemSaveIco.IsEnabled = _selectedConnection != null;

            MenuItemUndo.IsEnabled = BoxSql.CanUndo;
            MenuItemRedo.IsEnabled = BoxSql.CanRedo;
            MenuItemCopyIco.IsEnabled =
                MenuItemCopy.IsEnabled = BoxSql.SelectionLength > 0;
            MenuItemPasteIco.IsEnabled = MenuItemPaste.IsEnabled = Clipboard.ContainsText();
            MenuItemCutIco.IsEnabled =
                MenuItemCut.IsEnabled = BoxSql.SelectionLength > 0;
            MenuItemSelectAll.IsEnabled = true;

            MenuItemQueryAddDerived.IsEnabled = CanAddDerivedTable();

            MenuItemCopyUnionSq.IsEnabled = CanCopyUnionSubQuery();
            MenuItemAddUnionSq.IsEnabled = CanAddUnionSubQuery();
            MenuItemProp.IsEnabled = CanShowProperties();
            MenuItemAddObject.IsEnabled = CanAddObject();
            MenuItemProperties.IsEnabled = (QBuilder.SQLFormattingOptions != null && QBuilder.SQLContext != null);
            MenuItemUserExpression.IsEnabled = _selectedConnection != null;
            foreach (var item in MetadataItemMenu.Items.Cast<FrameworkElement>().Where(x => x is MenuItem).ToList())
            {
                item.IsEnabled = QBuilder.SQLContext != null;
            }
        }

        public bool CanAddObject()
        {
            return QBuilder.AddObjectDialog != null;
        }

        public bool CanShowProperties()
        {
            return QBuilder.ActiveUnionSubQuery != null;
        }

        public bool CanAddUnionSubQuery()
        {
            if (QBuilder.SyntaxProvider == null) return false;

            if (QBuilder.ActiveUnionSubQuery != null && QBuilder.ActiveUnionSubQuery.QueryRoot.IsSubQuery)
            {
                return QBuilder.SyntaxProvider.IsSupportSubQueryUnions();
            }

            return QBuilder.SyntaxProvider.IsSupportUnions();
        }

        public bool CanCopyUnionSubQuery()
        {
            return CanAddUnionSubQuery();
        }

        public bool CanAddDerivedTable()
        {
            if (QBuilder.SyntaxProvider == null) return false;

            if (QBuilder.ActiveUnionSubQuery != null && QBuilder.ActiveUnionSubQuery.QueryRoot.IsMainQuery)
            {
                return QBuilder.SyntaxProvider.IsSupportDerivedTables();
            }

            return QBuilder.SyntaxProvider.IsSupportSubQueryDerivedTables();
        }

        private void InitializeSqlContext()
        {
            try
            {
                QBuilder.Clear();

                BaseMetadataProvider metadataProvaider = null;

                // create new SqlConnection object using the connections string from the connection form
                if (!_selectedConnection.IsXmlFile)
                    metadataProvaider = _selectedConnection.ConnectionDescriptor.MetadataProvider;

                // setup the query builder with metadata and syntax providers
                QBuilder.SQLContext.MetadataContainer.Clear();
                QBuilder.MetadataProvider = metadataProvaider;
                QBuilder.SyntaxProvider = _selectedConnection.ConnectionDescriptor.SyntaxProvider;
                QBuilder.MetadataLoadingOptions.OfflineMode = metadataProvaider == null;

                if (metadataProvaider == null)
                    QBuilder.MetadataContainer.ImportFromXML(_selectedConnection.ConnectionString);

                // Instruct the query builder to fill the database schema tree
                QBuilder.InitializeDatabaseSchemaTree();

            }
            finally
            {
                if (QBuilder.MetadataContainer.LoadingOptions.OfflineMode)
                {
                    TsmiOfflineMode.IsChecked = true;
                }

                if (CBuilder.QueryTransformer != null)
                    CBuilder.QueryTransformer.SQLUpdated -= QueryTransformer_SQLUpdated;

                CBuilder.QueryTransformer = new QueryTransformer
                {
                    Query = QBuilder.SQLQuery,
                    SQLGenerationOptions = QBuilder.SQLGenerationOptions
                };

                CBuilder.QueryTransformer.SQLUpdated += QueryTransformer_SQLUpdated;

                DataGridResult.QueryTransformer = CBuilder.QueryTransformer;
                DataGridResult.SqlQuery = QBuilder.SQLQuery;
            }
        }

        private void QueryTransformer_SQLUpdated(object sender, EventArgs e)
        {
            // Handle the event raised by Query Transformer object that the text of SQL query is changed
            // update the text box
            if (DataGridResult.QueryTransformer == null || !Equals(TabItemData, TabControl.SelectedItem)) return;

            DataGridResult.FillData(DataGridResult.QueryTransformer.SQL);
            BoxSqlTransformer.Text = DataGridResult.QueryTransformer.SQL;
        }

        private void MenuItemQueryStatistics_OnClick(object sender, RoutedEventArgs e)
        {
            ShowQueryStatistics();
        }

        public void ShowQueryStatistics()
        {
            var qs = QBuilder.QueryStatistics;

            var stats = "Used Objects (" + qs.UsedDatabaseObjects.Count + "):\r\n";
            stats = qs.UsedDatabaseObjects.Aggregate(stats,
                (current, t) => current + ("\r\n" + t.ObjectName.QualifiedName));

            stats += "\r\n\r\n" + "Used Columns (" + qs.UsedDatabaseObjectFields.Count + "):\r\n";
            stats = qs.UsedDatabaseObjectFields.Aggregate(stats,
                (current, t) => current + ("\r\n" + t.ObjectName.QualifiedName));

            stats += "\r\n\r\n" + "Output Expressions (" + qs.OutputColumns.Count + "):\r\n";
            stats = qs.OutputColumns.Aggregate(stats, (current, t) => current + ("\r\n" + t.Expression));

            var f = new QueryStatisticsWindow(stats);

            f.ShowDialog();
        }

        private void CommandOpen_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // open a saved query
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

            if (QBuilder.SQLContext == null)
                CommandNew_OnExecuted(null, null);
            else
            {
                try
                {
                    // load query to the query builder by assigning its text to the SQL property
                    QBuilder.SQL = sb.ToString();
                }
                finally
                {

                }
            }
        }

        private void CommandSave_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SaveInFile();
        }

        private void CommandExit_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void CommandNew_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            QBuilder.QueryView.HideInformationMessage();

            var cf = new DatabaseConnectionWindow(_showHintConnection) { Owner = this };

            _showHintConnection = false;

            if (cf.ShowDialog() != true) return;
            _selectedConnection = cf.SelectedConnection;

            InitializeSqlContext();
        }

        private void CommandUndo_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            BoxSql.Undo();
        }

        private void CommandRedo_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            BoxSql.Redo();
        }

        private void CommandCopy_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            BoxSql.Copy();
        }

        private void CommandPaste_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            BoxSql.Paste();
        }

        private void CommandCut_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            BoxSql.Cut();
        }

        private void CommandSelectAll_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            BoxSql.SelectAll();
        }

        private void MenuItemQueryAddDerived_OnClick(object sender, RoutedEventArgs e)
        {
            AddDerivedTable();
        }

        private void AddDerivedTable()
        {
            using (new UpdateRegion(QBuilder.ActiveUnionSubQuery.FromClause))
            {
                var sqlContext = QBuilder.ActiveUnionSubQuery.SQLContext;

                var fq = new SQLFromQuery(sqlContext)
                {
                    Alias = new SQLAliasObjectAlias(sqlContext)
                    {
                        Alias = QBuilder.ActiveUnionSubQuery.QueryRoot.CreateUniqueSubQueryName()
                    },
                    SubQuery = new SQLSubSelectStatement(sqlContext)
                };

                var sqse = new SQLSubQuerySelectExpression(sqlContext);
                fq.SubQuery.Add(sqse);
                sqse.SelectItems = new SQLSelectItems(sqlContext);
                sqse.From = new SQLFromClause(sqlContext);

                QBuilder.QueryNavigationBar.Query.AddObject(QBuilder.ActiveUnionSubQuery, fq, typeof(DataSourceQuery));
            }
        }

        private void MenuItemCopyUnionSq_OnClick(object sender, RoutedEventArgs e)
        {
            CopyUnionSubQuery();
        }

        private void CopyUnionSubQuery()
        {
            // add an empty UnionSubQuery
            var usq = QBuilder.ActiveUnionSubQuery.ParentGroup.Add();

            // copy the content of existing union sub-query to a new one
            var usqAst = QBuilder.ActiveUnionSubQuery.ResultQueryAST;
            usqAst.RestoreColumnPrefixRecursive(true);

            var lCte = new List<SQLWithClauseItem>();
            var lFromObj = new List<SQLFromSource>();
            QBuilder.ActiveUnionSubQuery.GatherPrepareAndFixupContext(lCte, lFromObj, false);
            usqAst.PrepareAndFixupRecursive(lCte, lFromObj);

            usq.LoadFromAST(usqAst);
            QBuilder.ActiveUnionSubQuery = usq;
        }

        private void MenuItemAddUnionSq_OnClick(object sender, RoutedEventArgs e)
        {
            AddUnionSubQuery();
        }

        private void AddUnionSubQuery()
        {
            QBuilder.ActiveUnionSubQuery = QBuilder.ActiveUnionSubQuery.ParentGroup.Add();
        }

        private void MenuItemProp_OnClick(object sender, RoutedEventArgs e)
        {
            PropertiesQuery();
        }

        private void PropertiesQuery()
        {
            QBuilder.ShowActiveUnionSubQueryProperties();
        }

        private void MenuItemAddObject_OnClick(object sender, RoutedEventArgs e)
        {
            AddObject();
        }

        private void AddObject()
        {
            if (QBuilder.AddObjectDialog != null)
                QBuilder.AddObjectDialog.ShowModal();
        }

        private void MenuItemProperties_OnClick(object sender, RoutedEventArgs e)
        {
            var propWindow = new QueryBuilderPropertiesWindow(QBuilder);
            propWindow.ShowDialog();
        }

        private void LanguageMenuItemChecked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            var menuItem = (MenuItem)sender;
            var lng = menuItem.Tag.ToString();
            Language = XmlLanguage.GetLanguage(lng);
        }

        private void MenuItem_OfflineMode_OnChecked(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;

            if (menuItem.IsChecked)
            {
                try
                {
                    Cursor = Cursors.Wait;

                    QBuilder.MetadataContainer.LoadAll(true);
                }
                finally
                {
                    Cursor = Cursors.Arrow;
                }
            }

            QBuilder.MetadataContainer.LoadingOptions.OfflineMode = menuItem.IsChecked;
        }

        private void MenuItem_RefreshMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            if (QBuilder.SQLContext == null || QBuilder.SQLContext.MetadataProvider == null ||
                !QBuilder.SQLContext.MetadataProvider.Connected) return;
            // Force the query builder to refresh metadata from current connection
            // to refresh metadata, just clear MetadataContainer and reinitialize metadata tree
            QBuilder.MetadataContainer.Clear();
            QBuilder.InitializeDatabaseSchemaTree();
        }

        private void MenuItem_ClearMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            QBuilder.MetadataContainer.Clear();
        }

        private void MenuItem_LoadMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog { Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*" };

            if (fileDialog.ShowDialog() != true) return;

            QBuilder.MetadataContainer.LoadingOptions.OfflineMode = true;
            QBuilder.MetadataContainer.ImportFromXML(fileDialog.FileName);

            // Instruct the query builder to fill the database schema tree
            QBuilder.InitializeDatabaseSchemaTree();
        }

        private void MenuItem_SaveMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new SaveFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                FileName = "Metadata.xml"
            };

            if (fileDialog.ShowDialog() != true) return;

            QBuilder.MetadataContainer.LoadAll(true);
            QBuilder.MetadataContainer.ExportToXML(fileDialog.FileName);
        }

        private void MenuItem_About_OnClick(object sender, RoutedEventArgs e)
        {
            var f = new AboutWindow { Owner = this };

            f.ShowDialog();
        }

        private void SaveInFile()
        {
            // Save the query text to file
            if (QBuilder.ActiveUnionSubQuery == null) return;

            var saveFileDialog1 = new SaveFileDialog()
            {
                DefaultExt = "sql",
                FileName = "query",
                Filter = "SQL query files (*.sql)|*.sql|All files|*.*"
            };

            if (saveFileDialog1.ShowDialog() != true) return;

            using (var sw = new StreamWriter(saveFileDialog1.FileName))
            {
                sw.Write(FormattedSQLBuilder.GetSQL(QBuilder.ActiveUnionSubQuery.QueryRoot, QBuilder.SQLFormattingOptions));
            }
        }

        private void BoxSql_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                // Update the query builder with manually edited query text:
                QBuilder.SQL = BoxSql.Text;

                // Hide error banner if any
                ErrorBox.Show(null, QBuilder.SyntaxProvider);
            }
            catch (SQLParsingException ex)
            {
                // Show banner with error text
                ErrorBox.Show(ex.Message, QBuilder.SyntaxProvider);
                _errorPosition = ex.ErrorPos.pos;
            }
        }

        private void QBuilder_OnSQLUpdated(object sender, EventArgs e)
        {
            BoxSql.Text = QBuilder.FormattedSQL;
            _lastValidText = BoxSql.Text;
            SetSqlTextCurrentSubQuery();

            if (!TabItemFastResult.IsSelected || CheckBoxAutoRefreash.IsChecked == false) return;

            _timerStartingExecuteSql.Change(600, Timeout.Infinite);
        }

        private void TimerStartingExecuteSql_Elapsed(object state)
        {
            Dispatcher.BeginInvoke((Action)FillFastResult);
        }

        private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Execute a query on switching to the Data tab
            if (e.AddedItems.Count == 0 || _selectedConnection == null) return;

            if (QBuilder.SyntaxProvider == null || !Equals(e.AddedItems[0], TabItemData)) return;

            CBuilder.Clear();
            BoxSqlTransformer.Text = BoxSql.Text;

            if (Equals(TabItemData, TabControl.SelectedItem))
            {
                DataGridResult.FillData(BoxSql.Text);
            }
        }

        private void BoxSql_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorBox.Show(null, QBuilder.SyntaxProvider);
        }

        private void QBuilder_OnActiveUnionSubQueryChanged(object sender, EventArgs e)
        {
            SetSqlTextCurrentSubQuery();
        }

        private void SetSqlTextCurrentSubQuery()
        {
            ErrorBox2.Show(null, QBuilder.SyntaxProvider);

            if (_transformerSql == null) return;
            try
            {
                var activeUnionSubQuery = QBuilder.ActiveUnionSubQuery;
                if (activeUnionSubQuery == null || QBuilder.SleepMode)
                {
                    BoxSqlCurrentSubQuery.Text = "";
                    _transformerSql.Query = null;
                    return;
                }

                var parentSubQuery = activeUnionSubQuery.ParentSubQuery;

                var sqlForDataPreview = parentSubQuery.GetSqlForDataPreview();
                _transformerSql.Query = new SQLQuery(activeUnionSubQuery.SQLContext) { SQL = sqlForDataPreview };

                var sql = parentSubQuery.GetResultSQL(QBuilder.SQLFormattingOptions);
                BoxSqlCurrentSubQuery.Text = sql;
            }
            catch (SQLParsingException ex)
            {
                ErrorBox2.Show(ex.Message, QBuilder.SyntaxProvider);
            }
        }

        private void FillFastResult()
        {
            var result = _transformerSql.Take("10");
            ListViewFastResultSql.FillData(result.SQL, (SQLQuery)_transformerSql.Query);
        }

        private void TabControlSql_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ErrorBox.Visibility == Visibility.Visible)
                ErrorBox.Visibility = Visibility.Collapsed;
            if (!e.AddedItems.Contains(TabItemFastResult) || _transformerSql.Query == null ||
                CheckBoxAutoRefreash.IsChecked == false) return;

            FillFastResult();
        }

        private void ButtonRefreshFastResult_OnClick(object sender, RoutedEventArgs e) => FillFastResult();

        private void CheckBoxAutoRefresh_OnChecked(object sender, RoutedEventArgs e)
        {
            if (ButtonRefreshFastResult == null || CheckBoxAutoRefreash == null) return;
            ButtonRefreshFastResult.IsEnabled = CheckBoxAutoRefreash.IsChecked == false;
        }

        private void BoxSqlCurrentSubQuery_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (QBuilder.ActiveUnionSubQuery == null) return;
            try
            {
                ErrorBox.Show(null, QBuilder.SyntaxProvider);

                QBuilder.ActiveUnionSubQuery.ParentSubQuery.SQL = ((TextBox)sender).Text;

                var sql = QBuilder.ActiveUnionSubQuery.ParentSubQuery.GetResultSQL(QBuilder.SQLFormattingOptions);

                _transformerSql.Query = new SQLQuery(QBuilder.ActiveUnionSubQuery.SQLContext) { SQL = sql };
                _lastValidText1 = ((TextBox)sender).Text;
            }
            catch (SQLParsingException ex)
            {
                ErrorBox2.Show(ex.Message, QBuilder.SyntaxProvider);
                _errorPosition1 = ex.ErrorPos.pos;
            }
        }

        private void MenuItemEditMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            QueryBuilder.EditMetadataContainer(QBuilder.SQLContext);
        }

        private void ErrorBox_OnGoToErrorPosition(object sender, EventArgs e)
        {
            BoxSql.Focus();

            if (_errorPosition == -1) return;

            if (BoxSql.LineCount != 1)
                BoxSql.ScrollToLine(BoxSql.GetLineIndexFromCharacterIndex(_errorPosition));
            BoxSql.CaretIndex = _errorPosition;
        }

        private void ErrorBox_OnRevertValidText(object sender, EventArgs e)
        {
            BoxSql.Text = _lastValidText;
            BoxSql.Focus();
        }

        private void ErrorBox_OnGoToErrorPositionEvent(object sender, EventArgs e)
        {
            BoxSqlCurrentSubQuery.Focus();

            if (_errorPosition1 == -1) return;

            if (BoxSqlCurrentSubQuery.LineCount != 1)
                BoxSqlCurrentSubQuery.ScrollToLine(BoxSqlCurrentSubQuery.GetLineIndexFromCharacterIndex(_errorPosition1));
            BoxSqlCurrentSubQuery.CaretIndex = _errorPosition1;
        }

        private void ErrorBox_OnRevertValidTextEvent(object sender, EventArgs e)
        {
            BoxSqlCurrentSubQuery.Text = _lastValidText1;
            BoxSqlCurrentSubQuery.Focus();
        }

        private void MenuItemUserExpression_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new EditUserExpressionWindow { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            window.Load(QBuilder.QueryView);
            window.ShowDialog();
        }
    }
}
