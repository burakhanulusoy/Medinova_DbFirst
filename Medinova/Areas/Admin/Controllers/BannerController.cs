using Medinova.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    public class BannerController : Controller
    {
        MedinovaContext _context = new MedinovaContext();

        // GET: Admin/Banner
        public ActionResult Index()
        {
            var banners = _context.Banners.ToList();
            return View(banners);
        }

        public ActionResult DeleteBanner(int id)
        {

            var banner = _context.Banners.Find(id);
            _context.Banners.Remove(banner);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult CreateBanner()
        {
            return View();

        }

        [HttpPost]
        public ActionResult CreateBanner(Banner  banner)
        {
            _context.Banners.Add(banner);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }



        [HttpGet]
        public ActionResult UpdateBanner(int id)
        {

            var banner = _context.Banners.Find(id);
            return View(banner);

        }

        [HttpPost]
        public ActionResult UpdateBanner(Banner banner)
        {

            _context.Entry(banner).State = EntityState.Modified;

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

























    }
}