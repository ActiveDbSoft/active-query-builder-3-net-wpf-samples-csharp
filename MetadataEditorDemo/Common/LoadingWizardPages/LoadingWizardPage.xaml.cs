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
using System.ComponentModel;
using System.Windows;
using ActiveQueryBuilder.Core;

namespace MetadataEditorDemo.Common.LoadingWizardPages
{
    /// <summary>
    /// Interaction logic for LoadingWizardPage.xaml
    /// </summary>
    [DesignTimeVisible(false)]
    internal partial class LoadingWizardPage
    {
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public LoadingWizardPage()
        {
            InitializeComponent();
            Localize();

            var langProperty = DependencyPropertyDescriptor.FromProperty(LanguageProperty, GetType());
            langProperty.AddValueChanged(this, LanguageChanged);
        }

        private void LanguageChanged(object sender, EventArgs e)
        {
            Localize();
        }

        private void Localize()
        {
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataContainerLoadForm.LoadingWizardPagePrompt.Subscribe(x => TextBlockPrompt.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataContainerLoadForm.LoadingMetadataProgress.Subscribe(x => TextBlockTitle.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataContainerLoadForm.Loading.Subscribe(x => lblLoaded.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataContainerLoadForm.LoadingSuccess.Subscribe(x => TextBlockSuccess.Text = x));
        }

        public void ShowSuccess()
        {
            ImageSuccess.Visibility = Visibility.Visible;
            TextBlockSuccess.Visibility = Visibility.Visible;
        }
    }
}
