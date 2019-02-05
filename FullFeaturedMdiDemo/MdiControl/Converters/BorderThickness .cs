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
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FullFeaturedMdiDemo.MdiControl.Converters
{
    class BorderThickness: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null)
            {
                var h = SystemParameters.ResizeFrameVerticalBorderWidth ;

                return new Thickness(h, 0, h, 0);
            }

            var w = SystemParameters.ResizeFrameVerticalBorderWidth ;

            return new Thickness(w, 0, w, w);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
