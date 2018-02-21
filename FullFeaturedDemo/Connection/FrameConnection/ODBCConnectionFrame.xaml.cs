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
using System.Data.Odbc;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace FullFeaturedDemo.Connection.FrameConnection
{
    /// <summary>
    /// Interaction logic for ODBCConnectionFrame.xaml
    /// </summary>
    public partial class ODBCConnectionFrame : IConnectionFrame
    {
        public ODBCConnectionFrame()
        {
            InitializeComponent();
        }

        private string _connectionString;

        public string ConnectionString
        {
            get { return GetConnectionString(); }
            set { SetConnectionString(value); }
        }

        public ODBCConnectionFrame(string connectionString)
        {
            InitializeComponent();

            ConnectionString = connectionString;
        }

        public string GetConnectionString()
        {
            try
            {
                var builder = new OdbcConnectionStringBuilder {ConnectionString = tbConnectionString.Text};
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
                var builder = new OdbcConnectionStringBuilder();
                builder.ConnectionString = _connectionString;
                _connectionString = builder.ConnectionString;
                tbConnectionString.Text = _connectionString;
            }
            catch
            {
            }
        }

        public bool TestConnection()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                OdbcConnection connection = new OdbcConnection(ConnectionString);
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
