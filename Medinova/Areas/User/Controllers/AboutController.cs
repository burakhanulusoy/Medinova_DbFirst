using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Medinova.Areas.User.Controllers
{
    public class AboutController : Controller
    {
        // GET: User/About
        public ActionResult Index()
        {
            return View();
        }
    }
}