//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2019 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows;

namespace BasicDemo
{
    /// <summary>
    /// Interaction logic for QueryStatisticsWindow.xaml
    /// </summary>
    public partial class QueryStatisticsWindow
    {
        public QueryStatisticsWindow()
        {
            InitializeComponent();
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
