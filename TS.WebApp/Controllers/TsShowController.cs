using System.Web.Mvc;
using Rotativa;
using Rotativa.Options;
using TimeSheetMvc4WebApplication.Models;

namespace TimeSheetMvc4WebApplication.Controllers
{
    [Authorize]
    public class TsShowController : BaseController
    {

        public ActionResult Show(int id)
        {
            return View(id);
        }

        public PartialViewResult PartialShow(int id)
        {
            try
            {
                var timeSheetModel = TimeSheetModelConstructor(id, false);
                return PartialView("PartialView/PartialTimeSheetShow", timeSheetModel);
            }
            catch (System.Exception e)
            {
                Logger.Error(e);
                return PartialView("ErrorPartialView", "Во время отображения табеля возникли проблемы." + "<br/><small>" + e.Message + "</small>");
            }
        }

        [AllowAnonymous]
        public ActionResult Print(int id)
        {
            var timeSheetModel = TimeSheetModelConstructor(id, true);
            return View(timeSheetModel);
        }

        [AllowAnonymous]
        public ViewAsPdf Pdf(int id)
        {
            var timeSheetModel = TimeSheetModelConstructor(id, true);
            return new ViewAsPdf("Print", timeSheetModel) { PageOrientation = Orientation.Landscape };
        }

        //=====================================================================================================
        private TimeSheetModel[] TimeSheetModelConstructor(int id, bool isForPrint)
        {
            const int firstPaperEmployeeCount = 5;
            const int lastPaperEmployeeCount = 6;
            const int paperEmployeeCount = 8;
            var timeSheet = GetTimeSheetOrThrowException(id);
            var timeSheetModel = ModelConstructor.TimeSheetForDepartment(timeSheet, firstPaperEmployeeCount,
                lastPaperEmployeeCount,
                paperEmployeeCount, isForPrint);
            return timeSheetModel;
        }
    }
}