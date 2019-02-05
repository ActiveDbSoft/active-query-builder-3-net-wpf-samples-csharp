//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2019 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.ComponentModel;
using ActiveQueryBuilder.Core;

namespace FullFeaturedDemo.PropertiesForm
{
    /// <summary>
    /// Interaction logic for GeneralPage.xaml
    /// </summary>
    [ToolboxItem(false)]
    public partial class GeneralPage
    {
        private readonly SQLFormattingOptions _sqlFormattingOptions;

        public bool Modified { get; set; }

        public GeneralPage()
        {
            Modified = false;
            InitializeComponent();
        }

        public GeneralPage(SQLFormattingOptions sqlFormattingOptions)
        {
            Modified = false;
            _sqlFormattingOptions = sqlFormattingOptions;

            InitializeComponent();

            cbWordWrap.IsChecked = (_sqlFormattingOptions.RightMargin != 0);
            updownRightMargin.IsEnabled = cbWordWrap.IsChecked.Value;

            updownRightMargin.Value = _sqlFormattingOptions.RightMargin == 0 ?
                80 : _sqlFormattingOptions.RightMargin;

            comboKeywordsCasing.Items.Add("Capitalized");
            comboKeywordsCasing.Items.Add("Uppercase");
            comboKeywordsCasing.Items.Add("Lowercase");

            comboKeywordsCasing.SelectedIndex = (int)_sqlFormattingOptions.KeywordFormat;

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
                _sqlFormattingOptions.RightMargin = updownRightMargin.Value;
            }
            else
            {
                _sqlFormattingOptions.RightMargin = 0;
            }

            _sqlFormattingOptions.KeywordFormat = (KeywordFormat)comboKeywordsCasing.SelectedIndex;
        }		
    }
}
