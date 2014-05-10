using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSheetMvc4WebApplication.AppDomine
{
    /// <summary>
    /// Согласвоатель табеля
    /// </summary>
    public class Approver:Persone
    {
        public int IdApprover { get; set; }
        public IEnumerable<DepartmentApprover> DepartmentApprovers { get; set; }
        public DateTime AppointmentDate { get; set; }
        public DateTime TerminationDate { get; set; }



    }
}