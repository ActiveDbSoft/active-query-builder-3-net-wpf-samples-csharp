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
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.QueryView;
using Orientation = System.Windows.Controls.Orientation;

namespace CustomExpressionBuilderDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;  
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;

            QBuilder.QueryColumnListOptions.UseCustomExpressionBuilder = AffectedColumns.ConditionColumns | AffectedColumns.ExpressionColumn;

            // set syntax provider
            QBuilder.SyntaxProvider = new MSSQLSyntaxProvider();

            // Fill metadata container from the XML file. (For demonstration purposes.)
            try
            {
                QBuilder.MetadataLoadingOptions.OfflineMode = true;
                QBuilder.MetadataContainer.ImportFromXML("Northwind.xml");
                QBuilder.InitializeDatabaseSchemaTree();

                QBuilder.SQL = @"SELECT Orders.OrderID, Orders.CustomerID, Orders.OrderDate, [Order Details].ProductID,
										[Order Details].UnitPrice, [Order Details].Quantity, [Order Details].Discount
									  FROM Orders INNER JOIN [Order Details] ON Orders.OrderID = [Order Details].OrderID
									  WHERE Orders.OrderID > 0 AND [Order Details].Discount > 0";

                QBuilder.QueryColumnListOptions.UseCustomExpressionBuilder = AffectedColumns.ConditionColumns &
                                                                             AffectedColumns.ExpressionColumn;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void QBuilder_OnSQLUpdated(object sender, EventArgs e)
        {
            // Text of SQL query has been updated by the query builder.
            SqlEditor.Text = QBuilder.FormattedSQL;
        }

        private void SqlEditor_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                // Update the query builder with manually edited query text:
                QBuilder.SQL = SqlEditor.Text;
                ShowErrorBanner((FrameworkElement)sender, "");
            }
            catch (SQLParsingException ex)
            {
                // Set caret to error position
                SqlEditor.SelectionStart = ex.ErrorPos.pos;
                // Report error
                ShowErrorBanner((FrameworkElement)sender, ex.Message);
            }
        }

        public void ShowErrorBanner(FrameworkElement control, string text)
        {
            // Show new banner if text is not empty
            ErrorBox.Message = text;
        }

        private void QBuilder_OnCustomExpressionBuilder(QueryColumnListItem querycolumnlistitem, int conditionIndex, string expression)
        {
            var msg = new MessageContainer(this) { Title = "Edit " + (conditionIndex != -1 ? "condition" : "expression"), TextContent = expression };
            if (msg.ShowDialog() != true) return;

            // Update the criteria list with new expression text.
            if (conditionIndex > -1) // it's one of condition columns
            {
                querycolumnlistitem.ConditionStrings[conditionIndex] = msg.TextContent;
            }
            else // it's the Expression column
            {
                querycolumnlistitem.ExpressionString = msg.TextContent;
            }
        }

        public class MessageContainer: Window
        {
            public string TextContent { get { return _textBox.Text; } set { _textBox.Text = value; } }
            private readonly TextBox _textBox;

            public MessageContainer(Window owner)
            {
                Owner = owner;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                Width = 400;
                Height = 300;
                ShowInTaskbar = false;

                var grid = new Grid{Margin = new Thickness(5)};

                grid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(1, GridUnitType.Star)});
                grid.RowDefinitions.Add(new RowDefinition {Height = GridLength.Auto});

                _textBox = new TextBox
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                    TextWrapping = TextWrapping.Wrap
                };

                _textBox.SetValue(Grid.RowProperty, 0);
                grid.Children.Add(_textBox);

                var stack = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 10, 0, 0)
                };

                stack.SetValue(Grid.RowProperty, 1);
                grid.Children.Add(stack);

                var buttonOk = new Button {Height = 23, Width = 74, Content = "OK"};

                buttonOk.Click += delegate {
                    DialogResult = true;
                };

                var buttonCancel = new Button { Height = 23, Width = 74, Content = "Cancel", Margin = new Thickness(5,0,0,0)};

                buttonCancel.Click += delegate
                {
                    Close();
                };

                stack.Children.Add(buttonOk);
                stack.Children.Add(buttonCancel);
                Content = grid;
            }
        }

        private void SqlEditor_OnTextChanged(object sender, EventArgs e)
        {
            ErrorBox.Message = string.Empty;
        }
    }
}
