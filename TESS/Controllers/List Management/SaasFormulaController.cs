using System;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;

namespace TietoCRM.Controllers.List_Management
{
    public class SaasFormulaController : Controller
    {
        // GET: MiscSettings
        public ActionResult Index()
        {
            ViewBag.Properties = typeof(TietoCRM.Models.view_SaaS_Formula).GetProperties();
            var currentFormula = view_SaaS_Formula.getActiveSaaSFormula();

            currentFormula.LicensePart = currentFormula.LicensePart != null ? currentFormula.LicensePart : 0;
            currentFormula.Factor = currentFormula.Factor != null ? currentFormula.Factor : 0;
            currentFormula.IsActive = currentFormula.IsActive != null ? currentFormula.IsActive : false;

            this.ViewData["SaasFormula"] = currentFormula;

            return View();
        }

        public String SaveSaasFormula()
        {
            try
            {
                String license = Request.Form["license"];
                String factor = Request.Form["factor"];
                String isActive = Request.Form["activate"];

                license = license.Replace(",", ".");
                factor = factor.Replace(",", ".");

                view_SaaS_Formula.SaveSaaSFormula(bool.Parse(isActive), decimal.Parse(license, CultureInfo.InvariantCulture), decimal.Parse(factor, CultureInfo.InvariantCulture));

                return "1";
            }
            catch
            {
                return "-1";
            }
        }

        public string IsSaasFormulaActive()
        {
            var result = "";

            var currentSaasFormula = view_SaaS_Formula.getActiveSaaSFormula();

            if (currentSaasFormula != null) 
            { 
                result = currentSaasFormula._ID > 0 && currentSaasFormula.IsActive == true ? "true" : "false";
            }

            result = new JavaScriptSerializer().Serialize(result);
            return result;
        }
    }
}