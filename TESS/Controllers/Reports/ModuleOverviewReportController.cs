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
    public class ModuleOverviewReportController : Controller
    {
        /// <summary>
        /// GET: ModuleReport
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var Printable = new List<string> {
                "Article_number",
                "Module",
                "Price_category",
                "System",
                "Classification"
            };

            ViewData.Add("Printable", Printable);
            ViewData.Add("Properties", typeof(view_Module).GetProperties());

            List<String> OrderedCustomerNames = view_Customer.getCustomerNames(System.Web.HttpContext.Current.GetUser().Sign);
            OrderedCustomerNames.Sort();
            ViewData.Add("CustomerNames", OrderedCustomerNames);

            ViewData.Add("Systems", GetAllSystemNames(System.Web.HttpContext.Current.GetUser().Area));
            ViewData.Add("Classification", new List<SelectListItem>());


            this.ViewData["Title"] = "Customer Module Overview Report";

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Pdf()
        {
            string customer = Request["customer"];
            string system = Request["system"];
            string classifications = Request["classifications"];
            string expired = Request["expired"];
            string maxColRowsStr = Request["maxColRows"];

            var systemDic = (List<string>)new JavaScriptSerializer().Deserialize(system, typeof(List<string>));
            var classificationsDic = (List<string>)new JavaScriptSerializer().Deserialize(classifications, typeof(List<string>));
            var expiredBool = (bool)new JavaScriptSerializer().Deserialize(expired, typeof(bool));
            var maxColRows = (int)new JavaScriptSerializer().Deserialize(maxColRowsStr, typeof(int));

            String sortDir = Request["sort"];
            String sortKey = Request["prop"];
            String exportAll = Request["exportAll"];

            var systems = new List<string>();

            List<string> list = generateModuleInfo(customer, systemDic, classificationsDic, expiredBool).Select(s => GetValueFromDictionary(s)).ToList();

            if (list.Count > 0)
            {
                ViewData.Add("Customermodules", list);
            }
            else
            {
                ViewData.Add("Customermodules", new ArrayList());
            }

            var totalModuleList = view_Module.getAllModuleForModuleOverviewReport(expiredBool, systemDic, classificationsDic);
            ViewData.Add("AllModules", totalModuleList);
            ViewData.Add("Area", System.Web.HttpContext.Current.GetUser().Area);
            ViewData.Add("Customer", customer);
            ViewData.Add("MaxColRows", maxColRows);

            //this.ViewData["Title"] = "";
            //this.ViewData["Systems"] = list.Where(dict => dict.ContainsKey("System"))
            //         .Select(dict => dict["System"])
            //         .Distinct()
            //         .ToList();

            ViewAsPdf pdf = null;
            
            if(System.Web.HttpContext.Current.GetUser().Area == "IFO")
            {
                pdf = new ViewAsPdf("Pdf_ifo") { RotativaOptions = new Rotativa.Core.DriverOptions() { PageOrientation = Rotativa.Core.Options.Orientation.Landscape } };
            }
            else
            {
                pdf = new ViewAsPdf("Pdf") { RotativaOptions = new Rotativa.Core.DriverOptions() { PageOrientation = Rotativa.Core.Options.Orientation.Landscape } };
            }

            String headerPath = Server.MapPath("~/Views/CustomerOffer/Header_" + System.Web.HttpContext.Current.GetUser().Sign + ".html").Replace("\\", "/");
            String headerFilePath = "file:///" + headerPath;

            string customSwitches = string.Format("--print-media-type");
            pdf.RotativaOptions.CustomSwitches = customSwitches;
            pdf.RotativaOptions.PageMargins = new Rotativa.Core.Options.Margins(0, 0, 0, 0);

            var user = System.Web.HttpContext.Current.GetUser();

            FileStream hfs = updateReportHeader(headerPath, user);
            hfs.Close();

            return pdf;
            //return View("Pdf"); //felsökningssyfte, petar ut den till en html-vy
        }

        private string GetValueFromDictionary(Dictionary<string, object> s)
        {
            object value;

            s.TryGetValue("Module", out value);

            return !string.IsNullOrEmpty((string)value) ? (string)value : "";
        }

        public List<SelectListItem> GetAllSystemNames(String area)
        {
            IEnumerable<view_Sector> allSectors = view_Sector.getAllSectors()
                 .Where(a => a.Area == area)
                 .DistinctBy(a => a.System)
                 .OrderBy(a => a.SortNo);
            List<SelectListItem> returnList = allSectors.Select(a => new SelectListItem { Value = a.System, Text = a.System }).ToList();

            return returnList;
        }

        List<SelectListItem> GetAllClassificationNames(String area, String system)
        {
            IEnumerable<view_Sector> allSectors = view_Sector.getAllSectors()
                 .Where(a => a.Area == area && a.System == system)
                 .DistinctBy(a => a.Classification)
                 .OrderBy(a => a.Classification == "-");

            List<SelectListItem> returnList = allSectors.Select(a => new SelectListItem { Value = a.Classification, Text = a.Classification }).ToList();

            return returnList;
        }

        public FileStream updateReportHeader(String headerPath, view_User user)
        {
            String headerTxtPath = Server.MapPath("~/Views/CustomerOffer/Header.txt").Replace("\\", "/");
            String content = System.IO.File.ReadAllText(headerTxtPath);
            FileStream fs = new FileStream(headerPath, FileMode.Create, FileAccess.Write);
            content += @"<div style='padding-bottom:30px'> <div class='header-report'>";
            if (user.Use_logo)
            {
                //content += @"<div class='logo-report'>
                //                <img src='../../Content/img/TE-Lockup-RGB-BLUE.png' />
                //            </div>
                content += @"</div>";
            }
            
            //content += @"<div class='header-report-left'><div style='font-family:Arial;font-size: 16px; font-weight:bold'>" +
            //              "<span>Customer Module Report</span>" +
            //           "</div></div>";

            //content += @"<div class='header-report-right'><div style='font-family:Arial;font-size: 16px; font-weight:bold;'>" +
            //              "<span>" + DateTime.Now.ToString("yyyy-MM-dd") + "</span>" +
            //           "</div>";

            content += @"</div></div>
                        </html>
                    ";
            StreamWriter writer = new StreamWriter(fs);
            writer.Write(string.Empty);
            writer.Write(content);

            writer.Close();
            return fs;
        }

        public List<Dictionary<string, object>> generateModuleInfo(string customer, List<string> system, List<string> classifications, bool expired)
        {

            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            if (customer != null)
            {
                List<view_ContractRow> ContractRows = view_ContractRow.GetValidContractRows(customer, system, classifications, expired);
                List<view_Contract> contracts = new List<view_Contract>();

                foreach (view_ContractRow cr in ContractRows)
                {
                    Dictionary<string, object> Customers = new Dictionary<string, object>();
                    if (contracts.FindIndex(m => m.Contract_id == cr.Contract_id) <= 0)
                    {
                        view_Contract c = new view_Contract("Contract_id = '" + cr.Contract_id + "' AND Customer = '" + cr.Customer + "'");
                        contracts.Add(c);
                    }

                    view_Contract contract = contracts[contracts.Count - 1];

                    var module = new view_Module();
                    module.Select("Article_number = " + cr.Article_number);

                    if (contract.Status == "Giltigt")
                    {
                        Customers.Add("Customer", cr.Customer);
                        Customers.Add("Contract_id", cr.Contract_id);
                        Customers.Add("Module", cr.Alias);
                        Customers.Add("Representative", contract.Sign);
                        Customers.Add("System", module.System);
                        Customers.Add("Classification", module.Classification); // module.Classification);

                        rows.Add(Customers);
                    }
                }
            }

            return rows;
        }

        /// <summary>
        /// Searches the modules for the Module Overview Report
        /// </summary>
        /// <returns></returns>
        public string Module()
        {
            try
            {
                string customer = Request.Form["customer"];

                string system = Request.Form["system"];
                string classifications = Request.Form["classifications"];
                string expired = Request["expired"];

                var systemsDic = new List<string>();
                if (system != null)
                {
                    systemsDic.AddRange((List<string>)new JavaScriptSerializer().Deserialize(system, typeof(List<string>)));
                }

                var classificationsDic = new List<string>();
                if (classifications != null)
                {
                    classificationsDic.AddRange((List<string>)new JavaScriptSerializer().Deserialize(classifications, typeof(List<string>)));
                }
                var expiredBool = (bool)new JavaScriptSerializer().Deserialize(expired, typeof(bool));

                return "{\"data\":" + (new JavaScriptSerializer()).Serialize(generateModuleInfo(customer, systemsDic, classificationsDic, expiredBool)) + "}";
            }
            catch
            {
                return "0";
            }
        }

        public string GetClassificationData()
        {
            string system = Request.Form["system"];
            var systemDic = (List<string>)new JavaScriptSerializer().Deserialize(system, typeof(List<string>));

            string Area = System.Web.HttpContext.Current.GetUser().Area;

            List<SelectListItem> returnList = new List<SelectListItem>();



            List<view_Sector> allSectors = view_Sector.getAllSectors().Where(a => systemDic.Contains(a.System) && a.Area == Area).DistinctBy(a => a.Classification).ToList();
            returnList.AddRange(allSectors.Select(a => new SelectListItem { Value = a.Classification, Text = a.Classification }).ToList());

            returnList = returnList.OrderBy(a => a.Value == "-").ToList();

            return new JavaScriptSerializer().Serialize(returnList);
        }
    }
}