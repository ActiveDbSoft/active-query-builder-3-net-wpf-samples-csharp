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
using System.ComponentModel;
using ActiveQueryBuilder.View.WPF;

namespace GeneralAssembly.QueryBuilderProperties
{
    /// <summary>
    /// Interaction logic for PanesVisibilityPage.xaml
    /// </summary>
    [ToolboxItem(false)]
    public partial class PanesVisibilityPage
    {
        private readonly QueryBuilder _queryBuilder;
        public bool Modified { get; set; }

        public PanesVisibilityPage(QueryBuilder qb)
        {
            Modified = false;
            _queryBuilder = qb;

            InitializeComponent();

            cbShowDesignPane.IsChecked = _queryBuilder.PanesConfigurationOptions.DesignPaneVisible;
            cbShowQueryColumnsPane.IsChecked = _queryBuilder.PanesConfigurationOptions.QueryColumnsPaneVisible;
            cbShowDatabaseSchemaView.IsChecked = _queryBuilder.PanesConfigurationOptions.DatabaseSchemaViewVisible;
            cbShowQueryNavigationBar.IsChecked = _queryBuilder.PanesConfigurationOptions.QueryNavigationBarVisible;

            cbShowDesignPane.Checked += Changed;
            cbShowDesignPane.Unchecked += Changed;
            cbShowQueryColumnsPane.Checked += Changed;
            cbShowQueryColumnsPane.Unchecked += Changed;
            cbShowDatabaseSchemaView.Checked += Changed;
            cbShowDatabaseSchemaView.Unchecked += Changed;
            cbShowQueryNavigationBar.Checked += Changed;
            cbShowQueryNavigationBar.Unchecked += Changed;
        }

        public PanesVisibilityPage()
        {
            Modified = false;
            InitializeComponent();
        }

        private void Changed(object sender, EventArgs e)
        {
            if (Equals(sender, cbShowDesignPane))
            {
                if (!(cbShowDesignPane.IsChecked.HasValue && cbShowDesignPane.IsChecked.Value) && !(cbShowQueryColumnsPane.IsChecked.HasValue && cbShowQueryColumnsPane.IsChecked.Value))
                    cbShowQueryColumnsPane.IsChecked = true;
            }
            else if (Equals(sender, cbShowQueryColumnsPane))
            {
                if (!(cbShowDesignPane.IsChecked.HasValue && cbShowDesignPane.IsChecked.Value) && !(cbShowQueryColumnsPane.IsChecked.HasValue && cbShowQueryColumnsPane.IsChecked.Value))
                    cbShowDesignPane.IsChecked = true;
            }

            Modified = true;
        }

        public void ApplyChanges()
        {
            if (Modified)
            {
                _queryBuilder.PanesConfigurationOptions.BeginUpdate();

                try
                {
                    _queryBuilder.PanesConfigurationOptions.DesignPaneVisible = cbShowDesignPane.IsChecked.HasValue && cbShowDesignPane.IsChecked.Value;
                    _queryBuilder.PanesConfigurationOptions.QueryColumnsPaneVisible = cbShowQueryColumnsPane.IsChecked.HasValue && cbShowQueryColumnsPane.IsChecked.Value;
                    _queryBuilder.PanesConfigurationOptions.DatabaseSchemaViewVisible = cbShowDatabaseSchemaView.IsChecked.HasValue && cbShowDatabaseSchemaView.IsChecked.Value;
                    _queryBuilder.PanesConfigurationOptions.QueryNavigationBarVisible = cbShowQueryNavigationBar.IsChecked.HasValue && cbShowQueryNavigationBar.IsChecked.Value;
                }
                finally
                {
                    _queryBuilder.PanesConfigurationOptions.EndUpdate();
                }
            }
        }
    }
}
