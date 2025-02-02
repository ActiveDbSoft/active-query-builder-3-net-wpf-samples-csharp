//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2025 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ActiveQueryBuilder.View.WPF;

namespace MetadataEditorDemo.Common
{
    internal class ImageBox: UserControl
    {
        public static readonly DependencyProperty PreferredWidthProperty = DependencyProperty.Register(
            "PreferredWidth", typeof(double), typeof(ImageBox), new PropertyMetadata(double.NaN));

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public double PreferredWidth
        {
            get { return (double) GetValue(PreferredWidthProperty); }
            set { SetValue(PreferredWidthProperty, value); }
        }

        public static readonly DependencyProperty PreferredHeightProperty = DependencyProperty.Register(
            "PreferredHeight", typeof(double), typeof(ImageBox), new PropertyMetadata(double.NaN));

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public double PreferredHeight
        {
            get { return (double)GetValue(PreferredHeightProperty); }
            set { SetValue(PreferredHeightProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(CImage), typeof(ImageBox), new PropertyMetadata(default(CImage), SourcePropertyChanged));

        private static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as ImageBox;
            
            if(self == null) return;

            var cimage = e.NewValue as CImage;

            self.Content = cimage?.GetControlWithImage(new Size(self.PreferredWidth, self.PreferredHeight));
        }

        public CImage Source
        {
            get { return (CImage) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
    }
}
