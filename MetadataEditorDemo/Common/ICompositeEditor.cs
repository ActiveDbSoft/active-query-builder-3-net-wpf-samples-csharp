//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2025 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows;
using System.Windows.Controls;

namespace MetadataEditorDemo.Common
{
    public enum DirectionLabelAndContent
    {
        Horizontal,
        Vertical
    }
    internal interface ICompositeEditor
    {
        DirectionLabelAndContent DirectionLabelAndContent { get;  }
        LabelTextBlock LabelElement {  get; }
        FrameworkElement ContentElement { get; }
        TextBlock LabelDescription {  get; }
        FrameworkElement AdditionalElement { get; }
    }
}
