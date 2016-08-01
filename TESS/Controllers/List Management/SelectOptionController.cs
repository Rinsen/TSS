using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;

namespace TietoCRM.Controllers.List_Management
{
    public static class ListExtensions
    {
        public static List<SelectListItem> ToSelectListItemList<T>(this List<T> collection)
        {
            return collection.Select(a => new SelectListItem
            {
                Value = a.ToString(),
                Text = AddSpacesToSentence(a.ToString().Replace("view_", ""))
            }).ToList(); 
        }

        private static string AddSpacesToSentence(string text, bool preserveAcronyms = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

    }

    public class SelectOptionController : Controller
    {
        // GET: SelectOption
        public ActionResult Index()
        {
            ViewData.Add("SkipProp", new List<String>
            {
                "_ID",
                "Model",
                "Property"
            });

            ViewData.Add("Title", "Select Options");
            ViewData.Add("Properties", typeof(view_SelectOption).GetProperties());
            ViewData.Add("SelectModels", GetAllModels("Model='view_SelectOption' AND Property='Model'"));
            ViewData.Add("ModelNames", GetAllUniqueModels());
            return View();
        }
        public String SelectOptionJsonData()
        {
            this.Response.ContentType = "text/plain";
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(view_SelectOption.getAllSelectOptions()) + "}";
        }

        public String GetTableSelectOptions()
        {
            this.Response.ContentType = "text/plain";
            String model = Request.Form["model"];
            String property = Request.Form["property"];
            if(!String.IsNullOrEmpty(model) || !String.IsNullOrEmpty(property))
            {
                String condition = "Model='" + model + "' AND Property='" + property + "'";
                return "{\"data\":" + (new JavaScriptSerializer()).Serialize(view_SelectOption.getAllSelectOptionsWhere(condition)) + "}";
            }
            else
            {
                return "{\"data\":[]}";
            }
        }

        

        public String GetProperties()
        {
            String model = Request.Form["model"];

            List<SelectListItem> uniqueList = GetAllUniqueSelectListItemList(model);
            if (uniqueList.Count == 0)
            {
                uniqueList.Add(new SelectListItem { Value = "-1", Text = "All Properties are used." });
            }
            return (new JavaScriptSerializer()).Serialize(uniqueList);
        }

        private List<SelectListItem> GetAllUniqueSelectListItemList(String model)
        {
            List<String> ignoreProps = new List<string>
            {
                "id_pk",
                "ssma_timestamp"
            };

            List<PropertyInfo> pList = this.GetAllClassTypes().Where(a => a.Name.Contains(model)).Select(a => a.GetProperties()).First().ToList();
            pList = pList.Where(a => !a.Name.StartsWith("_") && !ignoreProps.Contains(a.Name.ToLower())).ToList();
            List<SelectListItem> returnList = pList.Select(a => new SelectListItem { Value = a.Name, Text = a.Name }).ToList();
            IEnumerable<SelectListItem> existingList = GetSelectOptions(model);
            returnList = returnList.Where(a => !existingList.Any(b => a.Value == b.Value) == true).ToList();

            return returnList;
        }

        private IEnumerable<SelectListItem> GetAllUniqueModels()
        {
 
            IEnumerable<SelectListItem> allModels = this.GetAllModelNames().ToSelectListItemList();
            List<SelectListItem> returnList = new List<SelectListItem>();
            foreach (SelectListItem sli in allModels)
            {
                if (!(GetAllUniqueSelectListItemList(sli.Value).Count == 0))
                    returnList.Add(sli);
            }
            
            return returnList;
        }

        private IEnumerable<SelectListItem> GetAllModels(String condition)
        {
            return view_SelectOption.getAllSelectOptionsWhere(condition).Select(a => new SelectListItem { Value = a.Value, Text = a.Text });
        }

        private IEnumerable<SelectListItem> GetSelectOptions(String model)
        {
            List<view_SelectOption> soList = new List<view_SelectOption>();
            String condition = "Model='" + model + "' AND Property='Property'";
            return view_SelectOption.getAllSelectOptionsWhere(condition).Select(a => new SelectListItem { Value = a.Value, Text = a.Text });
        }
        
        public String GetSelectProperties()
        {
            String model = Request.Form["model"];
            return (new JavaScriptSerializer()).Serialize(this.GetSelectOptions(model));
        }

        public string GetSelectModels()
        {
            return (new JavaScriptSerializer()).Serialize(GetAllModels("Model='view_SelectOption' AND Property='Model'"));
        }


        public String SaveSelectOption()
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

                view_SelectOption so = new view_SelectOption();
                so.Select("ID = " + variables["_ID"]);

                foreach (KeyValuePair<String, object> variable in variables)
                {
                    if (variable.Key != "_ID")
                        so.SetValue(variable.Key, variable.Value);
                }

                so.Update("ID = " + variables["_ID"]);

                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        /// <summary>
        /// Method to ensure that the Select options has a Property row and a Model row.
        /// If not, add them.
        /// </summary>
        /// <param name="viewObject"></param>
        private void InsertMissingRows(view_SelectOption viewObject)
        {
            // Check if the property option exist for the selected view model. If not isert it.
            view_SelectOption so = new view_SelectOption();
            String condition1 = "Model='" + viewObject.Model + "' AND Property='Property' AND Value='Property'";
            so.Select(condition1);
            if (so.Model == null && so.Property == null)
            {
                so.Model = viewObject.Model;
                so.Property = "Property";
                so.Value = "Property";
                so.Text = "(Property)";
                so.Insert();
            }

            // Check if model row for the view exists. If not insert it.
            so = new view_SelectOption();
            String condition2 = "Model='view_SelectOption' AND Property='Model' AND Value='" + viewObject.Model + "'";
            so.Select(condition2);
            if (so.Model == null && so.Property == null)
            {
                so.Model = "view_SelectOption";
                so.Property = "Model";
                so.Value = viewObject.Model;
                so.Text = AddSpacesToSentence(viewObject.Model.Replace("view_", ""));
                so.Insert();
            }
        }

        private string AddSpacesToSentence(string text, bool preserveAcronyms = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        public String InsertSelectOption()
        {
            try
            {
                String json = Request.Form["json"];
                view_SelectOption a = null;
                try
                {
                    a = (view_SelectOption)(new JavaScriptSerializer()).Deserialize(json, typeof(view_SelectOption));
                }
                catch (Exception e)
                {
                    return "0";
                }



                InsertMissingRows(a);

                a.Insert();

                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String DeleteSelectOption()
        {
            try
            {
                String ID = Request.Form["_ID"];
                view_SelectOption a = new view_SelectOption();
                a.Delete("ID = " + ID);
            }
            catch (Exception e)
            {
                return "-1";
            }
            return "1";
        }

        private Type[] GetAllClassTypes()
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(t => String.Equals(t.Namespace, "TietoCRM.Models", StringComparison.Ordinal)).ToArray();
        }

        private List<String> GetAllModelNames()
        {
            List<String> returnList = new List<string>();
            
            foreach (Type classType in this.GetAllClassTypes().Where(y => y.Name.Contains("view_")))
            {
                returnList.Add(classType.Name.Replace("Tieto.CRM.Models.", ""));
            }
            return returnList;
        }


    }
}