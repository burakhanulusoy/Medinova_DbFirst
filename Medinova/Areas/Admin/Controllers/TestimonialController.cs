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

        public ActionResult Detail(int id)
        {
            var testimonialOfDoctorById=_context.Testimonials.Where(x=>x.DoctorId == id).ToList();
            return View(testimonialOfDoctorById);
        }

        public ActionResult Read(int id)
        {
            var testimonial = _context.Testimonials.Find(id);
            testimonial.IsRead = true;
            _context.SaveChanges();
            return RedirectToAction("Index");

        }




    }
}