//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2021 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Windows;
using System.Windows.Input;

namespace GeneralAssembly.Common
{
    /// <summary>
    /// Interaction logic for CustomUpDown.xaml
    /// </summary>
    public partial class CustomUpDown
    {
        public delegate void ValueChangedHandler(object sender, EventArgs e);

        public event ValueChangedHandler ValueChanged;

        public int Value
        {
            set
            {
                TextBoxValue.Text = value.ToString(); 
                OnValueChanged();
            }
            get { return int.Parse(TextBoxValue.Text); }
        }

        public CustomUpDown()
        {
            InitializeComponent();
            Value = 0;
        }

        private void ValueUpButton_OnClick(object sender, RoutedEventArgs e)
        {
            Value++;
        }

        private void ValueDownButton_OnClick(object sender, RoutedEventArgs e)
        {
            Value--;
        }

        private void TextBox1_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextBoxTextAllowed(e.Text);

            int localValue;

            int.TryParse(TextBoxValue.Text, out localValue);
            Value = localValue;
        }

        private void textBoxValue_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text1 = (string) e.DataObject.GetData(typeof(string));
                if (!TextBoxTextAllowed(text1)) e.CancelCommand();
            }
            else e.CancelCommand();
        }

        private static bool TextBoxTextAllowed(string text2)
        {
            return Array.TrueForAll(text2.ToCharArray(),
                c => char.IsDigit(c) || char.IsControl(c));
        }

        protected virtual void OnValueChanged()
        {
            var handler = ValueChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
