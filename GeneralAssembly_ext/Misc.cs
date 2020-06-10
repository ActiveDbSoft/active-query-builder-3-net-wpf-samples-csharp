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
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using ActiveQueryBuilder.Core;

namespace GeneralAssembly
{
    public enum SourceType
    {
        File,
        New,
        UserQueries
    }

    public enum ConnectionTypes
    {
        MSAccess,
        Excel,
        MSSQL,
        MSSQLAzure,
        MySQL,
        OracleNative,
        PostgreSQL,
        ODBC,
        OLEDB,
        SQLite,
        Firebird,
        VistaDB5,
        DB2,
        Advantage,
        Sybase,
        Informix,
        MSSQLCE
    }

    [Serializable]
    [XmlInclude(typeof(ConnectionInfo))]
    public class ConnectionList
    {
        [XmlElement(Type = typeof(ConnectionInfo))]
        private ArrayList _connections = new ArrayList();

        public void SaveData()
        {
            var xmlSerializer = new ActiveQueryBuilder.Core.Serialization.XmlSerializer();
            foreach (ConnectionInfo connection in _connections)
            {
                connection.ConnectionString = connection.ConnectionDescriptor.ConnectionString;
                connection.LoadingOptions =
                    xmlSerializer.Serialize(connection.ConnectionDescriptor.MetadataLoadingOptions);
                connection.SyntaxProviderState =
                    xmlSerializer.SerializeObject(connection.ConnectionDescriptor.SyntaxProvider);
            }            
        }

        public void RemoveObsoleteConnectionInfos()
        {
            var connectionsToRemove = new List<ConnectionInfo>();

            foreach (ConnectionInfo connection in _connections)
            {
                if (connection.ConnectionDescriptor == null)
                {
                    connectionsToRemove.Add(connection);
                }
            }

            foreach (ConnectionInfo connection in connectionsToRemove)
            {
                _connections.Remove(connection);
            }
        }

        public void RestoreData()
        {
            var xmlSerializer = new ActiveQueryBuilder.Core.Serialization.XmlSerializer();

            foreach (ConnectionInfo connection in _connections)
            {
                if (connection.ConnectionDescriptor == null) continue;

                connection.ConnectionDescriptor.ConnectionString = connection.ConnectionString;

                if (!string.IsNullOrEmpty(connection.LoadingOptions))
                {
                    xmlSerializer.Deserialize(connection.LoadingOptions,
                        connection.ConnectionDescriptor.MetadataLoadingOptions);
                }

                if (!string.IsNullOrEmpty(connection.SyntaxProviderName) && connection.IsGenericConnection())
                {
                    connection.ConnectionDescriptor.SyntaxProvider =
                        ConnectionInfo.GetSyntaxByName(connection.SyntaxProviderName);
                }

                if (!string.IsNullOrEmpty(connection.SyntaxProviderState))
                {
                    xmlSerializer.DeserializeObject(connection.SyntaxProviderState, connection.ConnectionDescriptor.SyntaxProvider);
                    connection.ConnectionDescriptor.RecreateSyntaxProperties();
                }
            }
        }

        public ConnectionInfo this[int index] => (ConnectionInfo)_connections[index];

        public int Count => _connections.Count;

        public ArrayList Connections
        {
            get
            {
                return _connections;
            }
            set
            {
                _connections = value;
            }
        }

        public void Add(ConnectionInfo ci)
        {
            _connections.Add(ci);
        }

        public void Insert(int index, ConnectionInfo ci)
        {
            _connections.Insert(index, ci);
        }

        public void Remove(ConnectionInfo ci)
        {
            _connections.Remove(ci);
        }
    }

    [Serializable]
    public class ConnectionInfo
    {
        public string Name { get; set; }
        [XmlIgnore]
        public BaseConnectionDescriptor ConnectionDescriptor { get; set; }
        [XmlIgnore]
        public BaseSyntaxProvider SyntaxProvider { get; set; }
        public string ConnectionString { get;set; }
        public bool IsXmlFile { get;set; }
        public string XMLPath { get; set; }
        public string UserQueries { get; set; }
        public string LoadingOptions { get; set; }
        public string SyntaxProviderState { get; set; }
        public string SyntaxProviderName { get; set; }

