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
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace GeneralAssembly
{
    public static class Helpers
    {
        private static ICellStyle _cellStyleDateTime;

        public static bool ExportToExcel(DataTable dataTable, string pathToSave)
        {
            try
            {
                if (dataTable == null || string.IsNullOrEmpty(pathToSave)) return false;

                var workbook = new XSSFWorkbook();
                var sheet = (XSSFSheet) workbook.CreateSheet("Result");

                var row = sheet.CreateRow(0);

                foreach (DataColumn column in dataTable.Columns)
                {
                    var cell = row.CreateCell(dataTable.Columns.IndexOf(column));
                    cell.SetCellValue(column.ColumnName);
                }

                for (var i = 0; i < dataTable.Rows.Count; i++)
                {
                    var tableRow = dataTable.Rows[i];

                    var currentRow = sheet.CreateRow(i + 1);

                    var values = tableRow.ItemArray.ToList();

                    foreach (var item in values)
                    {
                        var currentCell = currentRow.CreateCell(values.IndexOf(item));

                        if (DateTime.TryParse(item.ToString(), out var dateValue))
                        {
                            if (_cellStyleDateTime == null)
                            {
                                var format = workbook.CreateDataFormat();
                                _cellStyleDateTime = workbook.CreateCellStyle();
                                _cellStyleDateTime.DataFormat = format.GetFormat("dd/MM/yyyy");
                            }
                            
                            currentCell.SetCellValue(dateValue);
                            currentCell.CellStyle = _cellStyleDateTime;
                        }
                        else if (double.TryParse(item.ToString(), out var doubleValue))
                            currentCell.SetCellValue(doubleValue);
                        else if (int.TryParse(item.ToString(), out var intValue))
                            currentCell.SetCellValue(intValue);
                        else if (item is byte[])
                            InsertImage(workbook, sheet, (byte[]) item, i + 1, values.IndexOf(item));
                        else
                            currentCell.SetCellValue(item.ToString());
                    }
                }

                using (var fs = new FileStream(pathToSave, FileMode.Create, FileAccess.Write))
                    workbook.Write(fs);

                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Export to Excel", MessageBoxButton.OK);
                return false;
            }
        }

        private static void InsertImage(XSSFWorkbook workbook, ISheet sheet, byte[] data, int row, int column)
        {
            try
            {
                int picInd = workbook.AddPicture(data, XSSFWorkbook.PICTURE_TYPE_PNG);
                IDrawing patriarch = sheet.CreateDrawingPatriarch();
                XSSFClientAnchor anchor = new XSSFClientAnchor(0, 0, 0, 0, column, row, column + 1, row + 1)
                {
                    AnchorType = AnchorType.MoveAndResize
                };

                XSSFPicture picture = (XSSFPicture) patriarch.CreatePicture(anchor, picInd);
                //Reset the image to the original size.
                //picture.Resize();   //Note: Resize will reset client anchor you set.

                picture.LineStyle = LineStyle.DashDotGel;
            }
            catch
            {
                //ignore
            }
        }

        public static bool ExportToCSV(DataTable dataTable, string strFilePath)
        {
            if (dataTable == null || string.IsNullOrEmpty(strFilePath)) return false;

            try
            {
                StreamWriter sw = new StreamWriter(strFilePath, false, Encoding.UTF8);
                //headers  
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    sw.Write(dataTable.Columns[i]);
                    if (i < dataTable.Columns.Count - 1)
                    {
                        sw.Write(";");
                    }
                }

                sw.Write(sw.NewLine);
                foreach (DataRow dr in dataTable.Rows)
                {
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        if (!Convert.IsDBNull(dr[i]))
                        {
                            string value = dr[i].ToString();
                            if (value.Contains(';'))
                            {
                                value = $"\"{value}\"";
                                sw.Write(value);
                            }
                            else
                            {
                                sw.Write(dr[i].ToString());
                            }
                        }

                        if (i < dataTable.Columns.Count - 1)
                        {
                            sw.Write(";");
                        }
                    }

                    sw.Write(sw.NewLine);
                }

                sw.Close();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Export to CVS", MessageBoxButton.OK);
                return false;
            }
        }
    }
}
