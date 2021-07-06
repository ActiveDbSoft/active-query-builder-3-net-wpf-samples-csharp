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
    /// Interaction logic for CommonTab.xaml
    /// </summary>
    public partial class CommonTab
    {
        public SQLBuilderSelectFormat SelectFormat { get; set; }
        public SQLFormattingOptions FormattingOptions { get; set; }

        public CommonTab()
        {
            InitializeComponent();
        }

        public CommonTab(SQLFormattingOptions sqlFormattingOptions, SQLBuilderSelectFormat sqlBuilderSelectFormat)
        {
            InitializeComponent();

            SelectFormat = sqlBuilderSelectFormat;
            FormattingOptions = sqlFormattingOptions;

            LoadOptions();
        }

        public void LoadOptions()
        {
            CheckBoxStartPartsNewLine.IsChecked = SelectFormat.MainPartsFromNewLine;
            CheckBoxInsertNewLineAfterPart.IsChecked = SelectFormat.NewLineAfterPartKeywords;
            UpDownIndent.Value = SelectFormat.IndentInPart;
            CheckBoxStartSelectListNewLine.IsChecked = SelectFormat.SelectListFormat.NewLineAfterItem;

            RadioButtonBeforeComma.IsChecked = SelectFormat.SelectListFormat.NewLineBeforeComma;
            RadioButtonAfterComma.IsChecked = SelectFormat.SelectListFormat.NewLineAfterItem;

            RadioButtonStartDataSource.IsChecked = SelectFormat.FromClauseFormat.NewLineAfterDatasource;
            RadioButtonStartJoinKeywords.IsChecked = SelectFormat.FromClauseFormat.NewLineAfterJoin;
            CheckBoxFromClauseNewLine.IsChecked = SelectFormat.FromClauseFormat.NewLineBeforeJoinExpression;
        }

        private void CheckBoxStartPartsNewLine_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            using (new UpdateRegion(SelectFormat))
                SelectFormat.MainPartsFromNewLine = CheckBoxStartPartsNewLine.IsChecked == true;
        }

        private void CheckBoxInsertNewLineAfterPart_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            using (new UpdateRegion(SelectFormat))
                SelectFormat.NewLineAfterPartKeywords = CheckBoxInsertNewLineAfterPart.IsChecked == true;
        }

        private void CustomUpDown_OnValueChanged(object sender, EventArgs e)
        {
            if (!IsInitialized) return;

            using (new UpdateRegion(SelectFormat))
                SelectFormat.IndentInPart = UpDownIndent.Value;
        }

        private void CheckBoxStartSelectListNewLine_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            using (new UpdateRegion(SelectFormat))
                SelectFormat.SelectListFormat.NewLineAfterItem = CheckBoxStartSelectListNewLine.IsChecked == true;
        }

        private void RadioButtonBeforeComma_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            using (new UpdateRegion(FormattingOptions))
            {
                SelectFormat.SelectListFormat.NewLineBeforeComma = RadioButtonBeforeComma.IsChecked == true;
                SelectFormat.OrderByFormat.NewLineBeforeComma = RadioButtonBeforeComma.IsChecked == true;
                SelectFormat.GroupByFormat.NewLineBeforeComma = RadioButtonBeforeComma.IsChecked == true;
            }
        }

        private void RadioButtonAfterComma_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            using (new UpdateRegion(FormattingOptions))
            {
                SelectFormat.SelectListFormat.NewLineAfterItem = RadioButtonAfterComma.IsChecked == true;
                SelectFormat.OrderByFormat.NewLineAfterItem = RadioButtonAfterComma.IsChecked == true;
                SelectFormat.GroupByFormat.NewLineAfterItem = RadioButtonAfterComma.IsChecked == true;
            }
        }

        private void RadioButtonStartJoinKeywords_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            using (new UpdateRegion(SelectFormat))
                SelectFormat.FromClauseFormat.NewLineAfterJoin = RadioButtonStartJoinKeywords.IsChecked == true;
        }

        private void RadioButtonStartDataSource_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            using (new UpdateRegion(SelectFormat))
                SelectFormat.FromClauseFormat.NewLineAfterDatasource = RadioButtonStartDataSource.IsChecked == true;
        }

        private void CheckBoxFromClauseNewLine_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            using (new UpdateRegion(SelectFormat))
                SelectFormat.FromClauseFormat.NewLineBeforeJoinExpression = CheckBoxFromClauseNewLine.IsChecked == true;
        }
    }
}
