//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2023 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows.Controls;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.WPF;

namespace GeneralAssembly.QueryBuilderProperties
{
    /// <summary>
    /// Interaction logic for SqlSyntaxPage.xaml
    /// </summary>
    public partial class SqlSyntaxPage
    {
        public bool Modified { get; set; }

        private readonly QueryBuilder _queryBuilder = null;
        private BaseSyntaxProvider _syntaxProvider = null;

        public SqlSyntaxPage()
        {
            Modified = false;
            InitializeComponent();
        }

        public SqlSyntaxPage(QueryBuilder queryBuilder, BaseSyntaxProvider syntaxProvider)
        {
            Modified = false;
            _queryBuilder = queryBuilder;
            _syntaxProvider = syntaxProvider;

            InitializeComponent();

            comboIdentCaseSens.Items.Add("All identifiers are case insensitive");
            comboIdentCaseSens.Items.Add("Quoted are sensitive, Unquoted are converted to uppercase");
            comboIdentCaseSens.Items.Add("Quoted are sensitive, Unquoted are converted to lowercase");

            comboSqlDialect.Items.Add("Auto");
            comboSqlDialect.Items.Add("ANSI SQL-2003");
            comboSqlDialect.Items.Add("ANSI SQL-89");
            comboSqlDialect.Items.Add("ANSI SQL-92");
            comboSqlDialect.Items.Add("Firebird 1.0");
            comboSqlDialect.Items.Add("Firebird 1.5");
            comboSqlDialect.Items.Add("Firebird 2.0");
            comboSqlDialect.Items.Add("Firebird 2.5");
            comboSqlDialect.Items.Add("IBM DB2");
            comboSqlDialect.Items.Add("IBM Informix 8");
            comboSqlDialect.Items.Add("IBM Informix 9");
            comboSqlDialect.Items.Add("IBM Informix 10");
            comboSqlDialect.Items.Add("MS Access 97 (MS Jet 3.0)");
            comboSqlDialect.Items.Add("MS Access 2000 (MS Jet 4.0)");
            comboSqlDialect.Items.Add("MS Access XP (MS Jet 4.0)");
            comboSqlDialect.Items.Add("MS Access 2003 (MS Jet 4.0)");
            comboSqlDialect.Items.Add("MS SQL Server 7");
            comboSqlDialect.Items.Add("MS SQL Server 2000");
            comboSqlDialect.Items.Add("MS SQL Server 2005");
            comboSqlDialect.Items.Add("MS SQL Server 2008");
            comboSqlDialect.Items.Add("MS SQL Server 2012");
            comboSqlDialect.Items.Add("MS SQL Server 2014");
            comboSqlDialect.Items.Add("MS SQL Server 2016");
            comboSqlDialect.Items.Add("MS SQL Server 2017");
            comboSqlDialect.Items.Add("MS SQL Server 2019");
            comboSqlDialect.Items.Add("MySQL 3.xx");
            comboSqlDialect.Items.Add("MySQL 4.0");
            comboSqlDialect.Items.Add("MySQL 4.1");
            comboSqlDialect.Items.Add("MySQL 5.0");
            comboSqlDialect.Items.Add("MySQL 8.0");
            comboSqlDialect.Items.Add("Oracle 7");
            comboSqlDialect.Items.Add("Oracle 8");
            comboSqlDialect.Items.Add("Oracle 9");
            comboSqlDialect.Items.Add("Oracle 10");
            comboSqlDialect.Items.Add("Oracle 11g");
            comboSqlDialect.Items.Add("Oracle 12c");
            comboSqlDialect.Items.Add("Oracle 18c");
            comboSqlDialect.Items.Add("Oracle 19c");
            comboSqlDialect.Items.Add("PostgreSQL");
            comboSqlDialect.Items.Add("Snowflake");
            comboSqlDialect.Items.Add("SQLite");
            comboSqlDialect.Items.Add("Sybase ASE");
            comboSqlDialect.Items.Add("Sybase SQL Anywhere");
            comboSqlDialect.Items.Add("Teradata");
            comboSqlDialect.Items.Add("VistaDB");
            comboSqlDialect.Items.Add("Generic");

            if (queryBuilder.SyntaxProvider is SQL92SyntaxProvider)
            {
                comboSqlDialect.SelectedItem = "ANSI SQL-92";
            }
            else if (queryBuilder.SyntaxProvider is AutoSyntaxProvider)
            {
                comboSqlDialect.SelectedItem = "Auto";
            }
            else if (queryBuilder.SyntaxProvider is SQL89SyntaxProvider)
            {
                comboSqlDialect.SelectedItem = "ANSI SQL-89";
            }
            else if (queryBuilder.SyntaxProvider is SQL2003SyntaxProvider)
            {
                comboSqlDialect.SelectedItem = "ANSI SQL-2003";
            }
            else if (queryBuilder.SyntaxProvider is FirebirdSyntaxProvider)
            {
                switch ((queryBuilder.SyntaxProvider as FirebirdSyntaxProvider).ServerVersion)
                {
                    case FirebirdVersion.Firebird10:
                        comboSqlDialect.SelectedItem = "Firebird 1.0";
                        break;
                    case FirebirdVersion.Firebird15:
                        comboSqlDialect.SelectedItem = "Firebird 1.5";
                        break;
                    case FirebirdVersion.Firebird25:
                        comboSqlDialect.SelectedItem = "Firebird 2.5";
                        break;
                    default:
                        comboSqlDialect.SelectedItem = "Firebird 2.0";
                        break;
                }
            }
            else if (queryBuilder.SyntaxProvider is DB2SyntaxProvider)
            {
                comboSqlDialect.SelectedItem = "IBM DB2";
            }
            else if (queryBuilder.SyntaxProvider is InformixSyntaxProvider)
            {
                switch ((queryBuilder.SyntaxProvider as InformixSyntaxProvider).ServerVersion)
                {
                    case InformixVersion.DS8:
                        comboSqlDialect.SelectedItem = "IBM Informix 8";
                        break;
                    case InformixVersion.DS9:
                        comboSqlDialect.SelectedItem = "IBM Informix 9";
                        break;
                    default:
                        comboSqlDialect.SelectedItem = "IBM Informix 10";
                        break;
                }
            }
            else if (queryBuilder.SyntaxProvider is MSAccessSyntaxProvider)
            {
                switch ((queryBuilder.SyntaxProvider as MSAccessSyntaxProvider).ServerVersion)
                {
                    case MSAccessServerVersion.MSJET3:
                        comboSqlDialect.SelectedItem = "MS Access 97 (MS Jet 3.0)";
                        break;
                    case MSAccessServerVersion.MSJET4:
                        comboSqlDialect.SelectedItem = "MS Access 2003 (MS Jet 4.0)";
                        break;
                    default:
                        comboSqlDialect.SelectedItem = "MS Access 2003 (MS Jet 4.0)";
                        break;
                }
            }
            else if (queryBuilder.SyntaxProvider is MSSQLSyntaxProvider)
            {
                switch ((queryBuilder.SyntaxProvider as MSSQLSyntaxProvider).ServerVersion)
                {
                    case MSSQLServerVersion.MSSQL7:
                        comboSqlDialect.SelectedItem = "MS SQL Server 7";
                        break;
                    case MSSQLServerVersion.MSSQL2000:
                        comboSqlDialect.SelectedItem = "MS SQL Server 2000";
                        break;
                    case MSSQLServerVersion.MSSQL2005:
                        comboSqlDialect.SelectedItem = "MS SQL Server 2005";
                        break;
                    case MSSQLServerVersion.MSSQL2008:
                        comboSqlDialect.SelectedItem = "MS SQL Server 2008";
                        break;
                    case MSSQLServerVersion.MSSQL2012:
                        comboSqlDialect.SelectedItem = "MS SQL Server 2012";
                        break;
                    case MSSQLServerVersion.MSSQL2014:
                        comboSqlDialect.SelectedItem = "MS SQL Server 2014";
                        break;
                    case MSSQLServerVersion.MSSQL2016:
                        comboSqlDialect.SelectedItem = "MS SQL Server 2016";
                        break;
                    case MSSQLServerVersion.MSSQL2017:
                        comboSqlDialect.SelectedItem = "MS SQL Server 2017";
                        break;
                    case MSSQLServerVersion.MSSQL2019:
                        comboSqlDialect.SelectedItem = "MS SQL Server 2019";
                        break;
                    default:
                        comboSqlDialect.SelectedItem = "MS SQL Server 2017";
                        break;
                }
            }
            else if (queryBuilder.SyntaxProvider is MySQLSyntaxProvider)
            {
                if ((queryBuilder.SyntaxProvider as MySQLSyntaxProvider).ServerVersionInt < 40000)
                {
                    comboSqlDialect.SelectedItem = "MySQL 3.xx";
                }
                else if ((queryBuilder.SyntaxProvider as MySQLSyntaxProvider).ServerVersionInt <= 40099)
                {
                    comboSqlDialect.SelectedItem = "MySQL 4.0";
                }
                else if ((queryBuilder.SyntaxProvider as MySQLSyntaxProvider).ServerVersionInt < 50000)
                {
                    comboSqlDialect.SelectedItem = "MySQL 4.1";
                }
                else if ((queryBuilder.SyntaxProvider as MySQLSyntaxProvider).ServerVersionInt < 80000)
                {
                    comboSqlDialect.SelectedItem = "MySQL 5.0";
                }
                else
                {
                    comboSqlDialect.SelectedItem = "MySQL 8.0";
                }
            }
            else if (queryBuilder.SyntaxProvider is OracleSyntaxProvider)
            {
                switch ((queryBuilder.SyntaxProvider as OracleSyntaxProvider).ServerVersion)
                {
                    case OracleServerVersion.Oracle7:
                        comboSqlDialect.SelectedItem = "Oracle 7";
                        break;
                    case OracleServerVersion.Oracle8:
                        comboSqlDialect.SelectedItem = "Oracle 8";
                        break;
                    case OracleServerVersion.Oracle9:
                        comboSqlDialect.SelectedItem = "Oracle 9";
                        break;
                    case OracleServerVersion.Oracle10:
                        comboSqlDialect.SelectedItem = "Oracle 10";
                        break;
                    case OracleServerVersion.Oracle11:
                        comboSqlDialect.SelectedItem = "Oracle 11g";
                        break;
                    case OracleServerVersion.Oracle12:
                        comboSqlDialect.SelectedItem = "Oracle 12c";
                        break;
                    case OracleServerVersion.Oracle18:
                        comboSqlDialect.SelectedItem = "Oracle 18c";
                        break;
                    case OracleServerVersion.Oracle19:
                        comboSqlDialect.SelectedItem = "Oracle 19c";
                        break;
                    default:
                        comboSqlDialect.SelectedItem = "Oracle 18c";
                        break;
                }
            }
            else if (queryBuilder.SyntaxProvider is PostgreSQLSyntaxProvider)
            {
                comboSqlDialect.SelectedItem = "PostgreSQL";
            }
            else if (queryBuilder.SyntaxProvider is SnowflakeSyntaxProvider)
            {
                comboSqlDialect.SelectedItem = "Snowflake";
            }
            else if (queryBuilder.SyntaxProvider is SQLiteSyntaxProvider)
            {
                comboSqlDialect.SelectedItem = "SQLite";
            }
            else if (queryBuilder.SyntaxProvider is SybaseSyntaxProvider)
            {
                switch ((queryBuilder.SyntaxProvider as SybaseSyntaxProvider).ServerVersion)
                {
                    case SybaseServerVersion.SybaseASE:
                        comboSqlDialect.SelectedItem = "Sybase ASE";
                        break;
                    case SybaseServerVersion.SybaseASA:
                        comboSqlDialect.SelectedItem = "Sybase SQL Anywhere";
                        break;
                    default:
                        comboSqlDialect.SelectedItem = "Sybase SQL Anywhere";
                        break;
                }
            }
            else if (queryBuilder.SyntaxProvider is TeradataSyntaxProvider)
            {
                comboSqlDialect.SelectedItem = "Teradata";
            }
            else if (queryBuilder.SyntaxProvider is VistaDBSyntaxProvider)
            {
                comboSqlDialect.SelectedItem = "VistaDB";
            }

            if (queryBuilder.SyntaxProvider is GenericSyntaxProvider)
            {
                comboSqlDialect.SelectedItem = "Generic";
            }

            if (queryBuilder.SyntaxProvider != null)
            {
                comboIdentCaseSens.SelectedIndex = (int)queryBuilder.SyntaxProvider.IdentCaseSens;
                textBeginQuotationSymbol.Text = queryBuilder.SyntaxProvider.QuoteBegin;
                textEndQuotationSymbol.Text = queryBuilder.SyntaxProvider.QuoteEnd;
            }

            cbQuoteAllIdentifiers.IsChecked = queryBuilder.SQLGenerationOptions.QuoteIdentifiers == IdentQuotation.All;

            comboSqlDialect.SelectionChanged += comboSqlDialect_SelectionChanged;
            comboIdentCaseSens.SelectionChanged += comboIdentCaseSens_SelectionChanged;
            cbQuoteAllIdentifiers.Checked += cbQuoteAllIdentifiers_Checked;
            cbQuoteAllIdentifiers.Unchecked += cbQuoteAllIdentifiers_Checked;
        }

