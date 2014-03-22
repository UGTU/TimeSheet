using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeSheetMvc4WebApplication.ClassesDTO;

namespace TimeSheetMvc4WebApplication.Controllers
{
    [Authorize]
    public class RegisterController : BaseController
    {
        //
        // GET: /Register/
        public ActionResult Index()
        {
            var approver = GetCurrentApprover();
            if (approver == null || approver.GetApproverDepartments() == null || !approver.GetApproverDepartments().Any())
                throw new HttpException(401, "Попытка несанкционированного доступа");
            //var ts= new List<DtoTimeSheet>

            return approver.GetApproverDepartments().Count() > 1 ? View(approver) : View();
        }

        public ActionResult Index1()
        {
            return View();
        }

        //==========    Json    =============================================
        [HttpGet]
        public JsonResult GetApprover()
        {
            return Json(GetCurrentApprover().GetApproverDepartments(), JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        public JsonResult GetTimeSheets(int id)
        {
            var timesheets = Client.GetTimeSheetList(id, 0, true);
            return Json(timesheets, JsonRequestBehavior.AllowGet);
        }
	}
}