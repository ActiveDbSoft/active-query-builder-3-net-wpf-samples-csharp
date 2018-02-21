//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows;
using System.Windows.Input;

namespace FullFeaturedMdiDemo.Common
{
    public partial class GridInformMessage 
    {
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message", typeof(string), typeof(GridInformMessage), new PropertyMetadata(default(string), PropertyChanged));

        private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GridInformMessage obj = d as GridInformMessage;
            if(obj == null) return;

            obj.BlockMessage.Text = e.NewValue as string;

            obj.Visibility = string.IsNullOrEmpty(obj.BlockMessage.Text) ? Visibility.Collapsed : Visibility.Visible;
        }

        public string Message
        {
            get { return (string) GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
        public GridInformMessage()
        {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
        }

        private void Close_Click(object sender, MouseButtonEventArgs e)
        {
            Message = "";
        }
    }
}
