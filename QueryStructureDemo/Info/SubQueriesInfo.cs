//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2022 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Text;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.WPF;

namespace QueryStructureDemo.Info
{
    public  class SubQueriesInfo 
    {
        private static void DumpSubQueryInfo(StringBuilder stringBuilder, int index, SubQuery subQuery)
        {
            var sql = subQuery.GetResultSQL();

            stringBuilder.AppendLine(index + ": " + sql);
        }

        public static void DumpSubQueriesInfo(StringBuilder stringBuilder, QueryBuilder queryBuilder)
        {
            for (var i = 0; i < queryBuilder.ActiveUnionSubQuery.QueryRoot.SubQueryCount; i++)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.AppendLine();
                }

                DumpSubQueryInfo(stringBuilder, i, queryBuilder.ActiveUnionSubQuery.QueryRoot.SubQueries[i]);
                DumpSubQueryStatistics(stringBuilder, queryBuilder.ActiveUnionSubQuery.QueryRoot.SubQueries[i]);
            }
        }

        private static void DumpSubQueryStatistics(StringBuilder stringBuilder, SubQuery subQuery)
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Subquery statistic:");
            StatisticsInfo.DumpQueryStatisticsInfo(stringBuilder, subQuery.QueryStatistics);
        }
    }
}
