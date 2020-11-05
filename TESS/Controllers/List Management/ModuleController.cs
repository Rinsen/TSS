using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security.AntiXss;
using TietoCRM.Extensions;
using TietoCRM.Models;

namespace TietoCRM.Controllers
{
    public class ModuleController : Controller
    {
        // GET: Module
        public ActionResult Index()
        {
            string amount = this.Request.QueryString["amount"];

            if (amount == "" || amount == null)
                amount = "0";

            this.ViewData.Add("Properties", typeof(TietoCRM.Models.view_Module).GetProperties());

            this.ViewData.Add("ControllerName", "module");

            this.ViewData["Title"] = "Articles";
            //GlobalVariables.MostVisitedSites = this.getMostVisitedSite();

            return View();
        }


        public String ModuleData()
        {
            this.Response.ContentType = "text/plain";
            view_User user = System.Web.HttpContext.Current.GetUser();
            List<view_Module> modules;
            if (user.User_level == 2)
                modules = view_Module.getAllModules(true, 0).Where(d => user.IfSameArea(d.Area)).ToList();
            else
                modules = view_Module.getAllModules(true, 0);

            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(modules) + "}";
        }

        public String ClassificationData()
        {
            String System = Request.Form["System"];
            List<String> classificationList = new List<String>();
            String connectionString = ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {  
                connection.Open();
                String queryTextClassification = @"SELECT Procapita, Indelning FROM V_Procapita WHERE Procapita = @Procapita";
                command.CommandText = queryTextClassification;
                command.Prepare();

                command.Parameters.AddWithValue("@Procapita", System);
                command.ExecuteNonQuery();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        classificationList.Add(reader["indelning"].ToString());
                    }
                }
            }
            string temp = classificationList[0];
            classificationList.Remove(temp);
            classificationList.Add(temp);

