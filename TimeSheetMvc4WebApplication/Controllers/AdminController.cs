using System.Web;
using System.Web.Mvc;
using TimeSheetMvc4WebApplication.ClassesDTO;

namespace TimeSheetMvc4WebApplication.Controllers
{
    [Authorize]
    public class AdminController :  BaseController
    {
        //todo:реализовать функцию отображения и редактирования согласователей для структурных подразделений
        //todo:реализовать для всех методов этого контроллера проверку на членство пользователя в группе администраторов
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
            //var result = Client.DeleteExeptionsDay(exceptionDay.IdExceptionDay);
            return Json(Client.GetDepartmentsList(), JsonRequestBehavior.AllowGet);
        }


    }
}
