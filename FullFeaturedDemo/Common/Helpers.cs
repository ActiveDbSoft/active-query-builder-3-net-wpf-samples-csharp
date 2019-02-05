//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2019 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using ActiveQueryBuilder.Core;

namespace FullFeaturedDemo.Common
{
    public static class Helpers
    {
        public static DbCommand CreateSqlCommand(string sqlCommand, SQLQuery sqlQuery)
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

        public static DataView ExecuteSql(string sqlCommand, SQLQuery sqlQuery)
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
                {
                    table.Columns.Add(dbReader.GetName(i));
                }

                while (dbReader.Read())
                {
                    object[] values = new object[dbReader.FieldCount];
                    dbReader.GetValues(values);
                    table.Rows.Add(values);
                }

                return table.DefaultView;
            }
        }
    }
}
