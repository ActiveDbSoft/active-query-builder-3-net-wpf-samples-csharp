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
using Npgsql;

namespace FullFeaturedDemo.Connection.FrameConnection
{
    /// <summary>
    /// Interaction logic for PostgreSQLConnectionFrame.xaml
    /// </summary>
    public partial class PostgreSQLConnectionFrame :IConnectionFrame
    {
        public PostgreSQLConnectionFrame()
        {
            InitializeComponent();
        }
        private string _connectionString;

        public string ConnectionString
        {
            get { return GetConnectionString(); }
            set { SetConnectionString(value); }
        }

        public event SyntaxProviderDetected OnSyntaxProviderDetected;

        public void SetServerType(string serverType)
        {

        }

        public PostgreSQLConnectionFrame(string connectionString)
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(connectionString))
            {
                tbHost.Text = "localhost";
                tbPort.Text = "5432";
                tbUserName.Text = "postgres";
            }
            else
            {
                ConnectionString = connectionString;
            }
        }

        public string GetConnectionString()
        {
            try
            {
                var builder = new NpgsqlConnectionStringBuilder
                {
                    ConnectionString = _connectionString,
                    Host = tbHost.Text,
                    Port = Convert.ToInt32(tbPort.Text),
                    Database = tbDatabase.Text,
                    Username = tbUserName.Text,
                    Password = tbPassword.Password
                };

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
                var builder = new NpgsqlConnectionStringBuilder
                {
                    ConnectionString = _connectionString
                };

                tbHost.Text = builder.Host;
                tbPort.Text = builder.Port.ToString();
                tbDatabase.Text = builder.Database;
                tbUserName.Text = builder.Username;

                _connectionString = builder.ConnectionString;
            }
            catch
            {
            }
        }

        private void btnEditConnectionString_Click(object sender, EventArgs e)
        {
            var csef = new ConnectionStringEditWindow {ConnectionString = ConnectionString};

            if (csef.ShowDialog() != true) return;
            if (csef.Modified)
            {
                ConnectionString = csef.ConnectionString;
            }
        }

        public bool TestConnection()
        {
            Mouse.OverrideCursor= Cursors.Wait;

            try
            {
                var connection = new NpgsqlConnection(ConnectionString);
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
