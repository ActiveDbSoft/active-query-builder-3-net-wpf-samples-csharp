//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Windows;

namespace FullFeaturedMdiDemo
{
    /// <summary>
    /// Interaction logic for QueryParametersWindow.xaml
    /// </summary>
    public partial class QueryParametersWindow
    {
        private readonly DbCommand _command;
        private readonly ObservableCollection<DataGridItem> _source;

        public QueryParametersWindow()
        {
            InitializeComponent();
        }

        public QueryParametersWindow(DbCommand command)
        {
            _source = new ObservableCollection<DataGridItem>();
            _command = command;

            InitializeComponent();

            for (var i = 0; i < _command.Parameters.Count; i++)
            {
                var p = _command.Parameters[i];

                _source.Add(new DataGridItem(p.ParameterName, p.DbType, p.Value));
            }
            GridData.ItemsSource = _source;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            for (var i = 0; i < _command.Parameters.Count; i++)
                _command.Parameters[i].Value = _source[i].Value;

            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }

    class DataGridItem
    {
        public string ParameterName { set; get; }
        public DbType DataType { set; get; }
        public object Value { set; get; }

        public DataGridItem(string parametrName, DbType dataType, object value)
        {
            ParameterName = parametrName;
            DataType = dataType;
            Value = value;
        }
    }
}
