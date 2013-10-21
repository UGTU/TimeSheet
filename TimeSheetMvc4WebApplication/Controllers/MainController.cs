using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;
using System.Web.Query.Dynamic;
using System.Web.Routing;
using Newtonsoft.Json;
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

        //
        // GET: /Main/

        public ActionResult Index()
        {
            //throw new System.Exception();

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
            throw new System.Exception("Парам пам пам");

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

        //======================    Отображение табеля     ====================

        public ActionResult TimeSheetShow(int idTimeSheet)
        {
            try
            {
                var timeSheet = Client.GetTimeSheet(idTimeSheet);
                var timeSheetModel = ModelConstructor.TimeSheetForDepartment(timeSheet, FirstPaperEmployeeCount,
                                                                             LastPaperEmployeeCount,
                                                                             PaperEmployeeCount, false);
                return View(timeSheetModel);
            }
            catch (System.Exception)
            {
                var message = new MessageModel
                {
                    //CSS = "ErrorMessage",
                    Message = "Табель не обнаружен."
                };
                return View("WarningMessageShow", message);
            }
        }

        [AllowAnonymous]
        public ActionResult TimeSheetPrint(int idTimeSheet)
        {
            try
            {
                var timeSheet = Client.GetTimeSheet(idTimeSheet);
                var timeSheetModel = ModelConstructor.TimeSheetForDepartment(timeSheet, FirstPaperEmployeeCount,
                                                                             LastPaperEmployeeCount, PaperEmployeeCount,
                                                                             true);
                return View(timeSheetModel);
            }
            catch (System.Exception)
            {
                var message = new MessageModel
                {
                    //CSS = "ErrorMessage",
                    Message = "Табель не обнаружен."
                };
                return View("WarningMessageShow", message);
            }
        }

        [AllowAnonymous]
        public ActionResult TimeSheetPdf(int idTimeSheet)
        {
            try
            {
                var timeSheet = Client.GetTimeSheet(idTimeSheet);
                var timeSheetModel = ModelConstructor.TimeSheetForDepartment(timeSheet, FirstPaperEmployeeCount,
                                                                             LastPaperEmployeeCount, PaperEmployeeCount,
                                                                             true);
                return new ViewAsPdf("TimeSheetPrint", timeSheetModel)
                {
                    PageOrientation = Orientation.Landscape

                };
            }
            catch (System.Exception)
            {
                var message = new MessageModel
                {
                    Message = "Табель не обнаружен."
                };
                return View("WarningMessageShow", message);
            }
        }

        //======================    Согласование табеля     ====================

        [HttpGet]
        public ActionResult TimeSheetApproval(int idTimeSheet)
        {
            var timeSheet = Client.GetTimeSheet(idTimeSheet);
            MessageModel message;
            if (timeSheet == null)
            {
                message = new MessageModel
                {
                    MessageTitile = "Согласование табеля недоступно.",
                    Message = "Запрашиваемый для согласования табель не обнаружен, обратитесь к администратору."
                };
                //ViewBag.IdTimeSheet = idTimeSheet;
                return View("DangerMessageShow", message);
            }
            ViewBag.IdTimeSheet = idTimeSheet;
            if (Client.CanApprove(idTimeSheet, GetUsername()))
            {
                ViewBag.TimeSheetModel = ModelConstructor.TimeSheetForDepartment(timeSheet, FirstPaperEmployeeCount,
                    LastPaperEmployeeCount, PaperEmployeeCount, false);
                var approver = Client.GetCurrentApproverByLogin(GetUsername());
                var timeSheetAprovalModel = new TimeSheetAprovalModel
                {
                    IdTimeSheet = timeSheet.IdTimeSheet,
                    ApprovalDate = DateTime.Now,
                    ApprovalResult = null,
                    Comment = "",
                    IdApprover = approver.DtoApproverDepartments.First().IdApprover
                };
                return View(timeSheetAprovalModel);
            }
            var currentApprover = Client.GetNextApproverForTimeSheet(idTimeSheet);
            if (currentApprover != null)
            {
                message = new MessageModel
                {
                    MessageTitile = "Согласование недоступно.",
                    Message = string.Format("Текущий соглпсователь: {0} {1} {2} ", currentApprover.Surname,
                                currentApprover.Name, currentApprover.Patronymic)
                };
                //ViewBag.IdTimeSheet = idTimeSheet;
                return View("WarningMessageShow", message);
            }
            message = new MessageModel
            {
                MessageTitile = "Табель согласован!",
                Message = "Сгласование табеля успешно завершено."
            };
            //ViewBag.IdTimeSheet = idTimeSheet;
            return View("SuccessMessageShow", message);
        }

        [HttpPost]
        public ViewResult TimeSheetApproval(TimeSheetAprovalModel timeSheetAprovalModel)
        {
            ViewBag.IdTimeSheet = timeSheetAprovalModel.IdTimeSheet;
            if (timeSheetAprovalModel.ApprovalResult != null &&
                (bool)timeSheetAprovalModel.ApprovalResult == false & timeSheetAprovalModel.Comment == null)
                ModelState.AddModelError("Причина не указана", "В случае отклонения табеля необходимо прокомментировать причину!");
            if (ModelState.IsValid)
            {
                if (Client.CanApprove(timeSheetAprovalModel.IdTimeSheet, GetUsername()))
                {
                    if (Client.TimeSheetApproval(timeSheetAprovalModel.IdTimeSheet, GetUsername(),
                        (bool)timeSheetAprovalModel.ApprovalResult, timeSheetAprovalModel.Comment))
                    {
                        //===Согласовано====
                        var currentApprover = Client.GetNextApproverForTimeSheet(timeSheetAprovalModel.IdTimeSheet);
                        var message = new MessageModel();
                        if (currentApprover != null)
                        {
                            message.MessageTitile = (bool)timeSheetAprovalModel.ApprovalResult ? "Согласование табеля выполнено успешно." : "Согласование табеля отклонено, табель направлен наредактирование.";
                            message.Message = string.Format("Следующий согласователь {0} {1} {2}.", currentApprover.Surname, currentApprover.Name, currentApprover.Patronymic);
                        }
                        else
                        {
                            message.MessageTitile = "Табель успешно согласован.";
                            message.Message = string.Format("Согласование табеля успешно завершено.");
                        }
                        return View("SuccessMessageShow", message);
                    }
                }
                else
                {
                    var currentApprover = Client.GetNextApproverForTimeSheet(timeSheetAprovalModel.IdTimeSheet);
                    if (currentApprover != null)
                    {
                        var message = new MessageModel
                        {
                            MessageTitile = "Согласование недоступно.",
                            Message = string.Format("Текущий соглпсователь: {0} {1} {2} ", currentApprover.Surname,
                                        currentApprover.Name, currentApprover.Patronymic)
                        };
                        //ViewBag.IdTimeSheet = timeSheetAprovalModel.IdTimeSheet;
                        return View("WarningMessageShow", message);
                    }
                }
            }
            var timeSheet = Client.GetTimeSheet(timeSheetAprovalModel.IdTimeSheet);
            ViewBag.TimeSheetModel = ModelConstructor.TimeSheetForDepartment(timeSheet, FirstPaperEmployeeCount,
                LastPaperEmployeeCount, PaperEmployeeCount, false);
            return View(timeSheetAprovalModel);
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
