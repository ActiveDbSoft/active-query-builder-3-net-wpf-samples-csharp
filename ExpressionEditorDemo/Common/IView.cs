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
using System.Collections.Generic;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.DatabaseSchemaView;
using ActiveQueryBuilder.View.EventHandlers.MetadataStructureItems;
using ActiveQueryBuilder.View.ExpressionEditor;
using ActiveQueryBuilder.View.MetadataStructureView;

namespace ExpressionEditorDemo.Common
{
    public interface IView
    {
		string FilterTextForFunctions { get; }
		bool FilterFunctions { get; }
        IDatabaseSchemaView ObjectsTree { get; }
		IDatabaseSchemaView QueryObjectsTree { get; }
		ITreeViewMod FunctionsTree { get; }
		bool Visible { get; set; }

        event MetadataStructureItemMenuEventHandler ObjectTreeValidateItemContextMenu;
        event MetadataStructureItemMenuEventHandler QueryObjectTreeValidateItemContextMenu;
        event MetadataStructureItemEventHandler ObjectTreeDoubleClick;

        event CKeyEventHandler FunctionTreeKeyDown;
		event EventHandler FunctionTreeDoubleClick;
		event CMouseEventHandler FunctionTreeMouseUp;
		event CMouseEventHandler FunctionTreeMouseMove;

        event EventHandler FilterFunctionsChanged;
        event EventHandler FilterTextForFunctionsChanged;
        event CKeyEventHandler FilterForFunctionsKeyDown;

        event MetadataStructureItemEventHandler QueryObjectTreeDoubleClick;

		event EventHandler OperatorButtonClick;

		void SetImageList(object imageList);
		void AddTextEditor(object textEditor);
		void FillObjectTree(MetadataStructure metadataStructure);
		void FillQueryObjectsTree(MetadataStructure nodes);
		void FillFunctionsTree(List<NodeData> nodes);
		void FillOperators(List<string> operatorList);

		object GetNodeTag(object node);
		string GetNodeText(object node);
		void LocalizeForm();
		bool ShouldBeginDrag(int x, int y);
		void ShowWaitCursor();
		void ResetCursor();
		CPoint GetScreenPoint(object sender, CPoint point);
        T GetDragObject<T>(object dragObject) where T : class;
    }
}
