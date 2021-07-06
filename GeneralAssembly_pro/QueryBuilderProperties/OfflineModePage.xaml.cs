//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2021 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.WPF;
using Microsoft.Win32;

namespace GeneralAssembly.QueryBuilderProperties
{
    /// <summary>
    /// Interaction logic for OfflineModePage.xaml
    /// </summary>
    [ToolboxItem(false)]
    public partial class OfflineModePage
    {
        private readonly SQLContext _sqlContext;
        private readonly SQLContext _sqlContextCopy;

        private readonly OpenFileDialog _openDialog;
        private readonly SaveFileDialog _saveDialog;

        public bool Modified { get; set; }

        public OfflineModePage(SQLContext context)
        {
            _sqlContext = context;
            _sqlContextCopy = new SQLContext();
            _sqlContextCopy.Assign(context);

            _openDialog = new OpenFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                Title = "Select XML file to load metadata from"
            };

            _saveDialog = new SaveFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                Title = "Select XML file to save metadata to"
            };

            //Modified = false;
            //_queryBuilder = queryBuilder;
            //_syntaxProvider = syntaxProvider;

            //_metadataContainerCopy = new MetadataContainer(queryBuilder.SQLContext);
            //_metadataContainerCopy.Assign(_queryBuilder.MetadataContainer);

            InitializeComponent();

            cbOfflineMode.IsChecked= _sqlContextCopy.LoadingOptions.OfflineMode;

            UpdateMode();

            cbOfflineMode.Checked+= checkOfflineMode_CheckedChanged;
            cbOfflineMode.Unchecked += checkOfflineMode_CheckedChanged;
            bEditMetadata.Click += buttonEditMetadata_Click;
            bSaveToXML.Click += buttonSaveToXML_Click;
            bLoadFromXML.Click += buttonLoadFromXML_Click;
        }

        public void ApplyChanges()
        {
            if (Modified)
            {
                _sqlContextCopy.LoadingOptions.OfflineMode = cbOfflineMode.IsChecked.HasValue &&
                                                                   cbOfflineMode.IsChecked.Value;

                if (_sqlContextCopy.LoadingOptions.OfflineMode)
                {
                    _sqlContextCopy.MetadataProvider?.Disconnect();

                    _sqlContext.Assign(_sqlContextCopy);
                }
                else
                {
                    _sqlContext.MetadataContainer.Items.Clear();
                }
            }
        }

        private void checkOfflineMode_CheckedChanged(object sender, EventArgs e)
        {
            Modified = true;
            UpdateMode();
        }

        private void UpdateMode()
        {
           // lMetadataObjectCount.Font = new Font(lMetadataObjectCount.Font, (cbOfflineMode.IsChecked.HasValue && cbOfflineMode.IsChecked.Value) ? FontStyle.Bold : FontStyle.Regular);
            bLoadFromXML.IsEnabled = cbOfflineMode.IsChecked.HasValue && cbOfflineMode.IsChecked.Value;
            bSaveToXML.IsEnabled = cbOfflineMode.IsChecked.HasValue && cbOfflineMode.IsChecked.Value;
            bEditMetadata.IsEnabled = cbOfflineMode.IsChecked.HasValue && cbOfflineMode.IsChecked.Value;

            UpdateMetadataStats();
        }

        private void UpdateMetadataStats()
        {
            List<MetadataObject> metadataObjects = _sqlContextCopy.MetadataContainer.Items.GetItemsRecursive<MetadataObject>(MetadataType.Objects);
            int t = 0, v = 0, p = 0, s = 0;

            for (var i = 0; i < metadataObjects.Count; i++)
            {
                MetadataObject mo = metadataObjects[i];

                switch (mo.Type)
                {
                    case MetadataType.Table:
                        t++;
                        break;
                    case MetadataType.View:
                        v++;
                        break;
                    case MetadataType.Procedure:
                        p++;
                        break;
                    case MetadataType.Synonym:
                        s++;
                        break;
                }
            }

            var tmp = "Loaded Metadata: {0} tables, {1} views, {2} procedures, {3} synonyms.";
            lMetadataObjectCount.Text = string.Format(tmp, t, v, p, s);
        }

        private void buttonLoadFromXML_Click(object sender, EventArgs e)
        {
            if (_openDialog.ShowDialog() == true)
            {
                _sqlContextCopy.MetadataContainer.ImportFromXML(_openDialog.FileName);
                Modified = true;
                UpdateMetadataStats();
            }
        }

        private void buttonSaveToXML_Click(object sender, EventArgs e)
        {
            if (_saveDialog.ShowDialog() == true)
            {
                _sqlContextCopy.MetadataContainer.ExportToXML(_saveDialog.FileName);
            }
        }

        private void buttonEditMetadata_Click(object sender, EventArgs e)
        {
            if (QueryBuilder.EditMetadataContainer(_sqlContextCopy))
            {
                Modified = true;
            }
        }
    }
}
