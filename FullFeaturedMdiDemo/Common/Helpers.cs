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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ActiveQueryBuilder.Core;

namespace FullFeaturedMdiDemo.Common
{
    public static class Helpers
    {
        public static readonly List<Type> ConnectionDescriptorList = new List<Type>
        {
            typeof(MSAccessConnectionDescriptor),
            typeof(ExcelConnectionDescriptor),
            typeof(MSSQLConnectionDescriptor),
            typeof(MSSQLAzureConnectionDescriptor),
            typeof(MySQLConnectionDescriptor),
            typeof(OracleNativeConnectionDescriptor),
            typeof(PostgreSQLConnectionDescriptor),
            typeof(ODBCConnectionDescriptor),
            typeof(OLEDBConnectionDescriptor),
            typeof(SQLiteConnectionDescriptor),
            typeof(FirebirdConnectionDescriptor)
        };

        public static readonly List<string> ConnectionDescriptorNames = new List<string>
        {
            "MS Access",
            "Excel",
            "MS SQL Server",
            "MS SQL Server Azure",
            "MySQL",
            "Oracle Native",
            "PostgreSQL",
            "Generic ODBC Connection",
            "Generic OLEDB Connection",
            "SQLite",
            "Firebird"
        };

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

        public enum SourceType
        {
            File,
            New,
            UserQueries
        }

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

    internal class SaveExistQueryDialog : WindowMessage
    {
        public bool? Result;

        private const int WIDTH_BUTTON = 90;

        public SaveExistQueryDialog()
        {
            Title = "Save dialog";
            Text = "Save changes?";
            ContetnAlignment = HorizontalAlignment.Center;
            Result = null;

            var saveButton = new Button
            {
                Content = "Save",
                Width = WIDTH_BUTTON,
                IsDefault = true
            };
            saveButton.Click += delegate
            {
                Result = true;
                Close();
            };

            var notSaveButton = new Button
            {
                Margin = new Thickness(5, 0, 5, 0),
                Content = "Don't save",
                Width = WIDTH_BUTTON,
                IsCancel = true
            };
            notSaveButton.Click += delegate
            {
                Result = false;
                Close();
            };

            var continueButton = new Button
            {
                Content = "Continue edit",
                Width = WIDTH_BUTTON
            };
            continueButton.Click += delegate { Close(); };

            Buttons.Add(saveButton);
            Buttons.Add(notSaveButton);
            Buttons.Add(continueButton);
        }
    }

    internal class SaveAsWindowDialog : Window
    {
        public enum ActionSave
        {
            UserQuery,
            File,
            NotSave,
            Continue
        }

        public ActionSave Action { set; get; }

        private const int WIDTH_BUTTON = 120;

        public SaveAsWindowDialog(string nameQuery)
        {
            Background = new SolidColorBrush(SystemColors.ControlColor);
            WindowStartupLocation = WindowStartupLocation.Manual;
            SizeToContent = SizeToContent.WidthAndHeight;

            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            Title = "Save Dialog";

            Action = ActionSave.NotSave;

            var border = new Border { BorderThickness = new Thickness(0), BorderBrush = Brushes.Gray };

            var root = new StackPanel { Margin = new Thickness(10) };
            border.Child = root;
            Content = border;

            var messsage = new TextBlock
            {
                Text = "Save changes to the [" + nameQuery + "]?",
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var bottomStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var buttonSaveFile = new Button { Width = WIDTH_BUTTON, Content = "Save to file..." };
            buttonSaveFile.Click += delegate
            {
                Action = ActionSave.File;
                Close();
            };

            var buttonSaveUserQuery = new Button
            {
                Width = WIDTH_BUTTON,
                Content = "Save as User Query",
                Margin = new Thickness(5, 0, 5, 0)
            };
            buttonSaveUserQuery.Click += delegate
            {
                Action = ActionSave.UserQuery;
                Close();
            };

            var buttonNotSave = new Button
            {
                Width = WIDTH_BUTTON,
                Content = "Don't save",
                Margin = new Thickness(0, 0, 5, 0)
            };
            buttonNotSave.Click += delegate
            {
                Action = ActionSave.NotSave;
                Close();
            };


            var buttonCancel = new Button { Width = WIDTH_BUTTON, Content = "Cancel" };
            buttonCancel.Click += delegate
            {
                Action = ActionSave.Continue;
                Close();
            };

            root.Children.Add(messsage);
            root.Children.Add(bottomStack);

            bottomStack.Children.Add(buttonSaveFile);
            bottomStack.Children.Add(buttonSaveUserQuery);
            bottomStack.Children.Add(buttonNotSave);
            bottomStack.Children.Add(buttonCancel);
        }
    }
}
