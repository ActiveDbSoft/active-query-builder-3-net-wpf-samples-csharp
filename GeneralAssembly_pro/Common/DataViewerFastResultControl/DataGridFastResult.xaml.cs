//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2021 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Collections;
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
            }
            catch (Exception exception)
            {
                ErrorMessage = exception.Message;
                ItemsSource = null;
            }
        }
    }
}
