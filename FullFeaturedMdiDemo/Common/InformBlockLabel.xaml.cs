﻿//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows;

namespace FullFeaturedMdiDemo.Common
{
    public partial class InformBlockLabel
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(InformBlockLabel), new PropertyMetadata(default(string)));

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public InformBlockLabel()
        {
            InitializeComponent();
        }
    }
}
