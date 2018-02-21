//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Collections;

namespace FullFeaturedMdiDemo.Common
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
            set { ErrorMessageBox.Text = value; }
            get { return ErrorMessageBox.Text; }
        }
        public DataGridFastResult()
        {
            InitializeComponent();
        }
    }
}
