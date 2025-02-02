//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2025 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Data;
using ActiveQueryBuilder.View.WPF.Annotations;
using FlexCel.Core;
using FlexCel.XlsAdapter;
using Microsoft.Win32;

namespace TmssoftwareExtension
{
    public class FlexCelHelper
    {
        public static void ExportToExcel([NotNull] DataTable dataTable)
        {
            var openDialog = new SaveFileDialog {
                Filter = "(*.xls, *.xlsx)|*.xls;*xlsx", 
                DefaultExt = ".xls"
            };
            var result = openDialog.ShowDialog();

            if (result != true)
                return;

            var xls = new XlsFile(1, TExcelFileFormat.v2019, true);

            xls.SetSheetSelected(1, true);
            var format = TFlxFormat.CreateStandard2007();
            format.Font = new TFlxFont 
            {
                Style = TFlxFontStyles.Italic
            };

            var formatHeader = xls.AddFormat(format);
            
            for(var c = 0; c < dataTable.Columns.Count; c++)
            {
                xls.SetCellValue(1, c + 1, dataTable.Columns[c].ColumnName, formatHeader);
            }

            for (var r = 0; r < dataTable.Rows.Count; r++)
            {
                for (var c = 0; c < dataTable.Columns.Count; c++)
                {
                    var cellValue = dataTable.Rows[r][c];
                    xls.SetCellValue(r + 2, c + 1, cellValue, 0);
                }
            }

            xls.Save(openDialog.FileName);
        }
    }
}
