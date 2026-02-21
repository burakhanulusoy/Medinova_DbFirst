using Medinova.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.User.Controllers
{
    public class TestimonialController : Controller
    {
        MedinovaContext _context=new MedinovaContext();




        // GET: User/Testimonial
        public ActionResult Index()
        {

            var userId = int.Parse(Session["userId"].ToString());
            var userTestimonial=_context.Testimonials.Include(x=>x.User).Include(x=>x.Doctor).Where(x=>x.UserId==userId).ToList();
            return View(userTestimonial);
        
        }


        private void DoctorList()
        {
            var doctors = _context.Doctors.ToList();
            ViewBag.doctorList=(from doctor in doctors
                                select new SelectListItem
                                {
                                    Text=doctor.FullName,
                                    Value=doctor.DoctorId.ToString()


                                }).ToList();
        }



        [HttpGet]
        public ActionResult CreateTestimonial()
        {
            DoctorList();
            return View();
        }

        [HttpPost]
        public ActionResult CreateTestimonial(Testimonial testimonial)
        {
            DoctorList();


            if (Session["userId"] is null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var userId = int.Parse(Session["userId"].ToString());

            var user = _context.Users.Find(userId);

            if(user is null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

          
            DateTime today = DateTime.Today;
            DateTime tomorrow=today.AddDays(1);

            var userTestimonialCountToday = _context.Testimonials
                                           .Count(x => x.UserId == userId && x.CommentDate >= today && x.CommentDate < tomorrow);

            if (userTestimonialCountToday>=2)
            {
                ModelState.AddModelError("", "Bugün en fazla 2 adet şikayet gönderebilirsiniz!");
                return View(testimonial);
            }

            bool IsHaveAppointment = _context.Appointments
                                  .Any(x => x.UserId == userId && x.DoctorId == testimonial.DoctorId);

            if (!IsHaveAppointment)
            {
                ModelState.AddModelError("", "Bu doktora ait randevunuz olmadığı için şikayet edemessiniz.");
                return View(testimonial);

            }


            testimonial.IsRead = false;
            testimonial.UserId = userId;
            testimonial.CommentDate = DateTime.Now;
            _context.Testimonials.Add(testimonial);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult DeleteTestimonial(int id)
        {
            var testimonial = _context.Testimonials.Find(id);
            _context.Testimonials.Remove(testimonial);
            _context.SaveChanges();
            return RedirectToAction("Index");


        }






    }
}