using Medinova.Models;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web.Mvc;

namespace Medinova.Areas.User.Controllers
{
    public class AppointmentController : Controller
    {
        MedinovaContext _context=new MedinovaContext();
        // GET: User/Appointment
        public ActionResult Index()
        {
            if (Session["userId"] is null)
            {
                return RedirectToAction("Login","Account");

            }

            int userId=(int)Session["userId"];
            ViewBag.userName =Session["fullName"];

            var myAppointments=_context.Appointments.Where(x=>x.UserId==userId).ToList();
            return View(myAppointments);
        }
        public ActionResult GetQueueTicket(int id)
        {
            // 1. Mevcut randevuyu bul
            var currentAppointment = _context.Appointments.FirstOrDefault(x => x.AppointmentId == id);

            if (currentAppointment == null) return HttpNotFound();

            // 2. KRİTİK MANTIK: Sıra Numarası Hesaplama
            // O doktora, o tarihte alınmış ve durumu True olan randevuları çekiyoruz.
            // Saate göre sıralıyoruz.
            var doctorsDailyAppointments = _context.Appointments
                .Where(x => x.DoctorId == currentAppointment.DoctorId
                         && x.AppointmentDate == currentAppointment.AppointmentDate
                         && x.IsActive == true) // Sadece onaylı randevular sıraya girer
                .OrderBy(x => x.AppointmentTime)
                .ToList();

            // Listede bizim randevumuzun indeksini bulup 1 ekliyoruz (Çünkü indeks 0'dan başlar)
            int queueNumber = doctorsDailyAppointments.IndexOf(currentAppointment) + 1;

            // View'a verileri taşıyalım
            ViewBag.QueueNumber = queueNumber;

            return View(currentAppointment);
        }


        public ActionResult DeleteAppointment(int id)
        {

            var appointment = _context.Appointments.Find(id);

            if(appointment!=null)
            {
                _context.Appointments.Remove(appointment);
                _context.SaveChanges();


            }

            return RedirectToAction("Index");


        }

        [HttpGet]
        public ActionResult WhatIsYourProblem(int id)
        {
            var user=_context.Appointments.Find(id);
            return View(user);
        }

        [HttpPost]
        public ActionResult WhatIsYourProblem(int id,string Description)
        {
            var appointments = _context.Appointments.Find(id);
            appointments.Description = Description;
            _context.SaveChanges();
            return RedirectToAction("Index");



        }








    }
}