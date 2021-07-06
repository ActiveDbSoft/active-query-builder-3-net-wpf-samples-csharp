//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2021 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using Oracle.ManagedDataAccess.Client;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace GeneralAssembly.Connection.FrameConnection
{
    /// <summary>
    /// Interaction logic for OracleConnectionFrame.xaml
    /// </summary>
    public partial class OracleConnectionFrame : IConnectionFrame
    {
        public OracleConnectionFrame()
        {
            InitializeComponent();
        }

        private string _connectionString;

        public string ConnectionString
        {
            get { return GetConnectionString(); }
            set { SetConnectionString(value); }
        }

        public event SyntaxProviderDetected SyntaxProviderDetected;

        public void SetServerType(string serverType)
        {

        }

        public OracleConnectionFrame(string connectionString)
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(connectionString))
            {
                tbUserID.IsEnabled = true;
                tbPassword.IsEnabled = true;
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
                var builder = new OracleConnectionStringBuilder
                {
                    ConnectionString = _connectionString,
                    DataSource = tbDataSource.Text,
                    UserID = tbUserID.Text,
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
                var builder = new OracleConnectionStringBuilder {ConnectionString = _connectionString};

                tbDataSource.Text = builder.DataSource;
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
            var csef = new ConnectionStringEditWindow {ConnectionString = ConnectionString};


            if (csef.ShowDialog() != true) return;
            if (csef.Modified)
            {
                ConnectionString = csef.ConnectionString;
            }
        }

        public bool TestConnection()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                var connection = new OracleConnection(ConnectionString);
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

        protected virtual void OnSyntaxProviderDetected(Type syntaxtype)
        {
            SyntaxProviderDetected?.Invoke(syntaxtype);
        }
    }
}
