//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2023 Active Database Software              //
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
    public enum SubQueryType { Derived, Cte, Expression }
    /// <summary>
    /// Interaction logic for SubQueryTab.xaml
    /// </summary>
    public partial class SubQueryTab
    {
        public SQLFormattingOptions FormattingOptions { get; set; }
        public SQLBuilderSelectFormat SelectFormat { get; set; }

        public SubQueryTab()
        {
            InitializeComponent();
        }

        public SubQueryTab(SQLFormattingOptions formattingOptions, SubQueryType subQueryType = SubQueryType.Expression)
        {
            FormattingOptions = new SQLFormattingOptions();

            InitializeComponent();

            FormattingOptions.Dispose();

            FormattingOptions = formattingOptions;

            switch (subQueryType)
            {
                case SubQueryType.Expression:
                    SelectFormat = formattingOptions.ExpressionSubQueryFormat;
                    break;
                case SubQueryType.Derived:
                    GroupBoxControl.Header = "Derived tables format options";
                    TextBlockCaptionUpDown.Text = "Derived tables indent:";
                    CheckBoxStartSubQueriesNewLine.Content = "Start derived tables from new lines";
                    TextBlockDescription.Text = "Derived Tables format options\n" +
                                                "determine the layout of sub-queries\n" +
                                                "used as data sources in the query.";

                    SelectFormat = formattingOptions.DerivedQueryFormat;
                    break;
                case SubQueryType.Cte:
                    GroupBoxControl.Header = "Common table expressions format options";
                    TextBlockCaptionUpDown.Text = "CTE indent:";
                    CheckBoxStartSubQueriesNewLine.Content = "Start CTE from new lines";
                    TextBlockDescription.Text = "CTE format options\n" +
                                                "determine the layout of sub-queries\n" +
                                                "used above the main query in the with clause.";

                    SelectFormat = formattingOptions.CTESubQueryFormat;
                    break;
            }

            LoadOptions();
        }

        public void LoadOptions()
        {
            UpDownSubQueryIndent.Value = SelectFormat.IndentInPart;
            CheckBoxStartSubQueriesNewLine.IsChecked = SelectFormat.SubQueryTextFromNewLine;
        }

        private void UpDownSubQueryIndent_OnValueChanged(object sender, EventArgs e)
        {
            SelectFormat.IndentInPart = UpDownSubQueryIndent.Value;
        }

        private void CheckBoxStartSubQueriesNewLine_OnChanged(object sender, RoutedEventArgs e)
        {
            SelectFormat.SubQueryTextFromNewLine = CheckBoxStartSubQueriesNewLine.IsChecked == true;
        }

        private void CheckBoxUseFormattingMainQuery_OnChanged(object sender, RoutedEventArgs e)
        {
            SelectFormat.Assign(FormattingOptions.MainQueryFormat);
        }
    }
}
