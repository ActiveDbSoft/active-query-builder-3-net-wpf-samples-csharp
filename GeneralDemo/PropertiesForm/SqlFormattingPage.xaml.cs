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
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.WPF;

namespace GeneralDemo.PropertiesForm
{
    public enum SqlBuilderOptionsPages { MainQuery, DerievedQueries, ExpressionSubqueries };
    /// <summary>
    /// Interaction logic for SqlFormattingPage.xaml
    /// </summary>
    public partial class SqlFormattingPage
    {

        private readonly SqlBuilderOptionsPages _page = SqlBuilderOptionsPages.MainQuery;
        private readonly QueryBuilder _queryBuilder;
        private readonly SQLBuilderSelectFormat _format;
        public bool Modified { get; set; }

        public SqlFormattingPage(SqlBuilderOptionsPages page, QueryBuilder queryBuilder)
        {
            Modified = false;
            _page = page;
            _queryBuilder = queryBuilder;
            _format = new SQLBuilderSelectFormat(null);

            if (_page == SqlBuilderOptionsPages.MainQuery)
                _format.Assign(_queryBuilder.SQLFormattingOptions.MainQueryFormat);
            else if (_page == SqlBuilderOptionsPages.DerievedQueries)
                _format.Assign(_queryBuilder.SQLFormattingOptions.DerivedQueryFormat);
            else if (_page == SqlBuilderOptionsPages.ExpressionSubqueries)
                _format.Assign(_queryBuilder.SQLFormattingOptions.ExpressionSubQueryFormat);

            InitializeComponent();

            cbPartsOnNewLines.IsChecked = _format.MainPartsFromNewLine;
            cbNewLineAfterKeywords.IsChecked = _format.NewLineAfterPartKeywords;
            updownGlobalIndent.Value = _format.IndentGlobal;
            updownPartIndent.Value = _format.IndentInPart;

            cbNewLineAfterSelectItem.IsChecked = _format.SelectListFormat.NewLineAfterItem;

            cbNewLineAfterDatasource.IsChecked = _format.FromClauseFormat.NewLineAfterDatasource;
            cbNewLineAfterJoin.IsChecked = _format.FromClauseFormat.NewLineAfterJoin;

            cbNewLineWhereTop.IsChecked = (_format.WhereFormat.NewLineAfter == SQLBuilderConditionFormatNewLine.AllLogical ||
                _format.WhereFormat.NewLineAfter == SQLBuilderConditionFormatNewLine.TopmostOr ||
                _format.WhereFormat.NewLineAfter == SQLBuilderConditionFormatNewLine.TopmostLogical);
            checkNewLineWhereTop_CheckedChanged(null, new EventArgs());
            cbNewLineWhereRest.IsChecked = (_format.WhereFormat.NewLineAfter == SQLBuilderConditionFormatNewLine.AllLogical);
            checkNewLineWhereRest_CheckedChanged(null, new EventArgs());
            updownWhereIndent.Value = _format.WhereFormat.IndentNestedConditions;

            cbNewLineAfterGroupItem.IsChecked = _format.GroupByFormat.NewLineAfterItem;

            cbNewLineHavingTop.IsChecked = (_format.HavingFormat.NewLineAfter == SQLBuilderConditionFormatNewLine.AllLogical ||
                _format.HavingFormat.NewLineAfter == SQLBuilderConditionFormatNewLine.TopmostOr ||
                _format.HavingFormat.NewLineAfter == SQLBuilderConditionFormatNewLine.TopmostLogical);
            checkNewLineHavingTop_CheckedChanged(null, new EventArgs());
            cbNewLineHavingRest.IsChecked = (_format.HavingFormat.NewLineAfter == SQLBuilderConditionFormatNewLine.AllLogical);
            checkNewLineHavingRest_CheckedChanged(null, new EventArgs());
            updownHavingIndent.Value = _format.HavingFormat.IndentNestedConditions;

            updownHavingIndent.ValueChanged += Changed;
            updownHavingIndent.ValueChanged += Changed;
            cbNewLineHavingRest.Checked += checkNewLineHavingRest_CheckedChanged;
            cbNewLineHavingRest.Unchecked += checkNewLineHavingRest_CheckedChanged;
            cbNewLineHavingTop.Checked += checkNewLineHavingTop_CheckedChanged;
            cbNewLineHavingTop.Unchecked += checkNewLineHavingTop_CheckedChanged;
            cbNewLineAfterGroupItem.Checked += Changed;
            cbNewLineAfterGroupItem.Unchecked += Changed;
            updownWhereIndent.ValueChanged += Changed;
            //updownWhereIndent.TextChanged += Changed;
            cbNewLineWhereRest.Checked += checkNewLineWhereRest_CheckedChanged;
            cbNewLineWhereRest.Unchecked += checkNewLineWhereRest_CheckedChanged;
            cbNewLineWhereTop.Checked += checkNewLineWhereTop_CheckedChanged;
            cbNewLineWhereTop.Unchecked += checkNewLineWhereTop_CheckedChanged;
            cbNewLineAfterJoin.Checked += Changed;
            cbNewLineAfterJoin.Unchecked += Changed;
            cbNewLineAfterDatasource.Checked += Changed;
            cbNewLineAfterDatasource.Unchecked += Changed;
            cbNewLineAfterSelectItem.Checked += Changed;
            cbNewLineAfterSelectItem.Unchecked += Changed;
            updownPartIndent.ValueChanged += Changed;
            updownGlobalIndent.ValueChanged += Changed;
            cbNewLineAfterKeywords.Checked += Changed;
            cbNewLineAfterKeywords.Unchecked += Changed;
            cbPartsOnNewLines.Checked += Changed;
            cbPartsOnNewLines.Unloaded += Changed;
        }

