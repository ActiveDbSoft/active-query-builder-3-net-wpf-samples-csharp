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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ActiveQueryBuilder.Core;

namespace SubQueryTextEditingDemo
{
    internal enum ModeEditor
    {
        Entire, SubQuery, Expression
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private ModeEditor _mode;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            _mode = ModeEditor.Entire;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;

            Builder.SyntaxProvider = new MSSQLSyntaxProvider();
            Builder.MetadataProvider = new MSSQLMetadataProvider();

            Builder.MetadataContainer.LoadingOptions.OfflineMode = true;
            Builder.MetadataContainer.ImportFromXML(@"Northwind.xml");

            Builder.DatabaseSchemaViewOptions.DefaultExpandLevel = 2;
            Builder.InitializeDatabaseSchemaTree();

            TextEditor.QueryProvider = Builder;

            Builder.SQL = "Select * From Customers";

            Breadcrumb.QueryView = Builder.QueryView;
            
            Builder.ActiveUnionSubQueryChanging += Builder_ActiveUnionSubQueryChanging;
            Builder.ActiveUnionSubQueryChanged += Builder_ActiveUnionSubQueryChanged;

            Breadcrumb.SizeChanged += Breadcrumb_SizeChanged;
        }

        private void Breadcrumb_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BottomGrid.InvalidateVisual();
        }

        private SQLParsingException CheckSql()
        {
            try
            {
                var sql =TextEditor.Text.Trim();

                switch (_mode)
                {
                    case ModeEditor.Entire:
                        if (!string.IsNullOrEmpty(sql))
                            Builder.SQLContext.ParseSelect(sql);
                        break;
                    case ModeEditor.SubQuery:
                        Builder.SQLContext.ParseSelect(sql);
                        break;
                    case ModeEditor.Expression:
                        Builder.SQLContext.ParseSelect(sql);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return null;
            }
            catch (SQLParsingException ex)
            {
                return ex;
            }
        }

        private void Builder_ActiveUnionSubQueryChanged(object sender, EventArgs e)
        {
            ApplyText();
        }

        private void ApplyText()
        {
            var sqlFormattingOptions = Builder.SQLFormattingOptions;

            if(TextEditor == null) return;

            switch (_mode)
            {
                case ModeEditor.Entire:
                    TextEditor.Text = Builder.FormattedSQL;
                    break;
                case ModeEditor.SubQuery:
                    if(Builder.ActiveUnionSubQuery == null) break;
                    var subQuery = Builder.ActiveUnionSubQuery.ParentSubQuery;
                    TextEditor.Text = FormattedSQLBuilder.GetSQL(subQuery, sqlFormattingOptions);
                    break;
                case ModeEditor.Expression:
                    if (Builder.ActiveUnionSubQuery == null) break;
                    var unionSubQuery = Builder.ActiveUnionSubQuery;
                    TextEditor.Text = FormattedSQLBuilder.GetSQL(unionSubQuery, sqlFormattingOptions);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Builder_ActiveUnionSubQueryChanging(object sender, ActiveQueryBuilder.View.SubQueryChangingEventArgs e)
        {
            var exception = CheckSql();

            if(exception == null) return;

            e.Abort = true;

            ShowErrorBanner(TextEditor, exception);
        }

        private void Builder_OnSQLUpdated(object sender, EventArgs e)
        {
            ApplyText();
        }

        private static void SetText(RichTextBox box, string value)
        {
            box.Document.Blocks.Clear();
            box.Document.Blocks.Add(new Paragraph(new Run(value)));
        }

        private static string GetText(RichTextBox box)
        {
            return new TextRange(box.Document.ContentStart, box.Document.ContentEnd).Text;
        }

        private void ShowErrorBanner(UIElement element, Exception exception)
        {
            if (!(exception is SQLParsingException)) return;

            ErrorBox.Message = exception.Message;
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                PopupSwitch.IsOpen = false;

                if (Equals(sender, RadioButtonEntire))
                {
                    _mode = ModeEditor.Entire;
                    return;
                }

                if (Equals(sender, RadioButtonSubQuery))
                {
                    _mode = ModeEditor.SubQuery;
                    return;
                }

                _mode = ModeEditor.Expression;
            }
            finally
            {
                ApplyText();
            }
        }

        private void ButtonSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            PopupSwitch.IsOpen = true;
        }

        private void TextEditor_OnPreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var text = TextEditor.Text.Trim();

            var isSuccess = true;

            Builder.BeginUpdate();

            try
            {
                if (!string.IsNullOrEmpty(text))
                    Builder.SQLContext.ParseSelect(text);

                switch (_mode)
                {
                    case ModeEditor.Entire:
                        Builder.SQL = text;
                        break;
                    case ModeEditor.SubQuery:
                        var subQuery = Builder.ActiveUnionSubQuery.ParentSubQuery;
                        subQuery.SQL = text;
                        break;
                    case ModeEditor.Expression:
                        var unionSubQuery = Builder.ActiveUnionSubQuery;
                        unionSubQuery.SQL = text;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception exception)
            {
                isSuccess = false;
                ShowErrorBanner(TextEditor, exception);
            }
            finally
            {
                Builder.EndUpdate();
            }

            e.Handled = !isSuccess;
        }

        private void TextEditor_OnTextChanged(object sender, EventArgs e)
        {
            ErrorBox.Message = string.Empty;
        }
    }
}
