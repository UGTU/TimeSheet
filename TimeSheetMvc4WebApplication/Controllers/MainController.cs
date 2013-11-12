using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rotativa;
using Rotativa.Options;
using TimeSheetMvc4WebApplication.ClassesDTO;
using TimeSheetMvc4WebApplication.Models;
using TimeSheetMvc4WebApplication.Models.Main;

namespace TimeSheetMvc4WebApplication.Controllers
{
    [Authorize]
    public class MainController : BaseController
    {
        private const int FirstPaperEmployeeCount = 5;
        private const int LastPaperEmployeeCount = 5;
        private const int PaperEmployeeCount = 8;

        public ActionResult Index()
        {
            var approver = GetCurrentApprover(); 
            if (!approver.DtoApproverDepartments.Any())
                throw new HttpException(401, "Попытка несанкционированного доступа");
            if (approver.DtoApproverDepartments.Count() > 1) return View(approver);
            return RedirectToAction("TimeSheetList", new { id = approver.DtoApproverDepartments.First().IdDepartment });
        }


        public ActionResult TimeSheetList(int id, bool showAll = false)
        {
            var approver = GetCurrentApprover();
            ViewBag.idDepartment = id;
            ViewBag.approver = approver;
            ViewBag.Department = approver.DtoApproverDepartments.First(w => w.IdDepartment == id);
            var timeSheetList = Client.GetEmptyTimeSheetList(id, showAll ? int.MinValue : 12);
            return View(timeSheetList);
        }

        public ActionResult TimeSheetEdit(int id)
        {
            return View(id);
        }

        //======================    Отображение табеля     ====================

        public ActionResult TimeSheetShow(int idTimeSheet)
        {
            if (Client.IsAnyTimeSheetWithTgisId(idTimeSheet))
            {
                return View(idTimeSheet);
            } 
            throw new HttpException(404, "Запрашиваемый табель не обнаружен, табель №" + idTimeSheet);
        }

        public PartialViewResult PartialTimeSheetShow(int idTimeSheet)
        {
            var timeSheet = GetTimeSheetOrThrowException(idTimeSheet);
            var timeSheetModel = ModelConstructor.TimeSheetForDepartment(timeSheet, FirstPaperEmployeeCount,
                LastPaperEmployeeCount,
                PaperEmployeeCount, false);
            return PartialView("PartialView/PartialTimeSheetShow",timeSheetModel);
        }

        [AllowAnonymous]
        public ActionResult TimeSheetPrint(int idTimeSheet)
        {
            var timeSheet = GetTimeSheetOrThrowException(idTimeSheet);
            var timeSheetModel = ModelConstructor.TimeSheetForDepartment(timeSheet, FirstPaperEmployeeCount,
                LastPaperEmployeeCount, PaperEmployeeCount,
                true);
            return View(timeSheetModel);
        }

        [AllowAnonymous]
        public ViewAsPdf TimeSheetPdf(int idTimeSheet)
        {
            var timeSheet = GetTimeSheetOrThrowException(idTimeSheet);
            var timeSheetModel = ModelConstructor.TimeSheetForDepartment(timeSheet, FirstPaperEmployeeCount,
                LastPaperEmployeeCount, PaperEmployeeCount,
                true);
            return new ViewAsPdf("TimeSheetPrint", timeSheetModel) { PageOrientation = Orientation.Landscape};
        }

        //======================    Согласование табеля     ====================
        [HttpGet]
        public ActionResult TimeSheetApprovalNew(int idTimeSheet)
        {
            ApproveViewBagInit(idTimeSheet);
            if (!Client.CanApprove(idTimeSheet, GetUsername())) return View();
            var timeSheetAprovalModel = new TimeSheetAprovalModel
            {
                IdTimeSheet=idTimeSheet,
                ApprovalDate = DateTime.Now,
                ApprovalResult = null,
                Comment = "",
                IdApprover = GetCurrentApprover().DtoApproverDepartments.First().IdApprover
            };
            return View(timeSheetAprovalModel);
        }

        [HttpPost]
        public ViewResult TimeSheetApprovalNew(TimeSheetAprovalModel timeSheetAprovalModel)
        {
            //валидация формы
            if (timeSheetAprovalModel.ApprovalResult != null &&
                (bool) timeSheetAprovalModel.ApprovalResult == false & timeSheetAprovalModel.Comment == null)
                ModelState.AddModelError("Причина не указана",
                    "В случае отклонения табеля необходимо прокомментировать причину!");
            var idTimeSheet = timeSheetAprovalModel.IdTimeSheet;
            if (ModelState.IsValid && Client.CanApprove(idTimeSheet,GetUsername()) && Client.TimeSheetApproval(timeSheetAprovalModel.IdTimeSheet, GetUsername(),
                (bool) timeSheetAprovalModel.ApprovalResult, timeSheetAprovalModel.Comment))
            {
                ApproveViewBagInit(idTimeSheet);
                return View();
            }
            ApproveViewBagInit(idTimeSheet);
            return View(timeSheetAprovalModel);
        }

        private void ApproveViewBagInit(int idTimeSheet)
        {
            ViewBag.IdTimeSheet = idTimeSheet;
            ViewBag.ApproveHistiry = Client.GetTimeSheetApproveHistory(idTimeSheet);
            ViewBag.CurrentApprover = Client.GetNextApproverForTimeSheet(idTimeSheet);
        }

        //======================    Json    ====================================

        public JsonResult GetTimeSheetModelJson(int idTimeSheet)
        {
            var timeSheet = GetTimeSheetOrThrowException(idTimeSheet);
            var dayStatusList = Client.GetDayStatusList();
            var ts = DtoToJsonMapper.TimeSheet(timeSheet, dayStatusList);
            return Json(ts, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateTimeSheetRecotds(JsTimeSheetRecordModel[] records)
        {
            var message = new DtoMessage { Result = Client.EditTimeSheetRecords(records) };
            if (!message.Result)
                message.Message = "При сохранении изменений возникли проблемы. Изменения не сохранены.";
            return Json(message, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEmployeesForTimeSheet(int idDep, int year, int month)
        {
            var date = new DateTime(year, month, 1);
            var empls = Client.GetEmployeesForTimeSheet(idDep, GetUsername(), date, date.AddMonths(1).AddDays(-1));
            return Json(empls, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateTimeSheet(int idDep, int year, int month, IEnumerable<DtoFactStaffEmployee> employees)
        {
            var dateStart = new DateTime(year, month, 1);
            var dateEnd = dateStart.AddMonths(1).AddDays(-1);
            DtoMessage message;
            if (employees != null && employees.Any())
            {
                message = Client.CreateTimeSheetByName(idDep, dateStart, dateEnd, GetUsername(), employees.ToArray());
            }
            else
            {
                message = Client.CreateTimeSheet(idDep, dateStart, dateEnd, GetUsername());
            }
            return Json(message, JsonRequestBehavior.AllowGet);
        }
    }
}
