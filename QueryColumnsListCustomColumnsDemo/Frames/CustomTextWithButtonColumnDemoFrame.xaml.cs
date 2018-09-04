//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.QueryView;
using ActiveQueryBuilder.View.WPF.QueryView;

namespace CustomColumnsDemo.Frames
{
    /// <summary>
    /// Interaction logic for CustomTextWithButtonColumnDemoFrame.xaml
    /// </summary>
    public partial class CustomTextWithButtonColumnDemoFrame
    {
        private readonly List<string> _customValuesProvider = new List<string>();

        public CustomTextWithButtonColumnDemoFrame()
        {
            InitializeComponent();

            // Fill query builder with demo data
            QueryBuilder1.SyntaxProvider = new MSSQLSyntaxProvider();
            QueryBuilder1.MetadataLoadingOptions.OfflineMode = true;
            QueryBuilder1.MetadataContainer.ImportFromXML("Northwind.xml");
            QueryBuilder1.InitializeDatabaseSchemaTree();
            QueryBuilder1.SQL = "select OrderID, CustomerID, OrderDate from Orders";

            // Fill custom values source (for demo purposes)
            for (var i = 0; i < 100; i++)
                _customValuesProvider.Add("Some Value " + i);
        }

        private void QueryBuilder1_OnQueryElementControlCreated(QueryElement owner, IQueryElementControl control)
        {
            if (control is IQueryColumnListControl)
            {
                var queryColumnListControl = (IQueryColumnListControl)control;
                var dataGridView = (DataGrid) queryColumnListControl.DataGrid;

                // Create binding and templates for the custom column
                var textBlock = new FrameworkElementFactory(typeof (TextBlock));
                textBlock.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
                textBlock.SetValue(MarginProperty, new Thickness(2,0,0,0));
                textBlock.SetBinding(TextBlock.TextProperty,
                    new Binding("CustomData")
                    {
                        Mode = BindingMode.OneWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    });

                // Creating a template to browse a cell
                var templateCell = new DataTemplate {VisualTree = textBlock};

                var columnLeft = new FrameworkElementFactory(typeof(ColumnDefinition));
                columnLeft.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));

                var columnRight = new FrameworkElementFactory(typeof(ColumnDefinition));
                columnRight.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Auto));

                var gridRoot = new FrameworkElementFactory(typeof(Grid));
                gridRoot.SetValue(BackgroundProperty, Brushes.White);
                gridRoot.AppendChild(columnLeft);
                gridRoot.AppendChild(columnRight);

                var textBox = new FrameworkElementFactory(typeof(TextBox));
                textBox.SetBinding(TextBox.TextProperty,
                    new Binding("CustomData")
                    {
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    });
                textBox.SetValue(MarginProperty, new Thickness(2, 0, 0, 0));
                textBox.SetValue(BorderThicknessProperty, new Thickness(0));
                textBox.SetValue(VerticalContentAlignmentProperty, VerticalAlignment.Center);
                textBox.SetValue(Grid.ColumnProperty, 0);

                // Defining a handler which is fired on loading the editor for a cell
                textBox.AddHandler(LoadedEvent, new RoutedEventHandler(LoadTextBox));
                gridRoot.AppendChild(textBox);

                var button = new FrameworkElementFactory(typeof(Button));
                button.SetValue(Grid.ColumnProperty, 1);
                button.SetValue(MarginProperty, new Thickness(0,2,2,2));
                button.SetValue(ContentProperty, "...");
                button.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
                button.SetValue(VerticalContentAlignmentProperty, VerticalAlignment.Center);
                button.SetValue(WidthProperty, 16.0);

                // Defining a handler of cliking the ellipsis button in a cell
                button.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ClickButton));
                gridRoot.AppendChild(button);

                // Creating a template to edit the custom cell
                var templateCellEdit = new DataTemplate {VisualTree = gridRoot};

                var customColumn = new DataGridTemplateColumn()
                {
                    Header = "Custom Column",
                    Width = new DataGridLength(200),
                    HeaderStyle =
                        new Style
                        {
                            Setters =
                            {
                                new Setter(FontFamilyProperty, new FontFamily("Tahoma")),
                                new Setter(FontWeightProperty, FontWeights.Bold)
                            }
                        },
                    // assigning templates to a column
                    CellTemplate = templateCell,
                    CellEditingTemplate = templateCellEdit
                };

                // Insert new column to the specified position
                dataGridView.Columns.Insert(2, customColumn);

                // Handle the necessary events
                dataGridView.BeginningEdit += dataGridView_BeginningEdit;
                dataGridView.CellEditEnding += dataGridView_CellEditEnding;
                dataGridView.LoadingRow += dataGridView_LoadingRow;
            }
        }

        /// <summary>
        /// The text editor load handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void LoadTextBox(object sender, RoutedEventArgs e)
        {
            // setting focus to the text box and selecting its content
            var textBox = (TextBox)sender;
            if (string.IsNullOrEmpty(textBox.Text)) return;
            Keyboard.Focus(textBox);
            textBox.SelectionStart = 0;
            textBox.SelectionLength = textBox.Text.Length;
        }

        /// <summary>
        /// The ellipsis button click event handler
        /// </summary>
        private static void ClickButton(object sender, RoutedEventArgs e)
        {
            var row = FindParent<DataGridRow>((FrameworkElement) sender);

            MessageBox.Show(string.Format("Button at row {0} clicked.", row.GetIndex()));
        }

        /// <summary>
        /// This handler is fired when the user finishes the editing of a cell. 
        /// </summary>
        void dataGridView_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.DisplayIndex != 2) return;

            var oldValue = _customValuesProvider[e.Row.GetIndex()];
            
            // get new value
            var item = e.Row.Item as ActiveQueryBuilder.View.WPF.QueryView.QueryColumnListItem;

            if (item == null) return;

            var newValue = item.CustomData.ToString();

            // do something with the new value
            if (oldValue != newValue)
            {
                _customValuesProvider[e.Row.GetIndex()] = newValue;
            }
        }

        private void dataGridView_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Loaded += row_Loaded;
        }

        /// <summary>
        /// Defining the initial value for a newly added row.
        /// </summary>
        private void row_Loaded(object sender, RoutedEventArgs e)
        {
            var row = (DataGridRow)sender;
            row.Loaded -= row_Loaded;

            var grid = FindParent<DataGrid>(row);

            if ((row.GetIndex() >= grid.Items.Count - 1))
                return;
            
            var item = row.Item as ActiveQueryBuilder.View.WPF.QueryView.QueryColumnListItem;
            if (item == null) return;

            // Initial setting of the custom column data
            item.CustomData = _customValuesProvider[row.GetIndex()];
        }

        /// <summary>
        /// This handler determines whether a custom column should be editable or not.
        /// </summary>
        private void dataGridView_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            var grid = (DataGrid)sender;
            if (e.Column.DisplayIndex == 2 && e.Row.GetIndex() < grid.Items.Count - 1)
            {
                // Make cell editable
                e.Cancel = false; // Change to "true" if you need a read-only cell.	
            }
        }

        public static T FindParent<T>(DependencyObject from) where T : class
        {
            T result = null;
            var parent = VisualTreeHelper.GetParent(from);

            if (parent is T)
                result = parent as T;
            else if (parent != null)
                result = FindParent<T>(parent);

            return result;
        }
    }
}