        public bool IsGenericConnection()
        {
            return ConnectionDescriptor is OLEDBConnectionDescriptor ||
                   ConnectionDescriptor is ODBCConnectionDescriptor;
        }

        public static BaseSyntaxProvider GetSyntaxByName(string name)
        {
            foreach (Type syntax in Helpers.SyntaxProviderList)
            {
                if (syntax.ToString() == name)
                {
                    return Activator.CreateInstance(syntax) as BaseSyntaxProvider;
                }
            }

            return null;
        }

        private ConnectionTypes _type = ConnectionTypes.MSSQL;

        public ConnectionTypes Type
        {
            get { return _type; }
            set
            {
                _type = value;
                CreateConnectionByType();

                if (!string.IsNullOrEmpty(SyntaxProviderName) && IsGenericConnection())
                {
                    ConnectionDescriptor.SyntaxProvider = GetSyntaxByName(SyntaxProviderName);
                }
            }
        }

        public ConnectionInfo(BaseConnectionDescriptor descriptor, string name, ConnectionTypes type, string connectionString)
        {
            Name = name;
            ConnectionDescriptor = descriptor;
            Type = type;
            ConnectionString = connectionString;
            IsXmlFile = false;
            ConnectionDescriptor.ConnectionString = connectionString;
        }

        public ConnectionInfo(string xmlPath, string name, ConnectionTypes type)
        {
            Name = name;
            XMLPath = xmlPath;
            Type = type;
            IsXmlFile = true;
            ConnectionString = string.Empty;
            CreateConnectionByType();
        }

        public ConnectionInfo(ConnectionTypes connectionType, string connectionName, string connectionString, bool isFromXml, string userQueriesXml)
        {
            Type = connectionType;
            Name = connectionName;
            ConnectionString = connectionString;
            IsXmlFile = isFromXml;
            UserQueries = userQueriesXml;
            CreateConnectionByType();
        }

        public ConnectionInfo()
        {
        }

