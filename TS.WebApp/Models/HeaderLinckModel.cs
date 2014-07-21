using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSheetMvc4WebApplication.Models
{
    public enum HeaderLink
    {
        Edit,Show,Pdf,Approve,ExceptionDay,Department
    }
    public class HeaderLinckModel
    {
        public IEnumerable<HeaderLink> Links { get; set; }
        public HeaderLink CurrentLink { get; set; }
        public int IdTimeSheet { get; set; }
    }
}