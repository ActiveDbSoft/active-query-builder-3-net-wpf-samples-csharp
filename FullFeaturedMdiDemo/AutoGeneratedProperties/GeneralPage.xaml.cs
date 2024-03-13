//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2024 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.ComponentModel;
using ActiveQueryBuilder.Core;

namespace FullFeaturedMdiDemo.PropertiesForm
{
    /// <summary>
    /// Interaction logic for GeneralPage.xaml
    /// </summary>
    [ToolboxItem(false)]
    public partial class GeneralPage
    {
        private readonly SQLFormattingOptions _sqlFormattingOptions;        

        public GeneralPage()
        {
            InitializeComponent();
        }

        public GeneralPage(SQLFormattingOptions sqlFormattingOptions)
        {
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
            ApplyChanges();
        }

        private void checkWordWrap_CheckedChanged(object sender, EventArgs e)
        {
            updownRightMargin.IsEnabled = cbWordWrap.IsChecked.HasValue && cbWordWrap.IsChecked.Value;
            ApplyChanges();
        }

        private void updownRightMargin_ValueChanged(object sender, EventArgs e)
        {
            ApplyChanges();
        }

        public void ApplyChanges()
        {
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
