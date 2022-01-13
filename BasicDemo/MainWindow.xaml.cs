//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2022 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.WPF;
using BasicDemo.Connection;
using GeneralAssembly;
using GeneralAssembly.QueryBuilderProperties;
using GeneralAssembly.Windows.QueryInformationWindows;
using Microsoft.Win32;
using Helpers = ActiveQueryBuilder.View.Helpers;
using Timer = System.Timers.Timer;
using BuildInfo = ActiveQueryBuilder.Core.BuildInfo;

namespace BasicDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private bool _showHintConnection = true;
        private ConnectionInfo _selectedConnection;

        private string _lastValidSql;
        private int _errorPosition = -1;

        private readonly OpenFileDialog _openFileDialog = new OpenFileDialog();
        private readonly SaveFileDialog _saveFileDialog = new SaveFileDialog();

        private readonly GenericSyntaxProvider _genericSyntaxProvider;

        public MainWindow()
        {
            InitializeComponent();

            _genericSyntaxProvider = new GenericSyntaxProvider();

            _openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            _saveFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";

            queryBuilder.SQLQuery.QueryRoot.AllowSleepMode = true;
            queryBuilder.QueryAwake += QueryBuilder_QueryAwake;
            queryBuilder.SleepModeChanged += QueryBuilder_SleepModeChanged;
            DataGridResult.SqlQuery = queryBuilder.SQLQuery;

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

                trialNoticePanel.Child = label;
                GridRoot.Children.Add(trialNoticePanel);
            }
        }

        private void QueryBuilder_SleepModeChanged(object sender, EventArgs e)
        {
            BorderSleepMode.Visibility = queryBuilder.SleepMode ? Visibility.Visible : Visibility.Collapsed;
            tbData.IsEnabled = !queryBuilder.SleepMode;
        }

        private void QueryBuilder_QueryAwake(QueryRoot sender, ref bool abort)
        {
            if (MessageBox.Show(
                    "You had typed something that is not a SELECT statement in the text editor and continued with visual query building." +
                    "Whatever the text in the editor is, it will be replaced with the SQL generated by the component. Is it right?",
                    "Active Query Builder .NET Demo", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                abort = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            queryBuilder.SyntaxProvider = _genericSyntaxProvider;

            var currentLang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            Language = XmlLanguage.GetLanguage(Helpers.Localizer.Languages.Contains(currentLang.ToLower()) ? currentLang : "en");
            queryBuilder.SQLQuery.QueryRoot.AllowSleepMode = true;
        }

        private void menuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            QueryBuilder.ShowAboutDialog();
        }

        // TextBox lost focus by keyboard
        private void textBox1_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            try
            {
                // Update the query builder with manually edited query text:
                queryBuilder.SQL = TextBox1.Text;
                ErrorBox.Show(null, queryBuilder.SyntaxProvider);
                _lastValidSql = queryBuilder.FormattedSQL;
                _errorPosition = -1;
            }
            catch (SQLParsingException ex)
            {
                // Set caret to error position
                TextBox1.SelectionStart = ex.ErrorPos.pos;
                _errorPosition = ex.ErrorPos.pos;
                // Report error
                ErrorBox.Show(ex.Message, queryBuilder.SyntaxProvider);
            }
        }

        private void WarnAboutGenericSyntaxProvider()
        {
            if (queryBuilder.SyntaxProvider is GenericSyntaxProvider)
            {
                panel1.Visibility = Visibility.Visible;

                // setup the panel to hide automatically
                var timer = new Timer();
                timer.Elapsed += TimerEvent;
                timer.Interval = 10000;
                timer.Start();
            }
        }

        private void TimerEvent(object source, EventArgs args)
        {
            Dispatcher?.Invoke(delegate
            {
                panel1.Visibility = Visibility.Collapsed;
            });

            ((Timer)source).Stop();
            ((Timer)source).Dispose();
        }

        private void RefreshMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            // Force the query builder to refresh metadata from current connection
            // to refresh metadata, just clear MetadataContainer and reinitialize metadata tree

            if (queryBuilder.MetadataProvider == null || !queryBuilder.MetadataProvider.Connected) return;

            queryBuilder.ClearMetadata();
            queryBuilder.InitializeDatabaseSchemaTree();
        }

        private void ClearMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            // Clear the metadata

            if (MessageBox.Show("Clear Metadata Container?", "Confirmation", MessageBoxButton.YesNo) ==
                MessageBoxResult.Yes)
            {
                queryBuilder.MetadataContainer.Clear();
            }
        }

        private void LoadMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            // Load metadata from XML file
            if (_openFileDialog.ShowDialog() != true) return;

            queryBuilder.MetadataLoadingOptions.OfflineMode = true;
            queryBuilder.MetadataContainer.ImportFromXML(_openFileDialog.FileName);
            queryBuilder.InitializeDatabaseSchemaTree();
        }

        private void SaveMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            // Save metadata to XML file
            _saveFileDialog.FileName = "Metadata.xml";

            if (_saveFileDialog.ShowDialog() != true) return;

            queryBuilder.MetadataContainer.LoadAll(true);
            queryBuilder.MetadataContainer.ExportToXML(_saveFileDialog.FileName);
        }

        private void QueryBuilder_OnSQLUpdated(object sender, EventArgs e)
        {
            // Handle the event raised by SQL Builder object that the text of SQL query is changed
            // update the text box
            TextBox1.Text = queryBuilder.FormattedSQL;

            if (!Equals(TabControl.SelectedItem, tbData))
            {
                return;
            }

            ExecuteSql();
        }

        public void ResetQueryBuilder()
        {
            queryBuilder.ClearMetadata();
            queryBuilder.MetadataProvider = null;
            queryBuilder.SyntaxProvider = null;
            queryBuilder.MetadataLoadingOptions.OfflineMode = false;
        }

        private void FillProgrammatically_OnClick(object sender, RoutedEventArgs e)
        {
            ResetQueryBuilder();

            // Fill the query builder metadata programmatically

            // setup the query builder with metadata and syntax providers
            queryBuilder.SyntaxProvider = _genericSyntaxProvider;
            queryBuilder.MetadataLoadingOptions.OfflineMode = true; // prevent querying objects from database

            // create database and schema
            MetadataNamespace database = queryBuilder.MetadataContainer.AddDatabase("MyDB");
            database.Default = true;
            MetadataNamespace schema = database.AddSchema("MySchema");
            schema.Default = true;

            // create table
            MetadataObject tableOrders = schema.AddTable("Orders");
            tableOrders.AddField("OrderID");
            tableOrders.AddField("OrderDate");
            tableOrders.AddField("CustomerID");
            tableOrders.AddField("ResellerID");

            // create another table
            MetadataObject tableCustomers = schema.AddTable("Customers");
            tableCustomers.AddField("CustomerID");
            tableCustomers.AddField("CustomerName");
            tableCustomers.AddField("CustomerAddress");

            // add a relation between these two tables
            MetadataForeignKey relation = tableCustomers.AddForeignKey("FK_CustomerID");
            relation.Fields.Add("CustomerID");
            relation.ReferencedObjectName = tableOrders.GetQualifiedName();
            relation.ReferencedFields.Add("CustomerID");

            //create view
            MetadataObject viewResellers = schema.AddView("Resellers");
            viewResellers.AddField("ResellerID");
            viewResellers.AddField("ResellerName");

            // kick the query builder to fill metadata tree
            queryBuilder.InitializeDatabaseSchemaTree();

            WarnAboutGenericSyntaxProvider(); // show warning (just for demonstration purposes)
        }

        private void QueryStatistic_OnClick(object sender, RoutedEventArgs e)
        {
            QueryStatistics queryStatistics = queryBuilder.QueryStatistics;
            StringBuilder builder = new StringBuilder();

            builder.Append("Used Objects (").Append(queryStatistics.UsedDatabaseObjects.Count).AppendLine("):");
            builder.AppendLine();

            for (int i = 0; i < queryStatistics.UsedDatabaseObjects.Count; i++)
                builder.AppendLine(queryStatistics.UsedDatabaseObjects[i].ObjectName.QualifiedName);

            builder.AppendLine().AppendLine();
            builder.Append("Used Columns (").Append(queryStatistics.UsedDatabaseObjectFields.Count).AppendLine("):");
            builder.AppendLine();

            for (int i = 0; i < queryStatistics.UsedDatabaseObjectFields.Count; i++)
                builder.AppendLine(queryStatistics.UsedDatabaseObjectFields[i].FullName.QualifiedName);

            builder.AppendLine().AppendLine();
            builder.Append("Output Expressions (").Append(queryStatistics.OutputColumns.Count).AppendLine("):");
            builder.AppendLine();

            for (int i = 0; i < queryStatistics.OutputColumns.Count; i++)
                builder.AppendLine(queryStatistics.OutputColumns[i].Expression);

            var f = new QueryStatisticsWindow(builder.ToString()) {Owner = this};
            f.ShowDialog();
        }

        private void Properties_OnClick(object sender, RoutedEventArgs e)
        {
            // Show Properties form
            QueryBuilderPropertiesWindow f = new QueryBuilderPropertiesWindow(queryBuilder);

            f.ShowDialog();

            WarnAboutGenericSyntaxProvider(); // show warning (just for demonstration purposes)
        }

        private void ExecuteSql()
        {
            DataGridResult.FillData(queryBuilder.SQL);
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            var tab = e.AddedItems[0] as TabItem;
            if (tab == null) return;

            if (queryBuilder.FormattedSQL != TextBox1.Text)
                queryBuilder.SQL = TextBox1.Text;

            if (!Equals(TabControl.SelectedItem, tbData) || queryBuilder.SQL == string.Empty)
            {
                
                return;
            }

            ExecuteSql();
        }

        private void TextBox1_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorBox.Show(null, queryBuilder.SyntaxProvider);
        }

        private void MenuItemEditMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            QueryBuilder.EditMetadataContainer(queryBuilder.SQLContext);
        }

        private void ErrorBox_OnSyntaxProviderChanged(object sender, SelectionChangedEventArgs e)
        {
            var oldSql = TextBox1.Text;
            var caretPosition = TextBox1.CaretIndex;

            queryBuilder.SyntaxProvider = (BaseSyntaxProvider) e.AddedItems[0];
            TextBox1.Text = oldSql;
            TextBox1.Focus();
            TextBox1.CaretIndex = caretPosition;
        }

        private void ErrorBox_OnGoToErrorPositionEvent(object sender, EventArgs e)
        {
            TextBox1.Focus();

            if (_errorPosition == -1) return;

            if (TextBox1.LineCount != 1)
                TextBox1.ScrollToLine(TextBox1.GetLineIndexFromCharacterIndex(_errorPosition));
            TextBox1.CaretIndex = _errorPosition;
        }

        private void ErrorBox_OnRevertValidTextEvent(object sender, EventArgs e)
        {
            TextBox1.Text = _lastValidSql;
            TextBox1.Focus();
        }
        
        private void ConnectTo_OnClick(object sender, RoutedEventArgs e)
        {
            var cf = new DatabaseConnectionWindow(_showHintConnection) { Owner = this };
            _showHintConnection = false;
            if (cf.ShowDialog() != true) return;
            _selectedConnection = cf.SelectedConnection;

            InitializeSqlContext();
        }
        private void InitializeSqlContext()
        {
            try
            {
                queryBuilder.Clear();

                BaseMetadataProvider metadataProvider = null;

                if (_selectedConnection == null) return;

                // create new SqlConnection object using the connections string from the connection form
                if (!_selectedConnection.IsXmlFile)
                    metadataProvider = _selectedConnection.ConnectionDescriptor?.MetadataProvider;

                // setup the query builder with metadata and syntax providers
                queryBuilder.SQLContext.MetadataContainer.Clear();
                queryBuilder.MetadataProvider = metadataProvider;
                queryBuilder.SyntaxProvider = _selectedConnection.ConnectionDescriptor?.SyntaxProvider;
                queryBuilder.MetadataLoadingOptions.OfflineMode = metadataProvider == null;

                if (metadataProvider == null)
                    queryBuilder.MetadataContainer.ImportFromXML(_selectedConnection.ConnectionString);

                // Instruct the query builder to fill the database schema tree
                queryBuilder.InitializeDatabaseSchemaTree();

            }
            finally
            {

                DataGridResult.SqlQuery = queryBuilder.SQLQuery;
            }
        }
    }
}
