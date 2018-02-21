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
using System.Collections;
using System.Configuration;
using System.Diagnostics;
using System.Xml.Serialization;
using ActiveQueryBuilder.Core;

namespace FullFeaturedDemo
{
    public enum ConnectionTypes
    {
        MSSQL,
        MSAccess,
        Oracle,
        MySQL,
        PostgreSQL,
        OLEDB,
        ODBC
    }

    [Serializable]
    [XmlInclude(typeof(ConnectionInfo))]
    [SettingsSerializeAs(SettingsSerializeAs.String)]
    public class ConnectionList
    {
        [XmlElement(Type = typeof(ConnectionInfo))]
        private ArrayList _connections = new ArrayList();

        public ConnectionInfo this[int index]
        {
            get { return (ConnectionInfo)_connections[index]; }
        }

        public int Count
        {
            get { return _connections.Count; }
        }

        public ArrayList Connections
        {
            get { return _connections; }
            set { _connections = value; }
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
        private BaseSyntaxProvider _syntaxProvider;
        private string _syntaxProviderName;

        public ConnectionTypes ConnectionType;
        public string ConnectionName;
        public string ConnectionString;
        public bool IsXmlFile;
        public string CacheFile;
        public string UserQueries;

        public string SyntaxProviderName
        {
            set
            {
                if (_syntaxProviderName == value) return;

                _syntaxProviderName = value;

                var foundSyntaxProviderType = typeof(GenericSyntaxProvider);

                // find by syntaxProvider.GetType().Name
                foreach (Type syntaxProviderType in Helpers.SyntaxProviderList)
                {
                    if (string.Equals(syntaxProviderType.Name, value, StringComparison.InvariantCultureIgnoreCase))
                    {
                        foundSyntaxProviderType = syntaxProviderType;
                        break;
                    }
                }

                // find by syntaxProvider.Description for backward compatibility
                foreach (Type syntaxProviderType in Helpers.SyntaxProviderList)
                {
                    using (var syntaxProvider = (BaseSyntaxProvider)Activator.CreateInstance(syntaxProviderType))
                    {
                        if (string.Equals(syntaxProvider.Description, value, StringComparison.InvariantCultureIgnoreCase))
                        {
                            foundSyntaxProviderType = syntaxProviderType;
                            break;
                        }
                    }
                }

                // same type?
                if (_syntaxProvider != null && _syntaxProvider.GetType() == foundSyntaxProviderType)
                    return;

                // replace syntax provider
                if (_syntaxProvider != null)
                    _syntaxProvider.Dispose();

                _syntaxProvider = (BaseSyntaxProvider)Activator.CreateInstance(foundSyntaxProviderType);
            }
            get { return _syntaxProviderName; }
        }

        [XmlIgnore]
        public BaseSyntaxProvider SyntaxProvider
        {
            set
            {
                _syntaxProvider = value;
                if (_syntaxProvider != null)
                    SyntaxProviderName = _syntaxProvider.GetType().Name;
            }
            get { return _syntaxProvider; }
        }

        public ConnectionInfo()
        {
            ConnectionType = ConnectionTypes.MSSQL;

            ConnectionName = null;
            ConnectionString = null;
            IsXmlFile = false;
            CacheFile = null;
        }

        public ConnectionInfo(ConnectionTypes connectionType, string connectionName, string connectionString, bool isFromXml, string cacheFile, string userQueriesXml)
        {
            ConnectionType = connectionType;
            ConnectionName = connectionName;
            ConnectionString = connectionString;
            IsXmlFile = isFromXml;
            CacheFile = cacheFile;
            UserQueries = userQueriesXml;
        }

        public override bool Equals(object obj)
        {
            if (obj is ConnectionInfo)
            {
                if (((ConnectionInfo)obj).ConnectionType == ConnectionType &&
                    ((ConnectionInfo)obj).ConnectionName == ConnectionName &&
                    ((ConnectionInfo)obj).ConnectionString == ConnectionString &&
                    ((ConnectionInfo)obj).IsXmlFile == IsXmlFile)
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
