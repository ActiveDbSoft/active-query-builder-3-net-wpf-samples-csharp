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
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FullFeaturedMdiDemo.MdiControl
{
    public class CustomCanvas: Canvas
    {
        protected override Size MeasureOverride(Size constraint)
        {
            var defaultValue = base.MeasureOverride(constraint);
            var desiredSize = new Size();

            desiredSize = Children.Cast<UIElement>()
                .Aggregate(desiredSize,
                    (current, child) =>
                        new Size(Math.Max(current.Width, GetLeft(child) + child.DesiredSize.Width),
                            Math.Max(current.Height, GetTop(child) + child.DesiredSize.Height)));

            return (double.IsNaN(desiredSize.Width) || double.IsNaN(desiredSize.Height)) ? defaultValue : desiredSize;
        }
    }
}
