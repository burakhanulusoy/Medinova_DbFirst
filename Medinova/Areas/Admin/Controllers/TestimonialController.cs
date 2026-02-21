using Medinova.Models;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace Medinova.Areas.Admin.Controllers
{
    public class TestimonialController : Controller
    {

        MedinovaContext _context = new MedinovaContext();
        // GET: Admin/Testimonial
        public ActionResult Index()
        {
            var doctors = _context.Doctors.Include(x=>x.Testimonials).ToList();
            ViewBag.adminName = Session["fullName"];
            ViewBag.adminEmail = Session["mail"];
     
            return View(doctors);
        }
    }
}