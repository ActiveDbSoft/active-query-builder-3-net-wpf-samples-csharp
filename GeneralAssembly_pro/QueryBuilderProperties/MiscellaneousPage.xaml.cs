//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2025 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.ComponentModel;
using ActiveQueryBuilder.View.QueryView;
using ActiveQueryBuilder.View.WPF;

namespace GeneralAssembly.QueryBuilderProperties
{
    /// <summary>
    /// Interaction logic for MiscellaneousPage.xaml
    /// </summary>
    [ToolboxItem(false)]
    public partial class MiscellaneousPage
    {
        private readonly QueryBuilder _queryBuilder;

        public bool Modified { get; set; }

        public MiscellaneousPage(QueryBuilder qb)
        {
            Modified = false;
            _queryBuilder = qb;

            InitializeComponent();

            comboLinksStyle.Items.Add("Simple style");
            comboLinksStyle.Items.Add("MS Access style");
            comboLinksStyle.Items.Add("SQL Server Enterprise Manager style");

            if (_queryBuilder.DesignPaneOptions.LinkStyle == LinkStyle.Simple)
            {
                comboLinksStyle.SelectedIndex = 0;
            }
            else if (_queryBuilder.DesignPaneOptions.LinkStyle == LinkStyle.MSAccess)
            {
                comboLinksStyle.SelectedIndex = 1;
            }
            else if (_queryBuilder.DesignPaneOptions.LinkStyle == LinkStyle.MSSQL)
            {
                comboLinksStyle.SelectedIndex = 2;
            }

            comboLinksStyle.SelectionChanged += Changed;
        }

        public MiscellaneousPage()
        {
            Modified = false;
            InitializeComponent();
        }

        public void ApplyChanges()
        {
            if (Modified)
            {
                switch (comboLinksStyle.SelectedIndex)
                {
                    case 0:
                        _queryBuilder.DesignPaneOptions.LinkStyle = LinkStyle.Simple;
                        break;
                    case 2:
                        _queryBuilder.DesignPaneOptions.LinkStyle = LinkStyle.MSSQL;
                        break;
                    default:
                        _queryBuilder.DesignPaneOptions.LinkStyle = LinkStyle.MSAccess;
                        break;
                }
            }
        }

        private void Changed(object sender, EventArgs e)
        {
            Modified = true;
        }
    }
}
