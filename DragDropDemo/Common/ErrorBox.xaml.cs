//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2019 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace DragDropDemo.Common
{
    public partial class ErrorBox
    {
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message", typeof(string), typeof(ErrorBox), new PropertyMetadata(default(string)));

        public string Message
        {
            get { return (string) GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
        public ErrorBox()
        {
            InitializeComponent();

            Visibility = Visibility.Collapsed;

            var property = DependencyPropertyDescriptor.FromProperty(MessageProperty, typeof(ErrorBox));
            property.AddValueChanged(this, MessagePropertyChanged);
        }

        private void MessagePropertyChanged(object sender, EventArgs e)
        {
            TextBlockErrorPrompt.Text = Message;

            Visibility = string.IsNullOrEmpty(Message) ? Visibility.Collapsed : Visibility.Visible;
            
            if(string.IsNullOrEmpty(Message)) return;

            var timer = new System.Threading.Timer(CallBackPopup, null, 3000, Timeout.Infinite);
        }

        private void CallBackPopup(object state)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                if (GridError.Visibility == Visibility.Collapsed) return;

                TextBlockErrorPrompt.Text = string.Empty;
                Visibility = Visibility.Collapsed;
            });
        }
    }
}
