//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2024 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using ActiveQueryBuilder.Core;
using QueryStructureDemo.Info;

namespace QueryStructureDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private string _lastValidSql;
        private int _errorPosition = -1;

        private const string CSampleSelect =
            "Select 1 as cid, Upper('2'), 3, 4 + 1, 5 + 2 IntExpression ";

        private const string CSampleSelectFromWhere =
            "Select c.CustomerId as cid, c.CompanyName, Upper(c.CompanyName), o.OrderId " +
            "From Customers c Inner Join Orders o On c.CustomerID = o.CustomerID Where o.OrderId > 0 and c.CustomerId < 10";

        private const string CSampleSelectFromJoin =
            "Select c.CustomerId as cid, Upper(c.CompanyName), o.OrderId, " +
            " p.ProductName + 1, 2+2 IntExpression From Customers c Inner Join " +
            "Orders o On c.CustomerID = o.CustomerID Inner Join " +
            "[Order Details] od On o.OrderID = od.OrderID Inner Join " +
            "Products p On p.ProductID = od.ProductID ";

        private const string CSampleSelectFromJoinSubqueries =
            "Select c.CustomerId as cid, Upper(c.CompanyName), o.OrderId, " +
            "p.ProductName + 1, 2+2 IntExpression From Customers c Inner Join " +
            "Orders o On c.CustomerID = o.CustomerID Inner Join " +
            "[Order Details] od On o.OrderID = od.OrderID Inner Join " +
            "(select pr.ProductId, pr.ProductName from Products pr) p On p.ProductID = od.ProductID ";

        private const string CSampleUnions =
            "Select c.CustomerId as cid, Upper(c.CompanyName), o.OrderId, " +
            "p.ProductName + 1, 2+2 IntExpression From Customers c Inner Join " +
            "Orders o On c.CustomerID = o.CustomerID Inner Join " +
            "[Order Details] od On o.OrderID = od.OrderID Inner Join " +
            "(select pr.ProductId, pr.ProductName from Products pr) p " +
            "On p.ProductID = od.ProductID union all " +
            "(select 1,2,3,4,5 union all select 6,7,8,9,0) union all " +
            "select (select Null as \"Null\") as EmptyValue, " +
            "SecondColumn = 2, Lower('ThirdColumn') as ThirdColumn, 0 as \"Quoted Alias\", 2+2*2 ";


        public MainWindow()
        {
            InitializeComponent();

            // set required syntax provider
            Builder.SyntaxProvider = new MSSQLSyntaxProvider();

            Loaded += MainWindow_Loaded;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;

            // Load sample database metadata from XML file
            try
            {
                Builder.MetadataLoadingOptions.OfflineMode = true;
                Builder.MetadataContainer.ImportFromXML("Northwind.xml");
                Builder.InitializeDatabaseSchemaTree();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            // load sample query
            Builder.SQL = CSampleSelectFromWhere;

        }

        private void Builder_OnSQLUpdated(object sender, EventArgs e)
        {
            // Builder generates new SQL query text. Show it to user.
            tbSQL.Text = Builder.FormattedSQL;
            _lastValidSql = tbSQL.Text;

            // update info for entire query
            UpdateQueryInfo();
        }

        private void UpdateQueryInfo()
        {
            // update Query Structure information
            UpdateQueryStatisticsInfo();

            // and update SubQueries list
            UpdateQuerySubQueriesInfo();

            // and update information for current SubQuery/UnionSubQuery
            UpdateSubQueryInfo();
        }

        private void UpdateQueryStatisticsInfo()
        {
            QueryStatistics statistics = Builder.QueryStatistics;
            StringBuilder stringBuilder = new StringBuilder();

            StatisticsInfo.DumpQueryStatisticsInfo(stringBuilder, statistics);

            tbStatistics.Text = stringBuilder.ToString();
        }

        private void UpdateQuerySubQueriesInfo()
        {
            var stringBuilder = new StringBuilder();

            SubQueriesInfo.DumpSubQueriesInfo(stringBuilder, Builder);

            tbSubQueries.Text = stringBuilder.ToString();
        }

        private void UpdateSubQueryStructureInfo()
        {
            SubQuery subQuery = Builder.ActiveUnionSubQuery.ParentSubQuery;
            StringBuilder stringBuilder = new StringBuilder();

            SubQueryStructureInfo.DumpQueryStructureInfo(stringBuilder, subQuery);

            tbSubQueryStructure.Text = stringBuilder.ToString();
        }

        private void UpdateWhereInfo()
        {
            UnionSubQuery unionSubQuery = Builder.ActiveUnionSubQuery;
            StringBuilder stringBuilder = new StringBuilder();

            SQLSubQuerySelectExpression unionSubQueryAst = unionSubQuery.ResultQueryAST;

            try
            {
                if (unionSubQueryAst.Where != null)
                {
                    WhereInfo.DumpWhereInfo(stringBuilder, unionSubQueryAst.Where);
                }
            }
            finally
            {
                unionSubQueryAst.Dispose();
            }

            tbWhere.Text = stringBuilder.ToString();
        }

        private void UpdateSelectedExpressionsInfo()
        {
            UnionSubQuery unionSubQuery = Builder.ActiveUnionSubQuery;
            StringBuilder stringBuilder = new StringBuilder();

            SelectedExpressionsInfo.DumpSelectedExpressionsInfoFromUnionSubQuery(stringBuilder, unionSubQuery);

            tbSelectedExpressions.Text = stringBuilder.ToString();
        }

        private void UpdateSubQueryInfo()
        {
            // update Query Structure information
            UpdateSubQueryStructureInfo();

            // update DataSources information
            UpdateDataSourcesInfo();

            // update Links information
            UpdateLinksInfo();

            // update Selected Expressions information
            UpdateSelectedExpressionsInfo();

            // and update WHERE clause information
            UpdateWhereInfo();
        }

        private void UpdateDataSourcesInfo()
        {
            UnionSubQuery unionSubQuery = Builder.ActiveUnionSubQuery;
            StringBuilder stringBuilder = new StringBuilder();

            DataSourcesInfo.DumpDataSourcesInfoFromUnionSubQuery(stringBuilder, unionSubQuery);

            tbDataSources.Text = stringBuilder.ToString();
        }

        private void UpdateLinksInfo()
        {
            UnionSubQuery unionSubQuery = Builder.ActiveUnionSubQuery;
            StringBuilder stringBuilder = new StringBuilder();

            LinksInfo.DumpLinksInfoFromUnionSubQuery(stringBuilder, unionSubQuery);
            
            tbLinks.Text = stringBuilder.ToString();
        }

        private void Builder_OnActiveUnionSubQueryChanged(object sender, EventArgs e)
        {
            if (Builder.ActiveUnionSubQuery == null) return;
            // update Query Structure information
            UpdateSubQueryStructureInfo();

            // update DataSources information
            UpdateDataSourcesInfo();

            // update Links information
            UpdateLinksInfo();

            // and update Selected expressions information
            UpdateSelectedExpressionsInfo();
        }

        private void SqlTextBox_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                // Update the query builder with manually edited query text:
                Builder.SQL = tbSQL.Text;
                ErrorBox.Show(null, Builder.SyntaxProvider);
            }
            catch (SQLParsingException ex)
            {
                // Show banner with error text
                ErrorBox.Show(ex.Message, Builder.SyntaxProvider);

                _errorPosition = ex.ErrorPos.pos;
            }
        }

        #region Button click

        private void btnSampleSelect_Click(object sender, EventArgs e)
        {
            Builder.SQL = CSampleSelect;
        }

        private void btnSampleSelectFromWhere_Click(object sender, RoutedEventArgs e)
        {
            Builder.SQL = CSampleSelectFromWhere;
        }

        private void btnSampleSelectFromJoin_Click(object sender, RoutedEventArgs e)
        {
            Builder.SQL = CSampleSelectFromJoin;
        }

        private void btnSampleSelectFromJoinSubqueries_Click(object sender, RoutedEventArgs e)
        {
            Builder.SQL = CSampleSelectFromJoinSubqueries;
        }

        private void btnSampleUnions_Click(object sender, RoutedEventArgs e)
        {
            Builder.SQL = CSampleUnions;
        }

        private void btnShowUnlinkedDatasourcesButton_Click(object sender, RoutedEventArgs e)
        {
            // get active UnionSubQuery
            var unionSubQuery = Builder.ActiveUnionSubQuery.ParentUnionSubQuery;

            // analize links and datasources
            var unlinkedDatasourcesInfo = UnlinkedDatasources.GetUnlinkedDataSourcesFromUnionSubQuery(unionSubQuery);

            MessageBox.Show(this, unlinkedDatasourcesInfo);
        }

        #endregion

        private void TbSQL_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorBox.Show(null, Builder.SyntaxProvider);
        }

        private void ErrorBox_OnGoToErrorPosition(object sender, EventArgs e)
        {
            tbSQL.Focus();

            if (_errorPosition == -1) return;

            if (tbSQL.LineCount != 1)
                tbSQL.ScrollToLine(tbSQL.GetLineIndexFromCharacterIndex(_errorPosition));
            tbSQL.CaretIndex = _errorPosition;
        }

        private void ErrorBox_OnRevertValidText(object sender, EventArgs e)
        {
            tbSQL.Text = _lastValidSql;
            tbSQL.Focus();
        }
    }
}
