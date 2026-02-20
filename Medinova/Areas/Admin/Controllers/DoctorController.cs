using Medinova.Models;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.ML;
using Medinova.DTOs; // Senin DTO'ların olduğu namespace
using System.IO;
using System.Collections.Generic;




namespace Medinova.Areas.Admin.Controllers
{
    public class DoctorController : Controller
    {

        MedinovaContext _context = new MedinovaContext();


        private void GetDoctorDepartment()
        {
            var departments = _context.Departments.ToList();

            ViewBag.departemnts = (from department in departments
                                   select new SelectListItem
                                   {
                                       Text = department.Name,
                                       Value = department.DepartmentId.ToString()



                                   }).ToList();







        }





        // GET: Doctor
        public ActionResult Index()
        {
            var doctors = _context.Doctors.Include(x=>x.Department).AsNoTracking().ToList();

           
          

    
            return View(doctors);
        }

        public ActionResult DeleteDoctor(int id)
        {
            var doctor=_context.Doctors.Find(id);
            _context.Doctors.Remove(doctor);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult CreateDoctor()
        {
            GetDoctorDepartment();
            return View();
        }

        [HttpPost]
        public ActionResult CreateDoctor(Medinova.Models.Doctor doctor,HttpPostedFileBase ImageFile)
        {
            GetDoctorDepartment();
            // Eğer bir dosya seçildiyse ve boyutu 0'dan büyükse
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                // 1. Dosyanın uzantısını al (örn: .jpg, .png)
                string extension = Path.GetExtension(ImageFile.FileName);

                // 2. Aynı isimde dosyalar çakışmasın diye benzersiz bir isim üret
                string uniqueFileName = Guid.NewGuid().ToString() + extension;

                // 3. Dosyanın kaydedileceği dizini belirle (Images klasörüne)
                string path = Path.Combine(Server.MapPath("~/Images/"), uniqueFileName);

                // 4. Resmi sunucuya fiziksel olarak kaydet
                ImageFile.SaveAs(path);

                // 5. Veritabanına kaydedilecek modelin ImageUrl alanına yolu ver
                doctor.ImageUrl = "/Images/" + uniqueFileName;
            }
            else
            {
                // Eğer kullanıcı fotoğraf seçmezse varsayılan bir fotoğraf atayabilirsin
                doctor.ImageUrl = "/Images/default-avatar.png";
            }

            doctor.UserName = "Dr. " + doctor.FullName;
            doctor.Password = (123456).ToString();


           
            _context.Doctors.Add(doctor);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Hekim kaydı başarıyla sisteme eklendi.";
            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult UpdateDoctor(int id)
        {
            GetDoctorDepartment();
            var doctor=_context.Doctors.Find(id);
            return View(doctor);
        }

        [HttpPost]
        public ActionResult UpdateDoctor(Medinova.Models.Doctor doctor, HttpPostedFileBase ImageFile)
        {

            GetDoctorDepartment();

            // Eğer bir dosya seçildiyse ve boyutu 0'dan büyükse
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                // 1. Dosyanın uzantısını al (örn: .jpg, .png)
                string extension = Path.GetExtension(ImageFile.FileName);

                // 2. Aynı isimde dosyalar çakışmasın diye benzersiz bir isim üret
                string uniqueFileName = Guid.NewGuid().ToString() + extension;

                // 3. Dosyanın kaydedileceği dizini belirle (Images klasörüne)
                string path = Path.Combine(Server.MapPath("~/Images/"), uniqueFileName);

                // 4. Resmi sunucuya fiziksel olarak kaydet
                ImageFile.SaveAs(path);

                // 5. Veritabanına kaydedilecek modelin ImageUrl alanına yolu ver
                doctor.ImageUrl = "/Images/" + uniqueFileName;
            }
            else
            {
                // Eğer kullanıcı fotoğraf seçmezse varsayılan bir fotoğraf atayabilirsin
                doctor.ImageUrl = "/Images/default-avatar.png";
            }

            doctor.UserName = "Dr. " + doctor.FullName;
            doctor.Password = (123456).ToString();





            _context.Entry(doctor).State = EntityState.Modified;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult DoctorDetail(int id)
        {
            var appointmentsOfDoctor = _context.Appointments.Where(x => x.DoctorId == id).Include(x => x.Doctor).AsNoTracking().ToList();

            // İstatistikler
            ViewBag.CompAppointments = appointmentsOfDoctor.Where(x => x.IsActive == true).Count();
            ViewBag.ApproveWaiting = appointmentsOfDoctor.Where(x => x.IsActive == null).Count();

            // --- YAPAY ZEKA (TRİYAJ) ENTEGRASYONU BAŞLANGICI ---
            MLContext mlContext = new MLContext();
            string modelPath = Path.Combine(Server.MapPath("~/App_Data"), "TriageModel.zip");

            // Hangi randevunun acil olduğunu tutacağımız sözlük (ID ve Aciliyet Durumu)
            Dictionary<int, bool> urgencyDict = new Dictionary<int, bool>();

            if (System.IO.File.Exists(modelPath))
            {
                // 1. Zekayı zip dosyasından yükle
                ITransformer trainedModel = mlContext.Model.Load(modelPath, out var modelInputSchema);

                // 2. Tahmin motorunu çalıştır
                var predictionEngine = mlContext.Model.CreatePredictionEngine<ComplainData, ComplaintPrediction>(trainedModel);

                foreach (var app in appointmentsOfDoctor)
                {
                    if (string.IsNullOrEmpty(app.Description))
                    {
                        urgencyDict.Add(app.AppointmentId, false);
                        continue;
                    }

                    // 3. Hastanın şikayetini yapay zekaya sor
                    var input = new ComplainData { Description = app.Description };
                    var prediction = predictionEngine.Predict(input);

                    // 4. Sonucu kaydet (True = Acil, False = Normal)
                    urgencyDict.Add(app.AppointmentId, prediction.IsUrgent);
                }
            }
            else
            {
                // Model bulunamazsa hata vermesin diye hepsini normal işaretliyoruz
                foreach (var app in appointmentsOfDoctor)
                {
                    urgencyDict.Add(app.AppointmentId, false);
                }
            }

            // View'da kullanabilmek için ViewBag'e atıyoruz
            ViewBag.UrgencyDict = urgencyDict;
            // --- YAPAY ZEKA ENTEGRASYONU BİTİŞİ ---

            return View(appointmentsOfDoctor);
        }




        [HttpGet]
        public ActionResult TrainTriageModel()
        {
            // 1. ML.NET Context'ini başlatıyoruz (Tüm işlemlerin ana motoru)
            MLContext mlContext = new MLContext();

            // 2. Örnek Eğitim Verisi (Triyaj Veri Seti)
            // Gerçek hayatta bu verileri veritabanından çekersin, şimdilik modeli eğitmek için biz veriyoruz.
            var trainingData = new List<ComplainData>
    {
        // ACİL DURUMLAR (IsUrgent = true)
        new ComplainData { Description = "Göğsümde çok şiddetli ağrı var ve sol kolum uyuşuyor.", IsUrgent = true },
        new ComplainData { Description = "Nefes alamıyorum, boğulacak gibi hissediyorum.", IsUrgent = true },
        new ComplainData { Description = "Başım inanılmaz dönüyor, bilincimi kaybedecek gibiyim.", IsUrgent = true },
        new ComplainData { Description = "Kalbim çok hızlı çarpıyor ve nefesim kesiliyor.", IsUrgent = true },
        new ComplainData { Description = "Babam aniden yere yığıldı ve tepki vermiyor.", IsUrgent = true },
        new ComplainData { Description = "Şiddetli kanamam var, durduramıyorum.", IsUrgent = true },

        // NORMAL / POLİKLİNİK DURUMLARI (IsUrgent = false)
        new ComplainData { Description = "İki gündür hafif öksürüğüm ve burnumda akıntı var.", IsUrgent = false },
        new ComplainData { Description = "Gözümde hafif bir kızarıklık ve kaşıntı oluştu.", IsUrgent = false },
        new ComplainData { Description = "Sırtım ağrıyor, sanırım dünkü maçta kasımı zedeledim.", IsUrgent = false },
        new ComplainData { Description = "Boğazım ağrıyor ve yutkunurken zorlanıyorum.", IsUrgent = false },
        new ComplainData { Description = "Cildimde ufak kırmızı lekeler çıktı.", IsUrgent = false },
        new ComplainData { Description = "Dizim ağrıyor, merdiven çıkarken zorlanıyorum.", IsUrgent = false }
    };

            // 3. Veriyi ML.NET'in anlayacağı IDataView formatına çevir
            IDataView dataView = mlContext.Data.LoadFromEnumerable(trainingData);

            // 4. Öğrenme Hattını (Pipeline) Kuruyoruz
            var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(ComplainData.Description))
                .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: nameof(ComplainData.IsUrgent), featureColumnName: "Features"));

            // 5. YAPAY ZEKAYI EĞİT! (Verileri okur ve patternleri çıkarır)
            var trainedModel = pipeline.Fit(dataView);

            // 6. Eğitilmiş Modeli (Beyni) bir .zip dosyası olarak projenin içine kaydet
            // App_Data klasörü MVC projelerinde veri saklamak için güvenli yerdir.
            string modelPath = Path.Combine(Server.MapPath("~/App_Data"), "TriageModel.zip");

            // Eğer App_Data klasörü yoksa oluştur
            var directory = Path.GetDirectoryName(modelPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            mlContext.Model.Save(trainedModel, dataView.Schema, modelPath);

            // Ekrana başarı mesajı bas
            return Content($"Yapay Zeka Modeli Başarıyla Eğitildi ve Kaydedildi! Dosya Yolu: {modelPath}");
        }




















    }
}