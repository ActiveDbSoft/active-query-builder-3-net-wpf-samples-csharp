//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2025 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using ActiveQueryBuilder.Core;
using GeneralAssembly.Connection.FrameConnection;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GeneralAssembly.Connection
{
    /// <summary>
    /// Interaction logic for AddConnectionWindow.xaml
    /// </summary>
    public partial class AddConnectionWindow
    {
        private readonly ConnectionInfo _connectionInfo;
        private IConnectionFrame _currentConnectionFrame;
        private bool _isLockUpdate = false;

        public AddConnectionWindow()
        {
            InitializeComponent();
            Dispatcher.CurrentDispatcher.Hooks.DispatcherInactive += Hooks_DispatcherInactive;
        }

        private void Hooks_DispatcherInactive(object sender, EventArgs e)
        {
            ButtonOk.IsEnabled = (TextBoxConnectionName.Text.Length > 0);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DialogResult == true)
            {
                if (_currentConnectionFrame != null &&
                    _currentConnectionFrame.TestConnection())
                {
                    _connectionInfo.Name = TextBoxConnectionName.Text;
                    if (!_connectionInfo.IsXmlFile)
                    {
                        //previous version of this demo uses deprecated System.Data.OracleClient
                        // current version uses Oracle.ManagedDataAccess.Client which doesn't support "Integrated Security" setting
                        if (_connectionInfo.Type == ConnectionTypes.Oracle)
                        {
                            _currentConnectionFrame.ConnectionString = Regex.Replace(_currentConnectionFrame.ConnectionString,
                              "Integrated Security=.*?;", "");
                        }

                        if (_connectionInfo.ConnectionDescriptor.MetadataProvider.Connected)
                            _connectionInfo.ConnectionDescriptor.MetadataProvider.Disconnect();

                        _connectionInfo.ConnectionDescriptor.MetadataProvider.Connection.ConnectionString = _currentConnectionFrame.ConnectionString;
                    }
                    else
                    {
                        _connectionInfo.ConnectionString = _currentConnectionFrame.ConnectionString;
                    }

                    e.Cancel = false;
                    Dispatcher.CurrentDispatcher.Hooks.DispatcherInactive -= Hooks_DispatcherInactive;
                }
                else
                {
                    e.Cancel = true;
                }
            }

            base.OnClosing(e);
        }

        public AddConnectionWindow(ConnectionInfo connectionInfo)
        {
            InitializeComponent();

            Debug.Assert(connectionInfo != null);

            _connectionInfo = connectionInfo;
            TextBoxConnectionName.Text = connectionInfo.Name;

            if (!string.IsNullOrEmpty(connectionInfo.ConnectionString))
            {

                if (!connectionInfo.IsXmlFile)
                {
                    rbMSSQL.IsEnabled = (connectionInfo.Type == ConnectionTypes.MSSQL);
                    rbMSAccess.IsEnabled = (connectionInfo.Type == ConnectionTypes.MSAccess);
                    rbOracle.IsEnabled = (connectionInfo.Type == ConnectionTypes.Oracle);
                    rbMySQL.IsEnabled = (connectionInfo.Type == ConnectionTypes.MySQL);
                    rbPostrgeSQL.IsEnabled = (connectionInfo.Type == ConnectionTypes.PostgreSQL);
                    rbOLEDB.IsEnabled = (connectionInfo.Type == ConnectionTypes.OLEDB);
                    rbODBC.IsEnabled = (connectionInfo.Type == ConnectionTypes.ODBC);
                }
            }

            if (connectionInfo.IsXmlFile)
            {
                rbOLEDB.IsEnabled = false;
                rbODBC.IsEnabled = false;
            }

            rbMSSQL.IsChecked = (connectionInfo.Type == ConnectionTypes.MSSQL);
            rbMSAccess.IsChecked = (connectionInfo.Type == ConnectionTypes.MSAccess);
            rbOracle.IsChecked = (connectionInfo.Type == ConnectionTypes.Oracle);
            rbMySQL.IsChecked = (connectionInfo.Type == ConnectionTypes.MySQL);
            rbPostrgeSQL.IsChecked = (connectionInfo.Type == ConnectionTypes.PostgreSQL);
            rbOLEDB.IsChecked = (connectionInfo.Type == ConnectionTypes.OLEDB);
            rbODBC.IsChecked = (connectionInfo.Type == ConnectionTypes.ODBC);

            SetActiveConnectionTypeFrame();

            rbMSSQL.Checked += ConnectionTypeChanged;
            rbMSAccess.Checked += ConnectionTypeChanged;
            rbOracle.Checked += ConnectionTypeChanged;
            rbMySQL.Checked += ConnectionTypeChanged;
            rbPostrgeSQL.Checked += ConnectionTypeChanged;
            rbOLEDB.Checked += ConnectionTypeChanged;
            rbODBC.Checked += ConnectionTypeChanged;

            FillSyntax();
        }

        private string SyntaxToString(BaseSyntaxProvider syntax)
        {
            if (syntax is SQL2003SyntaxProvider)
            {
                return "ANSI SQL-2003";
            }
            else if (syntax is SQL92SyntaxProvider)
            {
                return "ANSI SQL-92";
            }
            else if (syntax is SQL89SyntaxProvider)
            {
                return "ANSI SQL-89";
            }
            else if (syntax is FirebirdSyntaxProvider)
            {
                return "Firebird";
            }
            else if (syntax is DB2SyntaxProvider)
            {
                return "IBM DB2";
            }
            else if (syntax is InformixSyntaxProvider)
            {
                return "IBM Informix";
            }
            else if (syntax is MSAccessSyntaxProvider)
            {
                return "Microsoft Access";
            }
            else if (syntax is MSSQLSyntaxProvider)
            {
                return "Microsoft SQL Server";
            }
            else if (syntax is MySQLSyntaxProvider)
            {
                return "MySQL";
            }
            else if (syntax is OracleSyntaxProvider)
            {
                return "Oracle";
            }
            else if (syntax is PostgreSQLSyntaxProvider)
            {
                return "PostgreSQL";
            }
            else if (syntax is SQLiteSyntaxProvider)
            {
                return "SQLite";
            }
            else if (syntax is SybaseSyntaxProvider)
            {
                return "Sybase";
            }
            else if (syntax is VistaDBSyntaxProvider)
            {
                return "VistaDB";
            }
            else if (syntax is GenericSyntaxProvider)
            {
                return "Universal";
            }

            return string.Empty;
        }

        private void FillSyntax()
        {
            _isLockUpdate = true;

            BoxSyntaxProvider.Items.Clear();
            BoxServerVersion.Items.Clear();

            if (!string.IsNullOrEmpty(_connectionInfo.SyntaxProviderName) && _connectionInfo.SyntaxProvider == null)
            {
                BoxSyntaxProvider.Items.Add(_connectionInfo.SyntaxProviderName);
                BoxSyntaxProvider.SelectedItem = _connectionInfo.SyntaxProviderName;
                return;
            }

            if (_connectionInfo.SyntaxProvider == null)
            {
                switch (_connectionInfo.Type)
                {
                    case ConnectionTypes.MSSQL:
                        _connectionInfo.SyntaxProvider = new MSSQLSyntaxProvider();
                        break;
                    case ConnectionTypes.MSAccess:
                        _connectionInfo.SyntaxProvider = new MSAccessSyntaxProvider();
                        break;
                    case ConnectionTypes.Oracle:
                        _connectionInfo.SyntaxProvider = new OracleSyntaxProvider();
                        break;
                    case ConnectionTypes.MySQL:
                        _connectionInfo.SyntaxProvider = new MySQLSyntaxProvider();
                        break;
                    case ConnectionTypes.PostgreSQL:
                        _connectionInfo.SyntaxProvider = new PostgreSQLSyntaxProvider();
                        break;
                    case ConnectionTypes.OLEDB:
                        _connectionInfo.SyntaxProvider = new SQL92SyntaxProvider();
                        break;
                    case ConnectionTypes.ODBC:
                        _connectionInfo.SyntaxProvider = new SQL92SyntaxProvider();
                        break;
                }
            }

            if (_connectionInfo.Type == ConnectionTypes.ODBC ||
                _connectionInfo.Type == ConnectionTypes.OLEDB)
            {
                BoxSyntaxProvider.Items.Add("ANSI SQL-2003");
                BoxSyntaxProvider.Items.Add("ANSI SQL-92");
                BoxSyntaxProvider.Items.Add("ANSI SQL-89");
                BoxSyntaxProvider.Items.Add("Firebird");
                BoxSyntaxProvider.Items.Add("IBM DB2");
                BoxSyntaxProvider.Items.Add("IBM Informix");
                BoxSyntaxProvider.Items.Add("Microsoft Access");
                BoxSyntaxProvider.Items.Add("Microsoft SQL Server");
                BoxSyntaxProvider.Items.Add("MySQL");
                BoxSyntaxProvider.Items.Add("Oracle");
                BoxSyntaxProvider.Items.Add("PostgreSQL");
                BoxSyntaxProvider.Items.Add("SQLite");
                BoxSyntaxProvider.Items.Add("Sybase");
                BoxSyntaxProvider.Items.Add("VistaDB");
                BoxSyntaxProvider.Items.Add("Universal");
                BoxSyntaxProvider.SelectedItem = SyntaxToString(_connectionInfo.SyntaxProvider);
            }
            else if (_connectionInfo.SyntaxProvider is SQL2003SyntaxProvider)
            {
                BoxSyntaxProvider.Items.Add(SyntaxToString(_connectionInfo.SyntaxProvider));
                BoxSyntaxProvider.SelectedItem = SyntaxToString(_connectionInfo.SyntaxProvider);

                BoxSyntaxProvider.Items.Add("ANSI SQL-92");
                BoxSyntaxProvider.Items.Add("ANSI SQL-89");
                BoxSyntaxProvider.Items.Add("Firebird");
                BoxSyntaxProvider.Items.Add("IBM DB2");
                BoxSyntaxProvider.Items.Add("IBM Informix");
                BoxSyntaxProvider.Items.Add("Microsoft Access");
                BoxSyntaxProvider.Items.Add("Microsoft SQL Server");
                BoxSyntaxProvider.Items.Add("MySQL");
                BoxSyntaxProvider.Items.Add("Oracle");
                BoxSyntaxProvider.Items.Add("PostgreSQL");
                BoxSyntaxProvider.Items.Add("SQLite");
                BoxSyntaxProvider.Items.Add("Sybase");
                BoxSyntaxProvider.Items.Add("VistaDB");
                BoxSyntaxProvider.Items.Add("Universal");
            }
            else if (_connectionInfo.SyntaxProvider is SQL92SyntaxProvider)
            {
                BoxSyntaxProvider.Items.Add(SyntaxToString(_connectionInfo.SyntaxProvider));
                BoxSyntaxProvider.SelectedItem = SyntaxToString(_connectionInfo.SyntaxProvider);

                BoxSyntaxProvider.Items.Add("ANSI SQL-2003");

                BoxSyntaxProvider.Items.Add("ANSI SQL-89");
                BoxSyntaxProvider.Items.Add("Firebird");
                BoxSyntaxProvider.Items.Add("IBM DB2");
                BoxSyntaxProvider.Items.Add("IBM Informix");
                BoxSyntaxProvider.Items.Add("Microsoft Access");
                BoxSyntaxProvider.Items.Add("Microsoft SQL Server");
                BoxSyntaxProvider.Items.Add("MySQL");
                BoxSyntaxProvider.Items.Add("Oracle");
                BoxSyntaxProvider.Items.Add("PostgreSQL");
                BoxSyntaxProvider.Items.Add("SQLite");
                BoxSyntaxProvider.Items.Add("Sybase");
                BoxSyntaxProvider.Items.Add("VistaDB");
                BoxSyntaxProvider.Items.Add("Universal");
            }
            else if (_connectionInfo.SyntaxProvider is SQL89SyntaxProvider)
            {
                BoxSyntaxProvider.Items.Add(SyntaxToString(_connectionInfo.SyntaxProvider));
                BoxSyntaxProvider.SelectedItem = SyntaxToString(_connectionInfo.SyntaxProvider);

                BoxSyntaxProvider.Items.Add("ANSI SQL-2003");
                BoxSyntaxProvider.Items.Add("ANSI SQL-92");
                BoxSyntaxProvider.Items.Add("Firebird");
                BoxSyntaxProvider.Items.Add("IBM DB2");
                BoxSyntaxProvider.Items.Add("IBM Informix");
                BoxSyntaxProvider.Items.Add("Microsoft Access");
                BoxSyntaxProvider.Items.Add("Microsoft SQL Server");
                BoxSyntaxProvider.Items.Add("MySQL");
                BoxSyntaxProvider.Items.Add("Oracle");
                BoxSyntaxProvider.Items.Add("PostgreSQL");
                BoxSyntaxProvider.Items.Add("SQLite");
                BoxSyntaxProvider.Items.Add("Sybase");
                BoxSyntaxProvider.Items.Add("VistaDB");
                BoxSyntaxProvider.Items.Add("Universal");
            }
            else if (_connectionInfo.SyntaxProvider is GenericSyntaxProvider)
            {
                BoxSyntaxProvider.Items.Add(SyntaxToString(_connectionInfo.SyntaxProvider));
                BoxSyntaxProvider.SelectedItem = SyntaxToString(_connectionInfo.SyntaxProvider);

                BoxSyntaxProvider.Items.Add("ANSI SQL-2003");
                BoxSyntaxProvider.Items.Add("ANSI SQL-92");
                BoxSyntaxProvider.Items.Add("ANSI SQL-89");
                BoxSyntaxProvider.Items.Add("Firebird");
                BoxSyntaxProvider.Items.Add("IBM DB2");
                BoxSyntaxProvider.Items.Add("IBM Informix");
                BoxSyntaxProvider.Items.Add("Microsoft Access");
                BoxSyntaxProvider.Items.Add("Microsoft SQL Server");
                BoxSyntaxProvider.Items.Add("MySQL");
                BoxSyntaxProvider.Items.Add("Oracle");
                BoxSyntaxProvider.Items.Add("PostgreSQL");
                BoxSyntaxProvider.Items.Add("SQLite");
                BoxSyntaxProvider.Items.Add("Sybase");
                BoxSyntaxProvider.Items.Add("VistaDB");
                BoxSyntaxProvider.Items.Add("Universal");
            }
            else
            {
                BoxSyntaxProvider.Items.Add(SyntaxToString(_connectionInfo.SyntaxProvider));
                BoxSyntaxProvider.SelectedItem = SyntaxToString(_connectionInfo.SyntaxProvider);
            }

            FillVersions();

            _isLockUpdate = false;
        }

        private void FillVersions()
        {
            BoxServerVersion.Items.Clear();
            BoxServerVersion.Text = string.Empty;

            if (_connectionInfo.SyntaxProvider is SQL2003SyntaxProvider)
            {
                BoxServerVersion.IsEnabled = false;
            }
            else if (_connectionInfo.SyntaxProvider is SQL92SyntaxProvider)
            {
                BoxServerVersion.IsEnabled = false;
            }
            else if (_connectionInfo.SyntaxProvider is SQL89SyntaxProvider)
            {
                BoxServerVersion.IsEnabled = false;
            }
            else if (_connectionInfo.SyntaxProvider is FirebirdSyntaxProvider)
            {
                BoxServerVersion.IsEnabled = true;
                BoxServerVersion.Items.Add("Firebird 1.0");
                BoxServerVersion.Items.Add("Firebird 1.5");
                BoxServerVersion.Items.Add("Firebird 2.0");
                BoxServerVersion.Items.Add("Firebird 2.5");

                var firebirdSyntaxProvider = (FirebirdSyntaxProvider)_connectionInfo.SyntaxProvider;

                switch (firebirdSyntaxProvider.ServerVersion)
                {
                    case FirebirdVersion.Firebird10:
                        BoxServerVersion.SelectedItem = "Firebird 1.0";
                        break;
                    case FirebirdVersion.Firebird15:
                        BoxServerVersion.SelectedItem = "Firebird 1.5";
                        break;
                    case FirebirdVersion.Firebird20:
                        BoxServerVersion.SelectedItem = "Firebird 2.0";
                        break;
                    case FirebirdVersion.Firebird25:
                        BoxServerVersion.SelectedItem = "Firebird 2.5";
                        break;
                }
            }
            else if (_connectionInfo.SyntaxProvider is DB2SyntaxProvider)
            {
                BoxServerVersion.IsEnabled = false;
            }
            else if (_connectionInfo.SyntaxProvider is InformixSyntaxProvider)
            {
                BoxServerVersion.IsEnabled = true;
                BoxServerVersion.Items.Add("Informix 8");
                BoxServerVersion.Items.Add("Informix 9");
                BoxServerVersion.Items.Add("Informix 10");
                BoxServerVersion.Items.Add("Informix 11");

                var informixSyntaxProvider = (InformixSyntaxProvider)_connectionInfo.SyntaxProvider;

                switch (informixSyntaxProvider.ServerVersion)
                {
                    case InformixVersion.DS8:
                        BoxServerVersion.SelectedItem = "Informix 8";
                        break;
                    case InformixVersion.DS9:
                        BoxServerVersion.SelectedItem = "Informix 9";
                        break;
                    case InformixVersion.DS10:
                        BoxServerVersion.SelectedItem = "Informix 10";
                        break;
                    default:
                        BoxServerVersion.SelectedItem = "Informix 11";
                        break;
                }
            }
            else if (_connectionInfo.SyntaxProvider is MSAccessSyntaxProvider)
            {
                BoxServerVersion.IsEnabled = true;
                BoxServerVersion.Items.Add("Access 97");
                BoxServerVersion.Items.Add("Access 2000 and newer");

                var accessSyntaxProvider = (MSAccessSyntaxProvider)_connectionInfo.SyntaxProvider;

                BoxServerVersion.SelectedItem = accessSyntaxProvider.ServerVersion == MSAccessServerVersion.MSJET3 ? "Access 97" : "Access 2000 and newer";
            }
            else if (_connectionInfo.SyntaxProvider is MSSQLSyntaxProvider)
            {
                BoxServerVersion.IsEnabled = true;
                BoxServerVersion.Items.Add("Auto");
                BoxServerVersion.Items.Add("SQL Server 7");
                BoxServerVersion.Items.Add("SQL Server 2000");
                BoxServerVersion.Items.Add("SQL Server 2005");
                BoxServerVersion.Items.Add("SQL Server 2012");
                BoxServerVersion.Items.Add("SQL Server 2014");
                BoxServerVersion.Items.Add("SQL Server 2016");
                BoxServerVersion.Items.Add("SQL Server 2017");
                BoxServerVersion.Items.Add("SQL Server 2019");

                var mssqlSyntaxProvider = (MSSQLSyntaxProvider)_connectionInfo.SyntaxProvider;

                switch (mssqlSyntaxProvider.ServerVersion)
                {
                    case MSSQLServerVersion.MSSQL7:
                        BoxServerVersion.SelectedItem = "SQL Server 7";
                        break;
                    case MSSQLServerVersion.MSSQL2000:
                        BoxServerVersion.SelectedItem = "SQL Server 2000";
                        break;
                    case MSSQLServerVersion.MSSQL2005:
                        BoxServerVersion.SelectedItem = "SQL Server 2005";
                        break;
                    case MSSQLServerVersion.MSSQL2012:
                        BoxServerVersion.SelectedItem = "SQL Server 2012";
                        break;
                    case MSSQLServerVersion.MSSQL2014:
                        BoxServerVersion.SelectedItem = "SQL Server 2014";
                        break;
                    case MSSQLServerVersion.MSSQL2016:
                        BoxServerVersion.SelectedItem = "SQL Server 2016";
                        break;
                    case MSSQLServerVersion.MSSQL2017:
                        BoxServerVersion.SelectedItem = "SQL Server 2017";
                        break;
                    case MSSQLServerVersion.MSSQL2019:
                        BoxServerVersion.SelectedItem = "SQL Server 2019";
                        break;
                    default:
                        BoxServerVersion.SelectedItem = "Auto";
                        break;
                }
            }
            else if (_connectionInfo.SyntaxProvider is MySQLSyntaxProvider)
            {
                BoxServerVersion.IsEnabled = true;
                BoxServerVersion.Items.Add("3.0");
                BoxServerVersion.Items.Add("4.0");
                BoxServerVersion.Items.Add("5.0");
                BoxServerVersion.Items.Add("8.0+");

                var mySqlSyntaxProvider = (MySQLSyntaxProvider)_connectionInfo.SyntaxProvider;

                if (mySqlSyntaxProvider.ServerVersionInt < 40000)
                {
                    BoxServerVersion.SelectedItem = "3.0";
                }
                else if (mySqlSyntaxProvider.ServerVersionInt < 50000)
                {
                    BoxServerVersion.SelectedItem = "4.0";
                }
                else if (mySqlSyntaxProvider.ServerVersionInt < 80000)
                {
                    BoxServerVersion.SelectedItem = "5.0";
                }
                else
                {
                    BoxServerVersion.SelectedItem = "8.0+";
                }
            }
            else if (_connectionInfo.SyntaxProvider is OracleSyntaxProvider)
            {
                BoxServerVersion.IsEnabled = true;
                BoxServerVersion.Items.Add("Oracle 7");
                BoxServerVersion.Items.Add("Oracle 8");
                BoxServerVersion.Items.Add("Oracle 9");
                BoxServerVersion.Items.Add("Oracle 10");
                BoxServerVersion.Items.Add("Oracle 11g");
                BoxServerVersion.Items.Add("Oracle 12c");
                BoxServerVersion.Items.Add("Oracle 18c");
                BoxServerVersion.Items.Add("Oracle 19c");

                var oracleSyntaxProvider = (OracleSyntaxProvider)_connectionInfo.SyntaxProvider;

                switch (oracleSyntaxProvider.ServerVersion)
                {
                    case OracleServerVersion.Oracle7:
                        BoxServerVersion.SelectedItem = "Oracle 7";
                        break;
                    case OracleServerVersion.Oracle8:
                        BoxServerVersion.SelectedItem = "Oracle 8";
                        break;
                    case OracleServerVersion.Oracle9:
                        BoxServerVersion.SelectedItem = "Oracle 9";
                        break;
                    case OracleServerVersion.Oracle10:
                        BoxServerVersion.SelectedItem = "Oracle 10";
                        break;
                    case OracleServerVersion.Oracle11:
                        BoxServerVersion.SelectedItem = "Oracle 11g";
                        break;
                    case OracleServerVersion.Oracle12:
                        BoxServerVersion.SelectedItem = "Oracle 12c";
                        break;
                    case OracleServerVersion.Oracle18:
                        BoxServerVersion.SelectedItem = "Oracle 18c";
                        break;
                    case OracleServerVersion.Oracle19:
                        BoxServerVersion.SelectedItem = "Oracle 19c";
                        break;
                    default:
                        BoxServerVersion.SelectedItem = "Oracle 18c";
                        break;
                }
            }
            else if (_connectionInfo.SyntaxProvider is PostgreSQLSyntaxProvider)
            {
                BoxServerVersion.IsEnabled = false;
                BoxServerVersion.Text = "Auto";
            }
            else if (_connectionInfo.SyntaxProvider is SQLiteSyntaxProvider)
            {
                BoxServerVersion.IsEnabled = false;
            }
            else if (_connectionInfo.SyntaxProvider is SybaseSyntaxProvider)
            {
                BoxServerVersion.IsEnabled = true;
                BoxServerVersion.Items.Add("ASE");
                BoxServerVersion.Items.Add("SQL Anywhere");
                BoxServerVersion.Items.Add("SAP IQ");

                var sybaseSyntaxProvider = (SybaseSyntaxProvider)_connectionInfo.SyntaxProvider;

                switch (sybaseSyntaxProvider.ServerVersion)
                {
                    case SybaseServerVersion.SybaseASE:
                        BoxServerVersion.SelectedItem = "ASE";
                        break;
                    case SybaseServerVersion.SybaseASA:
                        BoxServerVersion.SelectedItem = "SQL Anywhere";
                        break;
                    case SybaseServerVersion.SybaseIQ:
                        BoxServerVersion.SelectedItem = "SAP IQ";
                        break;
                }
            }
            else if (_connectionInfo.SyntaxProvider is VistaDBSyntaxProvider)
            {
                BoxServerVersion.IsEnabled = false;
            }
            else if (_connectionInfo.SyntaxProvider is GenericSyntaxProvider)
            {
                BoxServerVersion.IsEnabled = false;
            }
        }

        private void ConnectionTypeChanged(object sender, RoutedEventArgs e)
        {
            if (((RadioButton)sender).IsChecked != true) return;

            var connectionType = ConnectionTypes.MSSQL;

            if (Equals(sender, rbMSSQL))
            {
                connectionType = ConnectionTypes.MSSQL;
            }
            else if (Equals(sender, rbMSAccess))
            {
                connectionType = ConnectionTypes.MSAccess;
            }
            else if (Equals(sender, rbOracle))
            {
                connectionType = ConnectionTypes.Oracle;
            }
            else if (Equals(sender, rbMySQL))
            {
                connectionType = ConnectionTypes.MySQL;
            }
            else if (Equals(sender, rbPostrgeSQL))
            {
                connectionType = ConnectionTypes.PostgreSQL;
            }
            else if (Equals(sender, rbOLEDB))
            {
                connectionType = ConnectionTypes.OLEDB;
            }
            else if (Equals(sender, rbODBC))
            {
                connectionType = ConnectionTypes.ODBC;
            }

            if (connectionType != _connectionInfo.Type)
            {
                _connectionInfo.Type = connectionType;

                if (!_connectionInfo.IsXmlFile)
                {
                    SetActiveConnectionTypeFrame();
                }
            }

            switch (_connectionInfo.Type)
            {
                case ConnectionTypes.MSSQL:
                    _connectionInfo.SyntaxProvider = new MSSQLSyntaxProvider();
                    break;
                case ConnectionTypes.MSAccess:
                    _connectionInfo.SyntaxProvider = new MSAccessSyntaxProvider();
                    break;
                case ConnectionTypes.Oracle:
                    _connectionInfo.SyntaxProvider = new OracleSyntaxProvider();
                    break;
                case ConnectionTypes.MySQL:
                    _connectionInfo.SyntaxProvider = new MySQLSyntaxProvider();
                    break;
                case ConnectionTypes.PostgreSQL:
                    _connectionInfo.SyntaxProvider = new PostgreSQLSyntaxProvider();
                    break;
                case ConnectionTypes.OLEDB:
                    _connectionInfo.SyntaxProvider = new SQL92SyntaxProvider();
                    break;
                case ConnectionTypes.ODBC:
                    _connectionInfo.SyntaxProvider = new SQL92SyntaxProvider();
                    break;
            }

            FillSyntax();

            BoxSyntaxProvider.IsEnabled = _connectionInfo.Type == ConnectionTypes.ODBC ||
                                         _connectionInfo.Type == ConnectionTypes.OLEDB;
        }

        private void SetActiveConnectionTypeFrame()
        {
            if (GridFrames.Children.Contains((FrameworkElement)_currentConnectionFrame))
            {
                _currentConnectionFrame.SyntaxProviderDetected -= CurrentConnectionFrame_SyntaxProviderDetected;
                GridFrames.Children.Remove((FrameworkElement)_currentConnectionFrame);
                _currentConnectionFrame = null;
            }

            if (!_connectionInfo.IsXmlFile)
            {
                switch (_connectionInfo.Type)
                {
                    case ConnectionTypes.MSSQL:
                        _currentConnectionFrame = new MSSQLConnectionFrame(_connectionInfo.ConnectionString);
                        break;
                    case ConnectionTypes.MSAccess:
                        _currentConnectionFrame = new MSAccessConnectionFrame(_connectionInfo.ConnectionString);
                        break;
                    case ConnectionTypes.Oracle:
                        _currentConnectionFrame = new OracleConnectionFrame(_connectionInfo.ConnectionString);
                        break;
                    case ConnectionTypes.MySQL:
                        _currentConnectionFrame = new MySQLConnectionFrame(_connectionInfo.ConnectionString);
                        break;
                    case ConnectionTypes.PostgreSQL:
                        _currentConnectionFrame = new PostgreSQLConnectionFrame(_connectionInfo.ConnectionString);
                        break;
                    case ConnectionTypes.OLEDB:
                        _currentConnectionFrame = new OLEDBConnectionFrame(_connectionInfo.ConnectionString);
                        break;
                    case ConnectionTypes.ODBC:
                        _currentConnectionFrame = new ODBCConnectionFrame(_connectionInfo.ConnectionString);
                        break;
                }
            }
            else
            {
                _currentConnectionFrame = new XmlFileFrame(_connectionInfo.ConnectionString);
            }

            if (_currentConnectionFrame != null)
            {
                GridFrames.Children.Add((FrameworkElement)_currentConnectionFrame);
                _currentConnectionFrame.SyntaxProviderDetected += CurrentConnectionFrame_SyntaxProviderDetected;
            }
        }

        private void CurrentConnectionFrame_SyntaxProviderDetected(Type syntaxType)
        {
            var syntaxProvider = Activator.CreateInstance(syntaxType) as BaseSyntaxProvider;
            BoxSyntaxProvider.SelectedItem = SyntaxToString(syntaxProvider);
            FillVersions();
        }

        private void ButtonBaseOK_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonBaseClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BoxSyntaxProvider_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLockUpdate) return;

            switch ((string) BoxSyntaxProvider.SelectedItem)
            {
                case "ANSI SQL-2003":
                    _connectionInfo.SyntaxProvider = new SQL2003SyntaxProvider();
                    break;
                case "ANSI SQL-92":
                    _connectionInfo.SyntaxProvider = new SQL92SyntaxProvider();
                    break;
                case "ANSI SQL-89":
                    _connectionInfo.SyntaxProvider = new SQL89SyntaxProvider();
                    break;
                case "Firebird":
                    _connectionInfo.SyntaxProvider = new FirebirdSyntaxProvider();
                    break;
                case "IBM DB2":
                    _connectionInfo.SyntaxProvider = new DB2SyntaxProvider();
                    break;
                case "IBM Informix":
                    _connectionInfo.SyntaxProvider = new InformixSyntaxProvider();
                    break;
                case "Microsoft Access":
                    _connectionInfo.SyntaxProvider = new MSAccessSyntaxProvider();
                    break;
                case "Microsoft SQL Server":
                    _connectionInfo.SyntaxProvider = new MSSQLSyntaxProvider();
                    break;
                case "MySQL":
                    _connectionInfo.SyntaxProvider = new MySQLSyntaxProvider();
                    break;
                case "Oracle":
                    _connectionInfo.SyntaxProvider = new OracleSyntaxProvider();
                    break;
                case "PostgreSQL":
                    _connectionInfo.SyntaxProvider = new PostgreSQLSyntaxProvider();
                    break;
                case "SQLite":
                    _connectionInfo.SyntaxProvider = new SQLiteSyntaxProvider();
                    break;
                case "Sybase":
                    _connectionInfo.SyntaxProvider = new SybaseSyntaxProvider();
                    break;
                case "VistaDB":
                    _connectionInfo.SyntaxProvider = new VistaDBSyntaxProvider();
                    break;
                case "Universal":
                    _connectionInfo.SyntaxProvider = new GenericSyntaxProvider();
                    break;
            }

            FillVersions();
        }

        private void BoxServerVersion_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLockUpdate) return;

            if (BoxServerVersion.SelectedItem == null)
            {
                return;
            }

            if (_connectionInfo.SyntaxProvider is FirebirdSyntaxProvider)
            {
                if ((string)BoxServerVersion.SelectedItem == "Firebird 1.0")
                {
                    ((FirebirdSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = FirebirdVersion.Firebird10;
                }
                else if ((string)BoxServerVersion.SelectedItem == "Firebird 1.5")
                {
                    ((FirebirdSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = FirebirdVersion.Firebird15;
                }
                if ((string)BoxServerVersion.SelectedItem == "Firebird 2.0")
                {
                    ((FirebirdSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = FirebirdVersion.Firebird20;
                }
                if ((string)BoxServerVersion.SelectedItem == "Firebird 2.5")
                {
                    ((FirebirdSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = FirebirdVersion.Firebird25;
                }
            }
            else if (_connectionInfo.SyntaxProvider is InformixSyntaxProvider)
            {
                if ((string)BoxServerVersion.SelectedItem == "Informix 8")
                {
                    ((InformixSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = InformixVersion.DS8;
                }
                else if ((string)BoxServerVersion.SelectedItem == "Informix 9")
                {
                    ((InformixSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = InformixVersion.DS9;
                }
                if ((string)BoxServerVersion.SelectedItem == "Informix 10")
                {
                    ((InformixSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = InformixVersion.DS10;
                }
            }
            else if (_connectionInfo.SyntaxProvider is MSAccessSyntaxProvider)
            {
                if ((string)BoxServerVersion.SelectedItem == "Access 97")
                {
                    ((MSAccessSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = MSAccessServerVersion.MSJET3;
                }
                else if ((string)BoxServerVersion.SelectedItem == "Access 2000 and newer")
                {
                    ((MSAccessSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = MSAccessServerVersion.MSJET4;
                }
            }
            else if (_connectionInfo.SyntaxProvider is MSSQLSyntaxProvider)
            {
                if ((string)BoxServerVersion.SelectedItem == "Auto")
                {
                    ((MSSQLSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = MSSQLServerVersion.Auto;
                }
                else if ((string)BoxServerVersion.SelectedItem == "SQL Server 7")
                {
                    ((MSSQLSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = MSSQLServerVersion.MSSQL7;
                }
                else if ((string)BoxServerVersion.SelectedItem == "SQL Server 2000")
                {
                    ((MSSQLSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = MSSQLServerVersion.MSSQL2000;
                }
                else if ((string)BoxServerVersion.SelectedItem == "SQL Server 2005")
                {
                    ((MSSQLSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = MSSQLServerVersion.MSSQL2005;
                }
                else if ((string)BoxServerVersion.SelectedItem == "SQL Server 2008")
                {
                    ((MSSQLSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = MSSQLServerVersion.MSSQL2008;
                }
                else if ((string)BoxServerVersion.SelectedItem == "SQL Server 2012")
                {
                    ((MSSQLSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = MSSQLServerVersion.MSSQL2012;
                }
                else if ((string)BoxServerVersion.SelectedItem == "SQL Server 2014")
                {
                    ((MSSQLSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = MSSQLServerVersion.MSSQL2014;
                }
                else if ((string)BoxServerVersion.SelectedItem == "SQL Server 2016")
                {
                    ((MSSQLSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = MSSQLServerVersion.MSSQL2016;
                }
                else if ((string)BoxServerVersion.SelectedItem == "SQL Server 2017")
                {
                    ((MSSQLSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = MSSQLServerVersion.MSSQL2017;
                }
                else if ((string)BoxServerVersion.SelectedItem == "SQL Server 2019")
                {
                    ((MSSQLSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = MSSQLServerVersion.MSSQL2019;
                }
            }
            else if (_connectionInfo.SyntaxProvider is MySQLSyntaxProvider)
            {
                if ((string)BoxServerVersion.SelectedItem == "3.0")
                {
                    ((MySQLSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersionInt = 39999;
                }
                else if ((string)BoxServerVersion.SelectedItem == "4.0")
                {
                    ((MySQLSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersionInt = 49999;
                }
                else if ((string)BoxServerVersion.SelectedItem == "5.0")
                {
                    ((MySQLSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersionInt = 50000;
                }
                else if ((string)BoxServerVersion.SelectedItem == "8.0+")
                {
                    ((MySQLSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersionInt = 80012;
                }
            }
            else if (_connectionInfo.SyntaxProvider is OracleSyntaxProvider)
            {
                if ((string)BoxServerVersion.SelectedItem == "Oracle 7")
                {
                    ((OracleSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = OracleServerVersion.Oracle7;
                }
                else if ((string)BoxServerVersion.SelectedItem == "Oracle 8")
                {
                    ((OracleSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = OracleServerVersion.Oracle8;
                }
                else if ((string)BoxServerVersion.SelectedItem == "Oracle 9")
                {
                    ((OracleSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = OracleServerVersion.Oracle9;
                }
                else if ((string)BoxServerVersion.SelectedItem == "Oracle 10")
                {
                    ((OracleSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = OracleServerVersion.Oracle10;
                }
                else if ((string)BoxServerVersion.SelectedItem == "Oracle 11g")
                {
                    ((OracleSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = OracleServerVersion.Oracle11;
                }
                else if ((string)BoxServerVersion.SelectedItem == "Oracle 12c")
                {
                    ((OracleSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = OracleServerVersion.Oracle12;
                }
                else if ((string)BoxServerVersion.SelectedItem == "Oracle 18c")
                {
                    ((OracleSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = OracleServerVersion.Oracle18;
                }
                else if ((string)BoxServerVersion.SelectedItem == "Oracle 19c")
                {
                    ((OracleSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = OracleServerVersion.Oracle19;
                }
            }
            else if (_connectionInfo.SyntaxProvider is SybaseSyntaxProvider)
            {
                if ((string)BoxServerVersion.SelectedItem == "ASE")
                {
                    ((SybaseSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = SybaseServerVersion.SybaseASE;
                }
                else if ((string)BoxServerVersion.SelectedItem == "SQL Anywhere")
                {
                    ((SybaseSyntaxProvider)_connectionInfo.SyntaxProvider).ServerVersion = SybaseServerVersion.SybaseASA;
                }
            }

            _currentConnectionFrame.SetServerType(BoxServerVersion.SelectedItem as string);
        }
    }
}
