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
                url: "EditLogin",
                defaults: new { controller = "Admin", action = "EditLogin", id = UrlParameter.Optional }
            );
            
            //routes.MapRoute(
            //    null,
            //    url: "Error",
            //    defaults: new { controller = "Error", action = "Error", id = UrlParameter.Optional }
            //);

            //routes.MapRoute(
            //    null,
            //    url: "NotFoundPage",
            //    defaults: new { controller = "Error", action = "NotFoundPage", id = UrlParameter.Optional }
            //);

            //=======================================================

            routes.MapRoute(
                "ShortTimeSheetPdf",
                "TabelPdf/{id}",
                new { controller = "TsShow", action = "Pdf", id = UrlParameter.Optional },
                constraints: new {id = @"\d+"}
                );

            routes.MapRoute(
                "ShortTimeSheetShow",
                "TabelShow/{id}",
                new { controller = "TsShow", action = "Show", id = UrlParameter.Optional },
                constraints: new {id = @"\d+"}
                );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new {controller = "Main", action = "Index", id = UrlParameter.Optional}
                );
        }
    }
}