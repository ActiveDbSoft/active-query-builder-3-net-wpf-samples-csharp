//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2021 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.WPF;

namespace GeneralAssembly.QueryBuilderProperties
{
    /// <summary>
    /// Interaction logic for QueryBuilderPropertiesWindow.xaml
    /// </summary>
    public partial class QueryBuilderPropertiesWindow
    {

        private TextBlock _currentSelectedLink;
        private readonly QueryBuilder _queryBuilder;
        private readonly SqlSyntaxPage _sqlSyntaxPage;
        private readonly OfflineModePage _offlineModePage;
        private readonly PanesVisibilityPage _panesVisibilityPage;
        private readonly DatabaseSchemaViewPage _databaseSchemaViewPage;
        private readonly MiscellaneousPage _miscellaneousPage;
        private readonly GeneralPage _generalPage;
        private readonly SqlFormattingPage _mainQueryPage;
        private readonly SqlFormattingPage _derivedQueriesPage;
        private readonly SqlFormattingPage _expressionSubqueriesPage;

        [DefaultValue(false)]
        [Browsable(false)]
        public bool Modified
        {
            get
            {
                return _sqlSyntaxPage.Modified || _offlineModePage.Modified || _panesVisibilityPage.Modified ||
                        _databaseSchemaViewPage.Modified || _miscellaneousPage.Modified || _generalPage.Modified ||
                        _mainQueryPage.Modified || _derivedQueriesPage.Modified || _expressionSubqueriesPage.Modified;
            }
            set
            {
                _sqlSyntaxPage.Modified = value;
                _offlineModePage.Modified = value;
                _panesVisibilityPage.Modified = value;
                _databaseSchemaViewPage.Modified = value;
                _miscellaneousPage.Modified = value;
                _generalPage.Modified = value;
                _mainQueryPage.Modified = value;
                _derivedQueriesPage.Modified = value;
                _expressionSubqueriesPage.Modified = value;
            }
        }

        public QueryBuilderPropertiesWindow()
        {
            InitializeComponent();
        }

        public QueryBuilderPropertiesWindow(QueryBuilder queryBuilder)
		{
			Debug.Assert(queryBuilder != null);

			InitializeComponent();

			_queryBuilder = queryBuilder;

			BaseSyntaxProvider syntaxProvider = queryBuilder.SyntaxProvider != null
				? queryBuilder.SyntaxProvider.Clone()
				: new GenericSyntaxProvider();

            _sqlSyntaxPage = new SqlSyntaxPage(_queryBuilder, syntaxProvider);
            _offlineModePage = new OfflineModePage(_queryBuilder.SQLContext);

            _panesVisibilityPage = new PanesVisibilityPage(_queryBuilder);
            _databaseSchemaViewPage = new DatabaseSchemaViewPage(_queryBuilder);
            _miscellaneousPage = new MiscellaneousPage(_queryBuilder);

            _generalPage = new GeneralPage(_queryBuilder);
            _mainQueryPage = new SqlFormattingPage(SqlBuilderOptionsPages.MainQuery, _queryBuilder);
            _derivedQueriesPage = new SqlFormattingPage(SqlBuilderOptionsPages.DerivedQueries, _queryBuilder);
            _expressionSubqueriesPage = new SqlFormattingPage(SqlBuilderOptionsPages.ExpressionSubqueries, _queryBuilder);

            // Activate the first page
            UIElement_OnMouseLeftButtonUp(linkSqlSyntax, null);
		}

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_currentSelectedLink != null)
                _currentSelectedLink.Foreground = Brushes.Black;

            if (Equals(sender, linkSqlSyntax)) SwitchPage(_sqlSyntaxPage);
            else if (Equals(sender, linkOfflineMode)) SwitchPage(_offlineModePage);
            else if (Equals(sender, linkPanesVisibility)) SwitchPage(_panesVisibilityPage);
            else if (Equals(sender, linkMetadataTree)) SwitchPage(_databaseSchemaViewPage);
            else if (Equals(sender, linkMiscellaneous)) SwitchPage(_miscellaneousPage);
            else if (Equals(sender, linkGeneral)) SwitchPage(_generalPage);
            else if (Equals(sender, linkMainQuery)) SwitchPage(_mainQueryPage);
            else if (Equals(sender, linkDerievedQueries)) SwitchPage(_derivedQueriesPage);
            else if (Equals(sender, linkExpressionSubqueries)) SwitchPage(_expressionSubqueriesPage);

            _currentSelectedLink = (TextBlock)sender;
            _currentSelectedLink.Foreground = Brushes.Blue;
        }

        private void SwitchPage(UserControl page)
        {
            panelPages.Children.Clear();
            page.Margin = new Thickness(10, 10, 0, 0);
            panelPages.Children.Add(page);

        }

        public void ApplyChanges()
        {
            _queryBuilder.BeginUpdate();

            try
            {
                _sqlSyntaxPage.ApplyChanges();
                _offlineModePage.ApplyChanges();
                _panesVisibilityPage.ApplyChanges();
                _databaseSchemaViewPage.ApplyChanges();
                _miscellaneousPage.ApplyChanges();
                _generalPage.ApplyChanges();
                _mainQueryPage.ApplyChanges();
                _derivedQueriesPage.ApplyChanges();
                _expressionSubqueriesPage.ApplyChanges();
            }
            finally
            {
                _queryBuilder.EndUpdate();
            }

            if (_databaseSchemaViewPage.Modified || _offlineModePage.Modified)
                _queryBuilder.InitializeDatabaseSchemaTree();

            Modified = false;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            ApplyChanges();
            Close();
        }

        private void ButtonApply_OnClick(object sender, RoutedEventArgs e)
        {
            ApplyChanges();
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
