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
using System.Data;
using System.Windows;
using System.Windows.Controls;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.WPF;
using GeneralAssembly;
using GeneralAssembly.Connection;

namespace LoadMetadataDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private ConnectionInfo _selectedConnection = new ConnectionInfo();
        private string _lastValidSql;
        private int _errorPosition = -1;
        private readonly EventMetadataProvider _way3EventMetadataProvider = new EventMetadataProvider();

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
                    if (types.Contains(MetadataType.Schema))
                    {
                        // only one "dbo" schema should be at the root level
                        if (item.Items.FindItem<MetadataNamespace>("dbo", MetadataType.Schema) == null)
                            item.AddSchema("dbo");
                    }
                    break;

                case MetadataType.Schema:
                    if (item.Name == "dbo" && types.Contains(MetadataType.Table))
                    {
                        item.AddTable("Orders");
                        item.AddTable("Order Details");
                    }
                    break;

                case MetadataType.Table:
                    if (item.Name == "Orders")
                    {
                        if (types.Contains(MetadataType.Field))
                        {
                            item.AddField("OrderId");
                            item.AddField("CustomerId");
                        }
                    }
                    else if (item.Name == "Order Details")
                    {
                        if (types.Contains(MetadataType.Field))
                        {
                            item.AddField("OrderId");
                            item.AddField("ProductId");
                        }

                        if (types.Contains(MetadataType.ForeignKey))
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
            var dbConnection = _selectedConnection?.ConnectionDescriptor?.MetadataProvider?.Connection;
            if (dbConnection != null)
            {
                try
                {
                    dbConnection.Close();
                    dbConnection.Open();

                    // allow QueryBuilder to request metadata
                    QBuilder.MetadataLoadingOptions.OfflineMode = false;

                    ResetQueryBuilderMetadata();

                    QBuilder.MetadataProvider = _way3EventMetadataProvider;
                    QBuilder.InitializeDatabaseSchemaTree();
                }
                catch (System.Exception ex)
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

            var dbConnection = _selectedConnection?.ConnectionDescriptor?.MetadataProvider?.Connection;

            if (dbConnection != null)
            {
                IDbCommand command = dbConnection.CreateCommand();
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
            _lastValidSql = QBuilder.FormattedSQL;
            // Hide error banner if any
            ErrorBox.Show(null, QBuilder.SyntaxProvider);

            // update the text box
            SqlTextBox.Text = QBuilder.FormattedSQL;
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
                ErrorBox.Show(null, QBuilder.SyntaxProvider);
            }
            catch (SQLParsingException ex)
            {
                // Set caret to error position
                SqlTextBox.SelectionStart = ex.ErrorPos.pos;
                _errorPosition = ex.ErrorPos.pos;
                // Show banner with error text
                ErrorBox.Show(ex.Message, QBuilder.SyntaxProvider);
            }
        }

        private void SqlTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorBox.Show(null, QBuilder.SyntaxProvider);
        }

        private void ErrorBox_OnGoToErrorPosition(object sender, EventArgs e)
        {
            SqlTextBox.Focus();

            if (_errorPosition == -1) return;

            if (SqlTextBox.LineCount != 1)
                SqlTextBox.ScrollToLine(SqlTextBox.GetLineIndexFromCharacterIndex(_errorPosition));
            SqlTextBox.CaretIndex = _errorPosition;
        }

        private void ErrorBox_OnRevertValidText(object sender, EventArgs e)
        {
            SqlTextBox.Text = _lastValidSql;
            SqlTextBox.Focus();
        }

        private void MenuItemConnectTo_OnClick(object sender, RoutedEventArgs e)
        {
            var cf = new Connection.DatabaseConnectionWindow() { Owner = this };

            if (cf.ShowDialog() != true) return;
            _selectedConnection = cf.SelectedConnection;

            InitializeSqlContext();
        }

        private void InitializeSqlContext()
        {
            try
            {
                QBuilder.Clear();

                BaseMetadataProvider metadataProvider = null;

                if (_selectedConnection == null) return;

                // create new SqlConnection object using the connections string from the connection form
                if (!_selectedConnection.IsXmlFile)
                    metadataProvider = _selectedConnection.ConnectionDescriptor?.MetadataProvider;

                // setup the query builder with metadata and syntax providers
                QBuilder.SQLContext.MetadataContainer.Clear();
                QBuilder.MetadataProvider = metadataProvider;
                QBuilder.SyntaxProvider = _selectedConnection.ConnectionDescriptor?.SyntaxProvider;
                QBuilder.MetadataLoadingOptions.OfflineMode = metadataProvider == null;

                if (metadataProvider == null)
                {
                    QBuilder.MetadataContainer.ImportFromXML(_selectedConnection.ConnectionString);
                }

                // Instruct the query builder to fill the database schema tree
                QBuilder.InitializeDatabaseSchemaTree();

            }
            finally { }
        }
    }
}
