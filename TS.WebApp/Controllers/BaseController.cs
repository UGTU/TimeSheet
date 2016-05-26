using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using TimeSheetMvc4WebApplication.ClassesDTO;
using TimeSheetMvc4WebApplication.Source;

namespace TimeSheetMvc4WebApplication.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        //protected static string ErrorPage = "~/Error";
        //protected static string NotFoundPage = "~/NotFoundPage";
        protected static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        protected readonly TimeSheetService Client = new TimeSheetService();
        protected readonly DataProvider Provider = new DataProvider();

        private static string GetUsername()
        {
            //return "atipunin@ugtu.net"; //получить логин пользователя
            //return "grahimova@ugtu.net"; //получить логин пользователя
            //return "kafedra@ugtu.net"; //получить логин пользователя
            //return "fmarakasov@ugtu.net"; //получить логин пользователя
            //return "tkazakova@ugtu.net"; //получить логин пользователя
            //return "tester1@ugtu.net"; //получить логин пользователя
            //return "yasidanova11@ugtu.net"; //получить логин пользователя
            //return "zabaturova@ugtu.net";
            return System.Web.HttpContext.Current.User.Identity.Name;
        }

        public RedirectResult RedirectToNotFoundPage(string error = "")
        {
            if (error == string.Empty)
                error = "Запрашиваемый ресурс не обнаружен";
            throw new HttpException(404, error);
            //get
            //{
            //    throw new HttpException(404, "Запрашиваемый ресурс не найден");
            //    //return Redirect(NotFoundPage);
            //}
        }

        public DtoApprover GetCurrentApprover()
        {
            return SessionHelper.Approver;
        }

        protected DtoTimeSheet GetTimeSheetOrThrowException(int id)
        {
            var timeSheet = Client.GetTimeSheet(id);
            if (timeSheet == null)
                throw new HttpException(404, "Запрашиваемый табель не обнаружен, табель №" + id);
            return timeSheet;
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            //SessionHelper.Approver = Client.GetCurrentApproverByLogin(GetUsername(),
            //    System.Web.HttpContext.Current.User.Identity.IsAuthenticated &&
            //    System.Web.HttpContext.Current.User.IsInRole("TabelAdmin"));

            SessionHelper.Approver = Client.GetCurrentApproverByLogin(GetUsername(),
                User.Identity.IsAuthenticated && User.IsInRole("TabelAdmin"));

            //SessionHelper.Approver = Client.GetCurrentApproverByLogin("reb00ter@ugtu.net",true);
        }
    }
}
