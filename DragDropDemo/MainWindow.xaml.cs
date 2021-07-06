//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2021 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View;

namespace DragDropDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Rect _dragBoxFromMouseDown = Rect.Empty;
        private string _lastValidSql;
        private int _errorPosition = -1;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;

            // Fill query builder with demo data
            QueryBuilder1.SyntaxProvider = new MSSQLSyntaxProvider();
            QueryBuilder1.MetadataLoadingOptions.OfflineMode = true;
            QueryBuilder1.MetadataContainer.ImportFromXML("Northwind.xml");
            QueryBuilder1.InitializeDatabaseSchemaTree(); 
        }

        private void ListBox1_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Prepare drag'n'drop:
            var mousePosition = e.GetPosition((FrameworkElement) sender);

            if (IsContainsItemAtPoint(mousePosition))
            {

                var dragSize = new Size(SystemParameters.MinimumHorizontalDragDistance,
                    SystemParameters.MinimumVerticalDragDistance);
                _dragBoxFromMouseDown =
                    new Rect(new Point(mousePosition.X - (dragSize.Width/2), mousePosition.Y - (dragSize.Height/2)),
                        dragSize);
            }
            else
            {
                _dragBoxFromMouseDown = Rect.Empty;
            }
        }

        private bool IsContainsItemAtPoint(Point target)
        {
            var answer = false;
            
            for (var i = 0; i < ListBox1.Items.Count; i++)
            {
                var item = (ListBoxItem)ListBox1.ItemContainerGenerator.ContainerFromIndex(i);
                if(item == null) continue;

                var rect = new Rect(item.TranslatePoint(new Point(0,0), ListBox1 ), new Size(item.ActualWidth, item.ActualHeight));

                if (!rect.Contains(target)) continue;

                answer = true;
                break;
            }

            return answer;
        }

        private void ListBox1_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _dragBoxFromMouseDown = Rect.Empty;
        }

        private void ListBox1_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Double click will add the object in automatic position:
            if (ListBox1.SelectedIndex == -1) return;
            var objectName = ((ListBoxItem) ListBox1.SelectedItem).Content.ToString();
            QueryBuilder1.AddObjectToActiveUnionSubQuery(objectName);
        }

        private void ListBox1_OnMouseMove(object sender, MouseEventArgs e)
        {
            // Do drag:
            var mousePosition = e.GetPosition((FrameworkElement) sender);
            if (ListBox1.SelectedIndex == -1) return;

            if (e.LeftButton != MouseButtonState.Pressed) return;

            if (_dragBoxFromMouseDown == Rect.Empty || _dragBoxFromMouseDown.Contains(mousePosition.X, mousePosition.Y))
                return;

            var objectName = ((ListBoxItem)ListBox1.SelectedItem).Content.ToString();
            var metadataObject = QueryBuilder1.MetadataContainer.FindItem<MetadataObject>(objectName);

            if (metadataObject == null) return;

            var dragObject = new MetadataDragObject();
            dragObject.MetadataDragged.Add(metadataObject);

            DragDrop.DoDragDrop(ListBox1, dragObject, DragDropEffects.Copy);
        }

        private void QueryBuilder1_OnSQLUpdated(object sender, EventArgs e)
        {
            if(!IsLoaded) return;
            // Handle the event raised by SQL Builder object that the text of SQL query is changed

            // Hide error banner if any
            ErrorBox.Show(null, QueryBuilder1.SyntaxProvider);

            // update the text box
            TextBox1.Text = QueryBuilder1.FormattedSQL;
        }

        private void TextBox_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {

            try
            {
                // Update the query builder with manually edited query text:
                QueryBuilder1.SQL = TextBox1.Text;

                // Hide error banner if any
                ErrorBox.Show(null, QueryBuilder1.SyntaxProvider);
                _lastValidSql = QueryBuilder1.FormattedSQL;
            }
            catch (SQLParsingException ex)
            {
                // Set caret to error position
                TextBox1.SelectionStart = ex.ErrorPos.pos;

                // Show banner with error text
                ErrorBox.Show(ex.Message, QueryBuilder1.SyntaxProvider);

                _errorPosition = ex.ErrorPos.pos;
            }
        }

        private void TextBox1_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorBox.Show(null, QueryBuilder1.SyntaxProvider);
        }

        private void ErrorBox_OnGoToErrorPosition(object sender, EventArgs e)
        {
            TextBox1.Focus();

            if (_errorPosition == -1) return;

            if (TextBox1.LineCount != 1)
                TextBox1.ScrollToLine(TextBox1.GetLineIndexFromCharacterIndex(_errorPosition));
            TextBox1.CaretIndex = _errorPosition;
        }

        private void ErrorBox_OnRevertValidText(object sender, EventArgs e)
        {
            TextBox1.Text = _lastValidSql;
            TextBox1.Focus();
        }
    }
}
