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
using System.Collections;
using System.ComponentModel;
using System.Linq;
using ActiveQueryBuilder.Core;

namespace MetadataEditorDemo.Common.LoadingWizardPages
{
    /// <summary>
    /// Interaction logic for LoadOptsWizardPage.xaml
    /// </summary>
    [DesignTimeVisible(false)]
    internal partial class LoadOptsWizardPage
    {
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public LoadOptsWizardPage()
        {
            InitializeComponent();
            var langProperty = DependencyPropertyDescriptor.FromProperty(LanguageProperty, GetType());
            langProperty.AddValueChanged(this, LanguageChanged);
        }

        public IList CheckedItems => ChecklistDatabases.Items.OfType<DatabaseObjectForListbox>().Where(x => x.IsChecked).ToList();

        private void LanguageChanged(object sender, EventArgs e)
        {
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataContainerLoadForm.LoadingWizardPageDatabaseWelcom.Subscribe(x => TextBlockWelcome.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataContainerLoadForm.MetadataFiltrationClickLoadOrCancel.Subscribe(x => lblNextToContinue.Text = x));
        }
    }
}
