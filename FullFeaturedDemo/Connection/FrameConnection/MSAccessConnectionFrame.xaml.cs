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
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace FullFeaturedDemo.Connection.FrameConnection
{
    /// <summary>
    /// Interaction logic for MSAccessConnectionFrame.xaml
    /// </summary>
    public partial class MSAccessConnectionFrame : IConnectionFrame
    {
        private string _connectionString;
        private string _serverType;

        private readonly List<string> _knownAceProviders = new List<string>
        {
            "Microsoft.ACE.OLEDB.16.0",
            "Microsoft.ACE.OLEDB.15.0",
            "Microsoft.ACE.OLEDB.14.0",
            "Microsoft.ACE.OLEDB.12.0"
        };

        private OpenFileDialog _openFileDialog1;

        public event SyntaxProviderDetected OnSyntaxProviderDetected;

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

        public void SetServerType(string serverType)
        {
            _serverType = serverType;
        }

        private static List<string> GetProvidersList()
        {
            var reader = OleDbEnumerator.GetRootEnumerator();
            var result = new List<string>();
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetName(i) == "SOURCES_NAME")
                        result.Add(reader.GetValue(i).ToString());
                }
            }
            reader.Close();

            return result;
        }

        private string DetectProvider()
        {
            var providersList = GetProvidersList();
            var provider = string.Empty;

            var ext = Path.GetExtension(tbDataSource.Text);
            if (ext == ".accdb")
            {
                for (int i = 0; i < _knownAceProviders.Count; i++)
                {
                    if (providersList.Contains(_knownAceProviders[i]))
                    {
                        provider = _knownAceProviders[i];
                        break;
                    }
                }
				
				if (provider == string.Empty)
				{
					provider = "Microsoft.ACE.OLEDB.12.0";
				}					
            }
            else if (_serverType == "Access 97")
            {
                provider = "Microsoft.Jet.OLEDB.3.0";
            }
            else if (_serverType == "Access 2000 and newer")
            {
                for (int i = 0; i < _knownAceProviders.Count; i++)
                {
                    if (providersList.Contains(_knownAceProviders[i]))
                    {
                        provider = _knownAceProviders[i];
                        break;
                    }
                }

                if (provider == string.Empty)
                {
                    provider = "Microsoft.Jet.OLEDB.4.0";
                }
            }

            return provider;
        }

        public string GetConnectionString()
        {
            try
            {
                OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder
                {
                    ConnectionString = _connectionString,
                    DataSource = tbDataSource.Text,
                    Provider = DetectProvider()
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
