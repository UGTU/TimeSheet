using System;
using System.Linq;
using System.Web.Mvc;
using TimeSheetMvc4WebApplication.Models.Register;

namespace TimeSheetMvc4WebApplication.Controllers
{
    /// <summary>
    /// Реестр табелей
    /// </summary>
    [Authorize]
    public class RegisterController : BaseController
    {
        //
        // GET: /Register/
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Index(DateTime? date)
        {
            ViewBag.Date = date ?? DateTime.Now;
            return View();
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult IndexNew(DateTime? date)
        {
            ViewBag.Date = date ?? DateTime.Now;
            return View();
        }

        //==========    Json    =============================================
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public JsonResult GetData1(int year, int mounth)
        {
            var date = new DateTime(year, mounth, 1);
            var departments = GetCurrentApprover().GetApproverDepartments().Select(s=>new DepatrmentModel(s)).ToArray();
            var timesheets = Client.GetTimeSheetListForDepartments(departments.Select(s => s.IdDepartment).ToArray(),date, 0, true);
            foreach (var department in departments)
            {
                department.timesheets =
                    timesheets.Where(w => w.Department.IdDepartment == department.IdDepartment).ToArray();
            }
            var dyn = new
            {
                deps = departments,
                dateString = String.Format("Табеля за {0} г!", date.ToString("MMMM yyyy"))
            };
            return Json(dyn, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public JsonResult GetData(DateTime date)
        {
            date = new DateTime(date.Year,date.Month,1);
            var departments = GetCurrentApprover().GetApproverDepartments().ToArray();
            var timesheets = Client.GetTimeSheetListForDepartments(departments.Select(s=>s.IdDepartment).ToArray(),date, 0, true);
            var dyn = new
            {
                deps = departments,
                ts = timesheets,
                dateString = String.Format("Табеля за {0} г!",date.ToString("MMMM yyyy")) 
            };
            return Json(dyn, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public JsonResult AddFakeTimeSheet(int idDep, DateTime date)
        {
            try
            {
                Client.CreateFakeTimeSheet(idDep,date, GetCurrentApprover());
                var result = new
                {
                    Result = true
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception)
            {
                var result = new
                {
                    Result = false
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public JsonResult DellFakeTimeSheet(int id)
        {
            try
            {
                Client.DelFakeTimeSheet(id);
                var result = new
                {
                    Result = true
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception)
            {
                var result = new
                {
                    Result = false
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
	}
}