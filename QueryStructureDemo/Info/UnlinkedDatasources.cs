//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2021 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Collections.Generic;
using System.Text;
using ActiveQueryBuilder.Core;

namespace QueryStructureDemo.Info
{
    public class UnlinkedDatasources
    {
        private static void GetUnlinkedDatsourcesRecursive(DataSource firstDataSource, IList<DataSource> dataSources, IList<Link> links)
        {
            // Remove reached datasource from list of available datasources.
            dataSources.Remove(firstDataSource);

            for (int i = 0; i < links.Count; i++)
            {
                var link = links[i];

                // If left end of the link is connected to firstDataSource,
                // then link.RightDatasource is reachable.
                // If it's still in dataSources list (not yet processed), process it recursivelly.
                if (link.LeftDataSource == firstDataSource && dataSources.IndexOf(link.RightDataSource) != -1)
                {
                    GetUnlinkedDatsourcesRecursive(link.RightDataSource, dataSources, links);
                }
                // If right end of the link is connected to firstDataSource,
                // then link.LeftDatasource is reachable.
                // If it's still in dataSources list (not yet processed), process it recursivelly.
                else if (link.RightDataSource == firstDataSource && dataSources.IndexOf(link.LeftDataSource) != -1)
                {
                    GetUnlinkedDatsourcesRecursive(link.LeftDataSource, dataSources, links);
                }
            }
        }

        public static string GetUnlinkedDataSourcesFromUnionSubQuery(UnionSubQuery unionSubQuery)
        {
            var dataSources = DataSourcesInfo.GetDataSourceList(unionSubQuery);

            // Process trivial cases
            if (dataSources.Count == 0)
                return "There are no datasources in current UnionSubQuery!";

            if (dataSources.Count == 1)
                return "There are only one datasource in current UnionSubQuery!";

            var links = LinksInfo.GetLinkList(unionSubQuery);

            // The first DataSource is the initial point of reachability algorithm
            var firstDataSource = dataSources[0];

            // Remove all linked DataSources from dataSources list
            GetUnlinkedDatsourcesRecursive(firstDataSource, dataSources, links);

            // Now dataSources list contains only DataSources unreachable from the firstDataSource

            if (dataSources.Count == 0)
            {
                return "All DataSources in the query are connected!";
            }
            else
            {
                // Some DataSources are not reachable - show them in a message box

                var sb = new StringBuilder();

                for (var i = 0; i < dataSources.Count; i++)
                {
                    var dataSource = dataSources[i];
                    sb.AppendLine((i + 1) + ": " + dataSource.GetResultSQL());
                }

                return "The following DataSources are not reachable from the first DataSource:\r\n" + sb;
            }
        }
    }
}
