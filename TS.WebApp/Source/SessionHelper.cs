using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Providers.Entities;
using TimeSheetMvc4WebApplication.ClassesDTO;

namespace TimeSheetMvc4WebApplication.Source
{
    public static class SessionHelper
    {
        public static DtoApprover Approver
        {
            get { return HttpContext.Current.Session["approver"] as DtoApprover; }
            set { HttpContext.Current.Session["approver"] = value; }
        }

        public static string ApproverName
        {
            get
            {
                var approver = Approver;
                return approver != null ? string.Format("{0} {1} {2}", approver.Surname, approver.Name, approver.Patronymic) : "";
            }
        }
    }
}