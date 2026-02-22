using Medinova.DTOs;
using Medinova.Models;
using System.Web.Mvc;

namespace Medinova.Areas.User.Controllers
{
    public class PasswordSettingsController : Controller
    {
        MedinovaContext _context=new MedinovaContext();
        // GET: User/PasswordSettings
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(ChangePassword model)
        {
            if (Session["userId"] == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var userId = Session["userId"];

            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });

            }
            var user = _context.Users.Find(userId);

            if (user.Password != model.OldPassword)
            {
                ModelState.AddModelError("", "Mevcut şifreniz hatalı kontrol edin");
                return View(model);

            }
            user.UserName = model.UserName;
            user.Password = model.NewPassword;
            _context.SaveChanges();
            return RedirectToAction("Login", "Account",new {area=""});

        }

    }
}