//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2025 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.ComponentModel;
using System.Windows;
using ActiveQueryBuilder.Core;
using Helpers = ActiveQueryBuilder.View.WPF.Helpers;

namespace MetadataEditorDemo.Common.LoadingWizardPages
{
    /// <summary>
    /// Interaction logic for MetadataFilterForm.xaml
    /// </summary>
    internal partial class MetadataFilterForm
    {
        private readonly MetadataFilter _filter = new MetadataFilter();
        private readonly MetadataFilter _originalFilter;

        public MetadataFilterControl FilterControl => MFilterControl;

        public MetadataFilterForm()
        {
            InitializeComponent();
            LanguageChanged(this, EventArgs.Empty);
        }

        public MetadataFilterForm(MetadataFilter filter)
            : this()
        {
            _originalFilter = filter;
            _filter.Assign(_originalFilter);

            MFilterControl.MetadataFilter = _filter;

            var langProperty = DependencyPropertyDescriptor.FromProperty(LanguageProperty, GetType());
            langProperty.AddValueChanged(this, LanguageChanged);
        }

        private void LanguageChanged(object sender, EventArgs e)
        {
            Title = ActiveQueryBuilder.View.Helpers.Localizer.GetString("strAdvancedMetadataFilter", LocalizableConstantsInternal.strAdvancedMetadataFilter);
            InformPanel.InfoText = ActiveQueryBuilder.View.Helpers.Localizer.GetString("strMetadataFilterControlDescription",
                Helpers.ConvertLanguageFromNative(Language),
                LocalizableConstantsInternal.strMetadataFilterControlDescription);
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            _originalFilter.Assign(_filter);
            DialogResult = true;
        }
    }
}
