using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Schema;
using TimeSheetMvc4WebApplication.ClassesDTO;

namespace TimeSheetMvc4WebApplication.Controllers
{
    [Authorize]
    public class RegisterController : BaseController
    {
        //
        // GET: /Register/
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Index(DateTime? date)
        {
            ViewBag.Date = date??DateTime.Now;
            return View();
        }

        //==========    Json    =============================================
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public JsonResult GetData(DateTime date)
        {
            var departments = GetCurrentApprover().GetApproverDepartments().ToArray();
            var timesheets = Client.GetTimeSheetListForDepartments(departments.Select(s=>s.IdDepartment).ToArray(),date, 0, true);
            var dyn = new
            {
                deps = departments,
                ts = timesheets
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