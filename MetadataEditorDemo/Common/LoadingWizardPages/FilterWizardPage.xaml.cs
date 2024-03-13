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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using ActiveQueryBuilder.Core;
using Helpers = ActiveQueryBuilder.View.WPF.Helpers;

namespace MetadataEditorDemo.Common.LoadingWizardPages
{
    /// <summary>
    /// Interaction logic for FilterWizardPage.xaml
    /// </summary>
    [DesignTimeVisible(false)]
    internal partial class FilterWizardPage
    {
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public bool LoadFileds
        {
            get { return CheckBoxLoadFields.IsChecked == true; }
            set { CheckBoxLoadFields.IsChecked = value; }
        }

        public MetadataFilter MetadataFilter { get; set; }

        public bool ShowServer { get; set; }
        public bool ShowDatabase { get; set; }
        public bool ShowSchema { get; set; }
        public bool ShowPackage { get; set; }

        public List<string> DatabaseList { get; set; }
        public List<string> SchemaList { get; set; }

        public FilterWizardPage()
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
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataContainerLoadForm.MetadataFiltration.Subscribe(x => TextBlockTitle.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataContainerLoadForm.MetadataFiltrationPrompt.Subscribe(x => TextBlockInfo.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataContainerLoadForm.LoadFieldsDescription.Subscribe(x => TextBlockLoadFieldsDescription.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataContainerLoadForm.AdvancedFiltrationDescrption.Subscribe(x => TextBlockAdvanced.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataContainerLoadForm.AdvancedFiltration.Subscribe(x => ButtonAdvanced.Content = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataContainerLoadForm.LoadFields.Subscribe(x => CheckBoxLoadFields.Content = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataContainerLoadForm.MetadataFiltrationClickLoadOrCancel.Subscribe(x => TextBlockLabelToStart.Text = x));
        }

        private void bntAdvanced_Click(object sender, RoutedEventArgs e)
        {
            var form = new MetadataFilterForm(MetadataFilter)
            {
                Owner = Helpers.FindVisualParent<Window>(this)
            };

            form.FilterControl.ShowServer = ShowServer;
            form.FilterControl.ShowDatabase = ShowDatabase;
            form.FilterControl.ShowSchema = ShowSchema;
            form.FilterControl.ShowPackage = ShowPackage;

            form.FilterControl.DatabaseList = DatabaseList;
            form.FilterControl.SchemaList = SchemaList;

            form.ShowDialog();
        }
    }
}
