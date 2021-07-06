//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2021 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows;

namespace GeneralAssembly.Windows.QueryInformationWindows
{
    /// <summary>
    /// Interaction logic for QueryStatisticsWindow.xaml
    /// </summary>
    public partial class QueryStatisticsWindow : Window
    {
        public QueryStatisticsWindow(string text)
        {
            InitializeComponent();
            textBox.Text = text;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
