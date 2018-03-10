//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.ExpressionEditor;

namespace ExpressionEditorDemo.Common
{
    class TreeViewMod : ListBox, ITreeViewMod
    {
        public event EventHandler SuperMouseDoubleClick;
        public void Dispose()
        {

        }

        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            e.Handled = true;
            OnSuperMouseDoubleClick();
        }

        public CDragDropEffects DoDragDrop(string text, CDragDropEffects effects)
        {
            return (CDragDropEffects)DragDrop.DoDragDrop(this, text, (DragDropEffects)effects);
        }

        public void Invalidate()
        {

        }

        protected virtual void OnSuperMouseDoubleClick()
        {
            var handler = SuperMouseDoubleClick;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
