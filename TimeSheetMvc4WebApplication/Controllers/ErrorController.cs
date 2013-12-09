using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace TimeSheetMvc4WebApplication.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        //
        // GET: /Error/
        public ActionResult Error()
        {
            //Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return View();
        }

        public ActionResult NotFoundPage()
        {
            //Response.StatusCode = (int)HttpStatusCode.NotFound;
            return View();
        }

        public ActionResult AccessDenied()
        {
            //Response.StatusCode = (int)HttpStatusCode.NotFound;
            return View();
        }

    }
}
