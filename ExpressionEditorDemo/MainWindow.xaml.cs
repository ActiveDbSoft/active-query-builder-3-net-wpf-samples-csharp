//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Text;
using System.Windows;
using ActiveQueryBuilder.Core;

namespace ExpressionEditorDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
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

            ExpressionEditor.Query = query;
            ExpressionEditor.ActiveUnionSubQuery = query.QueryRoot.FirstSelect();
            ExpressionEditor.Expression = "Person.Address.AddressLine1";
        }
    }
}
