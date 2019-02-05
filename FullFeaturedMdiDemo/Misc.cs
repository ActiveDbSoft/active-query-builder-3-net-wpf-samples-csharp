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

namespace FullFeaturedMdiDemo
{
    public enum ConnectionTypes
    {
        MSSQL,
        MSSQLAzure,
        MSAccess,
        Oracle,
        MySQL,
        PostgreSQL,
        OLEDB,
        ODBC,
        SQLite,
        DB2,
        Firebird,
        Excel
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

        public ConnectionInfo this[int index]
        {
            get
            {
                return (ConnectionInfo)_connections[index];
            }
        }

        public int Count
        {
            get
            {
                return _connections.Count;
            }
        }

        public System.Collections.ArrayList Connections
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
        public string ConnectionString { get; set; }
        public bool IsXmlFile { get; set; }
        public string XMLPath { get; set; }
        public string CacheFile { get; set; }
        public string UserQueries { get; set; }
        public string MetadataStructure { get; set; }
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
                    case ConnectionTypes.Oracle:
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
                }
            }
            catch
            {
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
                return ConnectionTypes.Oracle;
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

            return ConnectionTypes.MSSQL;
        }

        public override bool Equals(Object obj)
        {
            if (obj != null && obj is ConnectionInfo)
            {
                if (((ConnectionInfo)obj).Type == this.Type &&
                    ((ConnectionInfo)obj).Name == this.Name &&
                    ((ConnectionInfo)obj).ConnectionString == this.ConnectionString &&
                    ((ConnectionInfo)obj).IsXmlFile == this.IsXmlFile)
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
