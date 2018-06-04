using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClosedXML.Excel;
using System.Data;
using System.Net.Http;
using System.Runtime.Caching;
using System.IO;
using TietoCRM;

namespace TietoCRM
{
    public class ExportExcel
    {
        public string Export(DataTable dt, string fileName)
        {
            //string fn = KnownFolders.GetPath(KnownFolder.Downloads, false) + "\\" + fileName;
            using (XLWorkbook wb = new XLWorkbook())
            {
                HttpResponseMessage res = new HttpResponseMessage();

                wb.Worksheets.Add(dt);
                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                byte[] ms = new byte[] { };

                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    MyMemoryStream.Position = 0;
                    ms = ReadFully(MyMemoryStream);
                }

                using (var mstream = new System.IO.MemoryStream())
                {
                    HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
                    HttpContext.Current.Response.AppendHeader("content-disposition", String.Format("{1}; filename={0}", fileName, "attachment"));
                    HttpContext.Current.Response.BinaryWrite(ms);
                    HttpContext.Current.Response.End();
                }

                //String gd = Guid.NewGuid().ToString();
                //CacheItemPolicy policy = new CacheItemPolicy();
                //policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(30.0);
                //ObjectCache cache = MemoryCache.Default;
                //CacheItem fs = new CacheItem(gd, ms);
                //cache.Set(fs, policy);


                //return gd;
                return "0";
            }
        }
        private static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch
            {
                obj = null;
            }
            finally
            {
                GC.Collect();
            }
        }
    }
}
