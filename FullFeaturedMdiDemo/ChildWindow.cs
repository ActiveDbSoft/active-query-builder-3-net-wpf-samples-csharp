//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2019 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

#region Usings

using System;
using System.Windows;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.QueryView;
using FullFeaturedMdiDemo.Common;
using FullFeaturedMdiDemo.MdiControl;
using Helpers = FullFeaturedMdiDemo.Common.Helpers;

#endregion

namespace FullFeaturedMdiDemo
{
    public class ChildWindow : MdiChildWindow
    {
        public event EventHandler SaveQueryEvent
        {
            add { _content.SaveQueryEvent += value; }
            remove { _content.SaveQueryEvent -= value; }
        }
        public event EventHandler SaveAsInFileEvent
        {
            add { _content.SaveAsInFileEvent += value; }
            remove { _content.SaveAsInFileEvent -= value; }
        }
        public event EventHandler SaveAsNewUserQueryEvent
        {
            add { _content.SaveAsNewUserQueryEvent += value; }
            remove { _content.SaveAsNewUserQueryEvent -= value; }
        }

        public IQueryView QueryView { get { return _content.QueryView; } }

        public SQLQuery SqlQuery { get { return _content.SqlQuery; }}

        public SQLFormattingOptions SqlFormattingOptions
        {
            get { return _content.SqlFormattingOptions; }
            set { _content.SqlFormattingOptions = value; }
        }

        public SQLGenerationOptions SqlGenerationOptions
        {
            get { return _content.SqlGenerationOptions; }
            set { _content.SqlGenerationOptions = value; }
        }

        public string FileSourceUrl
        {
            get { return _content.FileSourceUrl; }
            set { _content.FileSourceUrl = value; }
        }

        public string QueryText
        {
            get { return _content.QueryText; }
            set { _content.QueryText = value; }
        }

        public Helpers.SourceType SqlSourceType
        {
            get { return _content.SqlSourceType; }
            set { _content.SqlSourceType = value; }
        }

        public MetadataStructureItem UserMetadataStructureItem
        {
            get { return _content.UserMetadataStructureItem; }
            set { _content.UserMetadataStructureItem = value; }
        }

        public bool IsNeedClose
        {
            get { return _content.IsNeedClose; }
            set { _content.IsNeedClose = value; }
        }

        public string FormattedQueryText
        {
            get { return _content.FormattedQueryText; }
        }

        public bool IsModified
        {
            get { return _content.IsModified; }
            set { _content.IsModified = value; }
        }

        private readonly ContentWindowChild _content;

        public ContentWindowChild ContentControl
        {
            get { return _content; }
        }

        public ChildWindow(SQLContext sqlContext)
        {
            _content = new ContentWindowChild(sqlContext);
            Children.Add(_content);


            Loaded+=delegate
            {
                if (double.IsNaN(Width)) Width = ActualWidth;
                if (double.IsNaN(Height)) Height = ActualHeight;
            };
        }

        public bool CanRedo()
        {
            return _content.CanRedo();
        }

        public bool CanCopy()
        {
            return _content.CanCopy();
        }

        public bool CanPaste()
        {
            return _content.CanPaste();
        }

        public bool CanCut()
        {
            return _content.CanCut();
        }

        public bool CanSelectAll()
        {
            return _content.CanSelectAll();
        }

        public bool CanAddDerivedTable()
        {
            return _content.CanAddDerivedTable();
        }

        public bool CanCopyUnionSubQuery()
        {
            return _content.CanCopyUnionSubQuery();
        }

        public bool CanAddUnionSubQuery()
        {
            return _content.CanAddUnionSubQuery();
        }

        public bool CanShowProperties()
        {
            return _content.CanShowProperties();
        }

        public bool CanAddObject()
        {
            return _content.CanAddObject();
        }

        public void AddDerivedTable()
        {
            _content.AddDerivedTable();
        }

        public void CopyUnionSubQuery()
        {
            _content.CopyUnionSubQuery();
        }

        public void AddUnionSubQuery()
        {
            _content.AddUnionSubQuery();
        }

        public void PropertiesQuery()
        {
            _content.PropertiesQuery();
        }

        public void AddObject()
        {
            _content.AddObject();
        }

        public void ShowQueryStatistics()
        {
            _content.ShowQueryStatistics();
        }

        public void Undo()
        {
            _content.Undo();
        }

        public void Redo()
        {
            _content.Redo();
        }

        public void Copy()
        {
            _content.Copy();
        }

        public void Paste()
        {
            _content.Paste();
        }

        public void Cut()
        {
            _content.Cut();
        }

        public void SelectAll()
        {
            _content.SelectAll();
        }

        public void OpenExecuteTab()
        {
            _content.OpenExecuteTab();
        }

        public void ForceClose()
        {
            base.Close();
        }

        public override void Close()
        {
            if (IsNeedClose || !IsModified)
            {
                base.Close();
                return;
            }

            var point = PointToScreen(new Point(0, 0));

            if (SqlSourceType == Helpers.SourceType.New)
            {
                IsNeedClose = true;

                var dialog = new SaveAsWindowDialog(Title)
                {
                    Left = point.X,
                    Top = point.Y
                };

                dialog.Loaded += Dialog_Loaded;
                dialog.ShowDialog();

                switch (dialog.Action)
                {
                    case SaveAsWindowDialog.ActionSave.UserQuery:
                        _content.OnSaveAsNewUserQueryEvent();
                        break;
                    case SaveAsWindowDialog.ActionSave.File:
                        _content.OnSaveAsInFileEvent();
                        break;
                    case SaveAsWindowDialog.ActionSave.NotSave:
                        base.Close();
                        break;
                    case SaveAsWindowDialog.ActionSave.Continue:
                        IsNeedClose = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                var saveDialog = new SaveExistQueryDialog
                {
                    Left = point.X,
                    Top = point.Y
                };

                saveDialog.Loaded += Dialog_Loaded;
                saveDialog.ShowDialog();

                if (!saveDialog.Result.HasValue) return;

                IsNeedClose = true;

                if (saveDialog.Result == true)
                {
                    _content.OnSaveQueryEvent();
                    return;
                }
            }

            base.Close();
        }

        private void Dialog_Loaded(object sender, RoutedEventArgs e)
        {
            var dialog = sender as Window;

            if (dialog == null) return;

            dialog.Loaded -= Dialog_Loaded;

            dialog.Left += (ActualWidth / 2 - dialog.ActualWidth / 2);
            dialog.Top += (ActualHeight / 2 - dialog.ActualHeight / 2);
        }

        public bool CanUndo()
        {
            return _content.CanUndo();
        }
    }
}
