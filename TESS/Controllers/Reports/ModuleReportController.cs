using Rotativa.MVC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Extensions;
using TietoCRM.Models;

namespace TietoCRM.Controllers.Reports
{
    public class ModuleReportController : Controller
    {
        // GET: ModuleReport
        public ActionResult Index()
        {
            TietoCRM.Models.GlobalVariables.Initializer();
            List<view_Module> modules = view_Module.getAllModules();
            modules = modules.Where(m => m.Discount_type == 0 && 
                System.Web.HttpContext.Current.GetUser().IfSameArea(m.Area)).ToList();

            ViewData.Add("Modules", modules);
            //ViewData.Add("Properties", typeof(view_Module).GetProperties());
            this.ViewData["Title"] = "Customer Module Report";

            return View();
        }

        public ActionResult Pdf()
        {
            String articleNumbers = Request["module"];
            var articleNumbersList = (List<int>)new JavaScriptSerializer().Deserialize(articleNumbers, typeof(List<int>));

            String sortDir = Request["sort"];
            String sortKey = Request["prop"];
            String exportAll = Request["exportAll"];

            var customerModulesList = new List<List<Dictionary<string, object>>>();

            var modules = new List<view_Module>();

            if(exportAll == "1") //Export ALL modules to the report...
            {
                //Get all article numbers
                articleNumbersList = view_Module.getAllModules().Select(s => (int)s.Article_number).ToList();
            }

            List<Dictionary<String, object>> list = generateModuleInfo(articleNumbersList);

            foreach (var article in articleNumbersList)
            {
                var customerModules = new List<Dictionary<string, object>>();

                view_Module module = new view_Module();
                module.Select("Article_number=" + article);
                modules.Add(module);

                foreach (var item in list)
                {
                    object moduleName = "";
                    if(item.TryGetValue("Module", out moduleName))
                    {
                        if (moduleName as string == module.Module)
                        {
                            //Match
                            customerModules.Add(item);
                        }
                    }
                }

                if(customerModules.Count > 0)
                {
                    customerModulesList.Add(new SortedByColumnCollection(customerModules, sortDir, sortKey).Collection.ToList());
                }
                else
                {
                    //Add empty entry to handle zero modules
                    customerModulesList.Add(new List<Dictionary<string, object>>());
                }
            }

            if (list.Count != 0)
            {
                ViewData.Add("Customermodules", customerModulesList);//(new SortedByColumnCollection(list, sortDir, sortKey)).Collection);
            }
            else
            {
                ViewData.Add("Customermodules", new ArrayList());
            }

            //this.ViewData["Title"] = module.Module;
            this.ViewData["Modules"] = modules;
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
                          "<span>Customer Module Report</span>" +
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

        public List<Dictionary<String, object>> generateModuleInfo(List<int> articleNumbers)
        {

            List<Dictionary<String, object>> rows = new List<Dictionary<String, object>>();
            if(articleNumbers != null)
            {
                foreach (var articleNumber in articleNumbers)
                {
                    List<view_ContractRow> ContractRows = view_ContractRow.GetValidContractRows(articleNumber);
                    List<view_Contract> contracts = new List<view_Contract>();
                    view_Module module = new view_Module();
                    module.Select("Article_number=" + articleNumber);

                    if (System.Web.HttpContext.Current.GetUser().IfSameArea(module.Area))
                    {
                        foreach (view_ContractRow cr in ContractRows)
                        {
                            Dictionary<String, object> Customers = new Dictionary<String, object>();
                            if (contracts.FindIndex(m => m.Contract_id == cr.Contract_id) <= 0)
                            {
                                view_Contract c = new view_Contract("Contract_id = '" + cr.Contract_id + "' AND Customer = '" + cr.Customer + "'");
                                contracts.Add(c);
                            }
                            view_Contract contract = contracts[contracts.Count - 1];
                            if (contract.Status == "Giltigt")
                            {
                                Customers.Add("Customer", cr.Customer);
                                Customers.Add("Contract_id", cr.Contract_id);
                                Customers.Add("Module", module.Module);
                                Customers.Add("Representative", contract.Sign);
                                Customers.Add("Classification", module.Classification);

                                rows.Add(Customers);
                            }
                        }
                    }
                }
            }

            return rows;
        }

        public String Module()
        {
            try
            {
                String articleNumbers = Request.Form["module"];
                var articleNumbersDic = (List<int>) new JavaScriptSerializer().Deserialize(articleNumbers, typeof(List<int>));

                return "{\"data\":" + (new JavaScriptSerializer()).Serialize(generateModuleInfo(articleNumbersDic)) + "}";
            }
            catch(Exception ex)
            {
                return "0";
            }
        }
        public string ExportExcel()
        {
            String exportAll = Request["exportAll"];
            var articleNumbers = Request["module"];

            if (exportAll == "1") //Export ALL modules to the report...
            {
                //Get all article numbers
                var articleNumbersList = view_Module.getAllModules().Select(s => (int)s.Article_number).ToList();

                foreach (var number in articleNumbersList)
                {
                    articleNumbers += number + ",";
                }

                articleNumbers = articleNumbers.Substring(0, articleNumbers.Length - 1);
            }

            System.Data.DataTable dt = view_ContractRow.ExportValidContractRowsToExcel(articleNumbers, exportAll != null ? true : false);
            TietoCRM.ExportExcel ex = new TietoCRM.ExportExcel();
            try
            {
                return ex.Export(dt, "ModuleReport.xlsx");
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }
    }
}