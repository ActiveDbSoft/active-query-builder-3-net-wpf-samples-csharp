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
    public  class DataSourcesInfo 
    {
        public static List<DataSource> GetDataSourceList(UnionSubQuery unionSubQuery)
        {
            var list = new List<DataSource>();

            unionSubQuery.FromClause.GetDatasourceByClass(list);

            return list;
        }

        private static void DumpDataSourceInfo(StringBuilder stringBuilder, DataSource dataSource)
        {
            // write full sql fragment
            stringBuilder.AppendLine(dataSource.GetResultSQL());

            // write alias
            stringBuilder.AppendLine("  alias: " + dataSource.Alias);

            // write referenced MetadataObject (if any)
            if (dataSource.MetadataObject != null)
            {
                stringBuilder.AppendLine("  ref: " + dataSource.MetadataObject.Name);
            }

            // write subquery (if datasource is actually a derived table)
            if (dataSource is DataSourceQuery)
            {
                stringBuilder.AppendLine("  subquery sql: " + ((DataSourceQuery)dataSource).GetResultSQL());
            }

            // write fields
            var fields = string.Empty;

            for (var i = 0; i < dataSource.Metadata.Count; i++)
            {
                if (fields.Length > 0)
                {
                    fields += ", ";
                }

                fields += dataSource.Metadata[i].Name;
            }

            stringBuilder.AppendLine("  fields (" + dataSource.Metadata.Count + "): " + fields);
        }

        private static void DumpDataSourcesInfo(StringBuilder stringBuilder, List<DataSource> dataSources)
        {
            for (var i = 0; i < dataSources.Count; i++)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.AppendLine();
                }

                DumpDataSourceInfo(stringBuilder, dataSources[i]);
            }
        }

        public static void DumpDataSourcesInfoFromUnionSubQuery(StringBuilder stringBuilder, UnionSubQuery unionSubQuery)
        {
            DumpDataSourcesInfo(stringBuilder, GetDataSourceList(unionSubQuery));
        }
    }
}
