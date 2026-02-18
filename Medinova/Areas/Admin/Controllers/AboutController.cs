using Medinova.Models;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    public class AboutController : Controller
    {
        MedinovaContext _context = new MedinovaContext();


        public ActionResult Index()
        {
            var abouts = _context.Abouts.ToList();
            return View(abouts);
        }



    }
}