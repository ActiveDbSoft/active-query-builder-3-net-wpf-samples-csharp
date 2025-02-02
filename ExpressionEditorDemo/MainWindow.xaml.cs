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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ActiveQueryBuilder.Core;

namespace ExpressionEditorDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine("Select Person.Address.AddressLine1,");
            sqlBuilder.AppendLine("Person.StateProvince.IsOnlyStateProvinceFlag");
            sqlBuilder.AppendLine("From Person.Address as CC");
            sqlBuilder.AppendLine("  Inner Join Person.StateProvince On Person.Address.StateProvinceID =");
            sqlBuilder.AppendLine("    Person.StateProvince.StateProvinceID");
            sqlBuilder.AppendLine("  Inner Join Sales.SalesTaxRate On Person.StateProvince.StateProvinceID =");
            sqlBuilder.AppendLine("    Sales.SalesTaxRate.StateProvinceID");

            var sqlContext = new SQLContext { SyntaxProvider = new MSSQLSyntaxProvider() };
            sqlContext.MetadataContainer.LoadingOptions.OfflineMode = true;
            sqlContext.MetadataContainer.ImportFromXML("AdventureWorks2014.xml");
            var query = new SQLQuery(sqlContext) { SQL = sqlBuilder.ToString() };

            EditorControl.Query = query;
            EditorControl.ActiveUnionSubQuery = query.QueryRoot.FirstSelect();
            EditorControl.Expression = "Person.Address.AddressLine1";
        }
    }
}
