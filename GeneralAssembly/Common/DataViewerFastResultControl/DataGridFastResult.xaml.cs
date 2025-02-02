//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2025 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Collections;
using System.Windows;
using ActiveQueryBuilder.Core;

namespace GeneralAssembly.Common.DataViewerFastResultControl
{
    public partial class DataGridFastResult
    {
        public IEnumerable ItemsSource
        {
            get { return DGrid.ItemsSource; }
            set { DGrid.ItemsSource = value; }
        }

        public string ErrorMessage
        {
            set { ErrorMessageBox.Message = value; }
            get { return ErrorMessageBox.Message; }
        }
        public DataGridFastResult()
        {
            InitializeComponent();
        }

        public void FillData(string resultSql, SQLQuery query)
        {
            try
            {
                ErrorMessage = "";

                var dv = SqlHelpers.GetDataView(resultSql, query);

                ItemsSource = dv;
                BorderSuccessExecuteQuery.Visibility = Visibility.Visible;

                TextBlockLoadedRowsCount.Text = dv == null ? "0" : dv.Table.Rows.Count.ToString();
            }
            catch (Exception exception)
            {
                BorderSuccessExecuteQuery.Visibility = Visibility.Collapsed;

                ErrorMessage = exception.Message;
                ItemsSource = null;
            }
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            BorderSuccessExecuteQuery.Visibility = Visibility.Collapsed;
        }
    }
}
