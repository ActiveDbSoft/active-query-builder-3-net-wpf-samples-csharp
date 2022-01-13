//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2022 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.DatabaseSchemaView;
using ActiveQueryBuilder.View.WPF;
using GeneralAssembly.Common;

namespace GeneralAssembly.QueryBuilderProperties
{
    /// <summary>
    /// Interaction logic for DatabaseSchemaViewPage.xaml
    /// </summary>
    [ToolboxItem(false)]
    internal partial class DatabaseSchemaViewPage
    {
        private MetadataType _expandMetadataType;
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

            _expandMetadataType = queryBuilder.DatabaseSchemaView.Options.DefaultExpandMetadataType;
            FillComboBox(typeof(MetadataType));
            SetExpandType(queryBuilder.DatabaseSchemaView.Options.DefaultExpandMetadataType);
        }
        private void FillComboBox(Type enumType)
        {
            var flags = GetFlagsFromType(enumType);
            foreach (var flag in flags)
            {
                cmbDefaultExpandLevel.Items.Add(new SelectableItem(flag));
            }
        }

        private void SetExpandType(object value)
        {
            cmbDefaultExpandLevel.ClearCheckedItems();
            var decomposed = DecomposeEnum(value);
            for (int i = 0; i < cmbDefaultExpandLevel.Items.Count; i++)
            {
                if (decomposed.Contains((int)cmbDefaultExpandLevel.Items[i].Content))
                    cmbDefaultExpandLevel.SetItemChecked(i, true);
            }
        }

        private List<int> DecomposeEnum(object value)
        {
            // decomposite enum by degrees of 2
            var binary = Convert.ToString((int)value, 2).Reverse().ToList();
            var result = new List<int>();
            for (int i = 0; i < binary.Count; i++)
            {
                if (binary[i] == '1')
                    result.Add((int)Math.Pow(2, i));
            }

            return result;
        }

        private List<Enum> GetFlagsFromType(Type enumType)
        {
            var values = Enum.GetValues(enumType);
            var result = new List<Enum>();
            foreach (var value in values)
            {
                // filter unity items
                if (IsDegreeOf2((int)value))
                    result.Add((Enum)value);
            }

            return result;
        }

        private bool IsDegreeOf2(int n)
        {
            return n != 0 && (n & (n - 1)) == 0;
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

                databaseSchemaViewOptions.DefaultExpandMetadataType = GetExpandType();
            }
            finally
            {
                databaseSchemaViewOptions.EndUpdate();
            }
        }

        private MetadataType GetExpandType()
        {
            var intValue = (int)_expandMetadataType;

            for (int i = 0; i < cmbDefaultExpandLevel.Items.Count; i++)
            {
                if (cmbDefaultExpandLevel.IsItemChecked(i))
                    intValue |= (int)cmbDefaultExpandLevel.Items[i].Content;
                else
                    intValue &= ~(int)cmbDefaultExpandLevel.Items[i].Content;
            }

            return (MetadataType)intValue;
        }

        private void Changed(object sender, EventArgs e)
        {
            Modified = true;
        }

        private void CmbDefaultExpandLevel_OnItemCheckStateChanged(object sender, EventArgs e)
        {
            _expandMetadataType = GetExpandType();
            Changed(this, EventArgs.Empty);
        }
    }
}
