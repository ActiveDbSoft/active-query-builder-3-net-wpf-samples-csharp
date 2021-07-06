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
using System.ComponentModel;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.WPF;

namespace GeneralAssembly.QueryBuilderProperties
{
    /// <summary>
    /// Interaction logic for GeneralPage.xaml
    /// </summary>
    [ToolboxItem(false)]
    public partial class GeneralPage
    {
        private readonly QueryBuilder _queryBuilder;


        public bool Modified { get; set; }

        public GeneralPage()
        {
            Modified = false;
            InitializeComponent();
        }

        public GeneralPage(QueryBuilder queryBuilder)
        {
            Modified = false;
            _queryBuilder = queryBuilder;

            InitializeComponent();

            cbWordWrap.IsChecked = (_queryBuilder.SQLFormattingOptions.RightMargin != 0);
            updownRightMargin.IsEnabled = cbWordWrap.IsChecked.Value;

            updownRightMargin.Value = _queryBuilder.SQLFormattingOptions.RightMargin == 0 ?
                80 : _queryBuilder.SQLFormattingOptions.RightMargin;

            comboKeywordsCasing.Items.Add("Capitalized");
            comboKeywordsCasing.Items.Add("Uppercase");
            comboKeywordsCasing.Items.Add("Lowercase");

            comboKeywordsCasing.SelectedIndex = (int)queryBuilder.SQLFormattingOptions.KeywordFormat;

            cbWordWrap.Checked += checkWordWrap_CheckedChanged;
            cbWordWrap.Unchecked += checkWordWrap_CheckedChanged;
            updownRightMargin.ValueChanged += updownRightMargin_ValueChanged;
            comboKeywordsCasing.SelectionChanged += comboKeywordsCasing_SelectedIndexChanged;
        }

        void comboKeywordsCasing_SelectedIndexChanged(object sender, EventArgs e)
        {
            Modified = true;
        }

        private void checkWordWrap_CheckedChanged(object sender, EventArgs e)
        {
            updownRightMargin.IsEnabled = cbWordWrap.IsChecked.HasValue && cbWordWrap.IsChecked.Value;
            Modified = true;
        }

        private void updownRightMargin_ValueChanged(object sender, EventArgs e)
        {
            Modified = true;
        }

        public void ApplyChanges()
        {
            if (!Modified) return;

            if (cbWordWrap.IsChecked.HasValue && cbWordWrap.IsChecked.Value)
            {
                _queryBuilder.SQLFormattingOptions.RightMargin = updownRightMargin.Value;
            }
            else
            {
                _queryBuilder.SQLFormattingOptions.RightMargin = 0;
            }

            _queryBuilder.SQLFormattingOptions.KeywordFormat = (KeywordFormat)comboKeywordsCasing.SelectedIndex;
        }
    }
}
