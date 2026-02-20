using Medinova.Models;
using System.Data.Entity;
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


        public ActionResult DeleteAbout(int id)
        {
            var about = _context.Abouts.Find(id);
            _context.Abouts.Remove(about);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult CreateAbout()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateAbout(About about)
        {
            _context.Abouts.Add(about);
            _context.SaveChanges();
            return RedirectToAction("Index");

        }

        [HttpGet]
        public ActionResult UpdateAbout(int id)
        {
            var about = _context.Abouts.Find(id);
            return View(about);
        }

        [HttpPost]
        public ActionResult UpdateAbout(About about)
        {

            _context.Entry(about).State = EntityState.Modified;
            _context.SaveChanges();
            return RedirectToAction("Index");

        }










    }
}