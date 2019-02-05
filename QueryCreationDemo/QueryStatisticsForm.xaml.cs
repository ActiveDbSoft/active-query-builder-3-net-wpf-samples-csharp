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

namespace QueryCreationDemo
{
    /// <summary>
    /// Interaction logic for QueryStatisticsForm.xaml
    /// </summary>
    public partial class QueryStatisticsForm : Window
    {
        public QueryStatisticsForm()
        {
            InitializeComponent();
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
