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
using ActiveQueryBuilder.Core;

namespace FullFeaturedMdiDemo.PropertiesForm.Tabs
{
    /// <summary>
    /// Interaction logic for ExpressionsTab.xaml
    /// </summary>
    public partial class ExpressionsTab
    {
        public SQLBuilderSelectFormat SelectFormat { get; set; }
        public SQLFormattingOptions FormattingOptions { get; set; }

        public ExpressionsTab()
        {
            InitializeComponent();
        }

        public ExpressionsTab(SQLFormattingOptions sqlFormattingOptions, SQLBuilderSelectFormat sqlBuilderSelectFormat)
        {
            InitializeComponent();

            SelectFormat = sqlBuilderSelectFormat;
            FormattingOptions = sqlFormattingOptions;

            LoadOptions();
        }

        public void LoadOptions()
        {
            if (SelectFormat.WhereFormat.NewLineAfter == SQLBuilderConditionFormatNewLine.TopmostLogical &&
                SelectFormat.HavingFormat.NewLineAfter == SQLBuilderConditionFormatNewLine.TopmostLogical)
            {
                CheckBoxStartUpperLevel.IsChecked = true;
                

                CheckBoxStartAllLogicalExprNewLine.IsEnabled = true;
            }
            if (SelectFormat.WhereFormat.NewLineAfter == SQLBuilderConditionFormatNewLine.None &&
                SelectFormat.HavingFormat.NewLineAfter == SQLBuilderConditionFormatNewLine.None)
            {
                CheckBoxStartAllLogicalExprNewLine.IsChecked = false;
                CheckBoxStartAllLogicalExprNewLine.IsEnabled = false;
            }

            if (SelectFormat.WhereFormat.NewLineAfter == SQLBuilderConditionFormatNewLine.AllLogical &&
                SelectFormat.HavingFormat.NewLineAfter == SQLBuilderConditionFormatNewLine.AllLogical)
            {
                CheckBoxStartAllLogicalExprNewLine.IsChecked = true;
                UpDownIndentNested.IsEnabled = true;
            }
            else
            {
                if (SelectFormat.WhereFormat.NewLineAfter == SQLBuilderConditionFormatNewLine.TopmostLogical &&
                    SelectFormat.HavingFormat.NewLineAfter == SQLBuilderConditionFormatNewLine.TopmostLogical)
                {
                    CheckBoxStartUpperLevel.IsChecked = true;
                }
                if (SelectFormat.WhereFormat.NewLineAfter == SQLBuilderConditionFormatNewLine.None &&
                    SelectFormat.HavingFormat.NewLineAfter == SQLBuilderConditionFormatNewLine.None)
                {
                    UpDownIndentNested.IsEnabled = false;
                }
            }

            if (SelectFormat.WhereFormat.NewLineBefore == SQLBuilderConditionFormatNewLine.TopmostLogical &&
                SelectFormat.HavingFormat.NewLineBefore == SQLBuilderConditionFormatNewLine.TopmostLogical)
            {

            }
            if (SelectFormat.WhereFormat.NewLineBefore == SQLBuilderConditionFormatNewLine.AllLogical &&
                SelectFormat.HavingFormat.NewLineBefore == SQLBuilderConditionFormatNewLine.AllLogical)
            {
                RadioButtonStartLines.IsChecked = true;
                CheckBoxStartUpperLevel.IsChecked = true;
                CheckBoxStartAllLogicalExprNewLine.IsChecked = true;
            }

            if (SelectFormat.WhereFormat.NewLineBefore == SQLBuilderConditionFormatNewLine.None &&
                SelectFormat.HavingFormat.NewLineBefore == SQLBuilderConditionFormatNewLine.None)
            {
                RadioButtonEndLines.IsChecked = true;
            }

            UpDownIndentNested.Value = SelectFormat.WhereFormat.IndentNestedConditions;
            UpDownIndentNested.Value = SelectFormat.HavingFormat.IndentNestedConditions;

            UpDownIndentNested.Value = SelectFormat.FromClauseFormat.JoinConditionFormat.IndentNestedConditions;

            CheckBoxBranchConditionKeywordNewLineWhen.IsChecked = SelectFormat.ConditionalOperatorsFormat.NewLineBeforeWhen;

            CheckBoxBranchConditionExpressionNewLine.IsChecked = SelectFormat.ConditionalOperatorsFormat.NewLineAfterWhen;

            CheckBoxBranchResultKeywordNewLineThen.IsChecked = SelectFormat.ConditionalOperatorsFormat.NewLineBeforeThen;

            CheckBoxBranchResultExpressionNewLine.IsChecked = SelectFormat.ConditionalOperatorsFormat.NewLineAfterThen;

            UpDownBranchIndent.Value = SelectFormat.ConditionalOperatorsFormat.IndentBranch;

            UpDownExpressionIndent.Value = SelectFormat.ConditionalOperatorsFormat.IndentExpressions;
        }

