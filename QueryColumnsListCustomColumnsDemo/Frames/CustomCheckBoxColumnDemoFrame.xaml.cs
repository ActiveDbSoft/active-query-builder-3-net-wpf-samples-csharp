//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2025 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.QueryView;

namespace CustomColumnsDemo.Frames
{
    /// <summary>
    /// Interaction logic for CustomCheckBoxColumnDemoFrame.xaml
    /// </summary>
    public partial class CustomCheckBoxColumnDemoFrame
    {
        private readonly List<bool> _customValuesProvider = new List<bool>();

        public CustomCheckBoxColumnDemoFrame()
        {
            InitializeComponent();

            // Fill query builder with demo data
            QueryBuilder1.SyntaxProvider = new MSSQLSyntaxProvider();
            QueryBuilder1.MetadataLoadingOptions.OfflineMode = true;
            QueryBuilder1.MetadataContainer.ImportFromXML("Northwind.xml");
            QueryBuilder1.InitializeDatabaseSchemaTree();
            QueryBuilder1.SQL = "select OrderID, CustomerID, OrderDate from Orders";

            // Fill the source of custom values (for demo purposes)
            for (var i = 0; i < 100; i++)
                _customValuesProvider.Add(true);
        }

        private void QueryBuilder1_OnQueryElementControlCreated(QueryElement owner, IQueryElementControl control)
        {
            if (control is IQueryColumnListControl)
            {
                var queryColumnListControl = (IQueryColumnListControl)control;
                var dataGridView = (DataGrid)queryColumnListControl.DataGrid;

                // Create custom column
                var customColumn = new DataGridCheckBoxColumn()
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
                    // Bind this column to the CustomData field of the QueryColumnListItem model, which is expected to be boolean 
                    Binding = new Binding("CustomData") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
                    IsThreeState = false
                };

                // Insert new column to the specified position
                dataGridView.Columns.Insert(2, customColumn);

                // Handle the necessary events
                dataGridView.BeginningEdit += DataGridView_BeginningEdit;
                dataGridView.CellEditEnding += dataGridView_CellEditEnding;
                dataGridView.LoadingRow += dataGridView_LoadingRow;
            }
        }

        /// <summary>
        /// This handler is fired when the user finishes the editing of a cell. 
        /// </summary>
        void dataGridView_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.DisplayIndex != 2) return;

            bool oldValue = _customValuesProvider[e.Row.GetIndex()];

            // get new value
            var item = e.Row.Item as ActiveQueryBuilder.View.WPF.QueryView.QueryColumnListItem;

            if (item == null) return;

            bool newValue = (bool?)item.CustomData == true;

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

            
            if (row.GetIndex() >= grid.Items.Count - 1)
            {
                //Setting custom value to false for an empty row
                var it = row.Item as ActiveQueryBuilder.View.WPF.QueryView.QueryColumnListItem;

                if(it == null) return;

                it.CustomData = false;
                return;
            }

            var item = row.Item as ActiveQueryBuilder.View.WPF.QueryView.QueryColumnListItem;
            if (item == null) return;

            // Initial setting of the custom column data
            item.CustomData = _customValuesProvider[row.GetIndex()];
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
