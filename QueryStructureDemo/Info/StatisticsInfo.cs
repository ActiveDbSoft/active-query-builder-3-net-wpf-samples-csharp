//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2021 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Text;
using ActiveQueryBuilder.Core;

namespace QueryStructureDemo.Info
{
    public  class StatisticsInfo
    {
        public static void DumpUsedObjectsInfo(StringBuilder stringBuilder, StatisticsDatabaseObjectList usedObjects)
        {
            stringBuilder.AppendLine("Used Objects (" + usedObjects.Count + "):");

            for (var i = 0; i < usedObjects.Count; i++)
            {
                stringBuilder.AppendLine(usedObjects[i].ObjectName.QualifiedName);
            }
        }

        public static void DumpUsedColumnsInfo(StringBuilder stringBuilder, StatisticsFieldList usedColumns)
        {
            stringBuilder.AppendLine("Used Columns (" + usedColumns.Count + "):");

            for (int i = 0; i < usedColumns.Count; i++)
            {
                stringBuilder.AppendLine(usedColumns[i].ObjectName.QualifiedName);
            }
        }

        public static void DumpOutputExpressionsInfo(StringBuilder stringBuilder, StatisticsOutputColumnList outputExpressions)
        {
            stringBuilder.AppendLine("Output Expressions (" + outputExpressions.Count + "):");

            for (int i = 0; i < outputExpressions.Count; i++)
            {
                stringBuilder.AppendLine(outputExpressions[i].Expression);
            }
        }

        public static void DumpQueryStatisticsInfo(StringBuilder stringBuilder, QueryStatistics queryStatistics)
        {
            DumpUsedObjectsInfo(stringBuilder, queryStatistics.UsedDatabaseObjects);

            stringBuilder.AppendLine();
            DumpUsedColumnsInfo(stringBuilder, queryStatistics.UsedDatabaseObjectFields);

            stringBuilder.AppendLine();
            DumpOutputExpressionsInfo(stringBuilder, queryStatistics.OutputColumns);
        }
    }
}
