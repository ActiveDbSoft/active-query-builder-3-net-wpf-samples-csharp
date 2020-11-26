//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2019 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using ActiveQueryBuilder.Core;
using GeneralAssembly.Windows.QueryInformationWindows;

namespace GeneralAssembly
{
    public class SqlHelpers
    {
        private const string AtNameParamFormat = "@s";
        private const string ColonNameParamFormat = ":s";
        private const string QuestionParamFormat = "?";
        private const string QuestionNumberParamFormat = "?n";
        private const string QuestionNameParamFormat = "?s";

        public static List<string> GetAcceptableParametersFormats(BaseMetadataProvider metadataProvider,
            BaseSyntaxProvider syntaxProvider)
        {
            if (metadataProvider is MSSQLMetadataProvider)
            {
                return new List<string> { AtNameParamFormat };
            }

            if (metadataProvider is OracleNativeMetadataProvider)
            {
                return new List<string> { ColonNameParamFormat };
            }

            if (metadataProvider is PostgreSQLMetadataProvider)
            {
                return new List<string> { ColonNameParamFormat };
            }

            if (metadataProvider is MySQLMetadataProvider)
            {
                return new List<string> { AtNameParamFormat, QuestionParamFormat, QuestionNumberParamFormat, QuestionNameParamFormat };
            }

            if (metadataProvider is OLEDBMetadataProvider)
            {
                if (syntaxProvider is MSAccessSyntaxProvider)
                {
                    return new List<string> { AtNameParamFormat, ColonNameParamFormat, QuestionParamFormat };
                }

                if (syntaxProvider is MSSQLSyntaxProvider)
                {
                    return new List<string> { QuestionParamFormat };
                }

                if (syntaxProvider is OracleSyntaxProvider)
                {
                    return new List<string> { ColonNameParamFormat, QuestionParamFormat, QuestionNumberParamFormat };
                }

                if (syntaxProvider is DB2SyntaxProvider)
                {
                    return new List<string> { QuestionParamFormat };
                }
            }

            if (metadataProvider is ODBCMetadataProvider)
            {
                if (syntaxProvider is MSAccessSyntaxProvider)
                {
                    return new List<string> { QuestionParamFormat };
                }

                if (syntaxProvider is MSSQLSyntaxProvider)
                {
                    return new List<string> { QuestionParamFormat };
                }

                if (syntaxProvider is MySQLSyntaxProvider)
                {
                    return new List<string> { QuestionParamFormat };
                }

                if (syntaxProvider is PostgreSQLSyntaxProvider)
                {
                    return new List<string> { QuestionParamFormat };
                }

                if (syntaxProvider is OracleSyntaxProvider)
                {
                    return new List<string> { ColonNameParamFormat, QuestionParamFormat, QuestionNumberParamFormat };
                }

                if (syntaxProvider is DB2SyntaxProvider)
                {
                    return new List<string> { QuestionParamFormat };
                }
            }

            return new List<string>();
        }

        public static bool CheckParameters(BaseMetadataProvider metadataProvider, BaseSyntaxProvider syntaxProvider, ParameterList parameters)
        {
            var acceptableFormats =
                GetAcceptableParametersFormats(metadataProvider, syntaxProvider);

            if (acceptableFormats.Count == 0)
                return true;

            foreach (var parameter in parameters)
            {
                if (!acceptableFormats.Any(x => IsSatisfiesFormat(parameter.FullName, x)))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsSatisfiesFormat(string name, string format)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(format))
                return false;

            if (format[0] != name[0])
                return false;

            var lastChar = format.Last();
            if (lastChar == '?')
                return name == format;

            if (lastChar == 's')
                return name.Length > 1 && Char.IsLetter(name[1]);

            if (lastChar == 'n')
            {
                if (name.Length == 1)
                    return false;

                foreach (var c in name)
                {
                    if (!Char.IsDigit(c))
                        return false;
                }

                return true;
            }

            return false;
        }

        private static DbCommand CreateSqlCommand(string sqlCommand, SQLQuery sqlQuery)
        {
            DbCommand command = (DbCommand)sqlQuery.SQLContext.MetadataProvider.Connection.CreateCommand();
            command.CommandText = sqlCommand;

            // handle the query parameters
            if (sqlQuery.QueryParameters.Count <= 0) return command;

            foreach (Parameter p in sqlQuery.QueryParameters.Where(item => !command.Parameters.Contains(item.FullName)))
            {
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = p.FullName,
                    DbType = p.DataType
                };
                command.Parameters.Add(parameter);
            }
            var qpf = new QueryParametersWindow(command);
            qpf.ShowDialog();
            return command;
        }

        public static DataTable GetDataTable(string sqlCommand, SQLQuery sqlQuery)
        {
            if (string.IsNullOrEmpty(sqlCommand)) return null;

            if (sqlQuery.SQLContext.MetadataProvider == null)
            {
                return null;
            }

            if (!sqlQuery.SQLContext.MetadataProvider.Connected)
            {
                sqlQuery.SQLContext.MetadataProvider.Connect();
            }

            if (string.IsNullOrEmpty(sqlCommand)) return null;

            if (!sqlQuery.SQLContext.MetadataProvider.Connected)
                sqlQuery.SQLContext.MetadataProvider.Connect();

            var command = CreateSqlCommand(sqlCommand, sqlQuery);

            DataTable table = new DataTable("result");

            using (var dbReader = command.ExecuteReader())
            {

                for (int i = 0; i < dbReader.FieldCount; i++)
                    table.Columns.Add(dbReader.GetName(i), dbReader.GetFieldType(i) ?? typeof(string));

                while (dbReader.Read())
                {
                    var values = new object[dbReader.FieldCount];
                    dbReader.GetValues(values);
                    table.Rows.Add(values);
                }
            }
            return table;
        }

        public static DataView GetDataView(string sqlCommand, SQLQuery sqlQuery)
        {
            var table = GetDataTable(sqlCommand, sqlQuery);
            return table?.DefaultView;
        }
    }
}
