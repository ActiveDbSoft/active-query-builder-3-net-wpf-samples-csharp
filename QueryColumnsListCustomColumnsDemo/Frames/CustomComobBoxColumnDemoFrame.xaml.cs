//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2019 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.QueryView;

namespace CustomColumnsDemo.Frames
{
    /// <summary>
    /// Interaction logic for CustomComobBoxColumnDemoFrame.xaml
    /// </summary>
    public partial class CustomComobBoxColumnDemoFrame
    {
        private readonly List<string> _customValues;
        private readonly List<string> _customValuesProvider = new List<string>();

        public CustomComobBoxColumnDemoFrame()
        {
            InitializeComponent();

            // Fill the source of custom values (for demo purposes)
            for (var i = 0; i < 100; i++)
                _customValuesProvider.Add("Some Value " + i);

            _customValues = _customValuesProvider.ToList();

            // Fill query builder with demo data
            QueryBuilder1.SyntaxProvider = new MSSQLSyntaxProvider();
            QueryBuilder1.MetadataLoadingOptions.OfflineMode = true;
            QueryBuilder1.MetadataContainer.ImportFromXML("Northwind.xml");
            QueryBuilder1.InitializeDatabaseSchemaTree();
            QueryBuilder1.SQL = "select OrderID, CustomerID, OrderDate from Orders";
        }

        private void QueryBuilder1_OnQueryElementControlCreated(QueryElement owner, IQueryElementControl control)
        {
            if (!(control is IQueryColumnListControl)) return;

            var queryColumnListControl = (IQueryColumnListControl) control;
            var dataGridView = (DataGrid) queryColumnListControl.DataGrid;

            // Create custom column
            var customColumn = new DataGridComboBoxColumn()
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
                ItemsSource = _customValuesProvider,

                // Bind this column to the QueryColumnListItem.CustomData object, which is expected to be a string.
                SelectedItemBinding = new Binding("CustomData") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged}
            };

            // Insert new column to the specified position
            dataGridView.Columns.Insert(2, customColumn);

            // Handle the necessary events
            dataGridView.BeginningEdit += DataGridView_BeginningEdit;
            dataGridView.CellEditEnding += DataGridView_CellEditEnding;
            dataGridView.LoadingRow += dataGridView_LoadingRow;
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

            if (row.GetIndex() >= grid.Items.Count - 1) return;

            var item = row.Item as ActiveQueryBuilder.View.WPF.QueryView.QueryColumnListItem;
            if (item == null) return;

            // Initial setting of the custom column data
            item.CustomData = _customValues[row.GetIndex()];
        }

        /// <summary>
        /// This handler is fired when the user finishes the editing of a cell. 
        /// </summary>
        private void DataGridView_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.DisplayIndex != 2) return;

            var oldValue = _customValuesProvider[e.Row.GetIndex()];

            // get new value
            var item = e.Row.Item as ActiveQueryBuilder.View.WPF.QueryView.QueryColumnListItem;

            if (item == null) return;

            var newValue = item.CustomData as string;

            // do something with the new value
            if (oldValue != newValue)
            {
                _customValuesProvider[e.Row.GetIndex()] = newValue;
            }
        }

        /// <summary>
        /// This handler determines whether a custom column should be editable or not.
        /// </summary>
        private static void DataGridView_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            var grid = (DataGrid)sender;
            if (e.Column.DisplayIndex == 2 && e.Row.GetIndex() < grid.Items.Count - 1)
            {
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
