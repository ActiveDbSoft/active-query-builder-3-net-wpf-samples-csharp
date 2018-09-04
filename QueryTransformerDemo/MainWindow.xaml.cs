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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.QueryTransformer;

namespace QueryTransformerDemo
{
    public partial class MainWindow
    {
        private SQLContext _sqlContext;
        private SQLQuery _sqlQuery;
        private QueryTransformer _queryTransformer;

        // List of query output columns of the current SQL query used for turning their visibility on and off
        private ObservableCollection<VisibleColumn> _collectionColumns;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void LoadQuery()
        {
            // Clear the input fields
            ClearFieldsSorting();
            ClearFieldsAggregate();
            ClearFieldsWhere();

            // Load a query into the SQLQuery object. 
            _sqlQuery.SQL = BoxSourceSql.Text;

            // Set counter values
            CounterSortingActive.Text = _queryTransformer.Sortings.Count.ToString();
            CounterAggregations.Text = _queryTransformer.Aggregations.Count.ToString();
            CounterWhere.Text = _queryTransformer.Filters.Count.ToString();

            _collectionColumns.Clear();

            // Fill the list of output columns to be used as ItemsSource for a combobox
            foreach (var column in _queryTransformer.Columns)
                _collectionColumns.Add(new VisibleColumn(column));

            ListBoxVisibleColumns.ItemsSource = _collectionColumns;

            RefreshBindings();

            CounterVisibleColumn.Text = _collectionColumns.Count(x => x.Visible).ToString();
        }

        private void RefreshBindings()
        {
            ComboBoxColumnsSorting.ItemsSource = null;
            ComboBoxColumnAggregate.ItemsSource = null;
            ComboBoxColumnWhere.ItemsSource = null;

            ComboBoxColumnsSorting.ItemsSource = _queryTransformer.Columns;
            ComboBoxColumnAggregate.ItemsSource = _queryTransformer.Columns;
            ComboBoxColumnWhere.ItemsSource = _queryTransformer.Columns;
        }

        private void ClearFieldsSorting()
        {
            ComboBoxColumnsSorting.SelectedItem = null;
            ComboBoxSorting.SelectedItem = null;
        }

        private void ClearFieldsAggregate()
        {
            ComboBoxColumnAggregate.SelectedItem = null;
            ComboBoxFunction.SelectedItem = null;
        }

        private void ClearFieldsWhere()
        {
            ComboBoxColumnWhere.SelectedItem = null;
            ComboBoxConditions.SelectedItem = null;
            ComboBoxConditions.SelectedItem = null;
            BoxWhereValue.Text = string.Empty;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;

            _collectionColumns = new ObservableCollection<VisibleColumn>();
            _collectionColumns.CollectionChanged += _collectionColumns_CollectionChanged;

            _sqlContext = new SQLContext
            {
                SyntaxProvider = new MSSQLSyntaxProvider { ServerVersion = MSSQLServerVersion.MSSQL2012 },
                LoadingOptions = { OfflineMode = true }
            };
            _sqlContext.MetadataContainer.ImportFromXML("Northwind.xml");

            // Load a sample query
            var sqlText = new StringBuilder();
            sqlText.AppendLine("Select Categories.CategoryName,");
            sqlText.AppendLine("Products.QuantityPerUnit");
            sqlText.AppendLine("From Categories");
            sqlText.AppendLine("Inner Join Products On Categories.CategoryID = Products.CategoryID");

            _sqlQuery = new SQLQuery(_sqlContext) ;
            _sqlQuery.SQLUpdated += _sqlQuery_SQLUpdated;
            _sqlQuery.SQL = sqlText.ToString();
            _queryTransformer = new QueryTransformer { Query = _sqlQuery };
            
            _queryTransformer.SQLUpdated += _queryTransformer_SQLUpdated;
            _queryTransformer.SQLGenerationOptions = new SQLFormattingOptions();
            LoadQuery();
        }

        private void _sqlQuery_SQLUpdated(object sender, EventArgs e)
        {
            BoxSourceSql.Text = _sqlQuery.SQL;
        }

        private void _queryTransformer_SQLUpdated(object sender, EventArgs e)
        {
            // Get the transformed SQL query text
            BoxResultSql.Text = _queryTransformer.SQL;
        }

