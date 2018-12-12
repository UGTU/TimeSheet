using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeSheetMvc4WebApplication.ClassesDTO;
using TimeSheetMvc4WebApplication.Models;
using TimeSheetMvc4WebApplication.Models.Main;

namespace TimeSheetMvc4WebApplication.Controllers
{
    [Authorize]
    public class MainController : BaseController
    {
        private const int TimeSheetsPerPage = 12;
        private const int DepartmentsPerPage = 16;

        public ActionResult Index(int page=1)
        {
            //throw new HttpException(401, "Попытка несанкционированного доступа");
            var approver = GetCurrentApprover();
            ViewBag.approver = approver;
            if (approver==null || approver.GetApproverDepartments() == null || !approver.GetApproverDepartments().Any())
                throw new HttpException(401, "В информационной системе табельного учёта за вами не закреплено ни одного структурного подразделения");
            if (approver.GetApproverDepartments().Count() <= 1)
                return RedirectToAction("TimeSheetList",
                    new {id = approver.GetApproverDepartments().First().IdDepartment});
            var skip = page > 1 ? DepartmentsPerPage * (page - 1) : 0;
            ViewBag.DepartmentsPageCount = (int)Math.Ceiling(approver.DtoApproverDepartments.Count() / (Decimal)DepartmentsPerPage);
            ViewBag.CurrentPage = page;
            ViewBag.Departments = approver.DtoApproverDepartments.Skip(skip).Take(DepartmentsPerPage);
            return View(approver);
        }

        public ActionResult TimeSheetList(int id,int page=1, TimeSheetFilter filter = TimeSheetFilter.All)
        {
            var approver = GetCurrentApprover();
            var skip = page>1?TimeSheetsPerPage*(page - 1):0;
            var r = Provider.GetTimeSheetList(id, filter, skip, TimeSheetsPerPage);
            ViewBag.TimeSheetCount = (int)Math.Ceiling(r.Count / (double)TimeSheetsPerPage);
            ViewBag.idDepartment = id;
            ViewBag.approver = approver;
            ViewBag.Filter = filter;
            ViewBag.CuttentPage = page;
            try
            {
                ViewBag.Department = approver.DtoApproverDepartments.First(w => w.IdDepartment == id);
            }
            catch (System.Exception e)
            {
                return RedirectToNotFoundPage("Запрашиваемый отдел не обнаружен. Отдел " + id);
            }
            
            var timeSheetList = r.TimeSheets.OrderBy(o=>o.DateBegin);
            return View(timeSheetList);
        }

        public ActionResult TimeSheetEdit(int id)
        {
            if (!Client.IsAnyTimeSheetWithThisId(id)) return RedirectToNotFoundPage("Запрашиваемый табель не обнаружен. Табель №"+id);
            return View(id);
        }

        //======================    Согласование табеля     ====================

        [HttpGet]
        public ActionResult TimeSheetApprovalNew(int idTimeSheet)
        {
            if (!Client.IsAnyTimeSheetWithThisId(idTimeSheet)) return RedirectToNotFoundPage("Запрашиваемый табель не обнаружен. Табель №"+idTimeSheet);
            ApproveViewBagInit(idTimeSheet);
            if (!Client.CanApprove(idTimeSheet, GetCurrentApprover())) return View();
            var approveStep = Client.GetTimeSheetApproveStep(idTimeSheet);
            var timeSheet = Client.GetTimeSheet(idTimeSheet, true);
            var currentApprover =
                GetCurrentApprover()
                    .GetDepartmentApproverNumbers(timeSheet.Department.IdDepartment)
                        .First(w => w.ApproveNumber == approveStep + 1);
            var timeSheetAprovalModel = new TimeSheetAprovalModel
            {
                IdTimeSheet = idTimeSheet,
                ApprovalDate = DateTime.Now,
                ApprovalResult = null,
                Comment = "",
                IdApprover = currentApprover.IdApprover
            };
            return View(timeSheetAprovalModel);
        }

