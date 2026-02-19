using System.Web.Mvc;

namespace Medinova.Areas.Doctor.Controllers
{
    public class AppointmentController : Controller
    {
        // GET: Doctor/Appointment
        public ActionResult Index()
        {
            ViewBag.fullname = Session["fullName"];

            return View();
        }
    }
}