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
using System.Text;
using System.Windows;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.WPF;
using Microsoft.Win32;

namespace SeparatedComponents
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly SQLQuery _sqlQuery;
        private readonly SQLContext _sqlContext;

        public MainWindow()
        {
            //sample query
            var builder = new StringBuilder();
            builder.AppendLine("Select Categories.CategoryID,");
            builder.AppendLine("Categories.CategoryName,");
            builder.AppendLine("Categories.Description,");
            builder.AppendLine("Categories.Picture,");
            builder.AppendLine("Products.CategoryID As CategoryID1,");
            builder.AppendLine("Products.QuantityPerUnit");
            builder.AppendLine("From Categories");
            builder.AppendLine("Inner Join Products On Categories.CategoryID = Products.CategoryID");

            InitializeComponent();

            _sqlContext = new SQLContext
            {
                SyntaxProvider = new MSSQLSyntaxProvider {ServerVersion = MSSQLServerVersion.MSSQL2012},
                LoadingOptions = {OfflineMode = true}
            };

            // Load demo metadata from XML file
            _sqlContext.MetadataContainer.ImportFromXML("Northwind.xml");

            databaseSchemaView1.SQLContext = _sqlContext;
            databaseSchemaView1.QueryView = QueryView1;
            databaseSchemaView1.InitializeDatabaseSchemaTree();

            _sqlQuery = new SQLQuery(_sqlContext);
            _sqlQuery.SQLUpdated += sqlQuery_SQLUpdated;
            // set example query text
            _sqlQuery.SQL = builder.ToString();

            QueryView1.Query = _sqlQuery;

            NavBar.Query = _sqlQuery;
            NavBar.QueryView = QueryView1;

            sqlTextEditor.Query = _sqlQuery;
            sqlTextEditor.Text = builder.ToString();
        }

        private void sqlQuery_SQLUpdated(object sender, EventArgs e)
        {
            // Text of SQL query has been updated.
            // To get the query text, ready for execution on SQL server with real object names just use SQL property.
            sqlTextEditor.Text = FormattedSQLBuilder.GetSQL(_sqlQuery.QueryRoot, new SQLFormattingOptions());
        }

        private void MenuItemLoadMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*"
            };

            // Load metadata from XML file
            if (openFileDialog.ShowDialog() != true) return;

            _sqlContext.LoadingOptions.OfflineMode = true;
            _sqlContext.MetadataContainer.ImportFromXML(openFileDialog.FileName);
            databaseSchemaView1.InitializeDatabaseSchemaTree();
        }

        private void MenuItemSaveMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                FileName = "Metadata.xml",
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*"
            };

            // Save metadata to XML file
            if (saveFileDialog.ShowDialog() != true) return;

            _sqlContext.MetadataContainer.LoadAll(true);
            _sqlContext.MetadataContainer.ExportToXML(saveFileDialog.FileName);
        }

        private void MenuItemOpenQueryStatistic_OnClick(object sender, RoutedEventArgs e)
        {
            var builder = new StringBuilder();

            var queryStatistics = _sqlQuery.QueryStatistics;

            builder.Append("Used Objects (").Append(queryStatistics.UsedDatabaseObjects.Count).AppendLine("):");
            builder.AppendLine();

            for (var i = 0; i < queryStatistics.UsedDatabaseObjects.Count; i++)
                builder.AppendLine(queryStatistics.UsedDatabaseObjects[i].ObjectName.QualifiedName);

            builder.AppendLine().AppendLine();
            builder.Append("Used Columns (").Append(queryStatistics.UsedDatabaseObjectFields.Count).AppendLine("):");
            builder.AppendLine();

            for (var i = 0; i < queryStatistics.UsedDatabaseObjectFields.Count; i++)
                builder.AppendLine(queryStatistics.UsedDatabaseObjectFields[i].FullName.QualifiedName);

            builder.AppendLine().AppendLine();
            builder.Append("Output Expressions (").Append(queryStatistics.OutputColumns.Count).AppendLine("):");
            builder.AppendLine();

            for (var i = 0; i < queryStatistics.OutputColumns.Count; i++)
                builder.AppendLine(queryStatistics.OutputColumns[i].Expression);

            MessageBox.Show(builder.ToString());
        }

        private void MenuItemOpenAbout_OnClick(object sender, RoutedEventArgs e)
        {
            QueryBuilder.ShowAboutDialog();
        }

        private void SqlTextEditor_OnLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                // Update the query builder with manually edited query text:
                _sqlQuery.SQL = sqlTextEditor.Text;
                ErrorBox.Message = string.Empty;
            }
            catch (SQLParsingException ex)
            {
                // Set caret to error position
                sqlTextEditor.SelectionStart = ex.ErrorPos.pos;

                // Show banner with error text
                ErrorBox.Message = ex.Message;
            }
        }

        private void SqlTextEditor_OnTextChanged(object sender, EventArgs e)
        {
            if (ErrorBox != null)
                ErrorBox.Message = string.Empty;
        }
    }
}
