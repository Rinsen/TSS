using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Extensions;
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
            //Kontrollera om EDU -> Visa list-vy
            if(System.Web.HttpContext.Current.GetUser().Area == "EDU")
            {
                if (System.Web.HttpContext.Current.GetUser().User_level > 1)
                {
                    return View("Denied");
                }

                List<String> properties = typeof(TietoCRM.Models.view_MainContractTemplate).GetProperties().Where(p =>
                    p.Name == "ID" ||
                    p.Name == "ShortDescription" ||
                    p.Name == "Description" ||
                    p.Name == "TopTitle").Select(p => p.Name).ToList();
                properties.Insert(0, "#");
                ViewData.Add("Properties", properties);

                this.ViewData.Add("ControllerName", "Template");

                return View("MainContractIndex");
            }
            else
            {
                //Om VoO eller IFO -> Visa mall-vy (singel)
                var mainContractTemplate = view_MainContractTemplate.GetMainContractTemplate("1");

                ViewData.Add("TopTitle", mainContractTemplate.TopTitle);
                ViewData.Add("Epilog", mainContractTemplate.Epilog);
                ViewData.Add("Prolog", mainContractTemplate.Prolog);
                ViewData.Add("ModulText", mainContractTemplate.ModulText);

                return View();
            }
        }

        public String SpecificMainContractTemplateData()
        {
            String id = Request.Form["ID"];

            view_MainContractTemplate template = new view_MainContractTemplate();
            template.Select("ID = " + id);

            dynamic j = null;

            j = new
            {
                template.TopTitle,
                template.ShortDescription,
                template.Description,
                template.Prolog,
                template.Epilog,
                ModulText = template.ModulText
            };

            return (new JavaScriptSerializer()).Serialize(j);
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
            
            return (new JavaScriptSerializer()).Serialize(j);
        }

        public String MainContractTemplateJsonData()
        {
            this.Response.ContentType = "text/plain";

            var l = view_MainContractTemplate.getAllMainContractTemplates().ToList();

            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(l) + "}";
        }

        public String TemplateJsonData()
        {
            String sign = Request.Form["sign"];
            //int mode = int.Parse(Request.Form["mode"]);
            this.Response.ContentType = "text/plain";

            var l = view_TextTemplate.getAllTextTemplates(sign).ToList();

            foreach (view_TextTemplate lm in l) 
            {
                if (lm.Document_type == "Offert")
                {
                    lm.Title = lm.Document_head;
                } 
            }

            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(l) + "}";
        }

        [HttpPost, ValidateInput(false)]
        public String SaveMainContractTemplate()
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

                    view_MainContractTemplate template = new view_MainContractTemplate();
                    template.Select("ID = " + dict["Id"]);

                    foreach (KeyValuePair<String, Object> keyValue in dict)
                    {
                        template.SetValue(keyValue.Key, keyValue.Value);
                    }

                    template.Update("ID = " + dict["Id"]);

                    return "1";
                }
                else if (method == "delete")
                {
                    String id = Request.Form["ID"];

                    view_MainContractTemplate template = new view_MainContractTemplate();
                    template.Delete("ID = " + id);

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

                    view_MainContractTemplate template = new view_MainContractTemplate();

                    foreach (KeyValuePair<String, Object> keyValue in dict)
                    {
                        template.SetValue(keyValue.Key, keyValue.Value);
                    }

                    template.Insert();

                    return "1";
                }
            }
            catch (Exception ex)
            {
                return "-1";
            }

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
            catch (Exception ex)
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
                String modulText = Request.Form["modulText"];
                String id = Request.Form["id"];

                try
                {
                    var mainContractTemplate = new view_MainContractTemplate();
                    mainContractTemplate.Select("ID = " + id);

                    mainContractTemplate.Epilog = epilog;
                    mainContractTemplate.Prolog = prolog;
                    mainContractTemplate.TopTitle = topTitle;
                    mainContractTemplate.ModulText = modulText;

                    mainContractTemplate.Update("ID = " + id);
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
    }
}