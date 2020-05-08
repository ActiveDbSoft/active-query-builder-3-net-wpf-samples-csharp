//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2019 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.Commands;
using ActiveQueryBuilder.View.WPF;
using ActiveQueryBuilder.View.WPF.Commands;
using Microsoft.Win32;
using Helpers = ActiveQueryBuilder.View.WPF.Helpers;
using MetadataEditorControl = MetadataEditorDemo.Common.MetadataEditorControl;

namespace MetadataEditorDemo
{
    /// <summary>
    /// Interaction logic for MetadataEditorStandalone.xaml
    /// </summary>
    internal partial class MetadataEditorStandalone
    {
        private readonly SQLContext _originalContext;
        private readonly SQLContext _sqlContext;

        private MetadataEditorOptions _options = 0;
        private int _structurePaneWidth = -1;

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        private Command _importContainerCommand;
        private Command _exportContainerCommand;
        private Command _importStructureCommand;
        private Command _exportStructureCommand;

        private Command _previewCommand;

        private Command _stopLoadingCommand;

        public MetadataEditorStandalone()
        {
            InitializeComponent();

            CreateAndBindCommands();

            foreach (Type type in ActiveQueryBuilder.Core.Helpers.SyntaxProviderList)
            {
                if (type == typeof(AutoSyntaxProvider))
                {
                    continue;
                }

                var syntax = Activator.CreateInstance(type) as BaseSyntaxProvider;

                if (syntax != null)
                {
                    cbSyntaxProvider.Items.Add(syntax);
                }
            }

            MetadataEditor.OpenContainerLoadFormIfNotConnected = true;
            MetadataEditor.LoadStart += LoadStart;
            MetadataEditor.LoadFinish += LoadFinish;
            MetadataEditor.LoadStep += LoadStep;

            cbSyntaxProvider.SelectionChanged += CbSyntaxProvider_SelectedIndexChanged;
            tsmiLoadFromDatabase.Click += loadFromDatabaseToolStripMenuItem_Click;


            UpdateLocalization();

            _originalContext = new SQLContext(){SyntaxProvider =  new MSSQLSyntaxProvider()};
            _originalContext.MetadataContainer.LoadingOptions.OfflineMode = true;
            _originalContext.MetadataContainer.ImportFromXML("Northwind.xml");

             _sqlContext = new SQLContext();
            _sqlContext.Assign(_originalContext);

            if (_sqlContext.SyntaxProvider == null)
            {
                _sqlContext.SyntaxProvider = new GenericSyntaxProvider();
            }

            _sqlContext.SyntaxProviderChanged += SqlContext_SyntaxProviderChanged;

            foreach (var item in cbSyntaxProvider.Items)
            {
                if (item.GetType() != _sqlContext.SyntaxProvider.GetType()) continue;

                cbSyntaxProvider.SelectionChanged -= CbSyntaxProvider_SelectedIndexChanged;
                try
                {
                    cbSyntaxProvider.SelectedItem = item;
                    break;
                }
                finally
                {
                    cbSyntaxProvider.SelectionChanged += CbSyntaxProvider_SelectedIndexChanged;
                }
            }

            MetadataEditor.Init(_sqlContext);
        }

        private void CbSyntaxProvider_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            _sqlContext.SyntaxProvider = cbSyntaxProvider.SelectedItem as BaseSyntaxProvider;
        }

        private bool IsStructureTreeEnabled()
        {
            return (_options & MetadataEditorOptions.DisableStructurePane) == 0;
        }

        public bool StructureTreeVisible
        {
            get { return MetadataEditor.StructureTreeVisible; }
            set
            {
                if (value == MetadataEditor.StructureTreeVisible)
                {
                    return;
                }

                if (value && !IsStructureTreeEnabled())
                {
                    return;
                }

                if (!value)
                {
                    //_structurePaneWidth = MetadataEditor.splitContainerInner.SplitterDistance;
                }

                MetadataEditor.StructureTreeVisible = value;

                if (!value)
                {
                    Width -= _structurePaneWidth;
                }
                else if (_structurePaneWidth != -1)
                {
                    Width += _structurePaneWidth;
                }
            }
        }

        public MetadataEditorOptions Options
        {
            get { return _options; }
            set
            {
                _options = value;

                StructureTreeVisible = IsStructureTreeEnabled();

                MetadataEditor.MenuItemMetadataLoadAll.Visibility =
                    ((value & MetadataEditorOptions.DisableLoadDatabaseButton) !=
                     MetadataEditorOptions.DisableLoadDatabaseButton)
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                MetadataEditor.HideVirtualObjects = (value & MetadataEditorOptions.DisableVirtualObjects) ==
                                                           MetadataEditorOptions.DisableVirtualObjects;
            }
        }

