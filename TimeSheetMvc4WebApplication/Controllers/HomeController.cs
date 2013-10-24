using System;
using System.Linq;
using System.Web.Mvc;
using Rotativa;
using Rotativa.Options;
using TimeSheetMvc4WebApplication.Models;

namespace TimeSheetMvc4WebApplication.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private const int FirstPaperEmployeeCount = 5;
        private const int LastPaperEmployeeCount = 5;
        private const int PaperEmployeeCount = 8;




        //
        // GET: /Home/

        



        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

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


        //todo:убраь все методы или совсем удалить контроллер, поправить все ссылки в хедере, написать хелпер
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

        //[AllowAnonymous]
        //public ActionResult TimeSheetPdf(int idTimeSheet)
        //{
        //    var urlHelper = new UrlHelper(Request.RequestContext);
        //    string url = urlHelper.Action("TimeSheetPrint", new { idTimeSheet = idTimeSheet });
        //    return new UrlAsPdf(url)
        //    {
        //        FileName = "Табель.pdf",PageOrientation = Orientation.Landscape
            
        //    };
        //}




        [AllowAnonymous]
        public ActionResult TimeSheetPrintByName(int idTimeSheet)
        {
            try
            {
                var timeSheet = Client.GetTimeSheet(idTimeSheet);
                timeSheet.Employees = timeSheet.Employees.Where(w => w.IsChecked).ToArray();
                var timeSheetModel = ModelConstructor.TimeSheetForDepartment(timeSheet, FirstPaperEmployeeCount,
                                                                             LastPaperEmployeeCount, PaperEmployeeCount,
                                                                             true);
                return View("TimeSheetPrint", timeSheetModel);
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
        public ActionResult TimeSheetPdfByName(int idTimeSheet)
        {
            var urlHelper = new UrlHelper(Request.RequestContext);
            string url = urlHelper.Action("TimeSheetPrintByName", new { idTimeSheet = idTimeSheet });
            return new UrlAsPdf(url)
            {
                FileName = "Табель.pdf",
            };
        }

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
                (bool) timeSheetAprovalModel.ApprovalResult == false & timeSheetAprovalModel.Comment == null)
                ModelState.AddModelError("Причина не указана", "В случае отклонения табеля необходимо прокомментировать причину!");
            if (ModelState.IsValid)
            {
                    if (Client.CanApprove(timeSheetAprovalModel.IdTimeSheet, GetUsername()))
                    {
                        if (Client.TimeSheetApproval(timeSheetAprovalModel.IdTimeSheet, GetUsername(),
                            (bool) timeSheetAprovalModel.ApprovalResult, timeSheetAprovalModel.Comment))
                        {
                            //===Согласовано====
                            var currentApprover = Client.GetNextApproverForTimeSheet(timeSheetAprovalModel.IdTimeSheet);
                            var message = new MessageModel();
                            if (currentApprover != null)
                            {
                                message.MessageTitile = (bool) timeSheetAprovalModel.ApprovalResult ? "Согласование табеля выполнено успешно." : "Согласование табеля отклонено, табель направлен наредактирование.";
                                message.Message = string.Format("Следующий согласователь {0} {1} {2}.",currentApprover.Surname, currentApprover.Name,currentApprover.Patronymic);
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

    }
}
