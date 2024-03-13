//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2024 Active Database Software              //
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
    public  class LinksInfo
    {
        public static List<Link> GetLinkList(UnionSubQuery unionSubQuery)
        {
            var links = new List<Link>();

            unionSubQuery.FromClause.GetLinksRecursive(links);

            return links;
        }

        private static void DumpLinkInfo(StringBuilder stringBuilder, Link link)
        {
            // write full sql fragment of link expression
            stringBuilder.AppendLine(link.LinkExpression.GetSQL(link.SQLContext.SQLGenerationOptionsForServer));

            // write information about left side of link
            stringBuilder.AppendLine("  left datasource: " + link.LeftDataSource.GetResultSQL());

            if (link.LeftType == LinkSideType.Inner)
            {
                stringBuilder.AppendLine("  left type: Inner");
            }
            else
            {
                stringBuilder.AppendLine("  left type: Outer");
            }

            // write information about right side of link
            stringBuilder.AppendLine("  right datasource: " + link.RightDataSource.GetResultSQL());

            if (link.RightType == LinkSideType.Inner)
            {
                stringBuilder.AppendLine("  lerightft type: Inner");
            }
            else
            {
                stringBuilder.AppendLine("  right type: Outer");
            }
        }

        private static void DumpLinksInfo(StringBuilder stringBuilder, IList<Link> links)
        {
            foreach (var link in links)
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.AppendLine();

                DumpLinkInfo(stringBuilder, link);
            }
        }

        public static void DumpLinksInfoFromUnionSubQuery(StringBuilder stringBuilder, UnionSubQuery unionSubQuery)
        {
            DumpLinksInfo(stringBuilder, GetLinkList(unionSubQuery));
        }
    }
}
