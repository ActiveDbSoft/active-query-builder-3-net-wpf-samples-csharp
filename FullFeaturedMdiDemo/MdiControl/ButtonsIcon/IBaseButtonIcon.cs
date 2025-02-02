//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2025 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows.Media;

namespace FullFeaturedMdiDemo.MdiControl.ButtonsIcon
{
    interface IBaseButtonIcon
    {
        Brush Stroke { set; get; }
        bool IsMaximized { set; get; }
    }
}
