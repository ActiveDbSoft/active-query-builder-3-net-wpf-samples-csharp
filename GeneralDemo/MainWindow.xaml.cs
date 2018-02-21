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
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using Oracle.ManagedDataAccess.Client;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.WPF;
using GeneralDemo.ConnectionWindow;
using GeneralDemo.PropertiesForm;
using Microsoft.Win32;
using Helpers = ActiveQueryBuilder.View.Helpers;
using Timer = System.Timers.Timer;

namespace GeneralDemo
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        private readonly OpenFileDialog _openFileDialog = new OpenFileDialog();
        private readonly SaveFileDialog _saveFileDialog = new SaveFileDialog();

        private readonly MSSQLMetadataProvider _mssqlMetadataProvider1;
        private readonly MSSQLSyntaxProvider _mssqlSyntaxProvider1;
        private readonly OLEDBMetadataProvider _oledbMetadataProvider1;
        private readonly MSAccessSyntaxProvider _msaccessSyntaxProvider1;
        private readonly OracleNativeMetadataProvider _oracleMetadataProvider1;
        private readonly OracleSyntaxProvider _oracleSyntaxProvider1;
        private readonly GenericSyntaxProvider _genericSyntaxProvider1;
        private readonly ODBCMetadataProvider _odbcMetadataProvider1;

        public Window1()
        {
            InitializeComponent();

            _mssqlMetadataProvider1 = new MSSQLMetadataProvider();
            _mssqlSyntaxProvider1 = new MSSQLSyntaxProvider();
            _oledbMetadataProvider1 = new OLEDBMetadataProvider();
            _msaccessSyntaxProvider1 = new MSAccessSyntaxProvider();
            _oracleMetadataProvider1 = new OracleNativeMetadataProvider();
            _oracleSyntaxProvider1 = new OracleSyntaxProvider();
            _genericSyntaxProvider1 = new GenericSyntaxProvider();
            _odbcMetadataProvider1 = new ODBCMetadataProvider();

            _openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            _saveFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";

			sqlTextEditor1.QueryProvider = queryBuilder;

// DEMO WARNING
            var trialNoticePanel = new Border
            {
                SnapsToDevicePixels = true,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Background = Brushes.LightPink,
                Padding = new Thickness(5),
                Margin = new Thickness(0,0,0,2)
            };
            trialNoticePanel.SetValue(Grid.RowProperty, 1);

            var label = new TextBlock
            {
                Text = @"Generation of random aliases for the query output columns is the limitation of the trial version. The full version is free from this behavior.",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            trialNoticePanel.Child = label;
            GridRoot.Children.Add(trialNoticePanel);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            queryBuilder.SyntaxProvider = _genericSyntaxProvider1;

            var currentLang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            Language = XmlLanguage.GetLanguage(Helpers.Localizer.Languages.Contains(currentLang.ToLower()) ? currentLang : "en");
        }

        private void menuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            QueryBuilder.ShowAboutDialog();
        }

        // TextBox lost focus by mouse
        private void SqlTextEditor_OnLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                // Update the query builder with manually edited query text:
                queryBuilder.SQL = sqlTextEditor1.Text;
            }
            catch (SQLParsingException ex)
            {
                // Set caret to error position
                sqlTextEditor1.SelectionStart = ex.ErrorPos.pos;
            }
        }

        // TextBox lost focus by keyboard
        private void SqlTextEditor_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            try
            {
                // Update the query builder with manually edited query text:
                queryBuilder.SQL = sqlTextEditor1.Text;
                ShowErrorBanner((FrameworkElement)sender, "");
            }
            catch (SQLParsingException ex)
            {
                // Set caret to error position
                sqlTextEditor1.SelectionStart = ex.ErrorPos.pos;
                // Report error
                ShowErrorBanner((FrameworkElement) sender, ex.Message);
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

        private void TimerEvent(Object source, EventArgs args)
        {
            Dispatcher.Invoke((Action) delegate
            {
                panel1.Visibility = Visibility.Collapsed;
            });

            ((Timer) source).Stop();
            ((Timer) source).Dispose();
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
                queryBuilder.ClearMetadata();
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
            sqlTextEditor1.Text = queryBuilder.FormattedSQL;
        }


        public void ShowErrorBanner(FrameworkElement control, string text)
        {
            // Show new banner if text is not empty
            ErrorBox.Message = text;
        }

        public void ResetQueryBuilder()
        {
            queryBuilder.ClearMetadata();
            queryBuilder.MetadataProvider = null;
            queryBuilder.SyntaxProvider = null;
            queryBuilder.MetadataLoadingOptions.OfflineMode = false;
        }

        private void connectToMSSQLServer_OnClick(object sender, RoutedEventArgs e)
        {
            // Connect to MS SQL Server

            // show the connection form
            var f = new MSSQLConnectionWindow {Owner = this};
            if (f.ShowDialog() != true) return;

            ResetQueryBuilder();

            // create new SqlConnection object using the connections string from the connection form
            _mssqlMetadataProvider1.Connection = new SqlConnection(f.ConnectionString);

            // setup the query builder with metadata and syntax providers
            queryBuilder.MetadataProvider = _mssqlMetadataProvider1;
            queryBuilder.SyntaxProvider = _mssqlSyntaxProvider1;

            queryBuilder.DatabaseSchemaViewOptions.DefaultExpandLevel = 1;

            // kick the query builder to fill metadata tree
            queryBuilder.InitializeDatabaseSchemaTree();
        }

        private void ConnectToAccess_OnClick(object sender, RoutedEventArgs e)
        {
            // Connect to MS Access database using OLE DB provider

            // show the connection form
            var f = new AccessConnectionWindow {Owner = this};

            if (f.ShowDialog() != true) return;

            ResetQueryBuilder();

            // create new OleDbConnection object using the connections string from the connection form
            _oledbMetadataProvider1.Connection = new OleDbConnection(f.ConnectionString);

            // setup the query builder with metadata and syntax providers
            queryBuilder.MetadataProvider = _oledbMetadataProvider1;
            queryBuilder.SyntaxProvider = _msaccessSyntaxProvider1;

            queryBuilder.DatabaseSchemaViewOptions.DefaultExpandLevel = 0;

            // kick the query builder to fill metadata tree
            queryBuilder.InitializeDatabaseSchemaTree();
        }

        private void ConnectToOracle_OnClick(object sender, RoutedEventArgs e)
        {
            // Connect to Oracle Server.
            // Connect using a metadata provider based on the native Oracle Data Provider for .NET (Oracle.DataAccess.Client).

            // show the connection form
            var f = new OracleConnectionWindow{Owner = this};

            if (f.ShowDialog() != true) return;

            ResetQueryBuilder();

            // create new OracleConnection object using the connections string from the connection form
            _oracleMetadataProvider1.Connection = new OracleConnection(f.ConnectionString);

            // setup the query builder with metadata and syntax providers
            queryBuilder.MetadataProvider = _oracleMetadataProvider1;
            queryBuilder.SyntaxProvider = _oracleSyntaxProvider1;

            queryBuilder.DatabaseSchemaViewOptions.DefaultExpandLevel = 1;

            // kick the query builder to fill metadata tree
            queryBuilder.InitializeDatabaseSchemaTree();
        }

        private void ConnectToOLEDB_OnClick(object sender, RoutedEventArgs e)
        {
            // Connect to a database through the OLE DB provider

            // show the connection form
            var f = new OLEDBConnectionWindow{Owner = this};

            if (f.ShowDialog() != true) return;

            ResetQueryBuilder();

            // create new OleDbConnection object using the connections string from the connection form
            _oledbMetadataProvider1.Connection = new OleDbConnection(f.ConnectionString);

            // setup the query builder with metadata and syntax providers
            queryBuilder.MetadataProvider = _oledbMetadataProvider1;
            queryBuilder.SyntaxProvider = _genericSyntaxProvider1;

            queryBuilder.DatabaseSchemaViewOptions.DefaultExpandLevel = 1;

            // kick the query builder to fill metadata tree
            queryBuilder.InitializeDatabaseSchemaTree();

            WarnAboutGenericSyntaxProvider(); // show warning (just for demonstration purposes)
        }

        private void ConnectToODBC_OnClick(object sender, RoutedEventArgs e)
        {
            // Connect to a database through the ODBC provider

            // show the connection form
            var f = new ODBCConnectionWindow{Owner = this};

            if (f.ShowDialog() != true) return;

            ResetQueryBuilder();

            // create new OdbcConnection object using the connections string from the connection form
            _odbcMetadataProvider1.Connection = new OdbcConnection(f.ConnectionString);

            // setup the query builder with metadata and syntax providers
            queryBuilder.MetadataProvider = _odbcMetadataProvider1;
            queryBuilder.SyntaxProvider = _genericSyntaxProvider1;

            queryBuilder.DatabaseSchemaViewOptions.DefaultExpandLevel = 1;

            // kick the query builder to fill metadata tree
            queryBuilder.InitializeDatabaseSchemaTree();

            WarnAboutGenericSyntaxProvider(); // show warning (just for demonstration purposes)
        }

        private void FillProgrammatically_OnClick(object sender, RoutedEventArgs e)
        {
            ResetQueryBuilder();

            // Fill the query builder metadata programmatically

            // setup the query builder with metadata and syntax providers
            queryBuilder.SyntaxProvider = _genericSyntaxProvider1;
            queryBuilder.MetadataLoadingOptions.OfflineMode = true; // prevent querying obejects from database

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

            queryBuilder.DatabaseSchemaViewOptions.DefaultExpandLevel = 1;

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

			var f = new QueryStatisticsWindow { textBox = { Text = builder.ToString() }, Owner = this};
            f.ShowDialog();
        }

        private void Properties_OnClick(object sender, RoutedEventArgs e)
        {
            // Show Properties form
            var f = new QueryBuilderPropertiesWindow(queryBuilder){Owner = this};

            f.ShowDialog();


            WarnAboutGenericSyntaxProvider(); // show warning (just for demonstration purposes)
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            var tab = e.AddedItems[0] as TabItem;
            if (tab == null) return;

            if (tab.Header.ToString() != "Data") return;

            dataGridView1.ItemsSource = null;

            if (queryBuilder.MetadataProvider != null && queryBuilder.MetadataProvider.Connected)
            {
                if (queryBuilder.MetadataProvider is MSSQLMetadataProvider)
                {
                    var command = (SqlCommand) queryBuilder.MetadataProvider.Connection.CreateCommand();
                    command.CommandText = queryBuilder.SQL;

                    // handle the query parameters
                    if (queryBuilder.Parameters.Count > 0)
                    {
                        for (int i = 0; i < queryBuilder.Parameters.Count; i++)
                        {
                            if (!command.Parameters.Contains(queryBuilder.Parameters[i].FullName))
                            {
                                SqlParameter parameter = new SqlParameter();
                                parameter.ParameterName = queryBuilder.Parameters[i].FullName;
                                parameter.DbType = queryBuilder.Parameters[i].DataType;
                                command.Parameters.Add(parameter);
                            }
                        }

                        QueryParametersWindow qpf = new QueryParametersWindow(command);
                        qpf.ShowDialog();
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataSet dataset = new DataSet();

                    try
                    {
                        adapter.Fill(dataset, "QueryResult");
                        dataGridView1.ItemsSource = dataset.Tables["QueryResult"].DefaultView;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "SQL query error");
                    }
                }
                else if (queryBuilder.MetadataProvider is OracleNativeMetadataProvider)
                {
                    var command = (OracleCommand) queryBuilder.MetadataProvider.Connection.CreateCommand();
                    command.CommandText = queryBuilder.SQL;

                    // handle the query parameters
                    if (queryBuilder.Parameters.Count > 0)
                    {
                        foreach (var t in queryBuilder.Parameters)
                        {
                            if (command.Parameters.Contains(t.FullName)) continue;

                            var parameter = new OracleParameter();
                            parameter.ParameterName = t.FullName;
                            parameter.DbType = t.DataType;
                            command.Parameters.Add(parameter);
                        }

                        QueryParametersWindow qpf = new QueryParametersWindow(command);

                        qpf.ShowDialog();

                    }

                    var adapter = new OracleDataAdapter(command);
                    var dataset = new DataSet();

                    try
                    {
                        adapter.Fill(dataset, "QueryResult");
                        dataGridView1.ItemsSource = dataset.Tables["QueryResult"].DefaultView;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "SQL query error");
                    }
                }
                else if (queryBuilder.MetadataProvider is OLEDBMetadataProvider)
                {
                    OleDbCommand command = (OleDbCommand) queryBuilder.MetadataProvider.Connection.CreateCommand();
                    command.CommandText = queryBuilder.SQL;

                    // handle the query parameters
                    if (queryBuilder.Parameters.Count > 0)
                    {
                        for (int i = 0; i < queryBuilder.Parameters.Count; i++)
                        {
                            if (!command.Parameters.Contains(queryBuilder.Parameters[i].FullName))
                            {
                                OleDbParameter parameter = new OleDbParameter();
                                parameter.ParameterName = queryBuilder.Parameters[i].FullName;
                                parameter.DbType = queryBuilder.Parameters[i].DataType;
                                command.Parameters.Add(parameter);
                            }
                        }

                        QueryParametersWindow qpf = new QueryParametersWindow(command);

                        qpf.ShowDialog();

                    }

                    OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                    DataSet dataset = new DataSet();

                    try
                    {
                        adapter.Fill(dataset, "QueryResult");
                        dataGridView1.ItemsSource = dataset.Tables["QueryResult"].DefaultView;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "SQL query error");
                    }
                }
                else if (queryBuilder.MetadataProvider is ODBCMetadataProvider)
                {
                    OdbcCommand command = (OdbcCommand) queryBuilder.MetadataProvider.Connection.CreateCommand();
                    command.CommandText = queryBuilder.SQL;

                    // handle the query parameters
                    if (queryBuilder.Parameters.Count > 0)
                    {
                        for (int i = 0; i < queryBuilder.Parameters.Count; i++)
                        {
                            if (!command.Parameters.Contains(queryBuilder.Parameters[i].FullName))
                            {
                                OdbcParameter parameter = new OdbcParameter();
                                parameter.ParameterName = queryBuilder.Parameters[i].FullName;
                                parameter.DbType = queryBuilder.Parameters[i].DataType;
                                command.Parameters.Add(parameter);
                            }
                        }

                        QueryParametersWindow qpf = new QueryParametersWindow(command);

                        qpf.ShowDialog();
                    }

                    OdbcDataAdapter adapter = new OdbcDataAdapter(command);
                    DataSet dataset = new DataSet();

                    try
                    {
                        adapter.Fill(dataset, "QueryResult");
                        dataGridView1.ItemsSource = dataset.Tables["QueryResult"].DefaultView;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "SQL query error");
                    }
                }
            }
        }

        private void SqlTextEditor1_OnTextChanged(object sender, EventArgs e)
        {
            ErrorBox.Message = string.Empty;
        }
    }
}
