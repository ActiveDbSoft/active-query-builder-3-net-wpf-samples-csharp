//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2024 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.QueryTransformer;

namespace GeneralAssembly.Common.DataViewerControl
{
    /// <summary>
    /// Interaction logic for DataViewer.xaml
    /// </summary>
    public partial class DataViewer
    {

        private string _currentTextSql;

        private Task<DataView> _currentTask;
        private Task<DataView> _nextTask;

        private QueryTransformer _queryTransformer;
        private SQLQuery _sqlQuery;

        private SQLGenerationOptions _sqlGenerationOptions;

        public QueryTransformer QueryTransformer
        {
            get { return _queryTransformer; }
            set
            {
                _queryTransformer = value;
                if (value != null)
                    _sqlQuery = (SQLQuery)_queryTransformer.Query;
                CheckCanPagination();
            }
        }

        public SQLGenerationOptions SqlGenerationOptions
        {
            get { return _sqlGenerationOptions; }
            set
            {
                _sqlGenerationOptions = value;
                if (value != null && _queryTransformer != null)
                    _queryTransformer.SQLGenerationOptions = _sqlGenerationOptions;
            }
        }
        public SQLQuery SqlQuery
        {
            set
            {
                _sqlQuery = value;

                if (_sqlQuery == null) return;

                if (_queryTransformer != null) return;

                _queryTransformer = new QueryTransformer {Query = _sqlQuery};

                if (_sqlGenerationOptions != null)
                    _queryTransformer.SQLGenerationOptions = _sqlGenerationOptions;

                CheckCanPagination();
            }
            get { return _sqlQuery; }
        }

        public int CountRows => DataView.Items.Count;

        public DataViewer()
        {
            InitializeComponent();
        }

        private void CheckCanPagination()
        {
            // The pagination panel is displayed if the current SyntaxProvider has support for pagination
            PaginationPanel.Visibility = (QueryTransformer.IsSupportLimitCount ||
                                          QueryTransformer.IsSupportLimitOffset) && _sqlQuery.SQLContext.SyntaxProvider != null
                ? Visibility.Visible
                : Visibility.Collapsed;


            PaginationPanel.IsSupportLimitCount = QueryTransformer.IsSupportLimitCount;
            PaginationPanel.IsSupportLimitOffset = QueryTransformer.IsSupportLimitOffset;
        }

        public void FillData(string sqlCommand)
        {
            BorderError.Visibility = Visibility.Collapsed;
            ButtonBreakLoad.IsEnabled = true;

            if (_currentTextSql == sqlCommand || string.IsNullOrWhiteSpace(sqlCommand))
            {
                if (!string.IsNullOrWhiteSpace(sqlCommand)) return;

                DataView.ItemsSource = null;
                OnRowsLoaded();
                return;
            }

            _currentTextSql = sqlCommand;

            DataView.ItemsSource = null;

            var task = new Task<DataView>(() => ExecuteSql(sqlCommand));

            if (_currentTask == null)
            {
                _currentTask = task;
                TryRunTask();
            }
            else
            {
                _nextTask = task;
            }
        }

        public void Clear()
        {
            DataView.ItemsSource = null;
        }

        private DataView ExecuteSql(string sqlCommand)
        {
            if (string.IsNullOrEmpty(sqlCommand)) return null;

            if (SqlQuery.SQLContext.MetadataProvider == null)
            {
                return null;
            }

            if (!SqlQuery.SQLContext.MetadataProvider.Connected)
                SqlQuery.SQLContext.MetadataProvider.Connect();

            try
            {
                var table = SqlHelpers.GetDataView(sqlCommand, SqlQuery);

                Dispatcher.BeginInvoke((Action) delegate
                {
                    BorderSuccessExecuteQuery.Visibility = Visibility.Visible;
                    TextBlockLoadedRowsCount.Text = table.Table.Rows.Count.ToString();
                });
                
                return table;

            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke((Action) delegate
                {
                    BorderSuccessExecuteQuery.Visibility = Visibility.Collapsed;
                });
                ShowException(ex);
            }

            return null;
        }

        private void TryRunTask()
        {
            if (_currentTask != null && (_currentTask.Status == TaskStatus.Running ||
                                         _currentTask.Status == TaskStatus.WaitingToRun))
                return;
            
            if (_currentTask != null)
            {
                Dispatcher?.Invoke(delegate
                {
                    GridLoadMessage.Visibility = Visibility.Visible;
                });
                
                _currentTask.ContinueWith(TaskCompleted);

                _currentTask.Start();

                return;
            }

            if (_nextTask == null) return;
            _currentTask = _nextTask;
            _nextTask = null;

            TryRunTask();
        }

