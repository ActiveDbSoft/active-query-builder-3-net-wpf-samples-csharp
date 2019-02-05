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
using System.Windows.Controls;
using ActiveQueryBuilder.View.WPF;

namespace ExpressionEditorDemo.Common
{
    internal class ImageBox: UserControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(CImage), typeof(ImageBox), new PropertyMetadata(default(CImage), SourcePropertyChanged));

        private static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as ImageBox;
            
            if(self == null) return;

            var cimage = e.NewValue as CImage;

            self.Content = cimage?.GetControlWithImage(Size.Empty);
        }

        public CImage Source
        {
            get { return (CImage) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
    }
}
