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

namespace FullFeaturedMdiDemo.PropertiesForm
{
    /// <summary>
    /// Interaction logic for SqlGenerationPage.xaml
    /// </summary>
    public partial class SqlGenerationPage
    {
        private readonly SQLGenerationOptions _generationOptions;
        private readonly SQLFormattingOptions _formattingOptions;        

        public SqlGenerationPage()
        {
            InitializeComponent();
        }

        public SqlGenerationPage(SQLGenerationOptions generationOptions, SQLFormattingOptions formattingOptions)
            : this()
        {
            _generationOptions = generationOptions;
            _formattingOptions = formattingOptions;

            foreach (var value in Enum.GetValues(_generationOptions.ObjectPrefixSkipping.GetType()))
            {
                cbObjectPrefixSkipping.Items.Add(value);
            }

            cbObjectPrefixSkipping.SelectedItem = _generationOptions.ObjectPrefixSkipping;
            cbQuoteAllIdentifiers.IsChecked = _generationOptions.QuoteIdentifiers == IdentQuotation.All;
        }

        private void cbObjectPrefixSkipping_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _generationOptions.ObjectPrefixSkipping = (ObjectPrefixSkipping) cbObjectPrefixSkipping.SelectedItem;
            _formattingOptions.ObjectPrefixSkipping = (ObjectPrefixSkipping) cbObjectPrefixSkipping.SelectedItem;
        }

        private void cbQuoteAllIdentifiers_Unchecked_1(object sender, RoutedEventArgs e)
        {
            _generationOptions.QuoteIdentifiers = IdentQuotation.IfNeed;
            _formattingOptions.QuoteIdentifiers = IdentQuotation.IfNeed;
        }

        private void cbQuoteAllIdentifiers_Checked_1(object sender, RoutedEventArgs e)
        {
            _generationOptions.QuoteIdentifiers = IdentQuotation.All;
            _formattingOptions.QuoteIdentifiers = IdentQuotation.All;
        }
    }
}
