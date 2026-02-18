using Medinova.DTOs;
using Medinova.Enums;
using Medinova.MailService;
using Medinova.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Controllers
{
    [AllowAnonymous]//bu controllerdaki actionlara herkes erişebilir demek
    public class DefaultController : Controller
    {
       MedinovaContext _context=new MedinovaContext();


        // GET: Default
        public ActionResult Index()
        {
            return View();
        }

        private void loadDropDown()
        {
            var departments = _context.Departments.ToList();
            ViewBag.Departments = (from deparment in departments
                                   select new SelectListItem
                                   {
                                       Text = deparment.Name,
                                       Value = deparment.DepartmentId.ToString()
                                   }).ToList();


            //part2 tarih bugun ve +7gün gellsin diyorum
            var dateList = new List<SelectListItem>();

            for (int i = 0; i < 7; i++)
            {
                var date = DateTime.Now.AddDays(i);
                dateList.Add(new SelectListItem
                {
                    Text = date.ToString("dd.MMMM.dddd"),//19 aralık cuma
                    Value = date.ToString("yyyy-MM-dd")  //database e kaydederken böyle kaydedilirrr..


                });


            }

            ViewBag.DateList = dateList;//tarihleri viewbag ile view a gönderiyoruz



        }



        [HttpGet]
        public PartialViewResult DefaultAppointment()
        {
           

            loadDropDown();
            return PartialView();
        }


        [HttpPost]
        public JsonResult SendVerificationCode(string phoneNumber)
        {
            var user = _context.Users.FirstOrDefault(x => x.PhoneNumber == phoneNumber);

            // 1. Kullanıcı Var mı?
            if (user == null)
            {
                return Json(new { success = false, message = "UserNotFound" });
            }

            // 2. Rastgele 6 Haneli Kod Üret
            Random rnd = new Random();
            string code = rnd.Next(100000, 999999).ToString();

            // 3. Kodu Session'a Kaydet (Sunucu tarafında geçici sakla)
            Session["VerificationCode"] = code;

            // 4. Mail Gönder
            try
            {
                MaiiHelper.SendVerificationCode(user.Email, code);
                return Json(new { success = true, message = "CodeSent" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "MailError" });
            }
        }

        // --- GÜNCELLENEN METOD: RANDEVU KAYDI ---
        [HttpPost]
        public ActionResult DefaultAppointment(Appointment appointment, string userCode)
        {
            // Session'daki kod ile kullanıcının girdiği kodu karşılaştır
            string serverCode = Session["VerificationCode"] as string;

            // Eğer kod boşsa veya uyuşmuyorsa hata dön
            if (string.IsNullOrEmpty(serverCode) || serverCode != userCode)
            {
                return Json(new { success = false, message = "WrongCode" });
            }

            // Kod doğruysa, kullanıcı kontrolünü tekrar yapmaya gerek yok ama garanti olsun diye bakabilirsin
            // Burada artık kullanıcı doğrulanmış demektir.

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            // Güvenlik için Session'ı temizle
            Session.Remove("VerificationCode");

            return Json(new { success = true, message = "Success" });
        }







        //ajax ile çalısırken js kodları ıle calısılır js de jsondur
        public JsonResult GetDoctorsByDepartmentId(int departmentId)
        {
            var doctors = _context.Doctors.Where(x => x.DepartmentId == departmentId).Select(doctor => new SelectListItem
            {
                Text = doctor.FullName,
                Value = doctor.DoctorId.ToString()
            }).ToList();

            return Json(doctors,JsonRequestBehavior.AllowGet);//json dakı izin veriyoruz veri alabilirmiyiz diye
        }



        [HttpPost]
        public JsonResult GetAvailableHours(DateTime selectedDate,int doctorId)
        {
            //doktorun o günkü randevu saatleri datadan çekiyoruz bro
            var bookedTimes=_context.Appointments.Where(x=>x.DoctorId==doctorId && x.AppointmentDate==selectedDate).Select(x=>x.AppointmentTime).ToList();

            var dtoList = new List<AppointmentAvailabilityDto>();


            //enumsdaki times sttaik oldugu için direk ulaşabşliriz
            foreach(var hour in Times.AppointmentsHour)
            {

                var dto = new AppointmentAvailabilityDto();

                dto.Time = hour;

                if(bookedTimes.Contains(hour))
                {
                    //işaretlimi yani doktorun os satte randevusu var mı diye bakıyoruz
                    dto.IsBooked = true;
                }
                else
                {
                    dto.IsBooked = false;
                }
                dtoList.Add(dto);
            }

            return Json(dtoList,JsonRequestBehavior.AllowGet);







        }




    }
}