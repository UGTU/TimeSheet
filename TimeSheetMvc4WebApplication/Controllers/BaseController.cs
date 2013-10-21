using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TimeSheetMvc4WebApplication.Controllers
{
    public class BaseController : Controller
    {
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


    }
}