        private void SqlContext_SyntaxProviderChanged(object sender, EventArgs e)
        {
            BaseSyntaxProvider currentSyntax = _sqlContext.SyntaxProvider;
            if (currentSyntax is AutoSyntaxProvider)
                currentSyntax = ((AutoSyntaxProvider) currentSyntax).DetectedSyntaxProvider;

            if (currentSyntax == null) return;

            for (int i = 0; i < cbSyntaxProvider.Items.Count; i++)
            {
                BaseSyntaxProvider sp = (BaseSyntaxProvider) cbSyntaxProvider.Items[i];

                if (sp.GetType() == currentSyntax.GetType())
                {
                    cbSyntaxProvider.SelectedIndex = i;
                }
            }
        }

        private void CreateAndBindCommands()
        {
            _importContainerCommand = new Command(btnImportMetadataFromXml_Click);
            _exportContainerCommand = new Command(btnExportMetadataToXml_Click);
            _importStructureCommand = new Command(btnImportStructureFromXml_Click);
            _exportStructureCommand = new Command(btnExportStructureToXml_Click);

            _previewCommand = new Command(BtnPreview_Click);
            _stopLoadingCommand = new Command(MetadataEditor.StopLoading);

            _subscriptions.Add(CommandBinder.Bind(tsmiExportContainer, _exportContainerCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.ExportContainer, x => tsmiExportContainer.IsEnabled = x));
            _subscriptions.Add(CommandBinder.Bind(tsmiImportContainer, _importContainerCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.ImportContainer, x => tsmiImportContainer.IsEnabled = x));
            _subscriptions.Add(CommandBinder.Bind(tsmiExportStructure, _exportStructureCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.ExportStructure, x => tsmiExportStructure.IsEnabled = x));
            _subscriptions.Add(CommandBinder.Bind(tsmiImportStructure, _importStructureCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.ImportStructure, x => tsmiImportStructure.IsEnabled = x));

            _subscriptions.Add(CommandBinder.Bind(BtnPreview, _previewCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.Preview, x => BtnPreview.IsEnabled = x));
            _subscriptions.Add(CommandBinder.Bind(ButtonBreakLoading, _stopLoadingCommand,
                ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.StopLoading, x => ButtonBreakLoading.IsEnabled = x));
        }


        private void UpdateLocalization()
        {
            Title = Helpers.Localizer.GetString("strMetadataEditor", Helpers.ConvertLanguageFromNative(Language), LocalizableConstantsInternal.strMetadataEditor);

            tsmiLoadFromDatabase.Header = Helpers.Localizer.GetString("strLoadFromDatabase", Helpers.ConvertLanguageFromNative(Language),
                LocalizableConstantsInternal.strLoadFromDatabase);
            containerToolStripMenuItem.Header = Helpers.Localizer.GetString("strMetadataContainer", Helpers.ConvertLanguageFromNative(Language),
                LocalizableConstantsInternal.strMetadataContainer);
            structureToolStripMenuItem.Header = Helpers.Localizer.GetString("strMetadataStructure", Helpers.ConvertLanguageFromNative(Language),
                LocalizableConstantsInternal.strMetadataStructure);
            toolStripLabel.Text =
                Helpers.Localizer.GetString("strSyntaxProvider", Helpers.ConvertLanguageFromNative(Language), LocalizableConstantsInternal.strSyntaxProvider);

        }

        protected override void OnClosed(EventArgs e)
        {
            _sqlContext.SyntaxProviderChanged -= SqlContext_SyntaxProviderChanged;

            MetadataEditor.LoadStart -= LoadStart;
            MetadataEditor.LoadFinish -= LoadFinish;
            MetadataEditor.LoadStep -= LoadStep;
            cbSyntaxProvider.SelectionChanged -= CbSyntaxProvider_SelectedIndexChanged;
            tsmiLoadFromDatabase.Click -= loadFromDatabaseToolStripMenuItem_Click;

            _subscriptions.Dispose();
            base.OnClosed(e);
        }

        private void ApplyChanges()
        {
            var groupFields = _originalContext.MetadataStructure.Options.GroupFieldsByTypes;
            _originalContext.Assign(_sqlContext);
            _originalContext.MetadataStructure.Options.GroupFieldsByTypes = groupFields;
            _originalContext.MetadataStructure.Refresh();
        }

        private void LoadStart(object sender, EventArgs e)
        {
            ProgressBarLoading.Visibility = Visibility.Visible;
            LabelLoadingIbjects.Visibility = Visibility.Visible;
            ButtonBreakLoading.Visibility = Visibility.Visible;
            ButtonBreakLoading.Focus();

            ProgressBarLoading.Value = 0;
        }

        private void LoadStep(object sender, EventArgs e)
        {
            if (ProgressBarLoading.Value >= ProgressBarLoading.Maximum)
            {
                ProgressBarLoading.Value = 0;
            }
        }

        private void LoadFinish(object sender, EventArgs e)
        {
            ProgressBarLoading.Visibility = Visibility.Collapsed;
            LabelLoadingIbjects.Visibility = Visibility.Collapsed;
            ButtonBreakLoading.Visibility = Visibility.Collapsed;
            ShowStatusMessage(ActiveQueryBuilder.Core.Localization.MetadataEditor.LoadCompleted.Value);
        }

