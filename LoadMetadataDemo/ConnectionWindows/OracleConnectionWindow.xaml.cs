//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Data.OracleClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LoadMetadataDemo.ConnectionWindows
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
            comboBoxAuthentication.SelectedIndex = 1; //1 - Oracle Server Authentication
        }

        private void ButtonConnect_OnClick(object sender, RoutedEventArgs e)
        {
            OracleConnectionStringBuilder builder = new OracleConnectionStringBuilder
            {
                DataSource = textBoxServerName.Text
            };

            if (comboBoxAuthentication.SelectedIndex == 0)
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.IntegratedSecurity = false;
                builder.UserID = textBoxLogin.Text;
                builder.Password = textBoxPassword.Password;
            }

            // check the connection

            using (OracleConnection connection = new OracleConnection(builder.ConnectionString))
            {
                Mouse.OverrideCursor = Cursors.Wait;

                try
                {
                    connection.Open();
                    ConnectionString = builder.ConnectionString;
                }
                catch (Exception ex)
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

        private void ComboBoxAuthentication_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxAuthentication.SelectedIndex == 0)
            {
                //Windows Authentication
                textBoxLogin.IsEnabled = false;
                textBoxPassword.IsEnabled = false;
            }
            else
            {
                //Oracle Server Authentication
                textBoxLogin.IsEnabled = true;
                textBoxPassword.IsEnabled = true;
            }
        }
    }
}
