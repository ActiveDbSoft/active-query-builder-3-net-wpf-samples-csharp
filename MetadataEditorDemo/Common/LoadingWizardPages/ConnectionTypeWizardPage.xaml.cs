//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2021 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows;
using ActiveQueryBuilder.Core;

namespace MetadataEditorDemo.Common.LoadingWizardPages
{
    /// <summary>
    /// Interaction logic for ConnectionTypeWizardPage.xaml
    /// </summary>
    internal partial class ConnectionTypeWizardPage
    {
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public ConnectionTypeWizardPage()
        {
            InitializeComponent();

            SubscribeLocalization();
        }

        private void SubscribeLocalization()
        {
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataContainerLoadForm.SelectConnectionTypeDescr.Subscribe(x => TextBlockWelcome.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataContainerLoadForm.SelectConnectionType.Subscribe(x => TextBlockConnectionType.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataContainerLoadForm.SelectSyntaxProvider.Subscribe(x => TextBlockSyntaxProvider.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataContainerLoadForm.MetadataFiltrationClickLoadOrCancel.Subscribe(x => TextBlockNextToContinue.Text = x));
        }

        public Visibility SyntaxVisible
        {
            set
            {
                ComboBoxSyntaxProvider.Visibility = value;
                TextBlockSyntaxProvider.Visibility = value;
            }
        }
    }
}
