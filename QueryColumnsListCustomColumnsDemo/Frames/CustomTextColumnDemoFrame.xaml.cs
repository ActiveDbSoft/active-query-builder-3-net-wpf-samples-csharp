//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2021 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.QueryView;
using CustomColumnsDemo.Annotations;

namespace CustomColumnsDemo.Frames
{
    /// <summary>
    /// Interaction logic for CustomTextColumnDemoFrame.xaml
    /// </summary>
    public partial class CustomTextColumnDemoFrame
    {
        private readonly ObservableCollection<CustomTextData> _customValuesProvider = new ObservableCollection<CustomTextData>();

        public CustomTextColumnDemoFrame()
        {
            InitializeComponent();

            // Fill the query builder with demo data
            QueryBuilder1.SyntaxProvider = new MSSQLSyntaxProvider();
            QueryBuilder1.MetadataLoadingOptions.OfflineMode = true;
            QueryBuilder1.MetadataContainer.ImportFromXML("Northwind.xml");
            QueryBuilder1.InitializeDatabaseSchemaTree();
            QueryBuilder1.SQL = "select OrderID, CustomerID, OrderDate from Orders";

            // Fill the source of custom values (for the demo purposes)
            for (var i = 0; i < 100; i++)
                _customValuesProvider.Add(new CustomTextData { Description = "Some Value " + i });
        }

        private void QueryBuilder1_OnQueryElementControlCreated(QueryElement queryElement, IQueryElementControl queryElementControl)
        {
            if (!(queryElementControl is IQueryColumnListControl)) return;

            var queryColumnListControl = (IQueryColumnListControl)queryElementControl;
            var dataGridView = (DataGrid)queryColumnListControl.DataGrid;

            // Create custom column
            var customColumn = new DataGridTextColumn
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
                // Bind this column to the QueryColumnListItem.CustomData object, which must contain an object with the Description field (defined below)
                Binding = new Binding("CustomData.Description") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged }
            };

            // Insert new column to the specified position
            dataGridView.Columns.Insert(2, customColumn);

            // Handle the necessary events
            dataGridView.BeginningEdit += dataGridView_BeginningEdit;
            dataGridView.CellEditEnding += dataGridView_CellEditEnding;
            dataGridView.LoadingRow += dataGridView_LoadingRow;
        }

        /// <summary>
        /// This handler is fired when the user finishes the editing of a cell. 
        /// </summary>
        private void dataGridView_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.DisplayIndex != 2) return;

            var oldValue = _customValuesProvider[e.Row.GetIndex()];

            // get new value
            var item = e.Row.Item as ActiveQueryBuilder.View.WPF.QueryView.QueryColumnListItem;

            if (item == null) return;

            var newValue = item.CustomData as CustomTextData;

            // do something with the new value
            if (oldValue.Description != newValue.Description) 
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
            var exists = grid.Columns[2].GetCellContent(row) as TextBlock;

            if ((row.GetIndex() >= grid.Items.Count - 1) || exists == null) return;

            var item = row.Item as ActiveQueryBuilder.View.WPF.QueryView.QueryColumnListItem;
            if (item == null) return;

            // Initial setting of the custom column data
            item.CustomData = _customValuesProvider[row.GetIndex()];
        }

        /// <summary>
        /// This handler determines whether a custom column should be editable or not.
        /// </summary>
        private static void dataGridView_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            var grid = (DataGrid) sender;
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

        private class CustomTextData : INotifyPropertyChanged
        {
            private string _description;

            public string Description
            {
                set
                {
                    _description = value;
                    OnPropertyChanged("Description");
                }
                get { return _description; }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            private void OnPropertyChanged(string propertyName)
            {
                var handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }


}
