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
using System.Windows;
using System.Windows.Controls;
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
        private string _lastValidSql;
        private int _errorPosition = -1;

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

            // Load demo metadata from XML file
            Builder.MetadataContainer.LoadingOptions.OfflineMode = true;
            Builder.MetadataContainer.ImportFromXML(@"Northwind.xml");

            Builder.InitializeDatabaseSchemaTree();

            // set example query text
            Builder.SQL = "Select * From Customers";

            
            Builder.ActiveUnionSubQueryChanging += Builder_ActiveUnionSubQueryChanging;
            Builder.ActiveUnionSubQueryChanged += Builder_ActiveUnionSubQueryChanged;
        }

        private void Breadcrumb_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BottomGrid.InvalidateVisual();
        }

        /// <summary>
        /// Validating the query in the text editor.
        /// </summary>
        private SQLParsingException ValidateSql()
        {
            try
            {
                var sql = TextEditor.Text.Trim();

                switch (_mode)
                {
                    case ModeEditor.Entire:
                        if (!string.IsNullOrEmpty(sql))
                            Builder.SQLContext.ParseSelect(sql);
                        break;
                    case ModeEditor.SubQuery:
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

        /// <summary>
        /// Sets the query text depending on the selected sub-query editing mode.
        /// </summary>
        private void ApplyText()
        {
            var sqlFormattingOptions = Builder.SQLFormattingOptions;

            if(TextEditor == null) return;

            switch (_mode)
            {
                case ModeEditor.Entire:
                    _lastValidSql = TextEditor.Text = Builder.FormattedSQL;
                    break;
                case ModeEditor.SubQuery:
                    if(Builder.ActiveUnionSubQuery == null) break;
                    var subQuery = Builder.ActiveUnionSubQuery.ParentSubQuery;
                    _lastValidSql = TextEditor.Text = FormattedSQLBuilder.GetSQL(subQuery, sqlFormattingOptions);
                    break;
                case ModeEditor.Expression:
                    if (Builder.ActiveUnionSubQuery == null) break;
                    var unionSubQuery = Builder.ActiveUnionSubQuery;
                    _lastValidSql = TextEditor.Text = FormattedSQLBuilder.GetSQL(unionSubQuery, sqlFormattingOptions);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Builder_ActiveUnionSubQueryChanging(object sender, ActiveQueryBuilder.View.SubQueryChangingEventArgs e)
        {
        	// Validating the query text before switching to another sub-query.
            var exception = ValidateSql();

            if(exception == null) return;

            e.Abort = true;

            ErrorBox.Show(exception.Message, Builder.SyntaxProvider);
        }

        private void Builder_OnSQLUpdated(object sender, EventArgs e)
        {
            ApplyText();
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            // Selecting the sub-query editing mode.
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

        private void TextEditor_OnPreviewLostKeyboardFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            var text = TextEditor.Text.Trim();


            Builder.BeginUpdate();

            try
            {
                if (!string.IsNullOrEmpty(text))
                    Builder.SQLContext.ParseSelect(text);

                // Updating the needed query part with the manually edited SQL query text.
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
                var exceptionParsing = exception as SQLParsingException;
                if (exceptionParsing != null)
                {
                    ErrorBox.Show(exceptionParsing.Message, Builder.SyntaxProvider);
                    _errorPosition = exceptionParsing.ErrorPos.pos;
                }
            }
            finally
            {
                Builder.EndUpdate();
            }
        }

        private void TextEditor_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorBox.Show(null, Builder.SyntaxProvider);
        }

        private void ErrorBox_OnGoToErrorPosition(object sender, EventArgs e)
        {
            TextEditor.Focus();

            if (_errorPosition == -1) return;

            if (TextEditor.LineCount != 1)
                TextEditor.ScrollToLine(TextEditor.GetLineIndexFromCharacterIndex(_errorPosition));
            TextEditor.CaretIndex = _errorPosition;
        }

        private void ErrorBox_OnRevertValidText(object sender, EventArgs e)
        {
            TextEditor.Text = _lastValidSql;
            TextEditor.Focus();
        }
    }
}
