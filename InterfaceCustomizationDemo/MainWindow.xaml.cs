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
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.NavigationBar;
using ActiveQueryBuilder.View.QueryView;
using ActiveQueryBuilder.View.WPF.QueryView;
using QueryColumnListItem = ActiveQueryBuilder.Core.QueryColumnListItem;

namespace InterfaceCustomizationDemo
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
                ShowErrorBanner((FrameworkElement) sender, "");
            }
            catch (SQLParsingException ex)
            {
                // Set caret to error position
                SqlEditor.SelectionStart = ex.ErrorPos.pos;
                // Report error
                ShowErrorBanner((FrameworkElement) sender, ex.Message);
            }
        }

        public void ShowErrorBanner(FrameworkElement control, string text)
        {
            // Display error banner if passed text is not empty
            ErrorBox.Message = text;
        }

        private void QBuilder_OnQueryElementControlCreated(QueryElement owner, IQueryElementControl control)
        {
            if (QElementCreated.IsChecked != true) return;

            BoxLogEvents.Text = "QueryElementControl Created \"" + control.GetType().Name + "\"" +
                                Environment.NewLine + BoxLogEvents.Text;

            if (!(control is DataSourceControl)) return;

            var dsc = (DataSourceControl) control;
            var element = (UserControl)control;

            var root = (Grid)element.FindName("PART_CONTENT"); // Grid
            var list = (ListView)element.FindName("ListBoxView"); // ListView

            if (root == null || list == null) return;

            var customItemTemplate = (DataTemplate)FindResource("CustomFieldTemplate");

            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            list.ItemTemplate = customItemTemplate;
            list.SetValue(Grid.RowProperty, 1);

            var border = new Border
            {
                BorderThickness = new Thickness(0, 0, 0, 1),
                BorderBrush = Brushes.Gray,
                Background = SystemColors.InfoBrush
            };

            var grid = new Grid
            {
                Margin = new Thickness(3, 0, 3, 0),
                Height = 24,
                VerticalAlignment = VerticalAlignment.Center
            };

            border.SetValue(Grid.RowProperty,0);

            border.Child = grid;
            root.Children.Add(border);

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            #region Search box and button
            var textSearchBox = new TextBox
            {
                VerticalContentAlignment = VerticalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var buttonSearchClear = new Button
            {
                Content = "X",
                FontSize = 10,
                Padding = new Thickness(5, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(3, 0, 0, 0),
                VerticalContentAlignment = VerticalAlignment.Center,
                IsEnabled = false
            };

            textSearchBox.SetValue(Grid.ColumnProperty, 0);

            // Filtering the list of fields in the DataSourceControl
            textSearchBox.TextChanged += delegate
            {
                list.Items.Filter =
                    x =>
                    {
                        var dataSourceControlItem= x as DataSourceControlItem;
                        return dataSourceControlItem != null && dataSourceControlItem.Name.Text.ToLower().Contains(textSearchBox.Text.ToLower());
                    };
                buttonSearchClear.IsEnabled = !string.IsNullOrWhiteSpace(textSearchBox.Text);

                dsc.Refresh();
            };

            grid.Children.Add(textSearchBox);

            buttonSearchClear.Click += (sender, args) => textSearchBox.Text = "";

            buttonSearchClear.SetValue(Grid.ColumnProperty, 1);
            #endregion
            grid.Children.Add(buttonSearchClear);
        }

        private void QBuilder_OnQueryElementControlDestroying(QueryElement owner, IQueryElementControl control)
        {
            if (QElementDestroying.IsChecked != true) return;

            BoxLogEvents.Text = "QueryElementControl Destroying \"" + control.GetType().Name + "\"" +
                                Environment.NewLine + BoxLogEvents.Text;
        }

        /// <summary>
        /// ValidateContextMenu event allows to modify or hide any query builder context menu.
        /// </summary>
        private void QBuilder_OnValidateContextMenu(object sender, QueryElement queryelement, ICustomContextMenu menu)
        {
            if (ValidateContextMenu.IsChecked != true) return;

            BoxLogEvents.Text = "OnValidateContextMenu" +
                                Environment.NewLine + BoxLogEvents.Text;
            // Insert custom menu item to the top of any context menu
            menu.InsertItem(0, "Custom Item 1", CustomItem1EventHandler);
            menu.InsertSeparator(1); // separator

            if (queryelement is Link) // Link context menu
            {
                // Add another item in the Link's menu
                menu.AddSeparator(); // separator
                menu.AddItem("Custom Item 2", CustomItem2EventHandler);
            }
            else if (queryelement is DataSourceObject) // Datasource context menu
            {
            }
            else if (queryelement is UnionSubQuery)
            {
                if (sender is IDesignPaneControl) // diagram pane context menu
                {
                }
                else if (sender is NavigationPopupBase)
                {
                }
            }
            else if (queryelement is QueryColumnListItem) // QueryColumnListControl context menu
            {
                menu.ClearItems();
            }
        }

        private static void CustomItem1EventHandler(object sender, EventArgs e)
        {
            MessageBox.Show("Custom Item 1");
        }

        private static void CustomItem2EventHandler(object sender, EventArgs e)
        {
            MessageBox.Show("Custom Item 2");
        }

        private void QBuilder_OnCustomizeDataSourceCaption(DataSource datasource, ref string caption)
        {
            if (CustomizeDataSourceCaption.IsChecked != true) return; 

            BoxLogEvents.Text = "CustomizeDataSourceCaption for \"" + caption + "\"" +
                                Environment.NewLine + BoxLogEvents.Text;
            caption = caption.ToUpper(CultureInfo.CurrentCulture);
        }

        private void QBuilder_OnCustomizeDataSourceFieldList(DataSource datasource, List<FieldListItemData> fieldlist)
        {
            if (CustomizeDataSourceFieldList.IsChecked != true) return;

            BoxLogEvents.Text = "CustomizeDataSourceFieldList" +
                                Environment.NewLine + BoxLogEvents.Text;
            var comparer = new FieldComparerByName();
            fieldlist.Sort(comparer);
        }

        private void SqlEditor_OnTextChanged(object sender, EventArgs e)
        {
            ErrorBox.Message = string.Empty;
        }
    }

    public class FieldComparerByName : IComparer<FieldListItemData>
    {
        public int Compare(FieldListItemData x, FieldListItemData y)
        {
            return string.Compare(x.Name.ToLower(CultureInfo.CurrentCulture), y.Name.ToLower(CultureInfo.CurrentCulture),
                StringComparison.Ordinal);
        }
    }
}
