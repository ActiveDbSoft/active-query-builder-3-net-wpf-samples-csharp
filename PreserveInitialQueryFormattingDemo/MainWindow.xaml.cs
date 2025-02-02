//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2025 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using ActiveQueryBuilder.Core;
using Microsoft.Win32;

namespace PreserveInitialQueryFormattingDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private bool _isCanUpdateSqlText = true;

        private string _defaultSql = "-- Any text for comment\n" +
                                         "Select Categories.CategoryName,\n" +
                                         "  Products.UnitPrice\n" +
                                         "From Categories\n" +
                                         "  Inner Join Products On Categories.CategoryID = Products.CategoryID";

        public MainWindow()
        {
            InitializeComponent();
            InitializeQueryBuilder();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) => TextBoxSql.Text = _defaultSql;

        private void InitializeQueryBuilder()
        {
            QBuilder.MetadataContainer.LoadingOptions.OfflineMode = true;
            QBuilder.SyntaxProvider = new MSSQLSyntaxProvider();
            QBuilder.MetadataContainer.ImportFromXML("Northwind.xml");
            QBuilder.InitializeDatabaseSchemaTree();
            QBuilder.SQL = _defaultSql;
        }

        private void TextBoxSql_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => SilentAssignSqlText(TextBoxSql.Text);

        private void QBuilder_OnSQLUpdated(object sender, EventArgs e)
        {
            if (!_isCanUpdateSqlText) 
                return;

            TextBoxSql.Text = QBuilder.FormattedSQL;
        }

        private void SilentAssignSqlText(string text)
        {
            _isCanUpdateSqlText = false;

            SqlErrorBox.Show("", QBuilder.SyntaxProvider);

            try
            {
                QBuilder.SQL = text;
            }
            catch (Exception ex)
            {
                SqlErrorBox.Show(ex.Message, QBuilder.SyntaxProvider);
            }

            _isCanUpdateSqlText = true;
        }

        private void ButtonOpenSql_OnClick(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            
            if(openDialog.ShowDialog() != true || !File.Exists(openDialog.FileName)) 
                return;

            var sqlText = string.Empty;

            using (var reader = new StreamReader(openDialog.FileName))
                sqlText = reader.ReadToEnd();

            TextBoxSql.Text = sqlText;
            SilentAssignSqlText(sqlText);
        }
    }
}
