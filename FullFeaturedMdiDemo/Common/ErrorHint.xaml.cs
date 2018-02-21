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
    public partial class ErrorHint 
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(ErrorHint), new PropertyMetadata(default(string), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ErrorHint current = dependencyObject as ErrorHint;

            if(current == null) return;

            current.Visibility = string.IsNullOrEmpty(e.NewValue as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public ErrorHint()
        {
            InitializeComponent();

            Visibility = Visibility.Collapsed;
        }

        private void CloseImage_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Text = string.Empty;
        }
    }
}
