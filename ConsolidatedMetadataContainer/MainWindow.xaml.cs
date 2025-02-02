//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2025 Active Database Software              //
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
        // list of connections, name -> innerContext
        private readonly Dictionary<string, SQLContext> _connections = InitConnections();

        // fill connections dictionary
        private static Dictionary<string, SQLContext> InitConnections()
        {
            var result = new Dictionary<string, SQLContext>();

            // first connection
            var innerXml = new SQLContext
            {
                SyntaxProvider = new MSSQLSyntaxProvider(),
            };
            innerXml.MetadataContainer.ImportFromXML("northwind.xml");
            result.Add("xml", innerXml);

            // second connection
            var innerMsSql = new SQLContext
            {
                SyntaxProvider = new MSSQLSyntaxProvider(),
                MetadataProvider = new MSSQLMetadataProvider
                {
                    Connection = new SqlConnection("Server=sql2014;Database=AdventureWorks;User Id=sa;Password=********;"),
                },
                LoadingOptions =
                {
                    LoadDefaultDatabaseOnly = false,
                },
            };
            result.Add("live", innerMsSql);

            return result;
        }

        public MainWindow()
        {
            InitializeComponent();

            // add connections
            var metadataContainer = QBuilder.MetadataContainer;
            foreach (var connectionDescription in _connections)
            {
                var name = connectionDescription.Key;
                var innerContext = connectionDescription.Value;
                metadataContainer.AddConnection(name, innerContext);
            }

            QBuilder.InitializeDatabaseSchemaTree();
        }
    }
}
