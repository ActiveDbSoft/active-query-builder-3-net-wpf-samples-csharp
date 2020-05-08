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
using ActiveQueryBuilder.View.WPF;

namespace MetadataEditorDemo.Common
{
    /// <summary>
    /// Interaction logic for ButtonWithImage.xaml
    /// </summary>
    internal partial class ButtonWithImage 
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(ButtonWithImage), new PropertyMetadata(default(string)));

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof(CImage), typeof(ButtonWithImage), new PropertyMetadata(default(CImage)));

        public CImage Icon
        {
            get { return (CImage) GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public ButtonWithImage()
        {
            InitializeComponent();
        }
    }
}