        [HttpPost]
        public ActionResult TimeSheetApprovalNew(TimeSheetAprovalModel timeSheetAprovalModel)
        {
            //валидация формы
            var appDominUrl = Url.Action("Index", null, null, Request.Url.Scheme);
            if (timeSheetAprovalModel.ApprovalResult != null &&
                (bool) timeSheetAprovalModel.ApprovalResult == false & timeSheetAprovalModel.Comment == null)
                ModelState.AddModelError("Причина не указана",
                    "В случае отклонения табеля необходимо прокомментировать причину!");
            var idTimeSheet = timeSheetAprovalModel.IdTimeSheet;
            if (ModelState.IsValid && Client.CanApprove(idTimeSheet, GetCurrentApprover()) && Client.TimeSheetApproval(timeSheetAprovalModel.IdTimeSheet, GetCurrentApprover(),
                (bool)timeSheetAprovalModel.ApprovalResult, timeSheetAprovalModel.Comment, appDominUrl))
            {
                return RedirectToAction("TimeSheetApprovalNew", new {idTimeSheet = timeSheetAprovalModel.IdTimeSheet});
            }
            ApproveViewBagInit(idTimeSheet);
            return View(timeSheetAprovalModel);
        }

        private void ApproveViewBagInit(int idTimeSheet)
        {
            ViewBag.IdTimeSheet = idTimeSheet;
            ViewBag.ApproveHistiry = Client.GetTimeSheetApproveHistory(idTimeSheet);
            ViewBag.CurrentApprover = Client.GetNextApproverForTimeSheet(idTimeSheet);
            ViewBag.IsAdvance = Client.IsAdvanceTimesheet(idTimeSheet);
        }

        //=======================================================================================================

        public FileResult Download()
        {
            const string buffer = @"~\Content\Manual\Руководство пользователя ИС Табель.pdf"; //bytes form this
            return File(buffer, "application/pdf"); 
        }

        //======================    Json    ====================================

        public JsonResult GetTimeSheetModelJson(int idTimeSheet)
        {
            var timeSheet = GetTimeSheetOrThrowException(idTimeSheet);
            var dayStatusList = Client.GetDayStatusList().Where(w=>w.IsVisible).ToArray();
            var ts = DtoToJsonMapper.TimeSheet(timeSheet, dayStatusList);
            return Json(ts, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateTimeSheetRecotds(JsTimeSheetRecordModel[] records)
        {
            var r = records.Select(record => new DtoTimeSheetRecord
            {IdTimeSheetRecord = record.IdTimeSheetRecord, JobTimeCount = record.JobTimeCount,
                NightTimeCount = (record.NightTimeCount > 0)? record.NightTimeCount.ToString() : "" ,
                DayStays = new DtoDayStatus {IdDayStatus = record.IdDayStatus}}).ToList();
            var message = new DtoMessage { Result = Client.EditTimeSheetRecords(r.ToArray()) };
            if (!message.Result)
                message.Message = "При сохранении изменений возникли проблемы. Изменения не сохранены.";
            return Json(message, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEmployeesForTimeSheet(int idDep, int year, int month)
        {
            var date = new DateTime(year, month, 1);
            var empls = Client.GetEmployeesForTimeSheet(idDep, GetCurrentApprover(), date, date.AddMonths(1).AddDays(-1));
            foreach (var empl in empls)
            {
                empl.WorkShedule.WorkSheduleName = empl.WorkShedule.WorkSheduleName.Split(' ').First();
            }
            var model = new
            {
                Employees = empls,
                Year = year,
                Month = month
            };
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateTimeSheet(int idDep, int year, int month, IEnumerable<DtoFactStaffEmployee> employees)
        {
            var dateStart = new DateTime(year, month, 1);
            var dateEnd = dateStart.AddMonths(1).AddDays(-1);
            employees = employees != null ? employees.Where(w => w.IsCheked).ToArray() : null;
            var message = Client.CreateTimeSheet(idDep, dateStart, dateEnd, GetCurrentApprover(), employees);
            return Json(message, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DellTimeSheet(int idTimeSheet)
        {
            var message = Client.RemoveTimeSheet(idTimeSheet);
            return Json(message, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Изменение табеля на авансовый
        /// </summary>
        /// <param name="idTimeSheet"></param>
        /// <returns></returns>
        public JsonResult remakeTimeSheetAdvance(int idTimeSheet)
        {
            var msg = Client.remakeTimeSheetAdvance(idTimeSheet);
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
    }
}
