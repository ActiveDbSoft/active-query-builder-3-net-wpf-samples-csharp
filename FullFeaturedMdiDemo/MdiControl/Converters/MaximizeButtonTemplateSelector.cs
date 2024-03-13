//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2024 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FullFeaturedMdiDemo.MdiControl.Converters
{
    public class MaximizeButtonTemplateSelector: DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var obj = container as FrameworkElement;
            if (item != null)
            {
                var state = (StateWindow) item;
                var mdi = FindParent<MdiChildWindow>(obj);

                DataTemplate template;
                if (state == StateWindow.Normal)
                    template = mdi.FindResource("MaximizeDefaultTemplate") as DataTemplate;
                else
                    template = mdi.FindResource("MaximizeTemplate") as DataTemplate;

                return template;
            }
            return null;
        }

        private static T FindParent<T>(DependencyObject from) where T : class
        {
            T result = null;
            var parent = VisualTreeHelper.GetParent(from);

            if (parent is T)
                result = parent as T;
            else if (parent != null)
                result = FindParent<T>(parent);

            return result;
        }
    }
}
