using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeSheetMvc4WebApplication.ClassesDTO;

namespace TimeSheetMvc4WebApplication.Models.Register
{
    public class DepatrmentModel : DtoDepartment
    {
        public IEnumerable<DtoTimeSheet> timesheets { get; set; }
        public DepatrmentModel(DtoDepartment department)
        {
            this.DepartmentFullName = department.DepartmentFullName;
            this.DepartmentSmallName = department.DepartmentSmallName;
            this.HasTimeSheet = department.HasTimeSheet;
            this.IdDepartment = department.IdDepartment;
            this.IdManagerDepartment = department.IdManagerDepartment;
        }
    }
}