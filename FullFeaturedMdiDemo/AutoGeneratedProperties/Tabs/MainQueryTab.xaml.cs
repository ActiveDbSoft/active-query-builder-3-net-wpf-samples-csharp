//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2022 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Windows;
using System.Windows.Controls;
using ActiveQueryBuilder.Core;

namespace FullFeaturedMdiDemo.PropertiesForm.Tabs
{
    /// <summary>
    /// Interaction logic for MainQueryTab.xaml
    /// </summary>
    public partial class MainQueryTab
    {
        public SQLFormattingOptions Options { get; set; }

        public MainQueryTab()
        {
            InitializeComponent();
        }

        public MainQueryTab(SQLFormattingOptions sqlFormattingOptions)
        {
            InitializeComponent();

            foreach (var value in typeof(KeywordFormat).GetEnumValues())
                ComboBoxKeywordCase.Items.Add(value);

            Options = sqlFormattingOptions;
            LoadOptions();
        }

        // Load options to form
        public void LoadOptions()
        {
            CheckBoxIndents.IsChecked = Options.DynamicIndents;
            CheckBoxRightMargin.IsChecked = Options.DynamicRightMargin;

            if (Options.RightMargin > 0)
            {
                CheckBoxEnableWordWrap.IsChecked = false;
                UpDownCharacterInLine.Value = Options.RightMargin;
            }
            else
            {
                // no margin
                CheckBoxEnableWordWrap.IsChecked = false;
                UpDownCharacterInLine.Value = 80;
            }
            CheckBoxWithinAnd.IsChecked = Options.ParenthesizeANDGroups;
            CheckBoxSingleCondition.IsChecked = Options.ParenthesizeSingleCriterion;

            ComboBoxKeywordCase.SelectedItem = Options.KeywordFormat;
        }

        private void CheckBoxEnableWordWrap_OnChanged(object sender, RoutedEventArgs e)
        {
            if(!IsInitialized) return;

            Options.RightMargin = !CheckBoxEnableWordWrap.IsChecked == true ? 0 : UpDownCharacterInLine.Value;
        }

        private void CheckBoxIndents_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            Options.DynamicIndents = CheckBoxIndents.IsChecked == true;
        }

        private void CheckBoxRightMargin_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            Options.DynamicRightMargin = CheckBoxRightMargin.IsChecked == true;
        }

        private void CheckBoxWithinAnd_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            Options.ParenthesizeANDGroups = CheckBoxWithinAnd.IsChecked == true;
        }

        private void CheckBoxSingleCondition_OnChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            Options.ParenthesizeSingleCriterion = CheckBoxSingleCondition.IsChecked == true;
        }

        private void ComboBoxKeywordCase_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsInitialized) return;

            Options.KeywordFormat = (KeywordFormat) ComboBoxKeywordCase.SelectedItem;
        }

        private void UpDownCharacterInLine_OnValueChanged(object sender, EventArgs e)
        {
            if (!IsInitialized) return;

            if (CheckBoxEnableWordWrap.IsChecked == true)
                Options.RightMargin = UpDownCharacterInLine.Value;
        }
    }
}
