using System.Web.Mvc;
using System.Web.Routing;

namespace TimeSheetMvc4WebApplication
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //=======================================================

            routes.MapRoute(
                null,
                url: "Error",
                defaults: new { controller = "Error", action = "Error", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                null,
                url: "NotFoundPage",
                defaults: new { controller = "Error", action = "NotFoundPage", id = UrlParameter.Optional }
            );

            //=======================================================

            routes.MapRoute(
                "ShortTimeSheetPdf",
                "tabel/{idTimeSheet}",
                new {controller = "Main", action = "TimeSheetPdf", id = UrlParameter.Optional},
                constraints: new {idTimeSheet = @"\d+"}
                );

            routes.MapRoute(
                "ShortTimeSheetShow",
                "tabelshow/{idTimeSheet}",
                new { controller = "Main", action = "TimeSheetShow", id = UrlParameter.Optional },
                constraints: new {idTimeSheet = @"\d+"}
                );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new {controller = "Main", action = "Index", id = UrlParameter.Optional}
                );
        }
    }
}