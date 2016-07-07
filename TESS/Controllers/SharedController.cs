using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TietoCRM.Controllers
{
    public class SharedController : Controller
    {
        // GET: Shared
        public ActionResult CurrentUserSection()
        {
            return PartialView("_CurrentUserSection");
        }
    }
}