using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TimeSheetMvc4WebApplication.Controllers
{
    [Authorize]
    public class AdminController :  BaseController
    {
        //todo:реализовать функцию добавления дней исключения
        //todo:реализовать функцию отображения и редактирования согласователей для структурных подразделений
        //todo:реализовать для всех методов этого контроллера проверку на членство пользователя в группе администраторов
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            return RedirectToAction("ExceptionDay");
            CheckIsAdmin();
            return View();
        }

        public ActionResult ExceptionDay()
        {
            CheckIsAdmin();
            return View();
        }

        public ActionResult DepartmentManagment()
        {
            CheckIsAdmin();
            return View();
        }

        private void CheckIsAdmin()
        {
            var approver = GetCurrentApprover();
            if(!approver.IsAdministrator)
                throw new HttpException(401, "Попытка несанкционированного доступа к админке");
        }

    }
}
