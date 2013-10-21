using System.Web.Mvc;
using System.Web.Routing;

namespace TimeSheetMvc4WebApplication
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            routes.MapRoute(
                "ShortTimeSheetPdf",
                "tabel/{idTimeSheet}",
                new { controller = "Home", action = "TimeSheetPdf", id = UrlParameter.Optional }, constraints: new { idTimeSheet = @"\d+" }
                );

            routes.MapRoute(
                "ShortTimeSheetShow",
                "tabelshow/{idTimeSheet}",
                new {controller = "Home", action = "TimeSheetShow", id = UrlParameter.Optional},
                constraints: new {idTimeSheet = @"\d+"}
                );


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}