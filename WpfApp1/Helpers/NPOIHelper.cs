using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace ERad5TestGUI.Helpers
{
    public class NPOIHelper
    {
        public static void ReadExcel(string filePath)
        {
            IWorkbook workBook = null;
            using (FileStream fsRead = new FileStream(filePath, FileMode.Open))
            {
                //创建工作薄
                string extension = System.IO.Path.GetExtension(filePath);
                if (extension.Equals(".xls"))
                    workBook = new HSSFWorkbook(fsRead);
                else
                    workBook = new XSSFWorkbook(fsRead);
            }
            if (workBook == null)
                return;
            //获取Sheet
            ISheet sheet = workBook.GetSheetAt(0);

            for (int i = 0; i < sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                //row.GetCell()
            }
        }

        public static IWorkbook GetWorkbookByExcel(string filePath)
        {
            IWorkbook workBook = null;
            using (FileStream fsRead = new FileStream(filePath, FileMode.Open))
            {
                //创建工作薄
                string extension = System.IO.Path.GetExtension(filePath);
                if (extension.Equals(".xls"))
                    workBook = new HSSFWorkbook(fsRead);
                else
                    workBook = new XSSFWorkbook(fsRead);
            }
            return workBook;
        }

        public static void WriteExcel(string path, IWorkbook workbook)
        {
            using (FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(file);
            }
        }

        public static XSSFCellStyle GetCellStyle(IWorkbook wb)
        {
            XSSFCellStyle redcell = (XSSFCellStyle)wb.CreateCellStyle();
            //redcell.FillForegroundColor = IndexedColors.Red.Index;
            redcell.SetFillForegroundColor(new XSSFColor(System.Drawing.Color.Red));
            redcell.FillPattern = FillPattern.SolidForeground; 
            redcell.Alignment = HorizontalAlignment.Center;
            //redcell.FillBackgroundColor = IndexedColors.Red.Index;

            return redcell;
        }

        ///// <summary>
        ///// 初始化DataTable
        ///// </summary>
        ///// <returns></returns>
        //private DataTable InitDataTable()
        //{
        //    DataTable dt_excel = new DataTable();
        //    dt_excel.Columns.Add("A");
        //    dt_excel.Columns.Add("B");
        //    dt_excel.Columns.Add("C");
        //    dt_excel.Columns.Add("D");
        //    return dt_excel;
        //}
    }

    /// <summary>
    /// NPOI 设置Excel 填充色（自定义的Html 格式的色值）
    /// 参考文献：https://www.codota.com/code/java/methods/org.apache.poi.hssf.usermodel.HSSFWorkbook/getCustomPalette
    /// https://stackoverflow.com/questions/10528516/poi-setting-cell-background-to-a-custom-color
    /// https://www.cnblogs.com/mq0036/p/9835975.html
    /// https://stackoverflow.com/questions/22687901/custom-color-for-icellstyle-fillforegroundcolor-than-provided-named-colors
    /// </summary>
    public class WorkBook
    {
        public static void CreateExcelFile2007(List<string> toFillColors)
        {
            var wb = new XSSFWorkbook();

            var sheet = wb.CreateSheet("第一页");

            for (int i = 0; i < toFillColors.Count; i++)
            {

                var row = sheet.CreateRow(i);

                var cell = row.CreateCell(0);
                cell.SetCellValue("第一页第一行");

                //设置单元格样式
                XSSFCellStyle cellStyle = (XSSFCellStyle)wb.CreateCellStyle();

                var itemColorStr = toFillColors[i];
                Color co = System.Drawing.ColorTranslator.FromHtml(itemColorStr);

                XSSFColor xssfColor = new XSSFColor(co);
                cellStyle.SetFillForegroundColor(xssfColor);
                cellStyle.FillPattern = FillPattern.SolidForeground;

                cell.CellStyle = cellStyle;
            }
            //save

            string savePath = $"Demo-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff")}.xlsx";

            using (FileStream fs = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                wb.Write(fs);

                fs.Close();
            }
        }

        public static void CreateExcelFile2003(List<string> toFillColors)
        {
            var wb = new HSSFWorkbook();
            HSSFPalette palette = wb.GetCustomPalette();
            var sheet = wb.CreateSheet("第一页");

            for (int i = 0; i < toFillColors.Count; i++)
            {

                var row = sheet.CreateRow(i);

                var cell = row.CreateCell(0);
                cell.SetCellValue("第一页第一行");

                //设置单元格样式
                ICellStyle cellStyle = wb.CreateCellStyle();

                var itemColorStr = toFillColors[i];
                Color co = System.Drawing.ColorTranslator.FromHtml(itemColorStr);
                HSSFColor myColor = palette.FindColor(co.R, co.G, co.B);
                if (null == myColor)
                {
                    //最多支持 56个设置区间
                    //参考：https://www.cnblogs.com/yxhblog/p/6225018.html
                    short idx = (short)(8 + i);
                    if (idx >= 64)
                    {
                        throw new Exception("colors  max  size is : 56 ");
                    }

                    palette.SetColorAtIndex(idx, co.R, co.G, co.B);
                    myColor = palette.FindColor(co.R, co.G, co.B);
                }

                cellStyle.FillForegroundColor = myColor.Indexed;
                cellStyle.FillPattern = FillPattern.SolidForeground;


                cell.CellStyle = cellStyle;
            }

            //save
            string savePath = $"Demo-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff")}.xls";

            using (FileStream fs = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                wb.Write(fs);

                fs.Close();
            }


        }

    }
}



