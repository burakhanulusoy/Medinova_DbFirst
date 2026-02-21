using Medinova.DTOs;
using Medinova.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    public class DashboardController : Controller
    {

        MedinovaContext _context=new MedinovaContext();

        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            ViewBag.adminName = Session["fullName"];

            ViewBag.FutureAppointments=_context.Appointments.Where(x=>x.AppointmentDate > DateTime.Now).Count();
            ViewBag.DoctorCount=_context.Doctors.Count();
            ViewBag.userCount=_context.Users.Count();
            ViewBag.departmenCount=_context.Departments.Count();

            DateTime today = DateTime.Today;
            DateTime endDay = today.AddDays(7);

            var upcomingAppointments = _context.Appointments
                 .Include(x => x.Doctor) // İlişkili tabloların varsa dahil et
                .Where(x => x.AppointmentDate >= today && x.AppointmentDate < endDay)
                .OrderBy(x => x.AppointmentDate)
                .ToList();

            var scheduleList = new List<DailyScheduleDto>();

            // 7 gün için döngü oluşturup verileri grupluyoruz
            for (int i = 0; i < 7; i++)
            {
                DateTime currentDate = today.AddDays(i);
                scheduleList.Add(new DailyScheduleDto
                {
                    Date = currentDate,
                    // Sadece o güne ait randevuları filtrele
                    Appointments = upcomingAppointments
                        .Where(x => x.AppointmentDate.Value == currentDate.Date)
                        .ToList()
                });
            }

            // View'a gönderiyoruz
            ViewBag.Schedules = scheduleList;





            var departmentStats = _context.Departments
    .Select(d => new
    {
        DepartmentName = d.Name, // Departman Adı
        DoctorCount = d.Doctors.Count(), // Departmandaki Doktor Sayısı
        // Departmandaki doktorların toplam randevu sayısı
        AppointmentCount = d.Doctors.SelectMany(doc => doc.Appointments).Count()
    })
    .ToList();

            // Bu verileri JavaScript'te kullanabilmek için ayrı ayrı listelere ayırıp ViewBag'e atıyoruz
            ViewBag.DeptNames = departmentStats.Select(d => d.DepartmentName).ToList();
            ViewBag.DeptDoctorCounts = departmentStats.Select(d => d.DoctorCount).ToList();
            ViewBag.DeptAppointmentCounts = departmentStats.Select(d => d.AppointmentCount).ToList();

            // Üstteki kartlar için toplam sayıları da hesaplayalım
            ViewBag.TotalDepts = departmentStats.Count;
            ViewBag.TotalDocs = departmentStats.Sum(d => d.DoctorCount);
            ViewBag.TotalApps = departmentStats.Sum(d => d.AppointmentCount);


            var topDoctors = _context.Doctors
                .OrderByDescending(x => x.Appointments.Count) 
                .Take(6) 
                .ToList();

            return View(topDoctors);
        }
    }
}