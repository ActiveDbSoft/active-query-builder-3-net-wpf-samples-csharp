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
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ActiveQueryBuilder.Core;

namespace DatasourceFieldsLinkingDragnDropDemo
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
                ShowErrorBanner((FrameworkElement)sender, "");
            }
            catch (SQLParsingException ex)
            {
                // Set caret to error position
                SqlEditor.SelectionStart = ex.ErrorPos.pos;
                // Report error
                ShowErrorBanner((FrameworkElement)sender, ex.Message);
            }
        }

        public void ShowErrorBanner(FrameworkElement control, string text)
        {
            // Display error banner if passed text is not empty.
            ErrorBox.Message = text;
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
            ErrorBox.Message = String.Empty;
        }
    }
}
