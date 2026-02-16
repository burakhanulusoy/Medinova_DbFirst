using Medinova.DTOs;
using Medinova.Enums;
using Medinova.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Controllers
{
    public class DefaultController : Controller
    {
       MedinovaContext _context=new MedinovaContext();


        // GET: Default
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public PartialViewResult DefaultAppointment()
        {
            var departments = _context.Departments.ToList();
            ViewBag.Departments = (from deparment in departments
                                   select new SelectListItem
                                   {
                                       Text = deparment.Name,
                                       Value = deparment.DepartmentId.ToString()
                                   }).ToList();



            var dateList = new List<SelectListItem>();

            for(int i=0;i<7;i++)
            {
                var date = DateTime.Now.AddDays(i);
                dateList.Add(new SelectListItem
                {
                    Text = date.ToString("dd.MMMM.dddd"),
                    Value = date.ToString("yyyy-MM-dd")


                });


            }

            ViewBag.DateList = dateList;//tarihleri viewbag ile view a gönderiyoruz




            return PartialView();
        }


        [HttpPost]
        public ActionResult DefaultAppointment(Appointment appointment)
        {
        
            var departments=_context.Departments.ToList();
            ViewBag.Departments = (from deparment in departments
                                   select new SelectListItem
                                   {
                                       Text = deparment.Name,
                                       Value = deparment.DepartmentId.ToString()
                                   }).ToList();


            var doctors = _context.Doctors.ToList();
           






            _context.Appointments.Add(appointment);
            _context.SaveChanges();
            return RedirectToAction("Index");
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

            var bookedTimes=_context.Appointments.Where(x=>x.DoctorId==doctorId && x.AppointmentDate==selectedDate).Select(x=>x.AppointmentTime).ToList();

            var dtoList = new List<AppointmentAvailabilityDto>();

            foreach(var hour in Times.AppointmentsHour)
            {

                var dto = new AppointmentAvailabilityDto();

                dto.Time = hour;

                if(bookedTimes.Contains(hour))
                {
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