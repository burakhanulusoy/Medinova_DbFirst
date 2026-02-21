using Medinova.Models;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System;

namespace Medinova.Areas.Admin.Controllers
{
    public class UserController : Controller
    {
        MedinovaContext _context=new MedinovaContext();
        // GET: Admin/User
        public ActionResult Index()
        {
            var users = _context.Users.Include(x=>x.Role).ToList();
            return View(users);
        }

        public  ActionResult DeleteUser(int id)
        {
            var user=_context.Users.Find(id);
            _context.Users.Remove(user);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Detail(int id)
        {
            ViewBag.TotalAppointments = _context.Appointments.Where(x => x.UserId == id).Count();

            ViewBag.FutureAppointments=_context.Appointments.Where(x=>x.UserId == id && x.AppointmentDate>DateTime.Now).Count();

            ViewBag.TotalTestimonials=_context.Testimonials.Where(x=>x.UserId == id).Count();

            var topDoctor = _context.Appointments
      .Where(x => x.UserId == id)               // Sadece bu hastanın randevularını filtrele
      .GroupBy(x => x.Doctor)                   // Gittiği doktorlara göre grupla
      .OrderByDescending(g => g.Count())        // Gidiş sayısına (Count) göre çoktan aza doğru sırala
      .Select(g => g.Key)                       // Grubun anahtarı olan Doktor nesnesini seç
      .FirstOrDefault();                        // En üsttekini (en çok gidileni) al

            // 2. Hastanın hiç randevusu olmama ihtimaline karşı null kontrolü yap (Hata almamak için önemli!)
            if (topDoctor != null)
            {
                ViewBag.TopDoctor = topDoctor.FullName;
            }

            var departmentStats = _context.Appointments
        .Where(x => x.UserId == id)
        .GroupBy(x => x.Doctor.Department.Name) // Burada bölüm adını seçiyoruz
        .Select(g => new
        {
            DepartmentName = g.Key,
            Count = g.Count()
        })
        .ToList();

            // Grafikte göstermek için İsimleri ve Sayıları ayrı ayrı listeler yapıp ViewBag'e atıyoruz.
            ViewBag.ChartLabels = departmentStats.Select(d => d.DepartmentName).ToList();
            ViewBag.ChartData = departmentStats.Select(d => d.Count).ToList();



            return View();
        }


    }
}