        void cbQuoteAllIdentifiers_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Modified = true;
        }

        void comboIdentCaseSens_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _syntaxProvider.IdentCaseSens = (IdentCaseSensitivity)comboIdentCaseSens.SelectedIndex;
            comboIdentCaseSens.SelectedIndex = (int)_syntaxProvider.IdentCaseSens;

            Modified = true;
        }

        void comboSqlDialect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (comboSqlDialect.SelectedItem.ToString())
            {
                case "ANSI SQL-92":
                    _syntaxProvider = new SQL92SyntaxProvider();
                    break;
                case "Auto":
                    _syntaxProvider = new AutoSyntaxProvider();
                    break;
                case "ANSI SQL-89":
                    _syntaxProvider = new SQL89SyntaxProvider();
                    break;
                case "ANSI SQL-2003":
                    _syntaxProvider = new SQL2003SyntaxProvider();
                    break;
                case "Firebird 1.0":
                    _syntaxProvider = new FirebirdSyntaxProvider();
                    (_syntaxProvider as FirebirdSyntaxProvider).ServerVersion = FirebirdVersion.Firebird10;
                    break;
                case "Firebird 1.5":
                    _syntaxProvider = new FirebirdSyntaxProvider();
                    (_syntaxProvider as FirebirdSyntaxProvider).ServerVersion = FirebirdVersion.Firebird15;
                    break;
                case "Firebird 2.0":
                    _syntaxProvider = new FirebirdSyntaxProvider();
                    (_syntaxProvider as FirebirdSyntaxProvider).ServerVersion = FirebirdVersion.Firebird20;
                    break;
                case "Firebird 2.5":
                    _syntaxProvider = new FirebirdSyntaxProvider();
                    (_syntaxProvider as FirebirdSyntaxProvider).ServerVersion = FirebirdVersion.Firebird25;
                    break;
                case "IBM DB2":
                    _syntaxProvider = new DB2SyntaxProvider();
                    break;
                case "IBM Informix 8":
                    _syntaxProvider = new InformixSyntaxProvider();
                    (_syntaxProvider as InformixSyntaxProvider).ServerVersion = InformixVersion.DS8;
                    break;
                case "IBM Informix 9":
                    _syntaxProvider = new InformixSyntaxProvider();
                    (_syntaxProvider as InformixSyntaxProvider).ServerVersion = InformixVersion.DS9;
                    break;
                case "IBM Informix 10":
                    _syntaxProvider = new InformixSyntaxProvider();
                    (_syntaxProvider as InformixSyntaxProvider).ServerVersion = InformixVersion.DS10;
                    break;
                case "MS Access 97 (MS Jet 3.0)":
                    _syntaxProvider = new MSAccessSyntaxProvider();
                    (_syntaxProvider as MSAccessSyntaxProvider).ServerVersion = MSAccessServerVersion.MSJET3;
                    break;
                case "MS Access 2000 (MS Jet 4.0)":
                case "MS Access XP (MS Jet 4.0)":
                case "MS Access 2003 (MS Jet 4.0)":
                    _syntaxProvider = new MSAccessSyntaxProvider();
                    (_syntaxProvider as MSAccessSyntaxProvider).ServerVersion = MSAccessServerVersion.MSJET4;
                    break;
                case "MS SQL Server 7":
                    _syntaxProvider = new MSSQLSyntaxProvider();
                    (_syntaxProvider as MSSQLSyntaxProvider).ServerVersion = MSSQLServerVersion.MSSQL7;
                    break;
                case "MS SQL Server 2000":
                    _syntaxProvider = new MSSQLSyntaxProvider();
                    (_syntaxProvider as MSSQLSyntaxProvider).ServerVersion = MSSQLServerVersion.MSSQL2000;
                    break;
                case "MS SQL Server 2005":
                    _syntaxProvider = new MSSQLSyntaxProvider();
                    (_syntaxProvider as MSSQLSyntaxProvider).ServerVersion = MSSQLServerVersion.MSSQL2005;
                    break;
                case "MS SQL Server 2008":
                    _syntaxProvider = new MSSQLSyntaxProvider();
                    (_syntaxProvider as MSSQLSyntaxProvider).ServerVersion = MSSQLServerVersion.MSSQL2008;
                    break;
                case "MS SQL Server 2012":
                    _syntaxProvider = new MSSQLSyntaxProvider();
                    (_syntaxProvider as MSSQLSyntaxProvider).ServerVersion = MSSQLServerVersion.MSSQL2012;
                    break;
                case "MS SQL Server 2014":
                    _syntaxProvider = new MSSQLSyntaxProvider();
                    (_syntaxProvider as MSSQLSyntaxProvider).ServerVersion = MSSQLServerVersion.MSSQL2014;
                    break;
                case "MS SQL Server 2016":
                    _syntaxProvider = new MSSQLSyntaxProvider();
                    (_syntaxProvider as MSSQLSyntaxProvider).ServerVersion = MSSQLServerVersion.MSSQL2016;
                    break;
                case "MS SQL Server 2017":
                    _syntaxProvider = new MSSQLSyntaxProvider();
                    (_syntaxProvider as MSSQLSyntaxProvider).ServerVersion = MSSQLServerVersion.MSSQL2017;
                    break;
                case "MS SQL Server 2019":
                    _syntaxProvider = new MSSQLSyntaxProvider();
                    (_syntaxProvider as MSSQLSyntaxProvider).ServerVersion = MSSQLServerVersion.MSSQL2019;
                    break;
                case "MySQL 3.xx":
                    _syntaxProvider = new MySQLSyntaxProvider();
                    (_syntaxProvider as MySQLSyntaxProvider).ServerVersionInt = 39999;
                    break;
                case "MySQL 4.0":
                    _syntaxProvider = new MySQLSyntaxProvider();
                    (_syntaxProvider as MySQLSyntaxProvider).ServerVersionInt = 40099;
                    break;
                case "MySQL 4.1":
                    _syntaxProvider = new MySQLSyntaxProvider();
                    (_syntaxProvider as MySQLSyntaxProvider).ServerVersionInt = 49999;
                    break;
                case "MySQL 5.0":
                    _syntaxProvider = new MySQLSyntaxProvider();
                    (_syntaxProvider as MySQLSyntaxProvider).ServerVersionInt = 50000;
                    break;
                case "MySQL 8.0":
                    _syntaxProvider = new MySQLSyntaxProvider();
                    (_syntaxProvider as MySQLSyntaxProvider).ServerVersionInt = 80012;
                    break;
                case "Oracle 7":
                    _syntaxProvider = new OracleSyntaxProvider();
                    (_syntaxProvider as OracleSyntaxProvider).ServerVersion = OracleServerVersion.Oracle7;
                    break;
                case "Oracle 8":
                    _syntaxProvider = new OracleSyntaxProvider();
                    (_syntaxProvider as OracleSyntaxProvider).ServerVersion = OracleServerVersion.Oracle8;
                    break;
                case "Oracle 9":
                    _syntaxProvider = new OracleSyntaxProvider();
                    (_syntaxProvider as OracleSyntaxProvider).ServerVersion = OracleServerVersion.Oracle9;
                    break;
                case "Oracle 10":
                    _syntaxProvider = new OracleSyntaxProvider();
                    (_syntaxProvider as OracleSyntaxProvider).ServerVersion = OracleServerVersion.Oracle10;
                    break;
                case "Oracle 11g":
                    _syntaxProvider = new OracleSyntaxProvider();
                    (_syntaxProvider as OracleSyntaxProvider).ServerVersion = OracleServerVersion.Oracle11;
                    break;
                case "Oracle 12c":
                    _syntaxProvider = new OracleSyntaxProvider();
                    (_syntaxProvider as OracleSyntaxProvider).ServerVersion = OracleServerVersion.Oracle12;
                    break;
                case "Oracle 18c":
                    _syntaxProvider = new OracleSyntaxProvider();
                    (_syntaxProvider as OracleSyntaxProvider).ServerVersion = OracleServerVersion.Oracle18;
                    break;
                case "Oracle 19c":
                    _syntaxProvider = new OracleSyntaxProvider();
                    (_syntaxProvider as OracleSyntaxProvider).ServerVersion = OracleServerVersion.Oracle19;
                    break;
                case "PostgreSQL":
                    _syntaxProvider = new PostgreSQLSyntaxProvider();
                    break;
                case "Snowflake":
                    _syntaxProvider = new SnowflakeSyntaxProvider();
                    break;
                case "SQLite":
                    _syntaxProvider = new SQLiteSyntaxProvider();
                    break;
                case "Sybase ASE":
                    _syntaxProvider = new SybaseSyntaxProvider();
                    (_syntaxProvider as SybaseSyntaxProvider).ServerVersion = SybaseServerVersion.SybaseASE;
                    break;
                case "Sybase SQL Anywhere":
                    _syntaxProvider = new SybaseSyntaxProvider();
                    (_syntaxProvider as SybaseSyntaxProvider).ServerVersion = SybaseServerVersion.SybaseASA;
                    break;
                case "Teradata":
                    _syntaxProvider = new TeradataSyntaxProvider();
                    break;
                case "VistaDB":
                    _syntaxProvider = new VistaDBSyntaxProvider();
                    break;
                case "Generic":
                    _syntaxProvider = new GenericSyntaxProvider();
                    ((GenericSyntaxProvider)_syntaxProvider).RedetectServer(_queryBuilder.SQLContext);
                    break;
                default:
                    _syntaxProvider = new GenericSyntaxProvider();
                    break;
            }

            comboIdentCaseSens.SelectedIndex = (int)_syntaxProvider.IdentCaseSens;
            textBeginQuotationSymbol.Text = _syntaxProvider.QuoteBegin;
            textEndQuotationSymbol.Text = _syntaxProvider.QuoteEnd;

            Modified = true;
        }

        public void ApplyChanges()
        {
            if (!Modified) return;

            var oldSyntaxProvider = _queryBuilder.SyntaxProvider;

            _queryBuilder.SyntaxProvider = _syntaxProvider;
            _queryBuilder.SQLGenerationOptions.QuoteIdentifiers = cbQuoteAllIdentifiers.IsChecked.HasValue && cbQuoteAllIdentifiers.IsChecked.Value? IdentQuotation.All : IdentQuotation.IfNeed;
            _queryBuilder.SQLFormattingOptions.QuoteIdentifiers = cbQuoteAllIdentifiers.IsChecked.HasValue && cbQuoteAllIdentifiers.IsChecked.Value ? IdentQuotation.All : IdentQuotation.IfNeed;

            if (oldSyntaxProvider != null)
            {
                oldSyntaxProvider.Dispose();
            }
        }
    }
}
