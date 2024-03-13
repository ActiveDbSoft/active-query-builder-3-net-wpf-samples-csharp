//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2024 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ActiveQueryBuilder.Core;

namespace QueryStructureDemo.Info
{
    public class SubQueryStructureInfo
    {
        private static void DumpUnionGroupInfo(StringBuilder stringBuilder, string indent, UnionGroup unionGroup)
        {
            var children = GetUnionChildren(unionGroup);

            foreach (var child in children)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.AppendLine();
                }

                if (child is UnionSubQuery)
                {
                    // UnionSubQuery is a leaf node of query structure.
                    // It represent a single SELECT statement in the tree of unions
                    DumpUnionSubQueryInfo(stringBuilder, indent, (UnionSubQuery)child);
                }
                else
                {
                    // UnionGroup is a tree node.
                    // It contains one or more leafs of other tree nodes.
                    // It represent a root of the subquery of the union tree or a
                    // parentheses in the union tree.
                    Debug.Assert(child is UnionGroup);

                    unionGroup = (UnionGroup)child;

                    stringBuilder.AppendLine(indent + unionGroup.UnionOperatorFull + "group: [");
                    DumpUnionGroupInfo(stringBuilder, indent + "    ", unionGroup);
                    stringBuilder.AppendLine(indent + "]");
                }
            }
        }

        private static void DumpUnionSubQueryInfo(StringBuilder stringBuilder, string indent, UnionSubQuery unionSubQuery)
        {
            string sql = unionSubQuery.GetResultSQL();

            stringBuilder.AppendLine(indent + unionSubQuery.UnionOperatorFull + ": " + sql);
        }

        private static IEnumerable<QueryBase> GetUnionChildren(UnionGroup unionGroup)
        {
            var result = new ArrayList();

            for (var i = 0; i < unionGroup.Count; i++)
            {
                result.Add(unionGroup[i]);
            }

            return (QueryBase[])result.ToArray(typeof(QueryBase));
        }

        public static void DumpQueryStructureInfo(StringBuilder stringBuilder, SubQuery subQuery)
        {
            DumpUnionGroupInfo(stringBuilder, "", subQuery);
        }
    }
}
