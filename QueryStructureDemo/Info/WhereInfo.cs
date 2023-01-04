//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2023 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Text;
using ActiveQueryBuilder.Core;

namespace QueryStructureDemo.Info
{
    public class WhereInfo
    {
        private static void DumpExpression(StringBuilder stringBuilder, string indent, SQLExpressionItem expression)
        {
            const string cIndentInc = "    ";

            string newIndent = indent + cIndentInc;

            if (expression == null) // NULL reference protection
            {
                stringBuilder.AppendLine(indent + "--nil--");
            }
            else if (expression is SQLExpressionBrackets)
            {
                // Expression is actually the brackets query structure node.
                // Create the "brackets" tree node and load content of
                // the brackets as children of the node.
                stringBuilder.AppendLine(indent + "()");
                DumpExpression(stringBuilder, newIndent, ((SQLExpressionBrackets)expression).LExpression);
            }
            else if (expression is SQLExpressionOr)
            {
                // Expression is actually the "OR" query structure node.
                // Create the "OR" tree node and load all items of
                // the "OR" collection as children of the tree node.
                stringBuilder.AppendLine(indent + "OR");

                for (var i = 0; i < ((SQLExpressionOr)expression).Count; i++)
                {
                    DumpExpression(stringBuilder, newIndent, ((SQLExpressionOr)expression)[i]);
                }
            }
            else if (expression is SQLExpressionAnd)
            {
                // Expression is actually the "AND" query structure node.
                // Create the "AND" tree node and load all items of
                // the "AND" collection as children of the tree node.
                stringBuilder.AppendLine(indent + "AND");

                for (var i = 0; i < ((SQLExpressionAnd)expression).Count; i++)
                {
                    DumpExpression(stringBuilder, newIndent, ((SQLExpressionAnd)expression)[i]);
                }
            }
            else if (expression is SQLExpressionNot)
            {
                // Expression is actually the "NOT" query structure node.
                // Create the "NOT" tree node and load content of
                // the "NOT" operator as children of the tree node.
                stringBuilder.AppendLine(indent + "NOT");
                DumpExpression(stringBuilder, newIndent, ((SQLExpressionNot)expression).LExpression);
            }
            else if (expression is SQLExpressionOperatorBinary)
            {
                // Expression is actually the "BINARY OPERATOR" query structure node.
                // Create a tree node containing the operator value and
                // two leaf nodes with the operator arguments.
                string s = ((SQLExpressionOperatorBinary)expression).OperatorObj.OperatorName;
                stringBuilder.AppendLine(indent + s);
                // left argument of the binary operator
                DumpExpression(stringBuilder, newIndent, ((SQLExpressionOperatorBinary)expression).LExpression);
                // right argument of the binary operator
                DumpExpression(stringBuilder, newIndent, ((SQLExpressionOperatorBinary)expression).RExpression);
            }
            else
            {
                // other type of AST nodes - out as a text
                string s = expression.GetSQL(expression.SQLContext.SQLGenerationOptionsForServer);
                stringBuilder.AppendLine(indent + s);
            }
        }

        public static void DumpWhereInfo(StringBuilder stringBuilder, SQLExpressionItem where)
        {
            DumpExpression(stringBuilder, "", where);
        }
    }
}
