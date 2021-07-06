//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2021 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

#region Usings

using System;
using System.Windows;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.QueryView;
using ActiveQueryBuilder.View.WPF;
using ActiveQueryBuilder.View.WPF.DatabaseSchemaView;
using ActiveQueryBuilder.View.WPF.ExpressionEditor;
using ActiveQueryBuilder.View.WPF.QueryView;
using FullFeaturedMdiDemo.Common;
using FullFeaturedMdiDemo.MdiControl;
using GeneralAssembly;
using GeneralAssembly.Windows.SaveWindows;

#endregion

namespace FullFeaturedMdiDemo
{
    public class ChildWindow : MdiChildWindow
    {
        public event EventHandler SaveQueryEvent
        {
            add { ContentControl.SaveQueryEvent += value; }
            remove { ContentControl.SaveQueryEvent -= value; }
        }
        public event EventHandler SaveAsInFileEvent
        {
            add { ContentControl.SaveAsInFileEvent += value; }
            remove { ContentControl.SaveAsInFileEvent -= value; }
        }
        public event EventHandler SaveAsNewUserQueryEvent
        {
            add { ContentControl.SaveAsNewUserQueryEvent += value; }
            remove { ContentControl.SaveAsNewUserQueryEvent -= value; }
        }

        public IQueryView QueryView { get { return ContentControl.QueryView; } }

        public SQLQuery SqlQuery { get { return ContentControl.SqlQuery; } }

        public MetadataLoadingOptions MetadataLoadingOptions
        {
            get { return ContentControl.SqlContext.LoadingOptions; }
            set { ContentControl.SqlContext.LoadingOptions.Assign(value); }
        }

        public MetadataStructureOptions MetadataStructureOptions
        {
            get { return ContentControl.SqlContext.MetadataStructureOptions; }
            set { ContentControl.SqlContext.MetadataStructureOptions.Assign(value); }
        }

        public SQLFormattingOptions SqlFormattingOptions
        {
            set
            {
                if (_sqlFormattingOptions != null)
                    _sqlFormattingOptions.Updated -= _sqlFormattingOptions_Updated;

                if (_sqlFormattingOptions == null)
                    _sqlFormattingOptions = value;
                else
                    _sqlFormattingOptions.Assign(value);

                if (_sqlFormattingOptions == null) return;
                _sqlFormattingOptions.Updated += _sqlFormattingOptions_Updated;
                ContentControl.CBuilder.QueryTransformer.SQLGenerationOptions = _sqlFormattingOptions;
            }
            get
            {
                return _sqlFormattingOptions;
            }
        }

        private void _sqlFormattingOptions_Updated(object sender, EventArgs e)
        {
            ContentControl.BoxSql.Text = FormattedQueryText;
        }

        public SQLGenerationOptions SqlGenerationOptions
        {
            get { return QueryView.SQLGenerationOptions; }
            set { QueryView.SQLGenerationOptions.Assign(value); }
        }

        public BehaviorOptions BehaviorOptions
        {
            get { return SqlQuery.BehaviorOptions; }
            set { SqlQuery.BehaviorOptions.Assign(value); }
        }

        public ExpressionEditorOptions ExpressionEditorOptions
        {
            get { return ContentControl.ExpressionEditorOptions; }
            set { ContentControl.ExpressionEditorOptions.Assign(value); }
        }

        public TextEditorOptions TextEditorOptions
        {
            get { return ContentControl.BoxSql.Options; }
            set
            {
                ContentControl.TextEditorOptions.Assign(value);
                ContentControl.BoxSql.Options.Assign(value);
                ContentControl.BoxSqlCurrentSubQuery.Options.Assign(value);
            }
        }

        public SqlTextEditorOptions TextEditorSqlOptions
        {
            get { return ContentControl.BoxSql.SqlOptions; }
            set
            {
                ContentControl.TextEditorSqlOptions.Assign(value);
                ContentControl.BoxSql.SqlOptions.Assign(value);
                ContentControl.BoxSqlCurrentSubQuery.SqlOptions.Assign(value);
            }
        }

        public DataSourceOptions DataSourceOptions
        {
            get { return ContentControl.DataSourceOptions; }
            set { ContentControl.DataSourceOptions.Assign(value); }
        }

        public DesignPaneOptions DesignPaneOptions
        {
            get { return ContentControl.DesignPaneOptions; }
            set { ContentControl.DesignPaneOptions.Assign(value); }
        }

        public QueryNavBarOptions QueryNavBarOptions
        {
            get { return ContentControl.QueryNavBarOptions; }
            set { ContentControl.QueryNavBarOptions.Assign(value); }
        }

        public AddObjectDialogOptions AddObjectDialogOptions
        {
            get { return ContentControl.AddObjectDialogOptions; }
            set { ContentControl.AddObjectDialogOptions.Assign(value); }
        }

        public UserInterfaceOptions UserInterfaceOptions
        {
            get { return ContentControl.QView.UserInterfaceOptions; }
            set { ContentControl.QView.UserInterfaceOptions.Assign(value); }
        }

        public VisualOptions VisualOptions
        {
            get { return ContentControl.DockManager.Options; }
            set { ContentControl.DockManager.Options.Assign(value); }
        }

        public QueryColumnListOptions QueryColumnListOptions
        {
            get { return ContentControl.QueryColumnListOptions; }
            set { ContentControl.QueryColumnListOptions.Assign(value); }
        }

        public string FileSourceUrl
        {
            get { return ContentControl.FileSourceUrl; }
            set { ContentControl.FileSourceUrl = value; }
        }

        public string QueryText
        {
            get { return ContentControl.QueryText; }
            set { ContentControl.QueryText = value; }
        }

