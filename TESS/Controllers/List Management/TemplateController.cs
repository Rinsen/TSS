using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;

namespace TietoCRM.Controllers.List_Management
{
    public class TemplateController : Controller
    {
        private String htmlMainTemplate ="";
        // GET: Template
        public ActionResult Index()
        {
            this.ViewData.Add("ControllerName", "Template");
            this.ViewData.Add("AjaxUrl", "/Template/TemplateJsonData/");
            this.ViewData.Add("TargetUrl", "/Template/Data/");
            this.ViewData.Add("InsertUrl", "/Template/Insert/");
            this.ViewData.Add("PrimaryKey", "SSMA_timestamp");

            this.ViewData["Title"] = "Template";

           /* List<view_TextTemplate> templateList = view_TextTemplate.getAllTextTemplates(0,System.Web.HttpContext.Current.GetUser().Sign);

            ViewData.Add("Templates", templateList);*/

            List<String> properties = typeof(TietoCRM.Models.view_TextTemplate).GetProperties().Where(p =>
                    p.Name == "ID_PK" ||
                    p.Name == "Document_type" ||
                    p.Name == "Short_descr" ||
                    p.Name == "Title").Select(p => p.Name).ToList();
            properties.Insert(0, "#");
            ViewData.Add("Properties", properties);

            view_User all = new view_User();
            all.Name = "Alla";
            all.Sign = "Alla";
            List<view_User> userList = view_User.getAllUsers();
            userList.Add(all);
            ViewData.Add("Users",userList);


            return View();
        }

        public ActionResult MainContractTemplate()
        {
            //List<MainContractText> list = MainContractText.getAllMainContractTexts();
            //TreeToHtml(list);
            //ViewData.Add("Templates", htmlMainTemplate);
            ViewData.Add("TopTitle", MainContractText.GetTitle1());
            ViewData.Add("Epilog", MainContractText.GetEpilog());
            ViewData.Add("Prolog", MainContractText.GetProlog());
            ViewData.Add("ModuleText", MainContractText.GetModuleText());
            return View();
        }

        public String SpecificTemplateData()
        {
            String id_pk = Request.Form["ID"];
            //String sign = Request.Form["sign"];
            //String templateNumber = Request.Form["templateNumber"];

            view_TextTemplate template = new view_TextTemplate();
            template.Select("ID_PK = " + id_pk);
            //template.Select("Sign = '" + sign + "' AND Template_number = " + templateNumber);
            dynamic j = null;

            if(template.Document_type == "Offert")
            {
                j = new
                {
                    Document_type = template.Document_type,
                    Short_descr = template.Short_descr,
                    Title = template.Document_head,
                    Page_head = template.Page_head,
                    Document_foot = template.Document_foot,
                    Document_foot_title = "",
                    Delivery_maint_title = "",
                    Delivery_maint_text = ""
                };
            }
           if( template.Document_type == "Tilläggsavtal" || template.Document_type == "Tjänsteavtal")
            {
                j = new
                {
                    Document_type = template.Document_type,
                    Short_descr = template.Short_descr,
                    Title = template.Title,
                    Page_head = template.Page_head,
                    Document_foot = template.Document_foot,
                    Document_foot_title = template.Document_foot_title,
                    Delivery_maint_title = template.Delivery_maint_title,
                    Delivery_maint_text = template.Delivery_maint_text
                };
            }
           if (template.Document_type == "Modulavslut")
           {
               j = new
               {
                   Document_type = template.Document_type,
                   Short_descr = template.Short_descr,
                   Title = "",
                   Page_head = template.Page_head,
                   Document_foot = "",
                   Document_foot_title = "",
                   Delivery_maint_title = "",
                   Delivery_maint_text = ""
               };
           }

            // Custom JsonSerializer to support HTML chars.
            StringBuilder returnString = new StringBuilder();
            returnString.Append("{");
            foreach (var prop in j.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                returnString.Append("\"" + prop.Name + "\":\"" + prop.GetValue(j, null) + "\",");
                Console.WriteLine("Name: {0}, Value: {1}", prop.Name, prop.GetValue(j, null));
            }
            returnString.Remove(returnString.Length - 1, 1);
            returnString.Append("}");
            
           
            // Replace any carrige return / new lines with a break line instead.
            if((new Regex(@"<\s*([^ >]+)[^>]*>.*?<\s*/\s*\1\s*>").IsMatch(returnString.ToString())))
            {
                return (new Regex("(\r\n|\r|\n)")).Replace(returnString.ToString(), "");
            }
            else
            {
                return (new Regex("(\r\n|\r|\n)")).Replace(returnString.ToString(), "<br>");
            }
        }

