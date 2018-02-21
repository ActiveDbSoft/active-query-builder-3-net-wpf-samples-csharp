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
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.WPF;

namespace AlternateNames
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

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

        private void TextBox1_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                // Update the query builder with manually edited query text:
                QueryBuilder1.SQL = TextBox1.Text;

                // Hide error banner if any
                ShowErrorBanner(TextBox1, "");
            }
            catch (SQLParsingException ex)
            {
                // Set caret to error position
                TextBox1.SelectionStart = ex.ErrorPos.pos;

                // Show banner with error text
                ShowErrorBanner(TextBox1, ex.Message);
            }
        }

        private void TextBox2_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                // Update the query builder with manually edited query text:
                QueryBuilder1.SQL = TextBox2.Text;

                // Hide error banner if any
                ShowErrorBanner(TextBox2, "");
            }
            catch (SQLParsingException ex)
            {
                // Set caret to error position
                TextBox2.SelectionStart = ex.ErrorPos.pos;

                // Show banner with error text
                ShowErrorBanner(TextBox2, ex.Message);
            }
        }

        public void ShowErrorBanner(FrameworkElement control, string text)
        {
            // Show new banner if text is not empty
            if (Equals(control, TextBox1))
            {
                ErrorBox1.Message = text;
            }
            if (Equals(control, TextBox2))
            {
                ErrorBox1.Message = text;
            }
        }


        private void MenuItemAbout_OnClick(object sender, RoutedEventArgs e)
        {
            QueryBuilder.ShowAboutDialog();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ErrorBox1.Message = string.Empty;
            ErrorBox2.Message = string.Empty;
        }
    }
}
