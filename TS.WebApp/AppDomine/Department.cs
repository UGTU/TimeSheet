using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSheetMvc4WebApplication.AppDomine
{
    public class Department
    {
        public int IdDepartment { get; set; }
        public string SmallName { get; set; }
        public string FullName { get; set; }
        public int? IdManagerDepartment { get; set; }
        public bool HasTimeSheet { get; set; }
    }
}