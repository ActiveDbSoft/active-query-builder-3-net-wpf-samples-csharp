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

namespace LoadMetadataDemo.ConnectionWindows
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
            Loaded += MSSQLConnectionWindow_Loaded;
            
        }

        void MSSQLConnectionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            comboBoxAuthentication.SelectedIndex = 0;
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

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = textBoxServerName.Text;

            if (comboBoxAuthentication.SelectedIndex == 0)
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.IntegratedSecurity = false;
                builder.UserID = textBoxLogin.Text;
                builder.Password = textBoxPassword.Text;
            }

            // try to connect
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                Mouse.OverrideCursor = Cursors.Wait;

                string currentDatabase = comboBoxDatabase.SelectedItem.ToString();

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

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            builder.DataSource = textBoxServerName.Text;

            if (comboBoxAuthentication.SelectedIndex == 0)
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.IntegratedSecurity = false;
                builder.UserID = textBoxLogin.Text;
                builder.Password = textBoxPassword.Text;
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

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
