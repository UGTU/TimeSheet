using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;
using System.Web.Query.Dynamic;
using System.Web.Routing;
using Newtonsoft.Json;
using TimeSheetMvc4WebApplication.ClassesDTO;
using TimeSheetMvc4WebApplication.Models;
using TimeSheetMvc4WebApplication.Models.Main;

namespace TimeSheetMvc4WebApplication.Controllers
{
    [Authorize]
    public class MainController : BaseController
    {
        //
        // GET: /Main/

        public ActionResult Index()
        {
            var approver = Client.GetCurrentApproverByLogin(GetUsername());
            if (!approver.DtoApproverDepartments.Any()) return View("DangerMessageShow",new MessageModel
            {
                MessageTitile = "Отсутствует доступ.",
                Message="У вас отсутствует доступ к ИС \"Табель\", обратитесь к администратору."  
            });

            if (approver.DtoApproverDepartments.Count() > 1) return View(approver);
            return RedirectToAction("TimeSheetList",new {id= approver.DtoApproverDepartments.First().IdDepartment});
        }


        public ActionResult TimeSheetList(int id,bool showAll = false)
        {
            var approver = Client.GetCurrentApproverByLogin(GetUsername());
            ViewBag.approver = approver;
            ViewBag.Department = approver.DtoApproverDepartments.First(w => w.IdDepartment == id);
            var timeSheetList = Client.GetEmptyTimeSheetList(id,showAll?int.MinValue:12);//.OrderByDescending(o => o.DateBegin).Take(12);
            return View(timeSheetList);
        }

        public ActionResult TimeSheetEdit(int id)
        {
            return View(id);
        }








        //======================    Json    ====================================

        public JsonResult GetTimeSheetModelJson(int idTimeSheet)
        {
            var cult = System.Globalization.CultureInfo.GetCultureInfo("ru-Ru");
            var timeSheet = Client.GetTimeSheet(idTimeSheet);
            if (timeSheet == null) throw new System.Exception();

            var ts = new JsTimeSheetViewModel
            {
                DayStatusList = Client.GetDayStatusList(),
                TimeSheet = new Models.Main.JsTimeSheetModel
                {
                    Department = timeSheet.Department.DepartmentFullName,
                    IdTimeSheet = timeSheet.IdTimeSheet,
                    TimeSheetApproveStep = timeSheet.ApproveStep
                }
            };

            var employees = new List<JsEmployeeModel>();
            foreach (var empl in timeSheet.Employees)
            {
                var records = new List<JsTimeSheetRecordModel>();
                var recordsArr = empl.Records.OrderBy(o => o.Date).ToArray();
                for (int i = 0; i < recordsArr.Length; i++)
                {
                    var rec = recordsArr[i];
                    records.Add(new JsTimeSheetRecordModel
                    {
                        IdTimeSheetRecord = rec.IdTimeSheetRecord,
                        //DayAweek = rec.DayAweek,
                        DayAweek = rec.Date.ToString("dddd", cult),
                        IdDayStatus = rec.DayStays.IdDayStatus,
                        JobTimeCount = rec.JobTimeCount,
                        //Date = rec.Date.ToShortDateString(),
                        Date = rec.Date.ToString("dd MMMM"),
                        Order = i
                    });
                }
                

                employees.Add(new JsEmployeeModel
                {
                    Surname = empl.FactStaffEmployee.Surname,
                    Name = empl.FactStaffEmployee.Name,
                    Patronymic = empl.FactStaffEmployee.Patronymic,
                    Post = empl.FactStaffEmployee.Post,
                    WorkShedule = empl.FactStaffEmployee.WorkShedule,
                    StaffRate = empl.FactStaffEmployee.StaffRate,
                    Records = records.ToArray()
                });
            }
            
            ts.TimeSheet.Employees = employees.ToArray();
            return Json(ts, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateTimeSheetRecotds(JsTimeSheetRecordModel[] records)
        {
            var message = new DtoMessage {Result = Client.EditTimeSheetRecords(records)};
            if (!message.Result)
            {
                message.Message = "При сохранении изменений возникли проблемы. Изменения не сохранены.";
            }
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
                message=Client.CreateTimeSheetByName(idDep, dateStart, dateEnd, GetUsername(), employees.ToArray());
            }
            else
            {
                message = Client.CreateTimeSheet(idDep, dateStart, dateEnd, GetUsername());
            }
            return Json(message, JsonRequestBehavior.AllowGet);
        }
    }
}
