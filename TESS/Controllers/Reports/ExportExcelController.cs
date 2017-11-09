using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Runtime.Caching;

namespace TietoCRM.Controllers
{
    public class ExportExcelController : Controller
    {
        // GET: ExportExcel
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public void CacheGet(string gid)
        {
            ObjectCache cache = MemoryCache.Default;
            //CacheItem fsGet = cache.GetCacheItem(cid);
            byte[] fsG = cache.Get(gid) as byte[];
            var fileName = "SentOffers.xlsx";

            using (var mstream = new System.IO.MemoryStream())
            {
                Response.ContentType = "application/vnd.ms-excel";
                Response.AppendHeader("content-disposition", String.Format("{1}; filename={0}", fileName, "attachment"));
                Response.BinaryWrite(fsG);
                Response.End();
            }
        }
    }
}