using System.Web;
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
    }
}