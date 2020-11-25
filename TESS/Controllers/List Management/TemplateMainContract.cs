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
    public class TemplateMainContractController : Controller
    {
        private String htmlMainTemplate = "";
        // GET: Template
        public ActionResult Index()
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
                    MainContractText.Update("rubrik1", topTitle);
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