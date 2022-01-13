//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2022 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Data;
using System.Text;
using ActiveQueryBuilder.Core;

namespace QueryStructureDemo.Info
{
    public  class SelectedExpressionsInfo 
    {
        private static void DumpSelectedExpressionInfo(StringBuilder stringBuilder, QueryColumnListItem selectedExpression)
        {
            // write full sql fragment of selected expression
            stringBuilder.AppendLine(selectedExpression.ExpressionString);

            // write alias
            if (!string.IsNullOrEmpty(selectedExpression.AliasString))
            {
                stringBuilder.AppendLine("  alias: " + selectedExpression.AliasString);
            }

            // write datasource reference (if any)
            if (selectedExpression.ExpressionDatasource != null)
            {
                stringBuilder.AppendLine("  datasource: " + selectedExpression.ExpressionDatasource.GetResultSQL());
            }

            // write metadata information (if any)
            if (selectedExpression.ExpressionField == null) return;

            var field = selectedExpression.ExpressionField;
            stringBuilder.AppendLine("  field name: " + field.Name);

            var s = Enum.GetName(typeof(DbType), field.FieldType);
            stringBuilder.AppendLine("  field type: " + s);
        }

        public static void DumpSelectedExpressionsInfoFromUnionSubQuery(StringBuilder stringBuilder, UnionSubQuery unionSubQuery)
        {
            // get list of CriteriaItems
            QueryColumnList criteriaList = unionSubQuery.QueryColumnList;

            // dump all items
            for (int i = 0; i < criteriaList.Count; i++)
            {
                QueryColumnListItem criteriaItem = criteriaList[i];

                // only items have .Select property set to True goes to SELECT list
                if (!criteriaItem.Selected)
                {
                    continue;
                }

                // separator
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.AppendLine();
                }

                DumpSelectedExpressionInfo(stringBuilder, criteriaItem);
                DumpSelectedExpressionsStatistics(stringBuilder, criteriaItem);
            }
        }

        private static void DumpSelectedExpressionsStatistics(StringBuilder stringBuilder, QueryColumnListItem criteriaItem)
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Statistic:");
            StatisticsInfo.DumpQueryStatisticsInfo(stringBuilder, criteriaItem.QueryStatistics);
        }
    }
}
