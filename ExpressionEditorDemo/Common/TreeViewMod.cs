//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2022 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows;
using System.Windows.Controls;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.ExpressionEditor;

namespace ExpressionEditorDemo.Common
{
    class TreeViewMod: ListBox, ITreeViewMod
    {
        public CDragDropEffects DoDragDrop(string text, CDragDropEffects effects)
        {
            return (CDragDropEffects) DragDrop.DoDragDrop(this, text, (DragDropEffects) effects);
        }

        public void Invalidate()
        {
            
        }
        public void Dispose()
        {
            
        }
    }
}
