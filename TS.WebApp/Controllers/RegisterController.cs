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
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Index()
        {
            return View();
        }

        //==========    Json    =============================================
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public JsonResult GetData(int year, int mounth)
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
                dateString = String.Format("Табеля за {0} г.", date.ToString("MMMM yyyy"))
            };
            return Json(dyn, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public JsonResult AddFakeTimeSheet(int idDep, int year, int mounth)
        {
            var date = new DateTime(year,mounth,1);
            try
            {
                var result = new
                {
                    Result = true,
                    ts = Client.CreateFakeTimeSheet(idDep, date, GetCurrentApprover())
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