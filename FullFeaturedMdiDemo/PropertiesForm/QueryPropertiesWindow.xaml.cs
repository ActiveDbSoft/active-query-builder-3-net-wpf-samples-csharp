//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ActiveQueryBuilder.Core;

namespace FullFeaturedMdiDemo.PropertiesForm
{
    /// <summary>
    /// Interaction logic for QueryPropertiesWindow.xaml
    /// </summary>
    public partial class QueryPropertiesWindow
    {

        private TextBlock _currentSelectedLink;
        private readonly SqlSyntaxPage _sqlSyntaxPage;
        private readonly OfflineModePage _offlineModePage;
        private readonly GeneralPage _generalPage;
        private readonly SqlFormattingPage _mainQueryPage;
        private readonly SqlFormattingPage _derievedQueriesPage;
        private readonly SqlFormattingPage _expressionSubqueriesPage;

        [DefaultValue(false)]
        [Browsable(false)]
        public bool Modified
        {
            get
            {
                return _sqlSyntaxPage.Modified || _offlineModePage.Modified || _generalPage.Modified ||
                        _mainQueryPage.Modified || _derievedQueriesPage.Modified || _expressionSubqueriesPage.Modified;
            }
            set
            {
                buttonApply.IsEnabled = value;
                _sqlSyntaxPage.Modified = value;
                _offlineModePage.Modified = value;

                _generalPage.Modified = value;
                _mainQueryPage.Modified = value;
                _derievedQueriesPage.Modified = value;
                _expressionSubqueriesPage.Modified = value;
            }
        }

        public QueryPropertiesWindow()
        {
            InitializeComponent();
        }

        public QueryPropertiesWindow(SQLContext sqlContext, SQLFormattingOptions sqlFormattingOptions)
        {
            InitializeComponent();

            BaseSyntaxProvider syntaxProvider = sqlContext.SyntaxProvider != null
                ? sqlContext.SyntaxProvider.Clone()
                : new GenericSyntaxProvider();

            _sqlSyntaxPage = new SqlSyntaxPage(sqlContext, syntaxProvider);
            _offlineModePage = new OfflineModePage(sqlContext);


            _generalPage = new GeneralPage(sqlFormattingOptions);
            _mainQueryPage = new SqlFormattingPage(SqlBuilderOptionsPages.MainQuery, sqlFormattingOptions);
            _derievedQueriesPage = new SqlFormattingPage(SqlBuilderOptionsPages.DerievedQueries, sqlFormattingOptions);
            _expressionSubqueriesPage = new SqlFormattingPage(SqlBuilderOptionsPages.ExpressionSubqueries, sqlFormattingOptions);

            // Activate the first page
            UIElement_OnMouseLeftButtonUp(linkSqlSyntax, null);
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_currentSelectedLink != null)
                _currentSelectedLink.Foreground = Brushes.Black;

            if (Equals(sender, linkSqlSyntax)) SwitchPage(_sqlSyntaxPage);
            else if (Equals(sender, linkOfflineMode)) SwitchPage(_offlineModePage);
            else if (Equals(sender, linkGeneral)) SwitchPage(_generalPage);
            else if (Equals(sender, linkMainQuery)) SwitchPage(_mainQueryPage);
            else if (Equals(sender, linkDerievedQueries)) SwitchPage(_derievedQueriesPage);
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
            _sqlSyntaxPage.ApplyChanges();
            _offlineModePage.ApplyChanges();
            _generalPage.ApplyChanges();
            _mainQueryPage.ApplyChanges();
            _derievedQueriesPage.ApplyChanges();
            _expressionSubqueriesPage.ApplyChanges();

            Modified = false;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            ApplyChanges();
            DialogResult = false;
        }

        private void ButtonApply_OnClick(object sender, RoutedEventArgs e)
        {
            ApplyChanges();
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