        private void checkNewLineWhereTop_CheckedChanged(object sender, EventArgs e)
        {
            cbNewLineWhereRest.IsEnabled = cbNewLineWhereTop.IsChecked.HasValue && cbNewLineWhereTop.IsChecked.Value;

            if (!(cbNewLineWhereTop.IsChecked.HasValue && cbNewLineWhereTop.IsChecked.Value))
            {
                cbNewLineWhereRest.IsChecked = false;
                checkNewLineWhereRest_CheckedChanged(cbNewLineWhereRest, new EventArgs());
            }

            if (sender != null)
            {
                Modified = true;
            }
        }

        private void checkNewLineWhereRest_CheckedChanged(object sender, EventArgs e)
        {
            updownWhereIndent.IsEnabled = cbNewLineWhereRest.IsChecked.HasValue && cbNewLineWhereRest.IsChecked.Value;

            if (sender != null)
            {
                Modified = true;
            }
        }

        private void checkNewLineHavingRest_CheckedChanged(object sender, EventArgs e)
        {
            updownHavingIndent.IsEnabled = cbNewLineHavingRest.IsChecked.HasValue && cbNewLineHavingRest.IsChecked.Value;

            if (sender != null)
            {
                Modified = true;
            }
        }

        private void checkNewLineHavingTop_CheckedChanged(object sender, EventArgs e)
        {
            cbNewLineHavingRest.IsEnabled = cbNewLineHavingTop.IsChecked.HasValue && cbNewLineHavingTop.IsChecked.Value;

            if (!(cbNewLineHavingTop.IsChecked.HasValue && cbNewLineHavingTop.IsChecked.Value))
            {
                cbNewLineHavingRest.IsChecked = false;
                checkNewLineHavingRest_CheckedChanged(cbNewLineHavingRest, new EventArgs());
            }

            if (sender != null)
            {
                Modified = true;
            }
        }

        private void Changed(object sender, EventArgs e)
        {
            Modified = true;
        }

        public void ApplyChanges()
        {
            if (!Modified) return;

            _format.MainPartsFromNewLine = cbPartsOnNewLines.IsChecked.HasValue && cbPartsOnNewLines.IsChecked.Value;
            _format.NewLineAfterPartKeywords = cbNewLineAfterKeywords.IsChecked.HasValue && cbNewLineAfterKeywords.IsChecked.Value;
            _format.IndentInPart = updownPartIndent.Value;
            _format.IndentGlobal = updownGlobalIndent.Value;

            _format.SelectListFormat.NewLineAfterItem = cbNewLineAfterSelectItem.IsChecked.HasValue && cbNewLineAfterSelectItem.IsChecked.Value;

            _format.FromClauseFormat.NewLineAfterDatasource = cbNewLineAfterDatasource.IsChecked.HasValue && cbNewLineAfterDatasource.IsChecked.Value;
            _format.FromClauseFormat.NewLineAfterJoin = cbNewLineAfterJoin.IsChecked.HasValue && cbNewLineAfterJoin.IsChecked.Value;

            if (cbNewLineWhereRest.IsChecked.HasValue && cbNewLineWhereRest.IsChecked.Value)
            {
                _format.WhereFormat.NewLineAfter = SQLBuilderConditionFormatNewLine.AllLogical;
            }
            else if (cbNewLineWhereTop.IsChecked.HasValue && cbNewLineWhereTop.IsChecked.Value)
            {
                _format.WhereFormat.NewLineAfter = SQLBuilderConditionFormatNewLine.TopmostLogical;
            }
            else
            {
                _format.WhereFormat.NewLineAfter = SQLBuilderConditionFormatNewLine.None;
            }

            _format.WhereFormat.IndentNestedConditions = updownWhereIndent.Value;

            _format.GroupByFormat.NewLineAfterItem = cbNewLineAfterGroupItem.IsChecked.HasValue && cbNewLineAfterGroupItem.IsChecked.Value;

            if (cbNewLineHavingRest.IsChecked.HasValue && cbNewLineHavingRest.IsChecked.Value)
            {
                _format.HavingFormat.NewLineAfter = SQLBuilderConditionFormatNewLine.AllLogical;
            }
            else if (cbNewLineHavingTop.IsChecked.HasValue && cbNewLineHavingTop.IsChecked.Value)
            {
                _format.HavingFormat.NewLineAfter = SQLBuilderConditionFormatNewLine.TopmostLogical;
            }
            else
            {
                _format.HavingFormat.NewLineAfter = SQLBuilderConditionFormatNewLine.None;
            }

            _format.HavingFormat.IndentNestedConditions = updownHavingIndent.Value;


            if (_page == SqlBuilderOptionsPages.MainQuery)
            {
                _queryBuilder.SQLFormattingOptions.MainQueryFormat.Assign(_format);
            }
            else if (_page == SqlBuilderOptionsPages.DerievedQueries)
            {
                _queryBuilder.SQLFormattingOptions.DerivedQueryFormat.Assign(_format);
            }
            else if (_page == SqlBuilderOptionsPages.ExpressionSubqueries)
            {
                _queryBuilder.SQLFormattingOptions.ExpressionSubQueryFormat.Assign(_format);
            }
        }		
    }
}
