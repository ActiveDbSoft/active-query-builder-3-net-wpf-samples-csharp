//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2019 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace GeneralAssembly.Connection.FrameConnection
{
    /// <summary>
    /// Interaction logic for MSSQLConnectionFrame.xaml
    /// </summary>
    public partial class MSSQLConnectionFrame : IConnectionFrame
    {
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

        public MSSQLConnectionFrame(string connectionString)
        {
            InitializeComponent();
            if (string.IsNullOrEmpty(connectionString))
            {
                tbDataSource.Text = "(local)";
                cmbIntegratedSecurity.SelectedIndex = 0;
                tbUserID.IsEnabled = false;
                tbPassword.IsEnabled = false;
                cmbInitialCatalog.SelectedIndex = 0;
            }
            else
            {
                ConnectionString = connectionString;
            }

            cmbIntegratedSecurity.SelectionChanged += cmbIntegratedSecurity_SelectedIndexChanged;
        }

        public MSSQLConnectionFrame()
        {
            InitializeComponent();
        }

        public string GetConnectionString()
        {
            var builder = new SqlConnectionStringBuilder
            {
                ConnectionString = _connectionString,
                DataSource = tbDataSource.Text,
                IntegratedSecurity = (cmbIntegratedSecurity.SelectedIndex == 0),
                UserID = tbUserID.Text,
                Password = tbPassword.Password,
                InitialCatalog = cmbInitialCatalog.Text == "<default>" ? "" : cmbInitialCatalog.Text
            };

            try
            {
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
            var builder = new SqlConnectionStringBuilder {ConnectionString = _connectionString};

            try
            {
                tbDataSource.Text = builder.DataSource;
                cmbIntegratedSecurity.SelectedIndex = (builder.IntegratedSecurity) ? 0 : 1;
                tbUserID.Text = builder.UserID;
                tbUserID.IsEnabled = !builder.IntegratedSecurity;
                tbPassword.Password = builder.Password;
                tbPassword.IsEnabled = !builder.IntegratedSecurity;
                cmbInitialCatalog.Text = builder.InitialCatalog;

                _connectionString = builder.ConnectionString;
            }
            catch
            {
            }
        }

        public  bool TestConnection()
		{
			Mouse.OverrideCursor = Cursors.Wait;

			try
			{
				SqlConnection connection = new SqlConnection(ConnectionString);
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

        private void cmbIntegratedSecurity_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbUserID.IsEnabled = (cmbIntegratedSecurity.SelectedIndex == 1);
            tbPassword.IsEnabled = (cmbIntegratedSecurity.SelectedIndex == 1);
        }

        private void CmbInitialCatalog_OnDropDownOpened(object sender, EventArgs e)
        {

            using (var connection = new SqlConnection(ConnectionString))
            {
                Mouse.OverrideCursor = Cursors.Wait;

                var currentDatabase = cmbInitialCatalog.Text;

                cmbInitialCatalog.Items.Clear();
                cmbInitialCatalog.Items.Add("<default>");
                cmbInitialCatalog.SelectedIndex = 0;

                try
                {
                    connection.Open();

                    var schemaTable = connection.GetSchema("Databases");

                    foreach (DataRow r in schemaTable.Rows)
                    {
                        cmbInitialCatalog.Items.Add(r[0]);
                    }

                    cmbInitialCatalog.SelectedItem = null;
                    cmbInitialCatalog.SelectedItem = currentDatabase;

                    if (cmbInitialCatalog.SelectedItem == null)
                    {
                        cmbInitialCatalog.Text = currentDatabase;
                    }
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

        private void BtnEditConnectionString_OnClick(object sender, RoutedEventArgs e)
        {
            var csef = new ConnectionStringEditWindow {ConnectionString = ConnectionString};


            if (csef.ShowDialog() != true) return;

            if (csef.Modified)
                ConnectionString = csef.ConnectionString;
        }
    }
}
