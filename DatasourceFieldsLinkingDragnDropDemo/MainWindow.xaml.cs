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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using ActiveQueryBuilder.Core;

namespace DatasourceFieldsLinkingDragnDropDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private int _errorPosition = -1;
        private string _lastValidSql;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;

            // set syntax provider
            QBuilder.SyntaxProvider = new MSSQLSyntaxProvider();

            // Fill metadata container from the XML file. (For demonstration purposes.)
            try
            {
                QBuilder.MetadataLoadingOptions.OfflineMode = true;
                QBuilder.MetadataContainer.ImportFromXML("Northwind.xml");
                QBuilder.InitializeDatabaseSchemaTree();

                QBuilder.SQL = @"SELECT Orders.OrderID, Orders.CustomerID, Orders.OrderDate, [Order Details].ProductID,
										[Order Details].UnitPrice, [Order Details].Quantity, [Order Details].Discount
									  FROM Orders INNER JOIN [Order Details] ON Orders.OrderID = [Order Details].OrderID
									  WHERE Orders.OrderID > 0 AND [Order Details].Discount > 0";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void QBuilder_OnSQLUpdated(object sender, EventArgs e)
        {
            // Update the text of SQL query when it's changed in the query builder.
            SqlEditor.Text = QBuilder.FormattedSQL;
        }

        private void SqlEditor_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                // Feed the text from text editor to the query builder when user exits the editor.
                QBuilder.SQL = SqlEditor.Text;
                ErrorBox.Show(null, QBuilder.SyntaxProvider);
                _lastValidSql = QBuilder.FormattedSQL;
            }
            catch (SQLParsingException ex)
            {
                // Set caret to error position
                SqlEditor.CaretIndex = ex.ErrorPos.pos;
                // Report error
                ErrorBox.Show(ex.Message, QBuilder.SyntaxProvider);
                _errorPosition = ex.ErrorPos.pos;
            }
        }

        /// <summary>
        /// The handler checks if the dragged field is a part of the primary key and denies dragging if it's not the case.
        /// </summary>
        private void QBuilder_OnBeforeDatasourceFieldDrag(DataSource datasource, MetadataField field, ref bool abort)
        {
            if (CheckBoxBeforeDsFieldDrag.IsChecked != true) return;

            // deny dragging if a field isn't a part of the primary key
            if (!field.PrimaryKey)
            {
                BoxLogEvents.Text = "OnBeforeDatasourceFieldDrag for field \"" + field.Name + " \" deny" +
                                    Environment.NewLine + BoxLogEvents.Text;
                abort = true;
                return;
            }

            BoxLogEvents.Text = "OnBeforeDatasourceFieldDrag for field \"" + field.Name + " \" allow" +
                                Environment.NewLine + BoxLogEvents.Text;
        }

        /// <summary>
        /// Checking the feasibility of creating a link between the given fields.
        /// </summary>
         private void QBuilder_OnLinkDragOver(DataSource fromDataDource, MetadataField fromField, DataSource toDataSource,
            MetadataField toField, MetadataForeignKey correspondingmetadataforeignkey, ref bool abort)
        {
            if (CheckBoxLinkDragOver.IsChecked != true) return;

            // Allow creation of links between fields of the same data type.
            if (fromField.FieldType == toField.FieldType)
            {
                BoxLogEvents.Text = "OnLinkDragOver from field \"" + fromField.Name + "\" to field \"" + toField.Name +
                                    "\" allow" +
                                    Environment.NewLine + BoxLogEvents.Text;
                return;
            }

            BoxLogEvents.Text = "OnLinkDragOver from field \"" + fromField.Name + "\" to field \"" + toField.Name +
                                "\" deny" +
                                Environment.NewLine + BoxLogEvents.Text;

            abort = true;
        }

        private void SqlEditor_OnTextChanged(object sender, EventArgs e)
        {
            ErrorBox.Show(null, QBuilder.SyntaxProvider);
        }

        private void ErrorBox_OnSyntaxProviderChanged(object sender, SelectionChangedEventArgs e)
        {
            var oldSql = SqlEditor.Text;
            var caretPosition = SqlEditor.CaretIndex;

            QBuilder.SyntaxProvider = (BaseSyntaxProvider)e.AddedItems[0];
            SqlEditor.Text = oldSql;
            SqlEditor.Focus();
            SqlEditor.CaretIndex = caretPosition;
        }

        private void ErrorBox_OnGoToErrorPosition(object sender, EventArgs e)
        {
            SqlEditor.Focus();

            if (_errorPosition == -1) return;

            if (SqlEditor.LineCount != 1)
                SqlEditor.ScrollToLine(SqlEditor.GetLineIndexFromCharacterIndex(_errorPosition));
            SqlEditor.CaretIndex = _errorPosition;
        }

        private void ErrorBox_OnRevertValidText(object sender, EventArgs e)
        {
            SqlEditor.Text = _lastValidSql;
            SqlEditor.Focus();
        }
    }
}
