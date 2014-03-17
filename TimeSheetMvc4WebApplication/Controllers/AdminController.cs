using System.Web;
using System.Web.Mvc;
using TimeSheetMvc4WebApplication.ClassesDTO;
using TimeSheetMvc4WebApplication.Hubs;
using TimeSheetMvc4WebApplication.Models;
using TimeSheetMvc4WebApplication.Source;

namespace TimeSheetMvc4WebApplication.Controllers
{
    [Authorize]
    public class AdminController :  BaseController
    {
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            return RedirectToAction("ExceptionDay");
        }

        public ActionResult ExceptionDay()
        {
            CheckIsAdmin();
            return View();
        }

        public ActionResult DepartmentManagment()
        {
            CheckIsAdmin();
            return View();
        }

        [Authorize(Roles = "ADtoKadr")]
        public ActionResult EditLogin()
        {
            //CheckIsAdmin();
            return View();
        }

        public string SendNotice(string username)
        {
            //===
            var noticeHub = NoticeHub.Instance;
            noticeHub.Test(username);
            //===
            return string.IsNullOrWhiteSpace(username) ? "notice send" : "notice send to " + username;
        }

        public FileResult Download()
        {

            var timeSheet = GetTimeSheetOrThrowException(576);
            var timeSheetModel = ModelConstructor.TimeSheetForDepartment(timeSheet, int.MaxValue,0,0, false);

            //var r = new TimeSheetToDbf(timeSheetModel);
            var r = new TimeSheetToDbf();
            //var fileBytes = r.GenerateDbf(timeSheetModel);
            var fileBytes = r.GenerateDbf(timeSheet);
            const string fileName = "myfile.dbf";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        private void CheckIsAdmin()
        {
            var approver = GetCurrentApprover();
            if(!approver.IsAdministrator)
                throw new HttpException(401, "Попытка несанкционированного доступа к админке");
        }


        //=================================== Json  ==============================================

        public JsonResult GetExceptionDay()
        {
            CheckIsAdmin();
            var exceptionDay = new
            {
                WorkScheduleList = Client.GetWorkScheduleList(),
                ExceptionDayList = Client.GetExeptionsDays(),
                DayStatusList = Client.GetDayStatusList(),
                CurrentExceptionDay = new DtoExceptionDay()
            };
            return Json(exceptionDay, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddOrEditExceptoinDay(DtoExceptionDay exceptionDay)
        {
            var result= exceptionDay.IdExceptionDay != int.MinValue
                ? Client.EditExeptionsDay(exceptionDay)
                : Client.InsertExeptionsDay(exceptionDay);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Remove(DtoExceptionDay exceptionDay)
        {
            var result = Client.DeleteExeptionsDay(exceptionDay.IdExceptionDay);  
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDepartment(DtoExceptionDay exceptionDay)
        {
            return Json(Client.GetDepartmentsList(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetApproverForDepartment(int id)
        {
            const int idKadrDepartment = 154;
            var approverModel = new
            {
                Id = id,
                Approver1 = Client.GetDepartmentApprover(id, 1),
                Approver2 = Client.GetDepartmentApprover(id, 2),
                Approver3 = Client.GetDepartmentApprover(id, 3),
                DepartmetEmployees = Client.GetDepartmentEmployees(id),
                KadrEmployees = Client.GetDepartmentEmployees(idKadrDepartment)
            };
            return Json(approverModel, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveApproverDepartment(int idDepartmen, int approveNumber, int idEmployee, string employeeLogin)
        {
            //todo:Сделать метод проверки и сохранять логин только если он обновлён
            var approveSaveResult = Client.AddApproverForDepartment(idEmployee, idDepartmen, approveNumber);
            var employeeSaveResult = Client.AddEmployeeLogin(idEmployee, employeeLogin);
            var result = (approveSaveResult && employeeSaveResult) ? true : false;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "ADtoKadr")]
        public JsonResult SaveEmployeeLogin(int idEmployee, string employeeLogin)
        {
            var employeeSaveResult = Client.AddEmployeeLogin(idEmployee, employeeLogin);
            var result = new
            {
                result = employeeSaveResult,
                idEmployee = idEmployee
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}
