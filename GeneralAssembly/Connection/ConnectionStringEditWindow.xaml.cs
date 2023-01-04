//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2023 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows;

namespace GeneralAssembly.Connection
{
    /// <summary>
    /// Interaction logic for ConnectionStringEditWindow.xaml
    /// </summary>
    public partial class ConnectionStringEditWindow
    {
        public string ConnectionString
        {
            get { return tbConnectionString.Text; }
            set
            {
                tbConnectionString.Text = value;
                Modified = false;
            }
        }

        public bool Modified { private set; get; }

        public ConnectionStringEditWindow()
        {
            InitializeComponent();
            Modified = true;
        }

        private void ButtonBaseOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonBaseClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
