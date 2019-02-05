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
using System.Windows.Input;
using System.Windows.Media;

namespace FullFeaturedMdiDemo.MdiControl.ButtonsIcon
{
    public class ButtonMinimizeIcon : BaseButtonIcon
    {
        

        protected override void OnRender(DrawingContext drawingContext)
        {
            MinHeight = SizeContent.Height;
            MinWidth = SizeContent.Width;

            var x = (ActualWidth - SizeContent.Width) / 2;
            var y = (ActualHeight - SizeContent.Height) / 2;

            var pen = new Pen(Stroke, 2);

            var brushPressed =  new SolidColorBrush((Color) ColorConverter.ConvertFromString("#3d6099"));

            var pressed = IsMouseOver && Mouse.LeftButton == MouseButtonState.Pressed;

            var brush = pressed? brushPressed: (IsMouseOver ? Background : Brushes.Transparent);

            drawingContext.DrawRectangle(brush, null,
               new Rect(new Point(0, 0), new Size(ActualWidth, ActualHeight)));



            drawingContext.DrawLine(pen, 
                new Point(x, y + SizeContent.Height - 1),
                new Point(x + SizeContent.Width, y + SizeContent.Height - 1));
        }
    }
}
