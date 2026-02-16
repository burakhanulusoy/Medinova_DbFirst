using Medinova.Models;
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







    }
}