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
using System.ComponentModel;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.WPF;
using Microsoft.Win32;

namespace FullFeaturedDemo.PropertiesForm
{
    /// <summary>
    /// Interaction logic for OfflineModePage.xaml
    /// </summary>
    [ToolboxItem(false)]
    public partial class OfflineModePage
    {
        private readonly MetadataContainer _metadataContainerCopy;

        private readonly OpenFileDialog _openDialog;
        private readonly SaveFileDialog _saveDialog;
        private readonly SQLContext _sqlContext;

        public bool Modified { get; set; }

        public OfflineModePage(SQLContext sqlContext)
        {
            _sqlContext = sqlContext;
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

            Modified = false;

            _metadataContainerCopy = new MetadataContainer(_sqlContext);
            _metadataContainerCopy.Assign(_sqlContext.MetadataContainer);

            InitializeComponent();

            cbOfflineMode.IsChecked = _sqlContext.LoadingOptions.OfflineMode;

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
                _sqlContext.LoadingOptions.OfflineMode = cbOfflineMode.IsChecked.HasValue &&
                                                                   cbOfflineMode.IsChecked.Value;

                if (_sqlContext.LoadingOptions.OfflineMode)
                {
                    if (_sqlContext.MetadataProvider != null)
                    {
                        _sqlContext.MetadataProvider.Disconnect();
                    }

                    _sqlContext.MetadataContainer.Assign(_metadataContainerCopy);
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
            List<MetadataObject> metadataObjects = _metadataContainerCopy.Items.GetItemsRecursive<MetadataObject>(MetadataType.Objects);
            int t = 0, v = 0, p = 0, s = 0;

            for (int i = 0; i < metadataObjects.Count; i++)
            {
                MetadataObject mo = metadataObjects[i];

                if (mo.Type == MetadataType.Table)
                {
                    t++;
                }
                else if (mo.Type == MetadataType.View)
                {
                    v++;
                }
                else if (mo.Type == MetadataType.Procedure)
                {
                    p++;
                }
                else if (mo.Type == MetadataType.Synonym)
                {
                    s++;
                }
            }

            var tmp = "Loaded Metadata: {0} tables, {1} views, {2} procedures, {3} synonyms.";
            lMetadataObjectCount.Text = string.Format(tmp, t, v, p, s);
        }

        private void buttonLoadFromXML_Click(object sender, EventArgs e)
        {
            if (_openDialog.ShowDialog() != true) return;

            _metadataContainerCopy.ImportFromXML(_openDialog.FileName);
            Modified = true;
            UpdateMetadataStats();
        }

        private void buttonSaveToXML_Click(object sender, EventArgs e)
        {
            if (_saveDialog.ShowDialog() == true)
            {
                _metadataContainerCopy.ExportToXML(_saveDialog.FileName);
            }
        }

        private void buttonEditMetadata_Click(object sender, EventArgs e)
        {
            if (QueryBuilder.EditMetadataContainer(_metadataContainerCopy, _sqlContext.MetadataStructure, _sqlContext.LoadingOptions))
            {
                Modified = true;
            }
        }
    }
}
