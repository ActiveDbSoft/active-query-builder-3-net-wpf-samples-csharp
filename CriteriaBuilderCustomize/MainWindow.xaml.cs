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
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.QueryTransformer;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.CriteriaBuilder;
using ActiveQueryBuilder.View.CriteriaBuilder.CustomEditors;
using GeneralAssembly;


namespace CriteriaBuilderCustomize
{
    public partial class MainWindow
    {
        private bool _showHintConnection = true;
        private ConnectionInfo _selectedConnection;
        private readonly BaseSyntaxProvider _genericSyntaxProvider = new MSSQLSyntaxProvider();

        public MainWindow()
        {
            InitializeComponent();

            CriteriaBuilder.QueryTransformer = new QueryTransformer { Query = queryBuilder.SQLQuery };
            CriteriaBuilder.NeedEditorForValue += CriteriaBuilderOnNeedEditorForValue;
            CriteriaBuilder.NeedCustomLookupButton += CriteriaBuilderOnNeedCustomLookupButton;
            CriteriaBuilder.NeedCustomLookupControl += CriteriaBuilderOnNeedCustomLookupControl;
        }

        private ICriteriaBuilderCustomLookupControl CriteriaBuilderOnNeedCustomLookupControl(object sender, CPoint location)
        {
            if (CheckBoxLookupList.IsChecked == false) return null;
            return new CustomLookupControl();
        }

        private ICriteriaBuilderCustomLookupButton CriteriaBuilderOnNeedCustomLookupButton(object sender, CRectangle bounds)
        {
            if (CheckBoxLookupButton.IsChecked == false) return null;
            return new CustomLookupButton {Bounds = bounds};
        }

        private ICriteriaBuilderCustomEditor CriteriaBuilderOnNeedEditorForValue(object sender, KindOfType kindTypeContent, MetadataField metadataField, CRectangle bounds)
        {
            if (CheckBoxEditor.IsChecked == false) return null;

            var editor = new CustomEditor {Bounds = bounds, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left};

            return editor;
        }

        private void QueryBuilder_OnSQLUpdated(object sender, EventArgs e)
        {
            TextBoxSql.Text = queryBuilder.FormattedSQL;
        }

        private void TextBoxSql_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                queryBuilder.SQL = TextBoxSql.Text;
            }
            catch
            {
                //ignore
            }
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
            catch { }
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
        }
    }
}
