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

namespace BasicDemo.ConnectionWindow
{
    /// <summary>
    /// Interaction logic for AccessConnectionWindow.xaml
    /// </summary>
    public partial class AccessConnectionWindow
    {
        public string ConnectionString = "";

        private readonly OpenFileDialog _openFileDialog = new OpenFileDialog();

        public AccessConnectionWindow()
        {
            InitializeComponent();
        }

        private void ButtonConnect_OnClick(object sender, RoutedEventArgs e)
        {
            ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0";
            ConnectionString += ";Data Source=" + textboxDatabase.Text;
            ConnectionString += ";User Id=" + textboxUserName.Text;
            ConnectionString += ";Password=";

            if (textboxPassword.Password.Length > 0)
            {
                ConnectionString += textboxPassword.Password + ";";
            }

            // check the connection

            using (var connection = new OleDbConnection(ConnectionString))
            {
                Mouse.OverrideCursor = Cursors.Wait;

                try
                {
                    connection.Open();
					DialogResult = true;
					Close();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message, "Connection Failure.");
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void ButtonBrowse_OnClick(object sender, RoutedEventArgs e)
        {
            if (_openFileDialog.ShowDialog() == true)
            {
                textboxDatabase.Text = _openFileDialog.FileName;
            }
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
