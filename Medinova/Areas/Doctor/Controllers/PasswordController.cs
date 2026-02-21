using Medinova.DTOs;
using Medinova.Models;
using System.Web.Mvc;

namespace Medinova.Areas.Doctor.Controllers
{
    public class PasswordController : Controller
    {
        MedinovaContext _context = new MedinovaContext();

        // GET: Doctor/Account
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (Session["DoctorId"] == null)
            {
                // "new { area = "" }" kısmı senin o 404 hatanı çözecek olan kısımdır. 
                // Doctor alanından çıkıp ana dizindeki Login'e yönlendirir.
                return RedirectToAction("DoctorLogin", "Account", new { area = "" });
            }

            return View();
        }




        [HttpPost]
        public ActionResult ChangePassword(ChangePassword model)
        {
            if (Session["DoctorId"] == null)
            {
                // "new { area = "" }" kısmı senin o 404 hatanı çözecek olan kısımdır. 
                // Doctor alanından çıkıp ana dizindeki Login'e yönlendirir.
                return RedirectToAction("DoctorLogin", "Account", new { area = "" });
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var docrorId = Session["DoctorId"];

            if (docrorId == null)
            {
                // Session düşmüş, adamı login sayfasına geri gönderelim
                return RedirectToAction("DoctorLogin", "Account", new { area = "" });
            }
            var doctor = _context.Doctors.Find(docrorId);

            if (doctor.Password != model.OldPassword)
            {
                ModelState.AddModelError("", "Mevcut şifreniz hatalı kontrol edin");
                return View(model);

            }




            doctor.UserName = model.UserName;
            doctor.Password = model.NewPassword;

            _context.SaveChanges();
            return RedirectToAction("Index", "Appointment");



        }

    }
}