            return (new JavaScriptSerializer()).Serialize(classificationList);
        }

        public String ModuleJsonData()
        {
            this.Response.ContentType = "text/plain";
            List<view_Module> l = view_Module.getAllModules().Where(m => System.Web.HttpContext.Current.GetUser().IfSameArea(m.Area)).ToList();
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(l) + "}";
        }

        public List<SelectListItem> GetAllSystems(String area = "")
        {
            if (String.IsNullOrEmpty(area))
            {
                area = System.Web.HttpContext.Current.GetUser().Area;
            }
            List<view_Sector> allSectors = view_Sector.getAllSectors()
                .Where(a => a.Area == area)
                .DistinctBy(a => a.System)
                .OrderBy(a => a.SortNo)
                .ToList();
            return allSectors.Select(a => new SelectListItem { Value = a.System, Text = a.System }).ToList();
        }
        public String GetAllSystemNames()
        {
            String area = "";
            if (!String.IsNullOrEmpty(Request.Form["area"]))
            {
                area = Request.Form["area"];
            }
            return (new JavaScriptSerializer()).Serialize(GetAllSystems(area));
        }

        public List<SelectListItem> GetAllClassifications(String system, String area = "")
        {
            if (String.IsNullOrEmpty(area))
                area = System.Web.HttpContext.Current.GetUser().Area;
            if (String.IsNullOrEmpty(system))
                throw new Exception("No system was provided.");

            List<view_Sector> allSectors = view_Sector.getAllSectors()
                .Where(a => a.System == system && a.Area == area)
                .DistinctBy(a => a.Classification)
                .ToList();
            List<SelectListItem> returnList = allSectors.Select(a => new SelectListItem { Value = a.Classification, Text = a.Classification }).ToList();
            return returnList.OrderBy(a => a.Value == "-").ToList();
        }
        public String GetAllClassificationNames()
        {
            String system = "";
            String area = "";
            if (!String.IsNullOrEmpty(Request.Form["area"]) && !String.IsNullOrEmpty(Request.Form["system"]))
            {
                system = Request.Form["system"];
                area = Request.Form["area"];
            }
            return (new JavaScriptSerializer()).Serialize(GetAllClassifications(system, area));
        }

        /// <summary>
        /// Check if Module does not already exist in database
        /// </summary>
        /// <param name="Article_number">The Article number of the Module to check</param>
        /// <returns>True if not already in database</returns>
        /// <exception cref="System.FormatException">Throws FormatException if Article_number is not parsable as an Integer</exception>
        public String ModuleDoesNotExist(String Article_number)
        {
            int _Article_number;
            if (int.TryParse(Article_number, out _Article_number))
                return (new view_Module()).Select("Article_number = " + _Article_number) ? "false" : "true";
            else
                throw new FormatException("Article_number is not parsable as Integer");
        }

        [HttpPost, ValidateInput(false)]
        public String InsertModule()
        {
            try
            {
                String json = Request.Form["json"];

                Dictionary<String, Object> variables = null;

                try
                {
                    variables = (Dictionary<String, dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(Dictionary<String, dynamic>));
                }
                catch
                {
                    return "0";
                }

                if (Convert.ToBoolean(ModuleDoesNotExist(variables["Article_number"].ToString())))
                {
                    view_Module module = new view_Module();
                    try
                    {
                        variables["Module_type"] = variables["Module_type"].ToString() == "Article" ? "1" : "2";

                        foreach (KeyValuePair<String, object> variable in variables)
                        {
                            module.SetValue(variable.Key, variable.Value);
                        }

                        module.Insert();

                        return "1";
                    }
                    catch (Exception ex)
                    {
                        return "0";
                    }
                }
                else
                {
                    return "2"; // Module already exists in the database.
                }

            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(FormatException))
                    return "-2";
                else
                    return "-1";
            }
            
        }

        [HttpPost, ValidateInput(false)]
        public String SaveModule()
        {
            try
            {
                String oldArtNr = Request.Form["oldArtNr"];

                String json = Request.Form["json"];

                Dictionary<String, Object> variables = null;

                try
                {
                    variables = (Dictionary<String, dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(Dictionary<String, dynamic>));
                }
                catch
                {
                    return "0";
                }

                if (!Convert.ToBoolean(ModuleDoesNotExist(variables["Article_number"].ToString())))
                {
                    view_Module module = new view_Module();
                    module.Select("Article_number = " + oldArtNr);
                    try
                    {
                        foreach (KeyValuePair<String, object> variable in variables)
                        {
                            if(variable.Key == "Module_type")
                            {
                                if(variable.Value.ToString() == "Article")
                                {
                                    module.SetValue(variable.Key, 1);
                                    continue;
                                }
                                else
                                {
                                    module.SetValue(variable.Key, 2);
                                    continue;
                                }
                            }

                            module.SetValue(variable.Key, variable.Value);
                        }

                        module.Update("Article_number = " + oldArtNr);

                        return "1";
                    }
                    catch (Exception ex)
                    {
                        return "0";
                    }
                }
                else
                {
                    return "2"; // Module does not exist, cannot update
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(FormatException))
                    return "-2";
                else
                    return "-1";
            }
            

        }
        public void ExportAsCsv()
        {
            ViewCsvParser<view_Module> vcp = new ViewCsvParser<view_Module>("Modules");
            String Area = System.Web.HttpContext.Current.GetUser().Area; 
            if(Area == "*")
                vcp.WriteExcelWithNPOI(view_Module.getAllModules());
            else
                vcp.WriteExcelWithNPOI(view_Module.getAllModules().Where(a => a.Area == Area).ToList());
        }
        public String GetTinyMCEData()
        {
            string article_no = Request.Form["artnr"];
            view_Module module = new view_Module();
            module.Select("article_number = " + article_no);
            dynamic j = null;

            j = new
            {
                Offer_description = module.Offer_description,
                Contract_description = module.Contract_description
            };

            return (new JavaScriptSerializer()).Serialize(j);
        }
    }
}