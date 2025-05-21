using Rotativa.MVC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Extensions;
using TietoCRM.Models;

namespace TietoCRM.Controllers.Reports
{
    /// <summary>
    /// 
    /// </summary>
    public class MissingModuleReportController : Controller
    {
        /// <summary>
        /// GET: ModuleReport
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            TietoCRM.Models.GlobalVariables.Initializer();
            List<view_Module> modules = view_Module.getAllModules();
            modules = modules.Where(m => m.Discount_type == 0 && 
                System.Web.HttpContext.Current.GetUser().IfSameArea(m.Area)).ToList();

            ViewData.Add("Modules", modules);
            ViewData.Add("Users", view_User.getAllUsers());
            ViewData.Add("Area", System.Web.HttpContext.Current.GetUser().Area);
            //ViewData.Add("Properties", typeof(view_Module).GetProperties());
            this.ViewData["Title"] = "Missing Module Report";

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Pdf()
        {
            string articleNumbers = Request["module"];
            string users = Request["user"];
            bool kironly = bool.Parse(Request["kironly"]);
            bool fconly = bool.Parse(Request["fconly"]);

            var articleNumbersList = (List<int>)new JavaScriptSerializer().Deserialize(articleNumbers, typeof(List<int>));
            var usersList = (List<string>)new JavaScriptSerializer().Deserialize(users, typeof(List<string>));

            String sortDir = Request["sort"];
            String sortKey = Request["prop"];
            String exportAll = Request["exportAll"];

            List<MissingModuleReportRow> list = generateModuleInfo(usersList, articleNumbersList, kironly, fconly);

            this.ViewData["Modules"] = list;

            ViewAsPdf pdf = new ViewAsPdf("Pdf");

            String headerPath = Server.MapPath("~/Views/CustomerOffer/Header_" + System.Web.HttpContext.Current.GetUser().Sign + ".html").Replace("\\", "/");
            String headerFilePath = "file:///" + headerPath;

            string customSwitches = string.Format("--print-media-type --header-spacing 3 --header-html \"{0}\"", headerFilePath);
            pdf.RotativaOptions.CustomSwitches = customSwitches;

            var user = System.Web.HttpContext.Current.GetUser();
            FileStream hfs = updateReportHeader(headerPath, user);
            hfs.Close();

            return pdf;
        }

        public FileStream updateReportHeader(String headerPath, view_User user)
        {
            String headerTxtPath = Server.MapPath("~/Views/CustomerOffer/Header.txt").Replace("\\", "/");
            String content = System.IO.File.ReadAllText(headerTxtPath);
            FileStream fs = new FileStream(headerPath, FileMode.Create, FileAccess.Write);
            content += @"<div style='padding-bottom:30px'> <div class='header-report'>";
            if (user.Use_logo)
            {
                content += @"<div class='logo-report'>
                                <img src='../../Content/img/TE-Lockup-RGB-BLUE.png' />
                            </div>
                          </div>";
            }
            
            content += @"<div class='header-report-left'><div style='font-family:Arial;font-size: 16px; font-weight:bold'>" +
                          "<span>Missing Module Report</span>" +
                       "</div></div>";

            content += @"<div class='header-report-right'><div style='font-family:Arial;font-size: 16px; font-weight:bold;'>" +
                          "<span>" + DateTime.Now.ToString("yyyy-MM-dd") + "</span>" +
                       "</div>";

            content += @"</div></div>
                        </html>
                    ";
            StreamWriter writer = new StreamWriter(fs);
            writer.Write(String.Empty);
            writer.Write(content);

            writer.Close();
            return fs;
        }

        public List<MissingModuleReportRow> generateModuleInfo(List<string> users, List<int> articleNumbers, bool kironly, bool fconly)
        {

            List<MissingModuleReportRow> rows = new List<MissingModuleReportRow>();
            if(articleNumbers != null)
            {
                rows = view_Customer.GetMissingModuleCustomerRows(users, articleNumbers, kironly, fconly);                    
            }

            return rows;
        }

        public string Module()
        {
            try
            {
                string articleNumbers = Request.Form["module"];
                string users = Request.Form["user"];
                bool kironly = bool.Parse(Request.Form["kironly"]);
                bool fconly = bool.Parse(Request.Form["fconly"]);

                var articleNumbersDic = (List<int>) new JavaScriptSerializer().Deserialize(articleNumbers, typeof(List<int>));
                var usersDic = (List<string>)new JavaScriptSerializer().Deserialize(users, typeof(List<string>));

                return "{\"data\":" + (new JavaScriptSerializer()).Serialize(generateModuleInfo(usersDic, articleNumbersDic, kironly, fconly)) + "}";
            }
            catch(Exception ex)
            {
                return "0";
            }
        }

        public string ExportExcel()
        {
            var articleNumbers = Request["module"];
            var users = Request["user"];
            bool kironly = bool.Parse(Request["kironly"]);
            bool fconly = bool.Parse(Request["fconly"]);

            var usersDic = new List<string>();
            
            if(!string.IsNullOrEmpty(users))
            {
                usersDic = users.Split(',').ToList();
            }

            System.Data.DataTable dt = view_Customer.ExportMissingModuleRowsToExcel(usersDic, articleNumbers, kironly, fconly);
            TietoCRM.ExportExcel ex = new TietoCRM.ExportExcel();
            try
            {
                return ex.Export(dt, "MissingModuleReport.xlsx");
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }
    }
}