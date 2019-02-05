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
using System.Windows.Media;

namespace FullFeaturedDemo.Common
{
    /// <summary>
    /// Interaction logic for MenuButton.xaml
    /// </summary>
    public partial class MenuButton 
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof (ImageSource), typeof (MenuButton), new PropertyMetadata(default(ImageSource),
                delegate(DependencyObject o, DependencyPropertyChangedEventArgs args)
                {
                    var mButton = o as MenuButton;
                    if (mButton != null)
                    {
                        mButton.ImageContent.Source = args.NewValue as ImageSource;
                    }
                }));

        public ImageSource Source
        {
            get { return (ImageSource) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public MenuButton()
        {
            InitializeComponent();
        }
    }
}
