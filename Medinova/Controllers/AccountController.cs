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
            Session["userId"] = user.UserId;

            if (user.RoleId == 2)
            {
                return RedirectToAction("Index", "About", new { area = "Admin" });
            }else if(user.RoleId==1)
            {
                return RedirectToAction("Index", "About", new { area = "User" });
            }
            
            return View(model);




        }


        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Index", "Default");
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();

        }

        [HttpPost]
        public ActionResult Register(RegisterDto model)
        {

            var userName = _context.Users.Where(x => x.UserName == model.UserName).FirstOrDefault();

            if(userName!=null)
            {
                ModelState.AddModelError("", "*Kullanıcı adı daha önce alınmış");
                return View(model);

            }

            var userPhone=_context.Users.Where(x=>x.PhoneNumber == model.PhoneNumber).FirstOrDefault();

            if(userPhone!=null)
            {
                ModelState.AddModelError("", "*Hesabınız zaten var lütfen kontrol edin.");
                return View(model);
            
            }


            if(!ModelState.IsValid)
            {
            return View(model);

            }


            User newUser = new User()
            {
                FirstName = model.UserName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                UserName = model.UserName,
                Password = model.Password,
                RoleId=1

            };




            _context.Users.Add(newUser);
            _context.SaveChanges();
            return RedirectToAction("Login");







        }




























    }
}