using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CommonBase;
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
        //[Authorize(Roles = "TabelAdmin")]
        public ActionResult Index()
        {
            return RedirectToAction("ExceptionDay");
        }

        [Authorize(Roles = "TabelExceptionDaysAdmin")]
        public ActionResult ExceptionDay()
        {
            //CheckIsAdmin();
            return View();
        }

        //[Authorize(Roles = "TabelDepartmentAdmin")]
        [Authorize(Roles = "Сотрудник кадров")]
        public ActionResult DepartmentManagment()
        {
            
            //CheckIsAdmin();
            return View();
        }

        [Authorize(Roles = "Сотрудник кадров")]
        public ActionResult EditLogin()
        {
            //CheckIsAdmin();
            return View();
        }

        [Authorize(Roles = "Сотрудник кадров")]
        public ActionResult EditRegim()
        {
            //CheckIsAdmin();
            return View();
        }
        /// <summary>
        /// Смена режима работы
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Сотрудник кадров")]
        public ActionResult ChangeRegimByCategory()
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
            var timeSheetModel = ModelConstructor.TimeSheetForDepartment(timeSheet, int.MaxValue, 0, 0, int.MaxValue, 0, 0, false);

            //var r = new TimeSheetToDbf(timeSheetModel);
            var r = new TimeSheetToDbf();
            //var fileBytes = r.GenerateDbf(timeSheetModel);
            var fileBytes = r.GenerateDbf(timeSheet);
            const string fileName = "myfile.dbf";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }



        //=================================== Json  ==============================================

        public JsonResult GetExceptionDay()
        {
            //CheckIsAdmin();
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

        public JsonResult GetApproverForDepartment(int id, bool LabAll = false)
        {
            const int idKadrDepartment = 154;
           var approverModel = new
            {
                Id = id,
                Approver1 = Client.GetDepartmentApprover(id, 1),
                Approver2 = Client.GetDepartmentApprover(id, 2),
                Approver3 = Client.GetDepartmentApprover(id, 3),
                DepartmetEmployees = LabAll  ? Client.GetAllEmployees() : Client.GetDepartmentEmployees(id),
                KadrEmployees = Client.GetDepartmentEmployees(idKadrDepartment)
            };
            return Json(approverModel, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDepartmentFactStaffs(int id)
        {
            var factStaffModel = new
            {
                Id = id,
                DepartmentFactStaffs = Client.GetDepartmentFactStaffs(id),
                AllRegimes = Client.GetWorkShedules()
            };
            return Json(factStaffModel, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Возвращает список режимов работы (5, 6, ~)
        /// </summary>
        /// <returns></returns>
        public JsonResult GetAllRegimes()
        {
            return Json(Client.GetWorkShedules(), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Возвращает список категорий персонала
        /// </summary>
        /// <returns></returns
        public JsonResult GetAllCategoryes()
        {
            return Json(Client.GetCategoryes(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveApproverDepartment(int idDepartmen, int approveNumber, int idEmployee, string employeeLogin)
        {
            //todo:Сделать метод проверки и сохранять логин только если он обновлён
            var approveSaveResult = Client.AddApproverForDepartment(idEmployee, idDepartmen, approveNumber);
            var employeeSaveResult = Client.AddEmployeeLogin(idEmployee, employeeLogin);
            
            var result = (approveSaveResult && employeeSaveResult) ? true : false;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveDepartment(DtoDepartment department)
        {
            ////todo:Сделать метод проверки и сохранять логин только если он обновлён
            //var approveSaveResult = Client.AddApproverForDepartment(idEmployee, idDepartmen, approveNumber);
            //var employeeSaveResult = Client.AddEmployeeLogin(idEmployee, employeeLogin);
            //var result = (approveSaveResult && employeeSaveResult) ? true : false;
            var result = Client.UpdateDepartment(department);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Сотрудник кадров")]
        public JsonResult SaveEmployeeLogin(int idEmployee, string employeeLogin)
        {
            if (employeeLogin.EndsWith("@ugtu.net") | string.IsNullOrEmpty(employeeLogin))
            {
                var employeeSaveResult = Client.AddEmployeeLogin(idEmployee, employeeLogin);
                var result = new
                {
                    result = employeeSaveResult,
                    idEmployee = idEmployee
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            return Json(new {result = false, message = "Логин не удовлетворяет требуемой маске."}, JsonRequestBehavior.AllowGet);
            
        }

        [Authorize(Roles = "Сотрудник кадров")]
        public JsonResult SaveEmployeeRegime(int IdFactStaff, int IdWorkShedule, bool isPersonalRegim, int HoursWeek)
        {

                var employeeSaveResult = Client.EditEmployeeRegim(IdFactStaff, IdWorkShedule, isPersonalRegim, HoursWeek);
                var result = new
                {
                    result = employeeSaveResult,
                    idFactStaff = IdFactStaff
                };
                return Json(result, JsonRequestBehavior.AllowGet);
         }


        [Authorize(Roles = "Сотрудник кадров")]
        public JsonResult SaveChangeSummerRegime(int IdCategory, int IdWorkShedule)
        {
            var result = new
            {
                result = Client.EditEmployeeRegimByCategory(IdCategory, IdWorkShedule)
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }

  }
