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
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.WPF;
using LoadMetadataDemo.ConnectionWindows;

namespace LoadMetadataDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private IDbConnection _dbConnection;
        private EventMetadataProvider _way3EventMetadataProvider;

        public MainWindow()
        {
            InitializeComponent();
        }

        //////////////////////////////////////////////////////////////////////////
        /// 1st way:
        /// This method demonstrates the direct access to the internal metadata 
        /// objects collection (MetadataContainer).
        //////////////////////////////////////////////////////////////////////////
        private void btn1Way_Click(object sender, EventArgs e)
        {
            // prevent QueryBuilder to request metadata
            QBuilder.MetadataLoadingOptions.OfflineMode = true;

            QBuilder.MetadataProvider = null;

            MetadataContainer metadataContainer = QBuilder.MetadataContainer;
            metadataContainer.BeginUpdate();

            try
            {
                metadataContainer.Clear();

                MetadataNamespace schemaDbo = metadataContainer.AddSchema("dbo");

                // prepare metadata for table "Orders"
                MetadataObject orders = schemaDbo.AddTable("Orders");
                // fields
                orders.AddField("OrderId");
                orders.AddField("CustomerId");

                // prepare metadata for table "Order Details"
                MetadataObject orderDetails = schemaDbo.AddTable("Order Details");
                // fields
                orderDetails.AddField("OrderId");
                orderDetails.AddField("ProductId");
                // foreign keys
                MetadataForeignKey foreignKey = orderDetails.AddForeignKey("OrderDetailsToOrders");

                using (MetadataQualifiedName referencedName = new MetadataQualifiedName())
                {
                    referencedName.Add("Orders");
                    referencedName.Add("dbo");

                    foreignKey.ReferencedObjectName = referencedName;
                }

                foreignKey.Fields.Add("OrderId");
                foreignKey.ReferencedFields.Add("OrderId");
            }
            finally
            {
                metadataContainer.EndUpdate();
            }

            QBuilder.InitializeDatabaseSchemaTree();
        }

        //////////////////////////////////////////////////////////////////////////
        /// 2rd way:
        /// This method demonstrates on-demand manual filling of metadata structure using 
        /// corresponding MetadataContainer.ItemMetadataLoading event
        //////////////////////////////////////////////////////////////////////////
        private void btn2Way_Click(object sender, EventArgs e)
        {
            // allow QueryBuilder to request metadata
            QBuilder.MetadataLoadingOptions.OfflineMode = false;

            QBuilder.MetadataProvider = null;
            QBuilder.MetadataContainer.ItemMetadataLoading += way2ItemMetadataLoading;
            QBuilder.InitializeDatabaseSchemaTree();
        }

        private void way2ItemMetadataLoading(object sender, MetadataItem item, MetadataType types)
        {
            switch (item.Type)
            {
                case MetadataType.Root:
                    if ((types & MetadataType.Schema) > 0) item.AddSchema("dbo");
                    break;

                case MetadataType.Schema:
                    if ((item.Name == "dbo") && (types & MetadataType.Table) > 0)
                    {
                        item.AddTable("Orders");
                        item.AddTable("Order Details");
                    }
                    break;

                case MetadataType.Table:
                    if (item.Name == "Orders")
                    {
                        if ((types & MetadataType.Field) > 0)
                        {
                            item.AddField("OrderId");
                            item.AddField("CustomerId");
                        }
                    }
                    else if (item.Name == "Order Details")
                    {
                        if ((types & MetadataType.Field) > 0)
                        {
                            item.AddField("OrderId");
                            item.AddField("ProductId");
                        }

                        if ((types & MetadataType.ForeignKey) > 0)
                        {
                            MetadataForeignKey foreignKey = item.AddForeignKey("OrderDetailsToOrder");
                            foreignKey.Fields.Add("OrderId");
                            foreignKey.ReferencedFields.Add("OrderId");
                            using (MetadataQualifiedName name = new MetadataQualifiedName())
                            {
                                name.Add("Orders");
                                name.Add("dbo");

                                foreignKey.ReferencedObjectName = name;
                            }
                        }
                    }
                    break;
            }

            item.Items.SetLoaded(types, true);
        }

        //////////////////////////////////////////////////////////////////////////
        /// 3rd way:
        ///
        /// This method demonstrates loading of metadata through .NET data providers 
        /// unsupported by our QueryBuilder component. If such data provider is able 
        /// to execute SQL queries, you can use our EventMetadataProvider with handling 
        /// it's ExecSQL event. In this event the EventMetadataProvider will provide 
        /// you SQL queries it use for the metadata retrieval. You have to execute 
        /// a query and return resulting data reader object.
        ///
        /// Note: In this sample we are using GenericSyntaxProvider that tries 
        /// to detect the the server type automatically. In some conditions it's unable 
        /// to detect the server type and it also has limited syntax parsing abilities. 
        /// For this reason you have to use specific syntax providers in your application, 
        /// e.g. MySQLSyntaxProver, OracleSyntaxProvider, etc.
        //////////////////////////////////////////////////////////////////////////
        private void btn3Way_Click(object sender, EventArgs e)
        {
            if (_dbConnection != null)
            {
                try
                {
                    _dbConnection.Close();
                    _dbConnection.Open();

                    // allow QueryBuilder to request metadata
                    QBuilder.MetadataLoadingOptions.OfflineMode = false;

                    ResetQueryBuilderMetadata();

                    QBuilder.MetadataProvider = _way3EventMetadataProvider;
                    QBuilder.InitializeDatabaseSchemaTree();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "btn3Way_Click()");
                }
            }
            else
            {
                MessageBox.Show("Please setup a database connection by clicking on the \"Connect\" menu item before testing this method.");
            }
        }

        private void way3EventMetadataProvider_ExecSQL(BaseMetadataProvider metadataProvider, string sql, bool schemaOnly, out IDataReader dataReader)
        {
            dataReader = null;

            if (_dbConnection != null)
            {
                IDbCommand command = _dbConnection.CreateCommand();
                command.CommandText = sql;
                dataReader = command.ExecuteReader();
            }
        }

        //////////////////////////////////////////////////////////////////////////
        /// 4th way:
        /// This method demonstrates manual filling of metadata structure from 
        /// stored DataSet.
        //////////////////////////////////////////////////////////////////////////
        private void btn4Way_Click(object sender, EventArgs e)
        {
            QBuilder.MetadataProvider = null;
            QBuilder.MetadataLoadingOptions.OfflineMode = true; // prevent QueryBuilder to request metadata from connection

            DataSet dataSet = new DataSet();

            // Load sample dataset created in the Visual Studio with Dataset Designer
            // and exported to XML using WriteXmlSchema() method.
            dataSet.ReadXmlSchema(@"StoredDataSetSchema.xml");

            QBuilder.MetadataContainer.BeginUpdate();

            try
            {
                QBuilder.ClearMetadata();

                // add tables
                foreach (DataTable table in dataSet.Tables)
                {
                    // add new metadata table
                    MetadataObject metadataTable = QBuilder.MetadataContainer.AddTable(table.TableName);

                    // add metadata fields (columns)
                    foreach (DataColumn column in table.Columns)
                    {
                        // create new field
                        MetadataField metadataField = metadataTable.AddField(column.ColumnName);
                        // setup field
                        metadataField.FieldType = TypeToDbType(column.DataType);
                        metadataField.Nullable = column.AllowDBNull;
                        metadataField.ReadOnly = column.ReadOnly;

                        if (column.MaxLength != -1)
                        {
                            metadataField.Size = column.MaxLength;
                        }

                        // detect the field is primary key
                        foreach (DataColumn pkColumn in table.PrimaryKey)
                        {
                            if (column == pkColumn)
                            {
                                metadataField.PrimaryKey = true;
                            }
                        }
                    }

                    // add relations
                    foreach (DataRelation relation in table.ParentRelations)
                    {
                        // create new relation on the parent table
                        MetadataForeignKey metadataRelation = metadataTable.AddForeignKey(relation.RelationName);

                        // set referenced table
                        using (MetadataQualifiedName referencedName = new MetadataQualifiedName())
                        {
                            referencedName.Add(relation.ParentTable.TableName);

                            metadataRelation.ReferencedObjectName = referencedName;
                        }

                        // set referenced fields
                        foreach (DataColumn parentColumn in relation.ParentColumns)
                        {
                            metadataRelation.ReferencedFields.Add(parentColumn.ColumnName);
                        }

                        // set fields
                        foreach (DataColumn childColumn in relation.ChildColumns)
                        {
                            metadataRelation.Fields.Add(childColumn.ColumnName);
                        }
                    }
                }
            }
            finally
            {
                QBuilder.MetadataContainer.EndUpdate();
            }

            QBuilder.InitializeDatabaseSchemaTree();
        }

        // =============================================================================


        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            QueryBuilder.ShowAboutDialog();
        }

        private void QBuilder_OnSQLUpdated(object sender, EventArgs e)
        {
            if(SqlTextBox == null) return;
            // Handle the event raised by SQL builder object that the text of SQL query is changed

            // Hide error banner if any
            ShowErrorBanner(SqlTextBox, "");

            // update the text box
            SqlTextBox.Text = QBuilder.FormattedSQL;
        }

        private void connectToMSSQLServerMenuItem_Click(object sender, EventArgs e)
        {
            // Connect to MS SQL Server

            MSSQLConnectionWindow f = new MSSQLConnectionWindow();

            if (f.ShowDialog() == true)
            {
                if (_dbConnection != null)
                {
                    _dbConnection.Close();
                    _dbConnection.Dispose();
                }

                _dbConnection = new SqlConnection(f.ConnectionString);
            }

        }

        private void connectToOracleServerMenuItem_Click(object sender, EventArgs e)
        {
            // Connect to Oracle Server

            OracleConnectionWindow f = new OracleConnectionWindow();

            if (f.ShowDialog() == true)
            {
                if (_dbConnection != null)
                {
                    _dbConnection.Close();
                    _dbConnection.Dispose();
                }

                _dbConnection = new OracleConnection(f.ConnectionString);
            }

        }

        private void connectToAccessDatabaseMenuItem_Click(object sender, EventArgs e)
        {
            // Connect to MS Access database using OLE DB provider

            AccessConnectionWindow f = new AccessConnectionWindow();

            if (f.ShowDialog() == true)
            {
                if (_dbConnection != null)
                {
                    _dbConnection.Close();
                    _dbConnection.Dispose();
                }

                _dbConnection = new OleDbConnection(f.ConnectionString);
            }
        }

        private void connectOleDbMenuItem_Click(object sender, EventArgs e)
        {
            // Connect to a database through the OLE DB provider

            OLEDBConnectionWindow f = new OLEDBConnectionWindow();

            if (f.ShowDialog() == true)
            {
                if (_dbConnection != null)
                {
                    _dbConnection.Close();
                    _dbConnection.Dispose();
                }

                _dbConnection = new OleDbConnection(f.ConnectionString);
            }
        }

        private void connectODBCMenuItem_Click(object sender, EventArgs e)
        {
            // Connect to a database through the ODBC provider

            ODBCConnectionWindow f = new ODBCConnectionWindow();

            if (f.ShowDialog() == true)
            {
                if (_dbConnection != null)
                {
                    _dbConnection.Close();
                    _dbConnection.Dispose();
                }

                _dbConnection = new OdbcConnection(f.ConnectionString);
            }
        }

        private static DbType TypeToDbType(Type type)
        {
            if (type == typeof(string)) return DbType.String;
            if (type == typeof(short)) return DbType.Int16;
            if (type == typeof(int)) return DbType.Int32;
            if (type == typeof(long)) return DbType.Int64;
            if (type == typeof(ushort)) return DbType.UInt16;
            if (type == typeof(uint)) return DbType.UInt32;
            if (type == typeof(ulong)) return DbType.UInt64;
            if (type == typeof(bool)) return DbType.Boolean;
            if (type == typeof(float)) return DbType.Single;
            if (type == typeof(double)) return DbType.Double;
            if (type == typeof(decimal)) return DbType.Decimal;
            if (type == typeof(DateTime)) return DbType.DateTime;
            if (type == typeof(TimeSpan)) return DbType.Time;
            if (type == typeof(byte)) return DbType.Byte;
            if (type == typeof(sbyte)) return DbType.SByte;
            if (type == typeof(char)) return DbType.String;
            if (type == typeof(byte[])) return DbType.Binary;
            if (type == typeof(Guid)) return DbType.Guid;
            return DbType.Object;
        }

        private void ResetQueryBuilderMetadata()
        {
            QBuilder.MetadataProvider = null;
            QBuilder.ClearMetadata();
            QBuilder.MetadataContainer.ItemMetadataLoading -= way2ItemMetadataLoading;
        }

        private void SqlTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                // Update the query builder with manually edited query text:
                QBuilder.SQL = SqlTextBox.Text;

                // Hide error banner if any
                ShowErrorBanner(SqlTextBox, "");
            }
            catch (SQLParsingException ex)
            {
                // Set caret to error position
                SqlTextBox.SelectionStart = ex.ErrorPos.pos;

                // Show banner with error text
                ShowErrorBanner(SqlTextBox, ex.Message);
            }
        }

        private void ShowErrorBanner(FrameworkElement sqlTextBox, string text)
        {
            ErrorBox.Message = text;
        }

        private void SqlTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorBox.Message = string.Empty;
        }
    }
}