        private void TaskCompleted(Task<DataView> obj)
        {
            _currentTask = null;

            Dispatcher?.Invoke(delegate
            {
                DataView.ItemsSource = obj.Result;

                GridLoadMessage.Visibility = _currentTask != null ? Visibility.Visible : Visibility.Collapsed;

                OnRowsLoaded();
            });
            TryRunTask();
        }

        private void ShowException(Exception ex)
        {
            Dispatcher?.BeginInvoke((Action) delegate
            {
                BorderError.Visibility = Visibility.Visible;
                LabelError.Text = ex.Message;
            });
        }

        private void HeaderColumn_OnClick(object sender, RoutedEventArgs e)
        {
            if (_queryTransformer == null) return;

            var headerColumn = sender as DataGridColumnHeader;
            if (headerColumn == null) return;

            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
            {
                _queryTransformer.Sortings.Clear();
            }

            var header = (HeaderDataModel) headerColumn.Content;

            var columnForSorting = _queryTransformer.Columns.FindColumnByOriginalName(header.Title);

            SortedColumn columnSorted;

            switch (header.Sorting)
            {
                case Sorting.Asc:
                    columnSorted = columnForSorting.Desc();
                    _queryTransformer.Sortings.Add(columnSorted);
                    break;
                case Sorting.Desc:
                    _queryTransformer.Sortings.Remove(columnForSorting);
                    break;
                case Sorting.None:
                    columnSorted = columnForSorting.Asc();
                    _queryTransformer.Sortings.Add(columnSorted);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            FillData(_queryTransformer.SQL);
        }

        private void DataView_AutoGeneratedColumns(object sender, EventArgs e)
        {
            if (_queryTransformer == null) return;

            foreach (var dataGridColumn in DataView.Columns)
            {
                var data = new HeaderDataModel { Title = dataGridColumn.SortMemberPath };
                dataGridColumn.Header = data;

                var sorting = _queryTransformer.Sortings.FirstOrDefault(sort => sort.Column.OriginalName == data.Title);

                if (sorting == null) continue;

                data.Counter = _queryTransformer.Sortings.IndexOf(sorting) + 1;

                switch (sorting.SortType)
                {
                    case ItemSortType.None:
                        data.Sorting = Sorting.None;
                        break;
                    case ItemSortType.Asc:
                        data.Sorting = Sorting.Asc;
                        break;
                    case ItemSortType.Desc:
                        data.Sorting = Sorting.Desc;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void CloseImage_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            BorderError.Visibility = Visibility.Collapsed;
        }

        private void ButtonCancelExecutingSql_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentTask == null || _currentTask.Status != TaskStatus.Running) return;

            ButtonBreakLoad.IsEnabled = false;
            _nextTask = null;
        }

        protected virtual void OnRowsLoaded()
        {
            if (!PaginationPanel.IsEnabled)
                PaginationPanel.CountRows = CountRows;
            BorderBlockPagination.Visibility = Visibility.Collapsed;
        }

        private void PaginationPanel_OnCurrentPageChanged(object sender, RoutedEventArgs e)
        {
            if (_queryTransformer == null) return;

            if (PaginationPanel.CurrentPage == 1)
            {
                _queryTransformer.Skip("");
                return;
            }

            // Select next n records
            _queryTransformer.Skip(
                (PaginationPanel.PageSize * (PaginationPanel.CurrentPage - 1)).ToString());
        }

        private void PaginationPanel_OnEnabledPaginationChanged(object sender, RoutedEventArgs e)
        {
            if (_queryTransformer == null) return;
            // Turn paging on and off
            if (PaginationPanel.IsEnabled)
            {
                _queryTransformer.Take(PaginationPanel.PageSize.ToString());
            }
            else
            {
                ResetPagination();
            }
        }

        private void PaginationPanel_OnPageSizeChanged(object sender, RoutedEventArgs e)
        {
            _queryTransformer.Take(PaginationPanel.PageSize.ToString());
        }

        private void ResetPagination()
        {
            if(_queryTransformer == null) return;

            PaginationPanel.Reset();
            _queryTransformer.Skip("");
            _queryTransformer.Take("");
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            BorderSuccessExecuteQuery.Visibility = Visibility.Collapsed;
        }
    }
}
