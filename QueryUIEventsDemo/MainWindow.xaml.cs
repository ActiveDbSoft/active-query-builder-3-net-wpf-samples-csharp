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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.QueryView;

namespace QueryUIEventsDemo
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

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;

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
            // Display error banner if passed text is not empty
            ErrorBox.Message = text;
        }

        /// <summary>
        /// Prompts the user if he/she really wants to add an object to the query.
        /// </summary>
        private void QBuilder_OnDataSourceAdding(MetadataObject metadataObject, ref bool abort)
        {
            if (CbDataSourceAdding.IsChecked != true) return;

            BoxLogEvents.Text = "DataSourceAdding: adding object \"" + metadataObject.Name + "\"" +
                        Environment.NewLine + BoxLogEvents.Text;

            var name = metadataObject.Name;
            var msg = "Are you sure to add \"" + name + "\" to the query?";

            if (MessageBox.Show(msg, "DataSourceAdding event handler", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                abort = true;
            }

        }

        /// <summary>
        /// Displays a prompt on deleting an object from the query.
        /// </summary>
        private void QBuilder_OnDataSourceDeleting(DataSource datasource, ref bool abort)
        {
            if (CbDataSourceDeleting.IsChecked != true) return;
            var name = datasource.NameInQuery;
            var msg = "Are you sure to remove \"" + name + "\" from the query?";

            if (MessageBox.Show(msg, "DataSourceDeleting event handler", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                abort = true;
            }
        }

        /// <summary>
        /// Prompts the user if he/she really wants to add a field to the SELECT list.
        /// </summary>
        private void QBuilder_OnDataSourceFieldAdding(DataSource datasource, MetadataField field, ref bool abort)
        {
            if (CbDataSourceFieldAdding.IsChecked != true) return;

            BoxLogEvents.Text = "DatasourceFieldAdding adding field \"" + field.Name + "\"" +
                           Environment.NewLine + BoxLogEvents.Text;

            var msg = "Are you sure to add \"" + field.Name + "\" to the SELECT list?";

            if (MessageBox.Show(msg, "DatasourceFieldAdding event handler", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                abort = true;
            }
        }

        // Displays a prompt on deleting a field from the SELECT list.
        private void QBuilder_OnDataSourceFieldRemoving(DataSource datasource, MetadataField field, ref bool abort)
        {
            if (CbDataSourceFieldRemoving.IsChecked != true) return;
            BoxLogEvents.Text = "DataSourceFieldRemoving removing field \"" + field.Name + "\" form \"" +
                                datasource.NameInQuery + "\"" +
                                Environment.NewLine + BoxLogEvents.Text;
            var name = datasource.NameInQuery;
            var msg = "Are you sure to remove \"" + "\".\"" + name + "\"" + field.Name + "\" from the query?";

            if (MessageBox.Show(msg, "DataSourceFieldRemoving event handler", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                abort = true;
            }
        }

        //
        // GridCellValueChanging event allows to prevent the cell value changing or modify the new cell value.
        // Note: Some columns hide/unhide dynamically but this does not affect the column index in the event parameters -
        //       it includes hidden columns.
        //
        private void QBuilder_OnQueryColumnListItemChanging(QueryColumnList querycolumnlist, QueryColumnListItem querycolumnlistitem, 
            QueryColumnListItemProperty property, int conditionindex, object oldvalue, ref object newValue, ref bool abort)
        {
            if (CbQueryColumnListItemChanging.IsChecked != true) return;
            BoxLogEvents.Text = "QueryColumnListItemChanging Changing the value for  property \"" + property + "\"" +
                                Environment.NewLine + BoxLogEvents.Text;

            if (property == QueryColumnListItemProperty.Expression) // Prevent changes in the Expression column.
            {

                abort = true;
                return;
            }

            if (property != QueryColumnListItemProperty.Alias) return;

            var s = newValue as string;
            if (s == null) return;

            if (s.Length <= 0 || s.StartsWith("_")) return;

            s = "_" + s;
            newValue = s;
        }

        //
        // LinkCreating event allows to prevent link creation
        //
        private void QBuilder_OnLinkCreating(DataSource fromDatasource, MetadataField fromField, DataSource toDatasource, MetadataField toField, MetadataForeignKey correspondingmetadataforeignkey, ref bool abort)
        {
            if (CbLinkCreating.IsChecked != true) return;

            BoxLogEvents.Text = "LinkCreating" +
                             Environment.NewLine + BoxLogEvents.Text;

            var msg = string.Format("Do you want to create the link between {0}.{1} and {2}.{3}?",
                                      fromDatasource.MetadataObject.Name, fromField.Name, toDatasource.MetadataObject.Name, toField.Name);

            if (MessageBox.Show(msg, "LinkCreating event handler", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                abort = true;
            }
        }

        //
        // LinkDeleting event allows to prevent link deletion.
        //
        private void QBuilder_OnLinkDeleting(Link link, ref bool abort)
        {
            if (CbLinkDeleting.IsChecked != true) return;

            BoxLogEvents.Text = "LinkDeleting" +
                                Environment.NewLine + BoxLogEvents.Text;

            var msg = string.Format("Do you want to delete the link between {0} and {1}?",
                link.LeftField, link.RightField);

            if (MessageBox.Show(msg, "LinkDeleting event handler", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                abort = true;
            }
        }

        private void QBuilder_OnQueryColumnListItemChanged(QueryColumnList querycolumnlist,
            QueryColumnListItem querycolumnlistitem, QueryColumnListItemProperty property, int conditionindex,
            object newvalue)
        {
            if (CbQueryColumnListItemChanged.IsChecked != true) return;

            BoxLogEvents.Text = "QueryColumnListItemChanged property \"" + property + "\" changed" +
                                Environment.NewLine + BoxLogEvents.Text;
        }

        private void QBuilder_OnLinkChanged(Link link)
        {
            if (CbLinkChanged.IsChecked != true) return;

            BoxLogEvents.Text = "LinkChanged" +
                               Environment.NewLine + BoxLogEvents.Text;
        }

        /// <summary>
        /// Denies changing of the link properties by the user.
        /// </summary>
        private void QBuilder_OnLinkChanging(Link link, ref bool abort)
        {
            if (CbLinkCreated.IsChecked == true)
            {
                BoxLogEvents.Text = "\"LinkChanging\" deny" +
                                    Environment.NewLine + BoxLogEvents.Text;

                abort = true;
                return;
            }

            BoxLogEvents.Text = "\"LinkChanging\" allow" +
                                    Environment.NewLine + BoxLogEvents.Text;
        }

        private void QBuilder_OnDatasourceFieldRemoved(DataSource datasource, MetadataField field)
        {
            if (CbDatasourceFieldRemoved.IsChecked != true) return;

            BoxLogEvents.Text = "DatasourceFieldRemoved " +
                               Environment.NewLine + BoxLogEvents.Text;
        }

        private void QBuilder_OnDataSourceFieldAdded(DataSource datasource, MetadataField field, QueryColumnListItem querycolumnlistitem, bool isFocused)
        {
            if (CbDataSourceFieldAdded.IsChecked != true) return;

            BoxLogEvents.Text = "LinkCreated" +
                               Environment.NewLine + BoxLogEvents.Text;
        }

        private void QBuilder_OnDataSourceAdded(SQLQuery sender, DataSource addedobject)
        {
            if (CbDataSourceAdded.IsChecked != true) return;

            BoxLogEvents.Text = "LinkCreated" +
                               Environment.NewLine + BoxLogEvents.Text;
        }

        private void QBuilder_OnLinkCreated(SQLQuery sender, Link link)
        {
            if (CbLinkCreated.IsChecked != true) return;

            BoxLogEvents.Text = "LinkCreated" +
                               Environment.NewLine + BoxLogEvents.Text;
        }

        private void QBuilder_OnQueryColumnListItemAdded(object sender, QueryColumnListItem item)
        {
            BoxLogEvents.Text = "QueryColumnListItemAdded [" + item.ExpressionString + "]" +
                                Environment.NewLine + BoxLogEvents.Text;
        }

        private void QBuilder_OnQueryColumnListItemRemoving(object sender, QueryColumnListItem item, ref bool abort)
        {
            if(CbQclRemoving.IsChecked != true) return;

            BoxLogEvents.Text = "QueryColumnListItemRemoving [" + item.ExpressionString + "]" +
                               Environment.NewLine + BoxLogEvents.Text;

            var answer = MessageBox.Show(this,
                "Do you want to delete the QueryColumnListItem [" + item.ExpressionString + "]",
                "QueryColumnListItem removing", MessageBoxButton.YesNo);

            abort = answer == MessageBoxResult.No;
        }

        private void ButtonCheckAll_OnClick(object sender, RoutedEventArgs e)
        {
            SwitchCheckBox(true);
        }

        private void ButtonUncheckAll_OnClick(object sender, RoutedEventArgs e)
        {
            SwitchCheckBox(false);
        }

        private void SwitchCheckBox(bool isChecked)
        {
            var collection = FindVisualChildren<CheckBox>(CheckBoxPanel).ToList();

            foreach (var checkBox in collection)
            {
                checkBox.IsChecked = isChecked;
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T)
                {
                    yield return (T)child;
                }

                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }

        private void SqlEditor_OnTextChanged(object sender, EventArgs e)
        {
            ErrorBox.Message = string.Empty;
        }
    }
}
