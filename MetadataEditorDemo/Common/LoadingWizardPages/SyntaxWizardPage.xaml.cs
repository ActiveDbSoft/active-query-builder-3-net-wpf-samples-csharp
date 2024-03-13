//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2024 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.ComponentModel;
using System.Windows.Controls;
using ActiveQueryBuilder.Core;

namespace MetadataEditorDemo.Common.LoadingWizardPages
{
    /// <summary>
    /// Interaction logic for SyntaxWizardPage.xaml
    /// </summary>
    [DesignTimeVisible(false)]
    internal partial class SyntaxWizardPage
    {
        public SyntaxWizardPage()
        {
            InitializeComponent();
        }

        private void ComboSelectSyntax_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (comboSelectSyntax.SelectedItem is GenericSyntaxProvider)
                lblWarning.Text = "Usage of Generic Syntax Provider is not recommended. Metadata may be not fully loaded.";
            else
                lblWarning.Text = "";
        }
    }
}
