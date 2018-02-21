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
using System.Data.OleDb;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace FullFeaturedMdiDemo.Connection.FrameConnection
{
    /// <summary>
    /// Interaction logic for MSAccessConnectionFrame.xaml
    /// </summary>
    public partial class MSAccessConnectionFrame : IConnectionFrame
    {
        private string _connectionString;
        private OpenFileDialog _openFileDialog1;

        public string ConnectionString
        {
            get { return GetConnectionString(); }
            set { SetConnectionString(value); }
        }

        public MSAccessConnectionFrame(string connectionString)
        {
            InitializeComponent();

            if (String.IsNullOrEmpty(connectionString))
            {
                tbUserID.Text = "Admin";
            }
            else
            {
                ConnectionString = connectionString;
            }
        }

        public MSAccessConnectionFrame()
        {
            InitializeComponent();
        }

        public string GetConnectionString()
        {
            try
            {
                var builder = new OleDbConnectionStringBuilder
                {
                    ConnectionString = _connectionString,
                    Provider = "Microsoft.ACE.OLEDB.12.0",
                    DataSource = tbDataSource.Text
                };

                builder["User ID"] = tbUserID.Text;
                builder["Password"] = tbPassword.Password;

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

            if (!string.IsNullOrEmpty(_connectionString))
            {
                try
                {
                    var builder = new OleDbConnectionStringBuilder
                    {
                        ConnectionString = _connectionString
                    };

                    tbDataSource.Text = builder.DataSource;
                    tbUserID.Text = builder["User ID"].ToString();
                    tbPassword.Password = builder["Password"].ToString();

                    _connectionString = builder.ConnectionString;
                }
                catch
                {
                }
            }
        }

        private void BtnBrowse_OnClick(object sender, RoutedEventArgs e)
        {
            _openFileDialog1 = new OpenFileDialog();

            if (_openFileDialog1.ShowDialog() == true)
            {
                tbDataSource.Text = _openFileDialog1.FileName;
            }
        }

        private void BtnEditConnectionString_OnClick(object sender, EventArgs e)
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
                var connection = new OleDbConnection(ConnectionString);
                connection.Open();
                connection.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, App.Name);
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
