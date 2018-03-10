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
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BasicDemo.ConnectionWindow
{
    /// <summary>
    /// Interaction logic for MSSQLConnectionWindow.xaml
    /// </summary>
    public partial class MSSQLConnectionWindow
    {
        public string ConnectionString = "";

        public MSSQLConnectionWindow()
        {
            InitializeComponent();

            comboBoxAuthentication.SelectedIndex = 0; //0 - Windows Authentication
            comboBoxDatabase.SelectedIndex = 0;

            textBoxLogin.IsEnabled = false;
            textBoxPassword.IsEnabled = false;
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
                //SQL Server Authentication
                textBoxLogin.IsEnabled = true;
                textBoxPassword.IsEnabled = true;
            }
        }

        private void ComboBoxDatabase_OnDropDownOpened(object sender, EventArgs e)
        {
            // Fill the drop down list with available database names

            var builder = new SqlConnectionStringBuilder {DataSource = textBoxServerName.Text};


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

            // try to connect
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                Mouse.OverrideCursor = Cursors.Wait;

                string currentDatabase = string.Empty;// = comboBoxDatabase.SelectedItem.ToString();

                comboBoxDatabase.Items.Clear();
                comboBoxDatabase.Items.Add("<default>");
                comboBoxDatabase.SelectedIndex = 0;

                try
                {
                    connection.Open();

                    // connected successfully
                    // retrieve available databases

                    DataTable schemaTable = connection.GetSchema("Databases");

                    foreach (DataRow r in schemaTable.Rows)
                    {
                        comboBoxDatabase.Items.Add(r[0]);
                    }

                    comboBoxDatabase.SelectedItem = currentDatabase;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Connection Failure.");
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void ButtonConnect_OnClick(object sender, RoutedEventArgs e)
        {
            var builder = new SqlConnectionStringBuilder {DataSource = textBoxServerName.Text};

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

            if (comboBoxDatabase.SelectedIndex > 0)
            {
                builder.InitialCatalog = comboBoxDatabase.SelectedItem.ToString();
            }

            // check the connection

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                Mouse.OverrideCursor = Cursors.Wait;

                try
                {
                    connection.Open();
                    ConnectionString = builder.ConnectionString;
                    DialogResult = true;
					Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Connection Failure.");
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

		private void buttonCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
    }
}
