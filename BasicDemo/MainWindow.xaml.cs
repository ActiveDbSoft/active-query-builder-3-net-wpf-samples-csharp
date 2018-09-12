//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.WPF;
using BasicDemo.PropertiesForm;
using Microsoft.Win32;
using Helpers = ActiveQueryBuilder.View.Helpers;
using Timer = System.Timers.Timer;
using BuildInfo = ActiveQueryBuilder.Core.BuildInfo;

namespace BasicDemo
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly OpenFileDialog _openFileDialog = new OpenFileDialog();
        private readonly SaveFileDialog _saveFileDialog = new SaveFileDialog();        

        public MainWindow()
        {
            InitializeComponent();
            
            _openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            _saveFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";

			sqlTextEditor1.QueryProvider = queryBuilder;

            // DEMO WARNING
            if (BuildInfo.GetEdition() == BuildInfo.Edition.Trial)
            {
                var grid = new Grid();

                var trialNoticePanel = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Background = Brushes.LightGreen,
                    Padding = new Thickness(5),
                    Margin = new Thickness(0, 0, 0, 2)
                };
                trialNoticePanel.SetValue(Grid.RowProperty, 0);

                var label = new TextBlock
                {
                    Text =
                        @"Generation of random aliases for the query output columns is the limitation of the trial version. The full version is free from this behavior.",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };

                var button = new Button
                {
                    Background = Brushes.Transparent,
                    Padding = new Thickness(0),
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Margin = new Thickness(0, 0, 5, 0),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Content = new Image
                    {
                        Source = Properties.Resources.cancel.GetImageSource(),
                        Stretch = Stretch.None
                    }
                };

                button.Click += delegate { PanelNotifications.Children.Remove(grid); };

                button.SetValue(Grid.RowProperty, 0);

                trialNoticePanel.Child = label;
                grid.Children.Add(trialNoticePanel);
                grid.Children.Add(button);

                PanelNotifications.Children.Add(grid);
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            queryBuilder.SyntaxProvider = new GenericSyntaxProvider();

            var currentLang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            Language = XmlLanguage.GetLanguage(Helpers.Localizer.Languages.Contains(currentLang.ToLower()) ? currentLang : "en");
        }

        private void menuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            QueryBuilder.ShowAboutDialog();
        }

        //// TextBox lost focus by mouse
        //private void SqlTextEditor_OnLostFocus(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        // Update the query builder with manually edited query text:
        //        queryBuilder.SQL = sqlTextEditor1.Text;
        //    }
        //    catch (SQLParsingException ex)
        //    {
        //        // Set caret to error position
        //        sqlTextEditor1.SelectionStart = ex.ErrorPos.pos;
        //    }
        //}

        // TextBox lost focus by keyboard
        private void SqlTextEditor_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            try
            {
                // Update the query builder with manually edited query text:
                queryBuilder.SQL = sqlTextEditor1.Text;
                ShowErrorBanner((FrameworkElement)sender, "");
            }
            catch (SQLParsingException ex)
            {
                // Set caret to error position
                sqlTextEditor1.SelectionStart = ex.ErrorPos.pos;
                // Report error
                ShowErrorBanner((FrameworkElement) sender, ex.Message);
            }
        }

        private void WarnAboutGenericSyntaxProvider()
        {
            if (queryBuilder.SyntaxProvider is GenericSyntaxProvider)
            {
                panel1.Visibility = Visibility.Visible;

                // setup the panel to hide automatically
                var timer = new Timer();
                timer.Elapsed += TimerEvent;
                timer.Interval = 10000;
                timer.Start();
            }
        }

        private void TimerEvent(Object source, EventArgs args)
        {
            Dispatcher.Invoke((Action) delegate
            {
                panel1.Visibility = Visibility.Collapsed;
            });

            ((Timer) source).Stop();
            ((Timer) source).Dispose();
        }

        private void RefreshMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            // Force the query builder to refresh metadata from current connection
            // to refresh metadata, just clear MetadataContainer and reinitialize metadata tree

            if (queryBuilder.MetadataProvider == null || !queryBuilder.MetadataProvider.Connected) return;

            queryBuilder.ClearMetadata();
            queryBuilder.InitializeDatabaseSchemaTree();
        }

        private void ClearMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            // Clear the metadata

            if (MessageBox.Show("Clear Metadata Container?", "Confirmation", MessageBoxButton.YesNo) ==
                MessageBoxResult.Yes)
            {
                queryBuilder.ClearMetadata();
            }
        }

        private void LoadMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            // Load metadata from XML file
            if (_openFileDialog.ShowDialog() != true) return;

            queryBuilder.MetadataLoadingOptions.OfflineMode = true;
            queryBuilder.MetadataContainer.ImportFromXML(_openFileDialog.FileName);
            queryBuilder.InitializeDatabaseSchemaTree();
        }

        private void SaveMetadata_OnClick(object sender, RoutedEventArgs e)
        {
            // Save metadata to XML file
            _saveFileDialog.FileName = "Metadata.xml";

            if (_saveFileDialog.ShowDialog() != true) return;

            queryBuilder.MetadataContainer.LoadAll(true);
            queryBuilder.MetadataContainer.ExportToXML(_saveFileDialog.FileName);
        }

        private void QueryBuilder_OnSQLUpdated(object sender, EventArgs e)
        {
            // Handle the event raised by SQL Builder object that the text of SQL query is changed
            // update the text box
            sqlTextEditor1.Text = queryBuilder.FormattedSQL;
            CheckParameters();
        }

        private void CheckParameters()
        {
            if (Misc.CheckParameters(queryBuilder))
                HideParametersErrorPanel();
            else
            {
                var acceptableFormats =
                    Misc.GetAcceptableParametersFormats(queryBuilder.MetadataProvider, queryBuilder.SyntaxProvider);
                ShowParametersErrorPanel(acceptableFormats);
            }
        }

        private void ShowParametersErrorPanel(List<string> acceptableFormats)
        {
            var formats = acceptableFormats.Select(x =>
            {
                var s = x.Replace("n", "<number>");
                return s.Replace("s", "<name>");
            });

            lbParamsError.Text = "Unsupported parameter notation detected. For this type of connection and database server use " + string.Join(", ", formats);
            pnlParamsError.Visibility = Visibility.Visible;
        }

        private void HideParametersErrorPanel()
        {
            pnlParamsError.Visibility = Visibility.Collapsed;
        }

        public void ShowErrorBanner(FrameworkElement control, string text)
        {
            // Show new banner if text is not empty
            ErrorBox.Message = text;
        }

        public void ResetQueryBuilder()
        {
            queryBuilder.ClearMetadata();
            queryBuilder.MetadataProvider = null;
            queryBuilder.SyntaxProvider = null;
            queryBuilder.MetadataLoadingOptions.OfflineMode = false;
        }

        private void connect_OnClick(object sender, RoutedEventArgs e)
        {
            var form = new ConnectionEditWindow() {Owner = this};
            var result = form.ShowDialog();
            if (result.HasValue && result.Value)
            {
                try
                {
                    queryBuilder.SQLContext.Assign(form.Connection.GetSqlContext());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void FillProgrammatically_OnClick(object sender, RoutedEventArgs e)
        {
            ResetQueryBuilder();

            // Fill the query builder metadata programmatically

            // setup the query builder with metadata and syntax providers
            queryBuilder.SyntaxProvider = new GenericSyntaxProvider();
            queryBuilder.MetadataLoadingOptions.OfflineMode = true; // prevent querying obejects from database

            // create database and schema
            MetadataNamespace database = queryBuilder.MetadataContainer.AddDatabase("MyDB");
            database.Default = true;
            MetadataNamespace schema = database.AddSchema("MySchema");
            schema.Default = true;

            // create table
            MetadataObject tableOrders = schema.AddTable("Orders");
            tableOrders.AddField("OrderID");
            tableOrders.AddField("OrderDate");
            tableOrders.AddField("CustomerID");
            tableOrders.AddField("ResellerID");

            // create another table
            MetadataObject tableCustomers = schema.AddTable("Customers");
            tableCustomers.AddField("CustomerID");
            tableCustomers.AddField("CustomerName");
            tableCustomers.AddField("CustomerAddress");

            // add a relation between these two tables
            MetadataForeignKey relation = tableCustomers.AddForeignKey("FK_CustomerID");
            relation.Fields.Add("CustomerID");
            relation.ReferencedObjectName = tableOrders.GetQualifiedName();
            relation.ReferencedFields.Add("CustomerID");

            //create view
            MetadataObject viewResellers = schema.AddView("Resellers");
            viewResellers.AddField("ResellerID");
            viewResellers.AddField("ResellerName");

            queryBuilder.DatabaseSchemaViewOptions.DefaultExpandLevel = 1;

            // kick the query builder to fill metadata tree
            queryBuilder.InitializeDatabaseSchemaTree();

            WarnAboutGenericSyntaxProvider(); // show warning (just for demonstration purposes)
        }

        private void QueryStatistic_OnClick(object sender, RoutedEventArgs e)
        {
			QueryStatistics queryStatistics = queryBuilder.QueryStatistics;
			StringBuilder builder = new StringBuilder();

			builder.Append("Used Objects (").Append(queryStatistics.UsedDatabaseObjects.Count).AppendLine("):");
			builder.AppendLine();

			for (int i = 0; i < queryStatistics.UsedDatabaseObjects.Count; i++)
				builder.AppendLine(queryStatistics.UsedDatabaseObjects[i].ObjectName.QualifiedName);

			builder.AppendLine().AppendLine();
			builder.Append("Used Columns (").Append(queryStatistics.UsedDatabaseObjectFields.Count).AppendLine("):");
			builder.AppendLine();

			for (int i = 0; i < queryStatistics.UsedDatabaseObjectFields.Count; i++)
				builder.AppendLine(queryStatistics.UsedDatabaseObjectFields[i].FullName.QualifiedName);

			builder.AppendLine().AppendLine();
			builder.Append("Output Expressions (").Append(queryStatistics.OutputColumns.Count).AppendLine("):");
			builder.AppendLine();

			for (int i = 0; i < queryStatistics.OutputColumns.Count; i++)
				builder.AppendLine(queryStatistics.OutputColumns[i].Expression);

			var f = new QueryStatisticsWindow { textBox = { Text = builder.ToString() }, Owner = this};
            f.ShowDialog();
        }

        private void Properties_OnClick(object sender, RoutedEventArgs e)
        {
            // Show Properties form
            var f = new QueryBuilderPropertiesWindow(queryBuilder){Owner = this};

            f.ShowDialog();
            WarnAboutGenericSyntaxProvider(); // show warning (just for demonstration purposes)
        }

        private void ExecuteQuery()
        {
            dataGridView1.ItemsSource = null;

            if (queryBuilder.MetadataProvider != null && queryBuilder.MetadataProvider.Connected)
            {
                try
                {
                    dataGridView1.ItemsSource = Misc.ExecuteSql(queryBuilder.SQL, queryBuilder.SQLQuery).DefaultView;
                    if (Misc.ParamsCache.Count != 0 && dataGridView1.ItemsSource != null)
                        ShowParamsPanel();
                    else
                        HideParamsPanel();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "SQL query error");
                }
            }
        }

        private void ShowParamsPanel()
        {
            var parameters = Misc.ParamsCache.Select(x => string.Format("{0} = {1}", x.Name, x.Value));
            lbQueryParams.Text = "Used parameters: " + string.Join(", ", parameters);
            pnlQueryParams.Visibility = Visibility.Visible;
        }

        private void HideParamsPanel()
        {
            pnlQueryParams.Visibility = Visibility.Collapsed;
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            var tab = e.AddedItems[0] as TabItem;
            if (tab == null) return;

            if (tab.Header.ToString() != "Data") return;

            if (sqlTextEditor1.Text != queryBuilder.FormattedSQL)
                queryBuilder.SQL = sqlTextEditor1.Text;

            ExecuteQuery();
        }

        private void SqlTextEditor1_OnTextChanged(object sender, EventArgs e)
        {
            ErrorBox.Message = string.Empty;
        }

        private void EditParams_Click(object sender, RoutedEventArgs e)
        {
            Misc.ParamsCache.Clear();
            ExecuteQuery();
        }
    }
}
