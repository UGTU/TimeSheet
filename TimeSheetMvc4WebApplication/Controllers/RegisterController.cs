using System;
using System.Collections.Generic;
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
        public ActionResult Index(DateTime? date)
        {
            ViewBag.Date = date??DateTime.Now;
            return View();
        }

        //==========    Json    =============================================
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
	}
}