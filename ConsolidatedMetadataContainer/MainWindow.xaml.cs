//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2019 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Collections.Generic;
using System.Data.SqlClient;
using ActiveQueryBuilder.Core;

namespace ConsolidatedMetadataContainer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Dictionary<string, SQLContext> _connections = new Dictionary<string, SQLContext>();
        private readonly Dictionary<MetadataItem, MetadataItem> _consolidatedToInner = new Dictionary<MetadataItem, MetadataItem>();
        private readonly Dictionary<MetadataItem, MetadataItem> _innerToConsolidated = new Dictionary<MetadataItem, MetadataItem>();

        public MainWindow()
        {
            InitializeComponent();

            // first connection
            var xmlNorthwind = new SQLContext
            {
                SyntaxProvider = new MSSQLSyntaxProvider(),
            };
            xmlNorthwind.MetadataContainer.ImportFromXML("northwind.xml");
            _connections.Add("xml", xmlNorthwind);

            // second connection
            var mssqlAdventureWorks = new SQLContext
            {
                SyntaxProvider = new MSSQLSyntaxProvider(),
                MetadataProvider = new MSSQLMetadataProvider
                {
                    Connection = new SqlConnection("Server=sql2014;Database=AdventureWorks;User Id=sa;Password=********;"),
                },
            };
            _connections.Add("live", mssqlAdventureWorks);

            // QueryBuilder with consolidated metadata
            QBuilder.MetadataContainer.ItemMetadataLoading += MetadataContainerOnItemMetadataLoading;

            QBuilder.InitializeDatabaseSchemaTree();
        }

        private void MetadataContainerOnItemMetadataLoading(object sender, MetadataItem item, MetadataType loadTypes)
        {
            // root of consolidated metadata contains connections
            if (item == QBuilder.MetadataContainer && loadTypes.Contains(MetadataType.Connection))
            {
                // add connections (as virtual "Connection" objects)
                foreach (var connectionDescription in _connections)
                {
                    var connectionName = connectionDescription.Key;
                    var connection = connectionDescription.Value;
                    var innerItem = connection.MetadataContainer;

                    if (_innerToConsolidated.ContainsKey(innerItem))
                        continue;

                    var newItem = item.AddConnection(connectionName);
                    newItem.Items = innerItem.Items;

                    MapConsolidatedToInnerRecursive(newItem, innerItem);
                }

                return;
            }

            // find "inner" item, load it's children and copy them to consolidated container
            {
                var innerItem = _consolidatedToInner[item];
                innerItem.Items.Load(loadTypes, false);

                foreach (var childItem in innerItem.Items)
                {
                    if (!loadTypes.Contains(childItem.Type))
                        continue;

                    if (_innerToConsolidated.ContainsKey(childItem))
                        continue;

                    var newItem = childItem.Clone(item.Items);
                    item.Items.Add(newItem);

                    MapConsolidatedToInnerRecursive(newItem, childItem);
                }
            }
        }

        private void MapConsolidatedToInnerRecursive(MetadataItem consolidated, MetadataItem inner)
        {
            _consolidatedToInner.Add(consolidated, inner);
            _innerToConsolidated.Add(inner, consolidated);

            for (var i = 0; i < inner.Items.Count; i++)
                MapConsolidatedToInnerRecursive(consolidated.Items[i], inner.Items[i]);
        }
    }
}
