//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using Oracle.ManagedDataAccess.Client;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GeneralDemo.ConnectionWindow
{
    /// <summary>
    /// Interaction logic for OracleConnectionWindow.xaml
    /// </summary>
    public partial class OracleConnectionWindow
    {
        public string ConnectionString = "";

        public OracleConnectionWindow()
        {
            InitializeComponent();
        }

        private void ButtonConnect_OnClick(object sender, RoutedEventArgs e)
        {
            var builder = new OracleConnectionStringBuilder {DataSource = textBoxServerName.Text};


            builder.UserID = textBoxLogin.Text;
            builder.Password = textBoxPassword.Password;

            // check the connection

            using (var connection = new OracleConnection(builder.ConnectionString))
            {
                Mouse.OverrideCursor = Cursors.Wait;

                try
                {
                    connection.Open();
                    ConnectionString = builder.ConnectionString;
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

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
