using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using TietoCRM.Models.Interfaces;

namespace TietoCRM.Models
{
    public class ViewCsvParser<T> where T : SQLBaseClass
    {
        private List<String> propertiesToSkip;
        public List<string> PropertiesToSkip
        {
            get
            {
                return propertiesToSkip;
            }
        }
        private String name;
        public string Name
        {
            get
            {
                return name;
            }
        }

        public ViewCsvParser(String name)
        {
            this.name = name;
            this.propertiesToSkip = new List<string>();
        }

        public ViewCsvParser(String name, List<String> propertiesToSkip)
        {
            this.name = name;
            this.propertiesToSkip = propertiesToSkip;
        }

        private DataTable ConvertToDataTable(IEnumerable<T> data)
        {
            DataTable dt = new DataTable();
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            foreach (PropertyDescriptor prop in props)
            {
                if(!this.propertiesToSkip.Contains(prop.DisplayName))
                    dt.Columns.Add(prop.DisplayName); // header
            }
            foreach (T item in data)
            {
                DataRow dr = dt.NewRow();
                foreach (PropertyDescriptor prop in props)
                {
                    if (!this.propertiesToSkip.Contains(prop.DisplayName))
                        dr[prop.DisplayName] = prop.Converter.ConvertToString(prop.GetValue(item));
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }

        public void WriteExcelWithNPOI(IEnumerable<T> data, String extension = "xlsx")
        {
            DataTable dt = this.ConvertToDataTable(data);
            IWorkbook workbook;

            if (extension == "xlsx")
            {
                workbook = new XSSFWorkbook();
            }
            else if (extension == "xls")
            {
                workbook = new HSSFWorkbook();
            }
            else
            {
                throw new Exception("This format is not supported");
            }

            ISheet sheet1 = workbook.CreateSheet("Sheet 1");

            //make a header row
            IRow row1 = sheet1.CreateRow(0);

            for (int j = 0; j < dt.Columns.Count; j++)
            {

                ICell cell = row1.CreateCell(j);
                String columnName = dt.Columns[j].ToString();
                cell.SetCellValue(columnName);
            }

            //loops through data
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                IRow row = sheet1.CreateRow(i + 1);
                for (int j = 0; j < dt.Columns.Count; j++)
                {

                    ICell cell = row.CreateCell(j);
                    String columnName = dt.Columns[j].ToString();
                    cell.SetCellValue(dt.Rows[i][columnName].ToString());
                }
            }

            using (var exportData = new MemoryStream())
            {
                HttpResponse response = System.Web.HttpContext.Current.Response;
                response.Clear();
                Encoding encoding = Encoding.UTF8;
                response.Charset = encoding.EncodingName;
                response.ContentEncoding = Encoding.Unicode;
                workbook.Write(exportData);
                if (extension == "xlsx") //xlsx file format
                {
                    response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", this.Name + ".xlsx"));
                    response.BinaryWrite(exportData.ToArray());
                }
                else if (extension == "xls")  //xls file format
                {
                    response.ContentType = "application/vnd.ms-excel";
                    response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", this.Name + "ContactNPOI.xls"));
                    response.BinaryWrite(exportData.GetBuffer());
                }
                response.End();
            }
        }
    }
}