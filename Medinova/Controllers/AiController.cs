using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Medinova.Controllers
{
    [AllowAnonymous]
    public class AiController : Controller
    {
        // GET: Ai
        public ActionResult Index()
        {
            return View();
        }
    }
}