        public SourceType SqlSourceType
        {
            get { return ContentControl.SqlSourceType; }
            set { ContentControl.SqlSourceType = value; }
        }

        public MetadataStructureItem UserMetadataStructureItem
        {
            get { return ContentControl.UserMetadataStructureItem; }
            set { ContentControl.UserMetadataStructureItem = value; }
        }

        public bool IsNeedClose
        {
            get { return ContentControl.IsNeedClose; }
            set { ContentControl.IsNeedClose = value; }
        }

        public string FormattedQueryText
        {
            get { return ContentControl.FormattedQueryText; }
        }

        public bool IsModified
        {
            get { return ContentControl.IsModified; }
            set { ContentControl.IsModified = value; }
        }
        
        public ContentWindowChild ContentControl { get; private set; }

        private readonly DatabaseSchemaView _databaseSchemaView;
        private SQLFormattingOptions _sqlFormattingOptions;

        public ChildWindow(SQLContext sqlContext, DatabaseSchemaView databaseSchemaView)
        {
            ContentControl = new ContentWindowChild(sqlContext);
            _sqlFormattingOptions = new SQLFormattingOptions { ExpandVirtualObjects = false };
            _databaseSchemaView = databaseSchemaView;

            Children.Add(ContentControl);


            Loaded += delegate
              {
                  if (double.IsNaN(Width)) Width = ActualWidth;
                  if (double.IsNaN(Height)) Height = ActualHeight;
              };
        }

        public bool CanRedo()
        {
            return ContentControl.CanRedo();
        }

        public bool CanCopy()
        {
            return ContentControl.CanCopy();
        }

        public bool CanPaste()
        {
            return ContentControl.CanPaste();
        }

        public bool CanCut()
        {
            return ContentControl.CanCut();
        }

        public bool CanSelectAll()
        {
            return ContentControl.CanSelectAll();
        }

        public bool CanAddDerivedTable()
        {
            return ContentControl.CanAddDerivedTable();
        }

        public bool CanCopyUnionSubQuery()
        {
            return ContentControl.CanCopyUnionSubQuery();
        }

        public bool CanAddUnionSubQuery()
        {
            return ContentControl.CanAddUnionSubQuery();
        }

        public bool CanShowProperties()
        {
            return ContentControl.CanShowProperties();
        }

        public bool CanAddObject()
        {
            return ContentControl.CanAddObject();
        }

        public void AddDerivedTable()
        {
            ContentControl.AddDerivedTable();
        }

        public void CopyUnionSubQuery()
        {
            ContentControl.CopyUnionSubQuery();
        }

        public void AddUnionSubQuery()
        {
            ContentControl.AddUnionSubQuery();
        }

        public void PropertiesQuery()
        {
            ContentControl.PropertiesQuery();
        }

        public void AddObject()
        {
            ContentControl.AddObject();
        }

        public void ShowQueryStatistics()
        {
            ContentControl.ShowQueryStatistics();
        }

        public void Undo()
        {
            ContentControl.Undo();
        }

        public void Redo()
        {
            ContentControl.Redo();
        }

        public void Copy()
        {
            ContentControl.Copy();
        }

        public void Paste()
        {
            ContentControl.Paste();
        }

        public void Cut()
        {
            ContentControl.Cut();
        }

        public void SelectAll()
        {
            ContentControl.SelectAll();
        }

        public void OpenExecuteTab()
        {
            ContentControl.OpenExecuteTab();
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

            if (SqlSourceType == SourceType.New)
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
                        ContentControl.OnSaveAsNewUserQueryEvent();
                        break;
                    case SaveAsWindowDialog.ActionSave.File:
                        ContentControl.OnSaveAsInFileEvent();
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
                    ContentControl.OnSaveQueryEvent();
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
            return ContentControl.CanUndo();
        }

        public void SetOptions(Options options)
        {

            AddObjectDialogOptions.Assign(options.AddObjectDialogOptions);
            BehaviorOptions.Assign(options.BehaviorOptions);

            _databaseSchemaView.Options.Assign(options.DatabaseSchemaViewOptions);

            DataSourceOptions.Assign(options.DataSourceOptions);
            DesignPaneOptions.Assign(options.DesignPaneOptions);
            ExpressionEditorOptions.Assign(options.ExpressionEditorOptions);
            QueryColumnListOptions.Assign(options.QueryColumnListOptions);
            QueryNavBarOptions.Assign(options.QueryNavBarOptions);
            SqlFormattingOptions.Assign(options.SqlFormattingOptions);
            SqlGenerationOptions.Assign(options.SqlGenerationOptions);
            TextEditorOptions.Assign(options.TextEditorOptions);
            TextEditorSqlOptions.Assign(options.TextEditorSqlOptions);
            UserInterfaceOptions.Assign(options.UserInterfaceOptions);
            VisualOptions.Assign(options.VisualOptions);

        }

        public Options GetOptions()
        {
            return new Options
            {
                AddObjectDialogOptions = AddObjectDialogOptions,
                BehaviorOptions = BehaviorOptions,
                DatabaseSchemaViewOptions = _databaseSchemaView.Options,
                DataSourceOptions = DataSourceOptions,
                DesignPaneOptions = DesignPaneOptions,
                ExpressionEditorOptions = ExpressionEditorOptions,
                QueryColumnListOptions = QueryColumnListOptions,
                QueryNavBarOptions = QueryNavBarOptions,
                SqlFormattingOptions = SqlFormattingOptions,
                SqlGenerationOptions = SqlGenerationOptions,
                TextEditorOptions = TextEditorOptions,
                TextEditorSqlOptions = TextEditorSqlOptions,
                UserInterfaceOptions = UserInterfaceOptions,
                VisualOptions = VisualOptions
            };
        }
    }
}