        private void ShowStatusMessage(string message)
        {
            LabelStatusMessage.Text = message;
            LabelStatusMessage.Visibility = Visibility.Visible;

            Timer t = new Timer {Interval = 3000};
            t.Elapsed += T_Tick;
            t.Start();
        }

        private void T_Tick(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action) delegate
            {
                LabelStatusMessage.Visibility = Visibility.Collapsed;
            });
           
            ((Timer) sender).Stop();
            ((Timer) sender).Elapsed -= T_Tick;
            ((Timer) sender).Dispose();
        }

        private void btnCancel_Click()
        {
            if (MetadataEditor.IsChanged)
            {
                var result = MessageBox.Show(
                    Helpers.Localizer.GetString("strAllChangesWillLost", Helpers.ConvertLanguageFromNative(Language),
                        LocalizableConstantsInternal.strAllChangesWillLost),
                    Helpers.Localizer.GetString("strWarning", Helpers.ConvertLanguageFromNative(Language), LocalizableConstantsUI.strWarning),
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Cancel)
                    DialogResult = false;
            }
            else
            {
                DialogResult = false;
            }
        }

        private void loadFromDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MetadataEditor.ShowContainerLoadForm();
        }

        private void btnExportMetadataToXml_Click()
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Title = Helpers.Localizer.GetString("strContainerExportDescr", Helpers.ConvertLanguageFromNative(Language),
                    LocalizableConstantsInternal.strContainerExportDescr),
                Filter = @"XML files (*.xml)|*.xml|All files (*.*)|*.*"
            };

            if (saveDialog.ShowDialog() == true)
            {
                _sqlContext.MetadataContainer.ExportToXML(saveDialog.FileName);
            }
        }

        private void btnImportMetadataFromXml_Click()
        {
            OpenFileDialog openDialog = new OpenFileDialog();

            openDialog.Title = Helpers.Localizer.GetString("strContainerExportDescr", Helpers.ConvertLanguageFromNative(Language),
                LocalizableConstantsInternal.strContainerImportDescr);
            openDialog.Filter = @"XML files (*.xml)|*.xml|All files (*.*)|*.*";

            if (openDialog.ShowDialog() != true) return;

            var errorList = new List<string>();
            var log = new MetadataEditorControl.ListLog(errorList);
            _sqlContext.MetadataContainer.SQLContext.Logger.Log = log;
            _sqlContext.MetadataContainer.ImportFromXML(openDialog.FileName);

            if (errorList.Count > 1)
            {
                var errorForm = new ErrorListForm("Warning",
                    Helpers.Localizer.GetString("strErrorWhileImport", Helpers.ConvertLanguageFromNative(Language),
                        LocalizableConstantsInternal.strErrorWhileImport) + ":", errorList);
                errorForm.ShowDialog();
            }

            MetadataEditor.RefreshContainerTree();
            MetadataEditor.IsChanged = false;
        }

        private void BtnPreview_Click()
        {
            var form = new MetadataTreePreviewForm(_sqlContext);
            form.ShowDialog();
        }

        private void btnExportStructureToXml_Click()
        {
            SaveFileDialog saveDialog = new SaveFileDialog();

            saveDialog.Title = Helpers.Localizer.GetString("strStructureExportDescr", Helpers.ConvertLanguageFromNative(Language),
                LocalizableConstantsInternal.strStructureExportDescr);
            saveDialog.Filter = @"XML files (*.xml)|*.xml|All files (*.*)|*.*";

            if (saveDialog.ShowDialog() == true)
            {
                _sqlContext.MetadataStructure.ExportToXML(saveDialog.FileName);
            }
        }

        private void btnOK_Click()
        {
            var errorList = MetadataEditor.ValidateBeforeApply();
            if (errorList.Count == 0)
            {
                ApplyChanges();
                DialogResult = true;
                return;
            }

            var form = new ErrorListForm("Warning", Helpers.Localizer.GetString("strErrorsFound", Helpers.ConvertLanguageFromNative(Language), LocalizableConstantsInternal.strErrorsFound), errorList, true);
            var result = form.ShowDialog();

            if (result != true) return;

            ApplyChanges();
            DialogResult = true;
        }

        private void btnImportStructureFromXml_Click()
        {
            OpenFileDialog openDialog = new OpenFileDialog
            {
                Title = Helpers.Localizer.GetString("strStructureImportDescr", Helpers.ConvertLanguageFromNative(Language),
                    LocalizableConstantsInternal.strStructureImportDescr),
                Filter = @"XML files (*.xml)|*.xml|All files (*.*)|*.*"
            };


            if (openDialog.ShowDialog() != true) return;

            _sqlContext.MetadataStructure.ImportFromXML(openDialog.FileName);
            _sqlContext.MetadataStructure.Refresh();

            MetadataEditor.IsChanged = false;

        }
    }
}
