using Medinova.DTOs;
using Medinova.Enums;
using Medinova.MailService;
using Medinova.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Medinova.Controllers
{
    [AllowAnonymous]//bu controllerdaki actionlara herkes erişebilir demek yessss sir
    public class DefaultController : Controller
    {
       MedinovaContext _context=new MedinovaContext();
        private readonly string GeminiApiKey = ConfigurationManager.AppSettings["GeminiApiKey"];


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
        public JsonResult SendVerificationCode(string phoneNumber,int doctorId,DateTime appointmentDate)
        {
            var user = _context.Users.FirstOrDefault(x => x.PhoneNumber == phoneNumber);

            // 1. Kullanıcı Var mı?
            if (user == null)
            {
                return Json(new { success = false, message = "UserNotFound" });
            }

            bool hasExistingAppointment = _context.Appointments.Any(x =>
        x.UserId == user.UserId &&
        x.DoctorId == doctorId &&
        x.AppointmentDate == appointmentDate);

            if (hasExistingAppointment)
            {
                return Json(new { success = false, message = "AlreadyBooked" });
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

            var user = _context.Users.FirstOrDefault(x => x.PhoneNumber == appointment.PhoneNumber);

            if(user is null)
            {
                return Json(new { success = false, message = "UserNotFound" });
            }

            bool hasExistingAppointment = _context.Appointments.Any(x =>
        x.UserId == user.UserId &&
        x.DoctorId == appointment.DoctorId &&
        x.AppointmentDate == appointment.AppointmentDate);

            if (hasExistingAppointment)
            {
                // Kullanıcı aynı anda iki sekmeden işlem yapmaya çalışırsa diye
                return Json(new { success = false, message = "AlreadyBooked" });
            }



            appointment.UserId = user.UserId;
            appointment.FullName = user.FirstName + " " + user.LastName;
            appointment.Email = user.Email;


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



        public PartialViewResult Header()
        {
            var banner=_context.Banners.OrderByDescending(x=>x.BannerId).FirstOrDefault();
            return PartialView(banner);
        }

        public PartialViewResult AboutUs()
        {
            var about=_context.Abouts.OrderByDescending(x=>x.AboutId).FirstOrDefault();
            return PartialView(about);


        }


        public PartialViewResult Services()
        {
            return PartialView();
        }

        public PartialViewResult MedicalPackets()
        {
            return PartialView();
        }

        public PartialViewResult OurTeams()
        {
            return PartialView();
        }

        public PartialViewResult Testimonils()
        {
            return PartialView();
        }

        public PartialViewResult AskYouAi()
        {
            return PartialView();
        }
       
        
        
        [HttpPost]
        public async Task<JsonResult> AnalyzeDocument(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                return Json(new { error = "Dosya bulunamadı." });

            try
            {
                // 1. Dosyayı Base64'e çevir
                byte[] fileBytes;
                using (var binaryReader = new BinaryReader(file.InputStream))
                {
                    fileBytes = binaryReader.ReadBytes(file.ContentLength);
                }
                string base64File = Convert.ToBase64String(fileBytes);
                string mimeType = file.ContentType;

                // 2. Gemini API İsteğini Hazırla

                // 2. Gemini API İsteğini Hazırla
                string prompt = @"Sen uzman bir tıbbi asistansın. Görevin YALNIZCA kan tahlili sonuçlarını analiz etmektir. 
Gönderilen belge bir kan tahlili raporu DEĞİLSE analizi reddet ve sadece '<div class=""alert alert-danger"">Sadece kan tahlili analizi yapabilirim.</div>' yaz.
Eğer tahlil ise, sonuçları Medinova temasına uygun, Bootstrap 5 HTML formatında, şık ve okunabilir bir şekilde hazırla. Markdown (**, ## vb.) KESİNLİKLE KULLANMA.
Sadece HTML kodunu döndür (başına veya sonuna ```html yazma).
Şu yapıyı kullan:
1. <h4 class=""text-primary mb-3 border-bottom pb-2"">Analiz Özeti</h4> (Genel durumun kısa özeti)
2. Eğer varsa <h5 class=""text-danger mt-4""> Yüksek Çıkan Değerler</h5> (Değerleri, ne anlama geldiğini listele)
3. Eğer varsa <h5 class=""text-info mt-4""> Düşük Çıkan Değerler</h5> (Değerleri, ne anlama geldiğini listele)
4. <h5 class=""text-success mt-4""> Ne Yapmalısınız? (Tavsiyeler)</h5> (Bu değerleri düzeltmek için beslenme, su tüketimi, uyku gibi günlük yaşam önerilerini listele)
5. En alta <div class=""alert alert-warning mt-4""><strong>Doktor Uyarısı:</strong> Bu analiz yapay zeka tarafından bir ön bilgilendirme amacıyla üretilmiştir. Kesin teşhis ve tedavi planı için lütfen sonuçlarınızı uzman bir hekime gösteriniz.</div> ekle.";

                var requestBody = new
                {
                    contents = new[]
                    {
                new
                {
                    parts = new object[]
                    {
                        new { text = prompt },
                        new { inline_data = new { mime_type = mimeType, data = base64File } }
                    }
                }
            }
                };

                string jsonBody = JsonConvert.SerializeObject(requestBody);

                // 3. HttpClient ile Gemini'ye İstek At
                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.1-pro-preview:generateContent?key={GeminiApiKey}";
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(apiUrl, content);
                    string responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        JObject parsedJson = JObject.Parse(responseString);
                        string aiText = parsedJson["candidates"][0]["content"]["parts"][0]["text"].ToString();

                        return Json(new { result = aiText });
                    }
                    else
                    {
                        return Json(new { error = $"Google API Hatası ({response.StatusCode}): {responseString}" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }


    }
}