        private void CreateConnectionByType()
        {
            try
            {
                switch (Type)
                {
                    case ConnectionTypes.MSAccess:
                        ConnectionDescriptor = new MSAccessConnectionDescriptor();
                        return;
                    case ConnectionTypes.MSSQL:
                        ConnectionDescriptor = new MSSQLConnectionDescriptor();
                        return;
                    case ConnectionTypes.MSSQLAzure:
                        ConnectionDescriptor = new MSSQLAzureConnectionDescriptor();
                        return;
                    case ConnectionTypes.MySQL:
                        ConnectionDescriptor = new MySQLConnectionDescriptor();
                        return;
                    case ConnectionTypes.OracleNative:
                        ConnectionDescriptor = new OracleNativeConnectionDescriptor();
                        return;
                    case ConnectionTypes.PostgreSQL:
                        ConnectionDescriptor = new PostgreSQLConnectionDescriptor();
                        return;
                    case ConnectionTypes.ODBC:
                        ConnectionDescriptor = new ODBCConnectionDescriptor();
                        return;
                    case ConnectionTypes.OLEDB:
                        ConnectionDescriptor = new OLEDBConnectionDescriptor();
                        return;
                    case ConnectionTypes.Firebird:
                        ConnectionDescriptor = new FirebirdConnectionDescriptor();
                        return;
                    case ConnectionTypes.SQLite:
                        ConnectionDescriptor = new SQLiteConnectionDescriptor();
                        return;
                    case ConnectionTypes.Excel:
                        ConnectionDescriptor = new ExcelConnectionDescriptor();
                        return;
                    case ConnectionTypes.VistaDB5:
                        ConnectionDescriptor = new VistaDB5ConnectionDescriptor();
                        break;
                    case ConnectionTypes.DB2:
                        ConnectionDescriptor = new DB2ConnectionDescriptor();
                        break;
                    case ConnectionTypes.Advantage:
                        ConnectionDescriptor = new AdvantageConnectionDescriptor();
                        break;
                    case ConnectionTypes.Sybase:
                        ConnectionDescriptor = new SybaseConnectionDescriptor();
                        break;
                    case ConnectionTypes.Informix:
                        ConnectionDescriptor = new InformixConnectionDescriptor();
                        break;
                    case ConnectionTypes.MSSQLCE:
                        ConnectionDescriptor = new MSSQLCEConnectionDescriptor();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            finally
            {
                //ignore
            }
        }

        public ConnectionTypes GetConnectionType(Type descriptorType)
        {
            if (descriptorType == typeof(MSAccessConnectionDescriptor))
            {
                return ConnectionTypes.MSAccess;
            }
            if (descriptorType == typeof(ExcelConnectionDescriptor))
            {
                return ConnectionTypes.Excel;
            }
            if (descriptorType == typeof(PostgreSQLConnectionDescriptor))
            {
                return ConnectionTypes.PostgreSQL;
            }
            if (descriptorType == typeof(MSSQLConnectionDescriptor))
            {
                return ConnectionTypes.MSSQL;
            }
            if (descriptorType == typeof(MSSQLAzureConnectionDescriptor))
            {
                return ConnectionTypes.MSSQLAzure;
            }
            if (descriptorType == typeof(MySQLConnectionDescriptor))
            {
                return ConnectionTypes.MySQL;
            }
            if (descriptorType == typeof(OracleNativeConnectionDescriptor))
            {
                return ConnectionTypes.OracleNative;
            }
            if (descriptorType == typeof(ODBCConnectionDescriptor))
            {
                return ConnectionTypes.ODBC;
            }
            if (descriptorType == typeof(OLEDBConnectionDescriptor))
            {
                return ConnectionTypes.OLEDB;
            }
            if (descriptorType == typeof(FirebirdConnectionDescriptor))
            {
                return ConnectionTypes.Firebird;
            }
            if (descriptorType == typeof(SQLiteConnectionDescriptor))
            {
                return ConnectionTypes.SQLite;
            }
            if (descriptorType == typeof(VistaDB5ConnectionDescriptor))
                return ConnectionTypes.VistaDB5;
            if (descriptorType == typeof(DB2ConnectionDescriptor))
                return ConnectionTypes.DB2;
            if (descriptorType == typeof(AdvantageConnectionDescriptor))
                return ConnectionTypes.Advantage;
            if (descriptorType == typeof(SybaseConnectionDescriptor))
                return ConnectionTypes.Sybase;
            if (descriptorType == typeof(InformixConnectionDescriptor))
                return ConnectionTypes.Informix;
            if (descriptorType == typeof(MSSQLCEConnectionDescriptor))
                return ConnectionTypes.MSSQLCE;

            return ConnectionTypes.MSSQL;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ConnectionInfo)) return false;

            return ((ConnectionInfo)obj).Type == Type &&
                   ((ConnectionInfo)obj).Name == Name &&
                   ((ConnectionInfo)obj).ConnectionString == ConnectionString &&
                   ((ConnectionInfo)obj).IsXmlFile == IsXmlFile;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class Misc
    {
        public static readonly List<Type> ConnectionDescriptorList = new List<Type>
        {
            typeof(MSAccessConnectionDescriptor),
            typeof(ExcelConnectionDescriptor),
            typeof(MSSQLConnectionDescriptor),
            typeof(MSSQLAzureConnectionDescriptor),
            typeof(MySQLConnectionDescriptor),
            typeof(OracleNativeConnectionDescriptor),
            typeof(PostgreSQLConnectionDescriptor),
            typeof(ODBCConnectionDescriptor),
            typeof(OLEDBConnectionDescriptor),
            typeof(SQLiteConnectionDescriptor),
            typeof(FirebirdConnectionDescriptor),
            typeof(VistaDB5ConnectionDescriptor),
            typeof(DB2ConnectionDescriptor),
            typeof(AdvantageConnectionDescriptor),
            typeof(SybaseConnectionDescriptor),
            typeof(InformixConnectionDescriptor),
            typeof(MSSQLCEConnectionDescriptor)
        };

        public static readonly List<string> ConnectionDescriptorNames = new List<string>
        {
            "MS Access",
            "Excel",
            "MS SQL Server",
            "MS SQL Server Azure",
            "MySQL",
            "Oracle Native",
            "PostgreSQL",
            "ODBC",
            "OLEDB",
            "SQLite",
            "Firebird",
            "VistaDB5",
            "DB2",
            "Advantage",
            "Sybase",
            "Informix",
            "MSSQLCE"
        };
    }
}
