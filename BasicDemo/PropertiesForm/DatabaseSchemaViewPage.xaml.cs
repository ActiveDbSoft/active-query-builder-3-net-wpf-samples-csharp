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
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using ActiveQueryBuilder.View.DatabaseSchemaView;
using ActiveQueryBuilder.View.WPF;

namespace BasicDemo.PropertiesForm
{
    /// <summary>
    /// Interaction logic for DatabaseSchemaViewPage.xaml
    /// </summary>
    [ToolboxItem(false)]
    public partial class DatabaseSchemaViewPage
    {
        private readonly QueryBuilder _queryBuilder;
        public bool Modified { get; set; }


        public DatabaseSchemaViewPage(QueryBuilder queryBuilder)
        {
            Modified = false;
            _queryBuilder = queryBuilder;

            InitializeComponent();

            cbGroupByServers.IsChecked = queryBuilder.MetadataStructure.Options.GroupByServers;
            cbGroupByDatabases.IsChecked = queryBuilder.MetadataStructure.Options.GroupByDatabases;
            cbGroupBySchemas.IsChecked = queryBuilder.MetadataStructure.Options.GroupBySchemas;
            cbGroupByTypes.IsChecked = queryBuilder.MetadataStructure.Options.GroupByTypes;
            cbShowFields.IsChecked = queryBuilder.MetadataStructure.Options.ShowFields;

            cmbSortObjectsBy.Items.Add("Sort by Name");
            cmbSortObjectsBy.Items.Add("Sort by Type, Name");
            cmbSortObjectsBy.Items.Add("No sorting");
            cmbSortObjectsBy.SelectedIndex = (int)queryBuilder.DatabaseSchemaViewOptions.SortingType;

            cmbDefaultExpandLevel.Text = queryBuilder.DatabaseSchemaViewOptions.DefaultExpandLevel.ToString(CultureInfo.InvariantCulture);

            cbGroupByServers.Checked += Changed;
            cbGroupByServers.Unchecked += Changed;
            cbGroupByDatabases.Checked += Changed;
            cbGroupByDatabases.Unchecked += Changed;
            cbGroupBySchemas.Checked += Changed;
            cbGroupBySchemas.Unchecked += Changed;
            cbGroupByTypes.Checked += Changed;
            cbGroupByTypes.Unchecked += Changed;
            cbShowFields.Checked += Changed;
            cbShowFields.Unchecked += Changed;
            cmbSortObjectsBy.SelectionChanged += Changed;
            cmbDefaultExpandLevel.SelectionChanged += Changed;
        }

        public DatabaseSchemaViewPage()
        {
            Modified = false;
            InitializeComponent();
        }

        public void ApplyChanges()
        {
            if (!Modified) return;

            var metadataStructureOptions = _queryBuilder.MetadataStructure.Options;
            metadataStructureOptions.BeginUpdate();

            try
            {
                metadataStructureOptions.GroupByServers = cbGroupByServers.IsChecked.HasValue && cbGroupByServers.IsChecked.Value;
                metadataStructureOptions.GroupByDatabases = cbGroupByDatabases.IsChecked.HasValue && cbGroupByDatabases.IsChecked.Value;
                metadataStructureOptions.GroupBySchemas = cbGroupBySchemas.IsChecked.HasValue && cbGroupBySchemas.IsChecked.Value;
                metadataStructureOptions.GroupByTypes = cbGroupByTypes.IsChecked.HasValue && cbGroupByTypes.IsChecked.Value;
                metadataStructureOptions.ShowFields = cbShowFields.IsChecked.HasValue && cbShowFields.IsChecked.Value;
            }
            finally
            {
                metadataStructureOptions.EndUpdate();
            }

            var databaseSchemaViewOptions = _queryBuilder.DatabaseSchemaViewOptions;
            databaseSchemaViewOptions.BeginUpdate();

            try
            {
                _queryBuilder.DatabaseSchemaViewOptions.SortingType = (ObjectsSortingType)cmbSortObjectsBy.SelectedIndex;

                int defaultExpandLevel;
                if (int.TryParse(cmbDefaultExpandLevel.Text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out defaultExpandLevel))
                    _queryBuilder.DatabaseSchemaViewOptions.DefaultExpandLevel = defaultExpandLevel;
            }
            finally
            {
                databaseSchemaViewOptions.EndUpdate();
            }
        }

        private void Changed(object sender, EventArgs e)
        {
            Modified = true;
        }

        private void CmbDefaultExpandLevel_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Modified = true;
        }
    }
}
