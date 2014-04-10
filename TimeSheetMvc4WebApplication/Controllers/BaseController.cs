using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeSheetMvc4WebApplication.ClassesDTO;
using TimeSheetMvc4WebApplication.Source;

namespace TimeSheetMvc4WebApplication.Controllers
{
    public class BaseController : Controller
    {
        protected static string ErrorPage = "~/Error";     
        protected static string NotFoundPage = "~/NotFoundPage";
        protected readonly TimeSheetService Client = new TimeSheetService();
        protected readonly DataProvider Provider = new DataProvider();

        //private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        [Authorize]
        public string GetUsername()
        {
            //return "atipunin@ugtu.net"; //получить логин пользователя
            //return "grahimova@ugtu.net"; //получить логин пользователя
            //return "kafedra@ugtu.net"; //получить логин пользователя
            //return "fmarakasov@ugtu.net"; //получить логин пользователя
            //return "tkazakova@ugtu.net"; //получить логин пользователя
            //return "tester1@ugtu.net"; //получить логин пользователя
            //return "yasidanova11@ugtu.net"; //получить логин пользователя
            return System.Web.HttpContext.Current.User.Identity.Name;
        }

        public RedirectResult RedirectToNotFoundPage
        {
            get { return Redirect(NotFoundPage); }
        }


        protected override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);
            filterContext.Result = Redirect(ErrorPage);
        }

        public DtoApprover GetCurrentApprover()
        {
            if (Session == null) return Client.GetCurrentApproverByLogin(GetUsername());
            if (Session["approver"] == null)
            {
                Session["approver"] = Client.GetCurrentApproverByLogin(GetUsername());
            }
            return Session["approver"] as DtoApprover;
        }

        protected DtoTimeSheet GetTimeSheetOrThrowException(int id)
        {
            var timeSheet = Client.GetTimeSheet(id);
            if (timeSheet == null)
                throw new HttpException(404, "Запрашиваемый табель не обнаружен, табель №" + id);
            return timeSheet;
        }


    }
}