        private void _collectionColumns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (INotifyPropertyChanged item in e.NewItems)
                    item.PropertyChanged += ColumnVisibleChanged;

            if (e.OldItems != null)
                foreach (INotifyPropertyChanged item in e.OldItems)
                    item.PropertyChanged -= ColumnVisibleChanged;
        }

        private void ColumnVisibleChanged(object sender, PropertyChangedEventArgs e)
        {
            CounterVisibleColumn.Text = _collectionColumns.Count(x => x.Visible).ToString();
        }

        private void ButtonLoad_OnClick(object sender, RoutedEventArgs e)
        {
            // Load a query and updating the form controls
            LoadQuery();
        }

        private void ButtonClearVisibleColumn_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var column in _collectionColumns)
                column.Visible = true;
        }

        private void ButtonAddSorting_OnClick(object sender, RoutedEventArgs e)
        {
            var column = (OutputColumn)ComboBoxColumnsSorting.SelectedItem;
            if (!column.IsSupportSorting) return;

            SortedColumn sortedColumn = null;
            if (ComboBoxSorting.SelectedValue == null) return;

            switch (ComboBoxSorting.SelectedValue.ToString())
            {
                case "Asc":
                    sortedColumn = column.Asc();
                    break;
                case "Desc":
                    sortedColumn = column.Desc();
                    break;
            }

            // Add sorting to the query - the sort order of original query will be overridden.
            _queryTransformer.OrderBy(sortedColumn);

            CounterSortingActive.Text = _queryTransformer.Sortings.Count.ToString();

            ClearFieldsSorting();

        }

        private void ButtonClearSorting_OnClick(object sender, RoutedEventArgs e)
        {
            ClearFieldsSorting();

            // Remove the added sortings from the query - the original sort order will be restored.
            _queryTransformer.Sortings.Clear();

            CounterSortingActive.Text = _queryTransformer.Sortings.Count.ToString();
        }

        private void ButtonAddAggregate_OnClick(object sender, RoutedEventArgs e)
        {
            var column = (OutputColumn)ComboBoxColumnAggregate.SelectedItem;

            AggregatedColumn aggregatedColumn = null;
            switch (ComboBoxFunction.SelectedItem.ToString())
            {
                case "Count":
                    aggregatedColumn = column.Count();
                    break;
                case "Avg":
                    aggregatedColumn = column.Avg();
                    break;
                case "Sum":
                    aggregatedColumn = column.Sum();
                    break;
                case "Min":
                    aggregatedColumn = column.Min();
                    break;
                case "Max":
                    aggregatedColumn = column.Max();
                    break;
            }

            // Add an aggregate to the query - if any aggregates are added, all other columns will be removed from the query.
            _queryTransformer.Aggregations.Add(aggregatedColumn);

            CounterAggregations.Text = _queryTransformer.Aggregations.Count.ToString();

            ClearFieldsAggregate();
        }

        private void ButtonClearAggregate_Click(object sender, RoutedEventArgs e)
        {
            // Clear all aggregates from the query - the columns of original query will be restored.
            _queryTransformer.Aggregations.Clear();

            CounterAggregations.Text = _queryTransformer.Aggregations.Count.ToString();

            ClearFieldsAggregate();
        }

        private void ButtonAddWhere_OnClick(object sender, RoutedEventArgs e)
        {
            var column = (OutputColumn)ComboBoxColumnWhere.SelectedItem;

            FilterCondition condition = null;
            switch (ComboBoxConditions.SelectedItem.ToString())
            {
                case "Equal":
                    condition = column.Equal(BoxWhereValue.Text);
                    break;
                case "Not Equal":
                    condition = column.Not_Equal(BoxWhereValue.Text);
                    break;
                case "Greater":
                    condition = column.Greater(BoxWhereValue.Text);
                    break;
                case "GreaterEqual":
                    condition = column.GreaterEqual(BoxWhereValue.Text);
                    break;
                case "Not Grater":
                    condition = column.Not_Greater(BoxWhereValue.Text);
                    break;
                case "Not GreaterEqual":
                    condition = column.Not_GreaterEqual(BoxWhereValue.Text);
                    break;
                case "IsNull":
                    condition = column.IsNull();
                    break;
                case "Not IsNull":
                    condition = column.Not_IsNull();
                    break;
                case "IsNotNull":
                    condition = column.IsNotNull();
                    break;
                case "Less":
                    condition = column.Less(BoxWhereValue.Text);
                    break;
                case "LessEqual":
                    condition = column.LessEqual(BoxWhereValue.Text);
                    break;
                case "Not Less":
                    condition = column.Not_Less(BoxWhereValue.Text);
                    break;
                case "Not LessEqual":
                    condition = column.Not_LessEqual(BoxWhereValue.Text);
                    break;
                case "In":
                    condition = column.In(BoxWhereValue.Text);
                    break;
                case "Not In":
                    condition = column.Not_In(BoxWhereValue.Text);
                    break;
                case "Like":
                    condition = column.Like(BoxWhereValue.Text);
                    break;
                case "Not Like":
                    condition = column.Not_Like(BoxWhereValue.Text);
                    break;
            }

            // Add new filter to the query - the filter will be added to the WHERE clause of original query.
            _queryTransformer.Filters.Add(condition);

            CounterWhere.Text = _queryTransformer.Filters.Count.ToString();

            ClearFieldsWhere();
        }

        private void ButtonClearWhere_OnClick(object sender, RoutedEventArgs e)
        {
            // Remove all additional filters from query - the original WHERE clause will be restored.
            _queryTransformer.Filters.Clear();

            CounterWhere.Text = _queryTransformer.Filters.Count.ToString();

            ClearFieldsWhere();
        }

        private void ButtonGetCode_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new Window
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Width = 600,
                Height = 300, ResizeMode = ResizeMode.NoResize, 
                Title = "Code Behind"
            };

            var grid = new Grid
            {
                RowDefinitions = { new RowDefinition(), new RowDefinition {Height = GridLength.Auto} }
            };

            var buttonClose = new Button
            {
                Content = "Close",
                Margin = new Thickness(0, 0, 10, 10),
                HorizontalAlignment = HorizontalAlignment.Right,
                Width = 75,
                Height = 23
            };

            buttonClose.Click += (o, args) =>
            {
                window.Close();
            };

            buttonClose.SetValue(Grid.RowProperty, 1);

            var textbox = new TextBox
            {
                Margin = new Thickness(10),
                Text = GetCodeBehind(),
                Background = new SolidColorBrush(SystemColors.InfoColor)
            };
            textbox.SetValue(Grid.RowProperty, 0);

            window.Content = grid;
            grid.Children.Add(textbox);
            grid.Children.Add(buttonClose);
            window.ShowDialog();
        }

        private string GetCodeBehind()
        {
            var builder = new StringBuilder();
            builder.AppendLine("_queryTransformer");

            foreach (var sorting in _queryTransformer.Sortings)
            {
                var text = string.Format("\t.OrderBy(_queryTransformer.Columns[{0}], {1})",
                 _queryTransformer.Columns.IndexOf(sorting.Column),
                 (sorting.SortType == ItemSortType.Asc).ToString().ToLower(CultureInfo.CurrentCulture));
                builder.AppendLine(text);
            }

            var reg = new Regex("([A-Z])\\w+");
            foreach (var aggregation in _queryTransformer.Aggregations)
            {
                var result = reg.Match(aggregation.Expression);
                if(!result.Success) continue;

                var text = string.Format("\t.Select(_queryTransformer.Columns[{0}].{1}())",
                    _queryTransformer.Columns.IndexOf(aggregation.Column), result.Value);
                builder.AppendLine(text);
            }

            foreach (var filter in _queryTransformer.Filters)
            {

                var text = string.Format("\t.Where(\"{0}\")", filter.Condition);
                builder.AppendLine(text);
            }

            return builder.ToString();
        }
    }

    public class VisibleColumn : INotifyPropertyChanged
    {
        private readonly OutputColumn _column;

        public bool Visible
        {
            get
            {
                return _column != null && _column.Visible;
            }
            set
            {
                if (_column == null) return;
                _column.Visible = value;
                OnPropertyChanged("Visible");
            }
        }

        public string Name
        {
            get
            {
                return _column == null ? string.Empty : _column.Column.Expression;
            }
        }

        public VisibleColumn(OutputColumn column)
        {
            _column = column;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
