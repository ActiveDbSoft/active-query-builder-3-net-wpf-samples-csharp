//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2019 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Net.Mime;
using System.Windows;
using System.Windows.Controls;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.WPF;

namespace AlternateNames
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private string _lastValidSql1 = string.Empty;
        private string _lastValidSql2 = string.Empty;

        private int _errorPosition1 = -1;
        private int _errorPosition2 = -1;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;

            // set required syntax provider
            QueryBuilder1.SyntaxProvider = new DB2SyntaxProvider();

            try
            {
                // Load demo metadata from XML file
                QueryBuilder1.MetadataLoadingOptions.OfflineMode = true;
                QueryBuilder1.MetadataContainer.ImportFromXML("db2_sample_with_alt_names.xml");
                QueryBuilder1.InitializeDatabaseSchemaTree();

                // set example query text
                QueryBuilder1.SQL = "Select DEPARTMENT.DEPTNO, DEPARTMENT.DEPTNAME, DEPARTMENT.MGRNO From DEPARTMENT";
            }
            catch (QueryBuilderException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void QueryBuilder1_OnSQLUpdated(object sender, EventArgs e)
        {
            // Text of SQL query has been updated.

            // To get the formatted query text with alternate object names just use FormattedSQL property
            TextBox1.Text = QueryBuilder1.FormattedSQL;

            // To get the query text, ready for execution on SQL server with real object names just use SQL property.
            TextBox2.Text = QueryBuilder1.SQL;

            // The query text containing in SQL property is unformatted. If you need the formatted text, but with real object names, 
            // do the following:
            //			SQLFormattingOptions formattingOptions = new SQLFormattingOptions();
            //			formattingOptions.Assign(queryBuilder1.SQLFormattingOptions); // copy actual formatting options from the QueryBuilder
            //			formattingOptions.UseAltNames = false; // disable alt names
            //			TextBox2.Text = FormattedSQLBuilder.GetSQL(queryBuilder1.SQLQuery.QueryRoot, formattingOptions);
        }

        private void TextBox1_OnLostKeyboardFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                // Update the query builder with manually edited query text:
                QueryBuilder1.SQL = TextBox1.Text;

                // Hide error banner if any
                ErrorBox1.Show(null, QueryBuilder1.SyntaxProvider);
                _lastValidSql1 = TextBox1.Text;
                _errorPosition1 = -1;
            }
            catch (SQLParsingException ex)
            {
                // Set caret to error position
                TextBox1.SelectionStart = ex.ErrorPos.pos;

                // Show banner with error text
                ErrorBox1.Show(ex.Message, QueryBuilder1.SyntaxProvider);
                _errorPosition1 = ex.ErrorPos.pos;
            }
        }

        private void TextBox2_OnLostKeyboardFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                // Update the query builder with manually edited query text:
                QueryBuilder1.SQL = TextBox2.Text;

                // Hide error banner if any
                ErrorBox2.Show(null, QueryBuilder1.SyntaxProvider);

                _lastValidSql2 = TextBox2.Text;
                _errorPosition2 = -1;
            }
            catch (SQLParsingException ex)
            {
                // Set caret to error position
                TextBox2.SelectionStart = ex.ErrorPos.pos;

                // Show banner with error text
                ErrorBox2.Show(ex.Message, QueryBuilder1.SyntaxProvider);
                _errorPosition2 = ex.ErrorPos.pos;
            }
        }

        private void MenuItemAbout_OnClick(object sender, RoutedEventArgs e)
        {
            QueryBuilder.ShowAboutDialog();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ErrorBox1.Show(null, QueryBuilder1.SyntaxProvider);
            ErrorBox2.Show(null, QueryBuilder1.SyntaxProvider);
        }

        private void ErrorBox2_OnSyntaxProviderChanged(object sender, SelectionChangedEventArgs e)
        {
            var oldSql = TextBox2.Text;
            var caretPosition = TextBox2.CaretIndex;

            QueryBuilder1.SyntaxProvider = (BaseSyntaxProvider)e.AddedItems[0];
            TextBox2.Text = oldSql;
            TextBox2.Focus();
            TextBox2.CaretIndex = caretPosition;
        }

        private void ErrorBox2_OnRevertValidTextEvent(object sender, EventArgs e)
        {
            TextBox2.Text = _lastValidSql2;
            ErrorBox2.Show(null, QueryBuilder1.SyntaxProvider);
            TextBox2.Focus();
        }

        private void ErrorBox2_OnGoToErrorPositionEvent(object sender, EventArgs e)
        {
            TextBox2.Focus();

            if (_errorPosition2 == -1) return;
            if (TextBox2.LineCount != 1)
                TextBox2.ScrollToLine(TextBox1.GetLineIndexFromCharacterIndex(_errorPosition2));
            TextBox2.CaretIndex = _errorPosition2;
        }

        private void ErrorBox1_OnSyntaxProviderChanged(object sender, SelectionChangedEventArgs e)
        {
            var oldSql = TextBox1.Text;
            var caretPosition = TextBox1.CaretIndex;

            QueryBuilder1.SyntaxProvider = (BaseSyntaxProvider)e.AddedItems[0];
            TextBox1.Text = oldSql;
            TextBox1.Focus();
            TextBox1.CaretIndex = caretPosition;
        }

        private void ErrorBox1_OnRevertValidTextEvent(object sender, EventArgs e)
        {
            TextBox1.Text = _lastValidSql1;
            TextBox1.Focus();
        }

        private void ErrorBox1_OnGoToErrorPositionEvent(object sender, EventArgs e)
        {
            TextBox1.Focus();

            if (_errorPosition1 != -1)
            {
                if (TextBox2.LineCount != 1)
                    TextBox1.ScrollToLine(TextBox1.GetLineIndexFromCharacterIndex(_errorPosition1));
                TextBox1.CaretIndex = _errorPosition1;
            }
        }
    }
}
