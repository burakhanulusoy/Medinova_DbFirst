using Medinova.DTOs;
using Medinova.Models;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace Medinova.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        MedinovaContext _context = new MedinovaContext();

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }




        [HttpPost]
        public ActionResult Login(LoginDto model)
        {

            var user = _context.Users.Where(x => x.UserName == model.UserName && x.Password == model.Password).FirstOrDefault();

            if(user is null)
            {
                ModelState.AddModelError("","*Kullanıcı adı veya şifre yanlış.");
                return View(model);
            }
            if(!ModelState.IsValid)
            {
                return View(model);
            }


            FormsAuthentication.SetAuthCookie(user.UserName, false);
            Session["userName"] = user.UserName;
            Session["fullName"] =string.Join(" ", user.FirstName, user.LastName);

            return RedirectToAction("Index", "About",new {area="Admin"});








        }


















    }
}