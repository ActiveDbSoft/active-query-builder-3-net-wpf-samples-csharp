//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Data.OleDb;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace LoadMetadataDemo.ConnectionWindows
{
    /// <summary>
    /// Interaction logic for AccessConnectionWindow.xaml
    /// </summary>
    public partial class AccessConnectionWindow
    {
        public string ConnectionString = "";
        OpenFileDialog _openFileDialog1;

        public AccessConnectionWindow()
        {
            InitializeComponent();
        }

        private void ButtonConnect_OnClick(object sender, RoutedEventArgs e)
        {
            ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0";
            ConnectionString += ";Data Source=" + textboxDatabase.Text;
            ConnectionString += ";User Id=" + textboxUserName.Text;
            ConnectionString += ";Password=";

            if (textboxPassword.Password.Length > 0)
            {
                ConnectionString += textboxPassword.Password + ";";
            }

            // check the connection

            using (OleDbConnection connection = new OleDbConnection(ConnectionString))
            {
                Mouse.OverrideCursor = Cursors.Wait;

                try
                {
                    connection.Open();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message, "Connection Failure.");
                    DialogResult = true;
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonOpenDb_OnClick(object sender, RoutedEventArgs e)
        {
            _openFileDialog1 = new OpenFileDialog();
            if (_openFileDialog1.ShowDialog() == true)
            {
                textboxDatabase.Text = _openFileDialog1.FileName;
            }
        }
    }
}
