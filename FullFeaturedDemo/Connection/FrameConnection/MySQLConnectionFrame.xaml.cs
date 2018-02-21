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
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace FullFeaturedDemo.Connection.FrameConnection
{
    /// <summary>
    /// Interaction logic for MySQLConnectionFrame.xaml
    /// </summary>
    public partial class MySQLConnectionFrame : IConnectionFrame
    {
        public MySQLConnectionFrame()
        {
            InitializeComponent();
        }

        private string _connectionString;

        public string ConnectionString
        {
            get { return GetConnectionString(); }
            set { SetConnectionString(value); }
        }

        public MySQLConnectionFrame(string connectionString)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(connectionString))
            {
                ConnectionString = connectionString;
            }
        }

        public string GetConnectionString()
        {
            try
            {
                MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
                builder.ConnectionString = _connectionString;

                builder.Server = tbServer.Text;
                builder.Database = tbDatabase.Text;
                builder.UserID = tbUserID.Text;
                builder.Password = tbPassword.Password;

                _connectionString = builder.ConnectionString;
            }
            catch
            {
            }

            return _connectionString;
        }

        public void SetConnectionString(string value)
        {
            _connectionString = value;

            if (string.IsNullOrEmpty(_connectionString)) return;
            try
            {
                var builder = new MySqlConnectionStringBuilder
                {
                    ConnectionString = _connectionString
                };

                tbServer.Text = builder.Server;
                tbDatabase.Text = builder.Database;
                tbUserID.Text = builder.UserID;
                tbPassword.Password = builder.Password;

                _connectionString = builder.ConnectionString;
            }
            catch
            {
            }
        }

        private void btnEditConnectionString_Click(object sender, EventArgs e)
        {
            var csef = new ConnectionStringEditWindow();
            
            {
                csef.ConnectionString = this.ConnectionString;

                if (csef.ShowDialog() != true) return;
                if (csef.Modified)
                {
                    ConnectionString = csef.ConnectionString;
                }
            }
        }

        public bool TestConnection()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                MySqlConnection connection = new MySqlConnection(ConnectionString);
                connection.Open();
                connection.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, Assembly.GetEntryAssembly().GetName().Name);
                return false;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            return true;
        }
    }
}