        private void CheckBoxStartUpperLevel_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            using (new UpdateRegion(FormattingOptions))
            {
                if (CheckBoxStartUpperLevel.IsChecked == true)
                {
                    SelectFormat.WhereFormat.NewLineAfter = SQLBuilderConditionFormatNewLine.TopmostLogical;
                    SelectFormat.HavingFormat.NewLineAfter = SQLBuilderConditionFormatNewLine.TopmostLogical;

                    CheckBoxStartAllLogicalExprNewLine.IsEnabled = true;
                }
                else
                {
                    SelectFormat.WhereFormat.NewLineAfter = SQLBuilderConditionFormatNewLine.None;
                    SelectFormat.HavingFormat.NewLineAfter = SQLBuilderConditionFormatNewLine.None;

                    CheckBoxStartAllLogicalExprNewLine.IsChecked = false;
                    CheckBoxStartAllLogicalExprNewLine.IsEnabled = false;
                }
            }
        }

        private void CheckBoxStartAllLogicalExprNewLine_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            using (new UpdateRegion(FormattingOptions))
            {
                if (CheckBoxStartAllLogicalExprNewLine.IsChecked == true)
                {
                    SelectFormat.WhereFormat.NewLineAfter = SQLBuilderConditionFormatNewLine.AllLogical;
                    SelectFormat.HavingFormat.NewLineAfter = SQLBuilderConditionFormatNewLine.AllLogical;

                    UpDownIndentNested.IsEnabled = true;
                }
                else
                {
                    if (CheckBoxStartUpperLevel.IsChecked == true)
                    {
                        SelectFormat.WhereFormat.NewLineAfter = SQLBuilderConditionFormatNewLine.TopmostLogical;
                        SelectFormat.HavingFormat.NewLineAfter = SQLBuilderConditionFormatNewLine.TopmostLogical;
                    }
                    else
                    {
                        SelectFormat.WhereFormat.NewLineAfter = SQLBuilderConditionFormatNewLine.None;
                        SelectFormat.HavingFormat.NewLineAfter = SQLBuilderConditionFormatNewLine.None;
                    }

                    UpDownIndentNested.IsEnabled = false;
                }
            }
        }

        private void RadioButtonStartLines_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            using (new UpdateRegion(FormattingOptions))
            {
                if (RadioButtonStartLines.IsChecked == true)
                {
                    if (CheckBoxStartUpperLevel.IsChecked == true && CheckBoxStartAllLogicalExprNewLine.IsChecked != true)
                    {
                        SelectFormat.WhereFormat.NewLineBefore = SQLBuilderConditionFormatNewLine.TopmostLogical;
                        SelectFormat.HavingFormat.NewLineBefore = SQLBuilderConditionFormatNewLine.TopmostLogical;
                    }
                    if (CheckBoxStartUpperLevel.IsChecked == true && CheckBoxStartAllLogicalExprNewLine.IsChecked == true)
                    {
                        SelectFormat.WhereFormat.NewLineBefore = SQLBuilderConditionFormatNewLine.AllLogical;
                        SelectFormat.HavingFormat.NewLineBefore = SQLBuilderConditionFormatNewLine.AllLogical;
                    }
                }
            }
        }

        private void RadioButtonEndLines_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            using (new UpdateRegion(FormattingOptions))
            {
                if (RadioButtonEndLines.IsChecked == true)
                {
                    SelectFormat.WhereFormat.NewLineBefore = SQLBuilderConditionFormatNewLine.None;
                    SelectFormat.HavingFormat.NewLineBefore = SQLBuilderConditionFormatNewLine.None;
                }
            }
        }

        private void UpDownIndentNested_OnValueChanged(object sender, EventArgs e)
        {
            if (!IsInitialized) return;

            using (new UpdateRegion(FormattingOptions))
            {
                SelectFormat.WhereFormat.IndentNestedConditions = UpDownIndentNested.Value;
                SelectFormat.HavingFormat.IndentNestedConditions = UpDownIndentNested.Value;

                SelectFormat.FromClauseFormat.JoinConditionFormat.IndentNestedConditions =
                    UpDownIndentNested.Value;
            }
        }

        private void CheckBoxBranchConditionKeywordNewLineWhen_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            SelectFormat.ConditionalOperatorsFormat.NewLineBeforeWhen = CheckBoxBranchConditionKeywordNewLineWhen.IsChecked == true;
        }

        private void CheckBoxBranchConditionExpressionNewLine_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            SelectFormat.ConditionalOperatorsFormat.NewLineAfterWhen = CheckBoxBranchConditionExpressionNewLine.IsChecked == true;
        }

        private void CheckBoxBranchResultKeywordNewLineThen_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            SelectFormat.ConditionalOperatorsFormat.NewLineBeforeThen = CheckBoxBranchResultKeywordNewLineThen.IsChecked == true;
        }

        private void CheckBoxBranchResultExpressionNewLine_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            SelectFormat.ConditionalOperatorsFormat.NewLineAfterThen = CheckBoxBranchResultExpressionNewLine.IsChecked == true;
        }

        private void UpDownBranchIndent_OnValueChanged(object sender, EventArgs e)
        {
            if (!IsInitialized) return;

            SelectFormat.ConditionalOperatorsFormat.IndentBranch = (int)UpDownBranchIndent.Value;
        }

        private void UpDownExpressionIndent_OnValueChanged(object sender, EventArgs e)
        {
            if (!IsInitialized) return;

            SelectFormat.ConditionalOperatorsFormat.IndentExpressions = (int)UpDownExpressionIndent.Value;
        }
    }
}
