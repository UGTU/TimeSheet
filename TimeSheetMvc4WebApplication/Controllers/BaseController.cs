using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TimeSheetMvc4WebApplication.Controllers
{
    public class BaseController : Controller
    {
        protected static string ErrorPage = "~/Error";
        
        protected static string NotFoundPage = "~/NotFoundPage";
        
        public readonly TimeSheetService Client = new TimeSheetService();

        [Authorize]
        public string GetUsername()
        {
            //return "atipunin@ugtu.net"; //получить логин пользователя
            //return "grahimova@ugtu.net"; //получить логин пользователя
            //return "kafedra@ugtu.net"; //получить логин пользователя
            //return "fmarakasov@ugtu.net"; //получить логин пользователя
            //return "tkazakova@ugtu.net"; //получить логин пользователя
            //return "tester1@ugtu.net"; //получить логин пользователя
            //return "ovisokolyan@ugtu.net"; //получить логин пользователя
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


    }
}
