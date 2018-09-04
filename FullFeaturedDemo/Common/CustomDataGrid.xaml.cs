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
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.QueryTransformer;

namespace FullFeaturedDemo.Common
{
    /// <summary>
    /// Interaction logic for CustomDataGrid.xaml
    /// </summary>
    public partial class CustomDataGrid
    {
        public event EventHandler RowsLoaded;

        private string _currentTextSql;
        private bool _needCancelOperation;
        private Task<DataView> _currentTask;
        private Task<DataView> _nextTask;

        public QueryTransformer QueryTransformer { set; get; }
        public SQLQuery SqlQuery { set; get; }
        public int CountRows
        {
            get { return DataView.Items.Count; }
        }

        public CustomDataGrid()
        {
            InitializeComponent();
        }

        public void FillDataGrid(string sqlCommand)
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
                _needCancelOperation = true;
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

            var command = Helpers.CreateSqlCommand(sqlCommand, SqlQuery);

            DataTable table = new DataTable("result");

            try
            {
                using (var dbReader = command.ExecuteReader())
                {

                    for (int i = 0; i < dbReader.FieldCount; i++)
                    {
                        table.Columns.Add(dbReader.GetName(i));
                    }
                    while (dbReader.Read() && !_needCancelOperation)
                    {
                        object[] values = new object[dbReader.FieldCount];
                        dbReader.GetValues(values);
                        table.Rows.Add(values);
                    }

                    return table.DefaultView;
                }
            }
            catch (Exception ex)
            {
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
                Dispatcher.Invoke(delegate
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
            _needCancelOperation = false;

            Dispatcher.Invoke(delegate
            {
                DataView.ItemsSource = obj.Result;

                GridLoadMessage.Visibility = _currentTask != null ? Visibility.Visible : Visibility.Collapsed;

                OnRowsLoaded();
            });
            TryRunTask();
        }

        private void ShowException(Exception ex)
        {
            Dispatcher.BeginInvoke((Action) delegate
            {
                BorderError.Visibility = Visibility.Visible;
                LabelError.Text = ex.Message;
            });
        }

        private void HeaderColumn_OnClick(object sender, RoutedEventArgs e)
        {
            var headerColumn = sender as DataGridColumnHeader;
            if (headerColumn == null) return;

            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
            {
                QueryTransformer.Sortings.Clear();
            }

            HeaderData header = (HeaderData)headerColumn.Content;

            var columnForSorting = QueryTransformer.Columns.FindColumnByOriginalName(header.Title);

            SortedColumn columnSorted;

            switch (header.Sorting)
            {
                case Sorting.Asc:
                    columnSorted = columnForSorting.Desc();
                    QueryTransformer.Sortings.Add(columnSorted);
                    break;
                case Sorting.Desc:
                    QueryTransformer.Sortings.Remove(columnForSorting);
                    break;
                case Sorting.None:
                    columnSorted = columnForSorting.Asc();
                    QueryTransformer.Sortings.Add(columnSorted);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            FillDataGrid(QueryTransformer.SQL);
        }

        private void DataView_AutoGeneratedColumns(object sender, EventArgs e)
        {
            foreach (var dataGridColumn in DataView.Columns)
            {
                var data = new HeaderData { Title = dataGridColumn.SortMemberPath };
                dataGridColumn.Header = data;

                var sorting = QueryTransformer.Sortings.FirstOrDefault(sort => sort.Column.OriginalName == data.Title);

                if (sorting == null) continue;

                data.Counter = QueryTransformer.Sortings.IndexOf(sorting) + 1;

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
            _needCancelOperation = true;
            _nextTask = null;
        }

        protected virtual void OnRowsLoaded()
        {
            var handler = RowsLoaded;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