        public String TemplateJsonData()
        {
            String sign = Request.Form["sign"];
            //int mode = int.Parse(Request.Form["mode"]);
            this.Response.ContentType = "text/plain";
            List<view_TextTemplate> l;
            //if (mode == 1)
            //{
            //    l = view_TextTemplate.getAllTextTemplates(sign).Where(t => t.Title != null && t.Title != "").ToList();
            //    foreach (view_TextTemplate item in l)
            //    {
            //        if (item.Document_head == null)
            //        {
            //            item.Document_head = item.Title;
            //        }
            //    }
            //}
            //else
            {
                //l = view_TextTemplate.getAllTextTemplates(sign).Where(t => t.Document_head != null && t.Document_type == "Offert").ToList();
                l = view_TextTemplate.getAllTextTemplates(sign).ToList();
            }

            foreach (view_TextTemplate lm in l) {
                if (lm.Document_type == "Offert")
                {
                    lm.Title = lm.Document_head;
                } 

            }
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(l) + "}";
        }

        [HttpPost, ValidateInput(false)]
        public String SaveTemplate()
        {
            try
            {
                String method = Request.Form["method"];
                String data = "";
                if (Request.Form["data"] != "" && Request.Form["data"] != null)
                    data = Request.Form["data"];

                Dictionary<String, Object> dict = null;

                if (method == "update")
                {
                    try
                    {
                        dict = (Dictionary<String, Object>)(new JavaScriptSerializer()).Deserialize(data, typeof(Dictionary<String, Object>));
                    }
                    catch (Exception e)
                    {
                        return "0";
                    }

                    view_TextTemplate template = new view_TextTemplate();
                    template.Select("ID_PK = " + dict["ID"]);
                    //template.Select("Sign = '" + dict["Sign"] + "' AND Template_number = " + dict["Template_number"] + " AND Document_type = " + dict["Document_type"]);

                    foreach (KeyValuePair<String, Object> keyValue in dict)
                    {
                        template.SetValue(keyValue.Key, keyValue.Value);
                    }

                    template.Update("ID_PK = " + dict["ID"]);
                    //template.Update("Sign = '" + dict["Sign"] + "' AND Template_number = " + dict["Template_number"] + " AND Document_type = " + dict["Document_type"]);

                    return "1";
                }
                else if (method == "delete")
                {
                    String id = Request.Form["ID"];
                    //String templateNumber = Request.Form["templateNumber"];
                    //String documenttype = Request["documenttype"];

                    view_TextTemplate template = new view_TextTemplate();
                    template.Delete("ID_PK = " + id);

                    return "1";
                }
                else
                {
                    try
                    {
                        dict = (Dictionary<String, Object>)(new JavaScriptSerializer()).Deserialize(data, typeof(Dictionary<String, Object>));
                    }
                    catch (Exception e)
                    {
                        return "0";
                    }

                    view_TextTemplate template = new view_TextTemplate();

                    List<view_TextTemplate> templates = view_TextTemplate.getAllTextTemplates((String)dict["Sign"]);

                    foreach (KeyValuePair<String, Object> keyValue in dict)
                    {
                        {
                            template.SetValue(keyValue.Key, keyValue.Value);
                        }
                    }
                    if (templates.Count <= 0)
                    {
                        template.Template_number = 1;
                    }
                    else
                    {
                        template.Template_number = templates[templates.Count - 1].Template_number + 1;
                    }
                    

                    template.Insert();

                    return "1";
                }
            }
            catch
            {
                return "-1";
            }
            
        }

        [ValidateInput(false)]
        public String SaveMainContractText()
        {
            try
            {
                String epilog = Request.Form["epilog"];
                String prolog = Request.Form["prolog"];
                String topTitle = Request.Form["title"];
                String moduleText = Request.Form["moduleText"];
               
                try
                {
                    MainContractText.Update("Epilog", epilog);
                    MainContractText.Update("Prolog", prolog);
                    MainContractText.Update("rubrik1",topTitle);
                    MainContractText.Update("ModulText", moduleText);
                }
                catch (Exception e)
                {
                    return "0";
                }
                    

                
                return "1";
            }
            catch
            {
                return "-1";
            }
           
            
        }
        private void TreeToHtml(List<MainContractText> textList)
        {
            foreach (MainContractText item in textList)
            {
                if (item.Type == MainContractText.MainContractType.MainHead)
                {
                    htmlMainTemplate += "<input data-name='" + item.Name + "' type='text' class='form-control main-template-mainhead' value='" + item.Value + "'/>";
                    
                }
                else if (item.Type == MainContractText.MainContractType.Subheading)
                {
                    htmlMainTemplate += "<input data-name='" + item.Name + "' type='text' class='form-control main-template-subheading' value='" + item.Value.Replace("\t", "") + "'/>";
                }
                else if (item.Type == MainContractText.MainContractType.Text)
                {
                    htmlMainTemplate += "<textarea data-name='" + item.Name + "' class='form-control main-template-text'>" + item.Value + "</textarea>";
                }
                if (item.Children.Count > 0)
                {
                    TreeToHtml(item.Children);
                }
            }
        }
    }
}