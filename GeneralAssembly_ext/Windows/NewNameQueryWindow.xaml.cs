//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2019 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows;
using System.Windows.Input;

namespace GeneralAssembly.Windows
{
    /// <summary>
    /// Interaction logic for NewNameQueryWindow.xaml
    /// </summary>
    public partial class NewNameQueryWindow
    {
        public string NameQuery { get { return TextBoxNameQuery.Text; } }
        public NewNameQueryWindow()
        {
            InitializeComponent();

            Loaded += WindowNameQuery_Loaded;
        }

        private void WindowNameQuery_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= WindowNameQuery_Loaded;

            Keyboard.Focus(TextBoxNameQuery);
            TextBoxNameQuery.SelectionStart = 0;
            TextBoxNameQuery.SelectionLength = TextBoxNameQuery.Text.Length;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
