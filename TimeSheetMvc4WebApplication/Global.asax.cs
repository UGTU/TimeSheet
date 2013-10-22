using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TimeSheetMvc4WebApplication.Controllers;

namespace TimeSheetMvc4WebApplication
{
    // Примечание: Инструкции по включению классического режима IIS6 или IIS7 
    // см. по ссылке http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public readonly TimeSheetService Client = new TimeSheetService();
        protected void Application_Start()
        {
            logger.Info("Application Start");
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }



        public override void Init()
        {
            logger.Info("Application Init");
            base.Init();
        }

        public override void Dispose()
        {
            logger.Info("Application Dispose");
            base.Dispose();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var httpContext = ((MvcApplication)sender).Context;
            var ex = Server.GetLastError(); 
            logger.Error(ex);
            const string redirectController = "Error";
            var action = "Error";
            if (ex is HttpException)
            {
                var httpEx = ex as HttpException;

                switch (httpEx.GetHttpCode())
                {
                    case 404:
                        action = "NotFoundPage";
                        break;

                    case 401:
                        action = "AccessDenied";
                        break;
                }
            }
            httpContext.ClearError();
            httpContext.Response.Clear();
            httpContext.Response.StatusCode = ex is HttpException ? ((HttpException)ex).GetHttpCode() : 500;
            httpContext.Response.TrySkipIisCustomErrors = true;
            Response.Redirect(string.Format("~/{0}/{1}", redirectController, action));
        }


        protected void Application_End()
        {
            logger.Info("Application End");
        }

        protected void Application_BeginRequest()
        {
            //logger.Info("Application BeginRequest");
        }

        protected void Application_EndRequest()
        {
            //logger.Info("Application EndRequest");
        }

        protected void Application_PreRequestHandlerExecute()
        {
            //logger.Info("Application PreRequestHandlerExecute");
        }

        protected void Application_PostRequestHandlerExecute()
        {
            //logger.Info("Application PreRequestHandlerExecute");
        }

        protected void Application_PreSendRequestHeaders()
        {
            //logger.Info("Application PreSendRequestHeaders");
        }

        protected void Application_PreSendContent()
        {
            //logger.Info("Application PreSendContent");
        }

        protected void Application_AcquireRequestState()
        {
            //logger.Info("Application AcquireRequestState");
        }

        protected void Application_ReleaseRequestState()
        {
            //logger.Info("Application ReleaseRequestState");
        }

        protected void Application_ResolveRequestCache()
        {
            //logger.Info("Application ResolveRequestCache");
        }

        protected void Application_UpdateRequestCache()
        {
            //logger.Info("Application UpdateRequestCache");
        }

        protected void Application_AuthenticateRequest()
        {
            //logger.Info("Application AuthenticateRequest");
        }

        protected void Application_AuthorizeRequest()
        {
            //logger.Info("Application AuthorizeRequest");
        }

        protected void Session_Start()
        {
            //var controller = new BaseController();
            //var approver = controller.GetCurrentApprover();
            //logger.Info(approver != null ? "Anonimus Session Start" : approver.EmployeeLogin + "  Session Start"); 
        }

        protected void Session_End()
        {
            //logger.Info("Session End");
        }

    }
}