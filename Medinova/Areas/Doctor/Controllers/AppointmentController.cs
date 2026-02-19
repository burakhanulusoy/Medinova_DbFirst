using Medinova.Models;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Doctor.Controllers
{
    public class AppointmentController : Controller
    {
        MedinovaContext _context = new MedinovaContext();
        // GET: Doctor/Appointment
        public ActionResult Index()
        {
            if (Session["DoctorId"] == null)
            {
                
                return RedirectToAction("DoctorLogin", "Account");
            }


            ViewBag.fullname = Session["fullName"];
            int doctorId = int.Parse(Session["DoctorId"].ToString());
            var appointmentsOfDoctor = _context.Appointments.Where(x => x.DoctorId == doctorId)
                                                            .OrderBy(x=>x.AppointmentDate)
                                                            .ThenBy(x=>x.AppointmentTime).ToList();
            return View(appointmentsOfDoctor);
        }

        public ActionResult ConfirmYes(int id)
        {
            var appointment = _context.Appointments.Where(x => x.AppointmentId == id).FirstOrDefault();

            appointment.IsActive = true;
            _context.SaveChanges();

            return RedirectToAction("Index");



        }
        public ActionResult ConfirmNo(int id)
        {
            var appointment = _context.Appointments.Where(x => x.AppointmentId == id).FirstOrDefault();

            appointment.IsActive = false;
            _context.SaveChanges();
            return RedirectToAction("Index");



        }








    }
}