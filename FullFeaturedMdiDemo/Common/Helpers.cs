//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using ActiveQueryBuilder.Core;

namespace FullFeaturedMdiDemo.Common
{
    public static class Helpers
    {
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

        public static string GetLocalizedText(string text, XmlLanguage lang)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            var properties = typeof(Constants).GetFields();

            var property = properties.FirstOrDefault(prop => prop.Name == text);

            if (property == null) return text;

            var constValue = property.GetValue(null).ToString();

            return ActiveQueryBuilder.Core.Helpers.Localizer.GetString(property.Name,
                ActiveQueryBuilder.View.WPF.Helpers.ConvertLanguageFromNative(lang),
                constValue);
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
