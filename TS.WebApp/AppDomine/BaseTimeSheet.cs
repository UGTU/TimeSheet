using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSheetMvc4WebApplication.AppDomine
{
    /// <summary>
    /// Базовый класс табеля, не содержит записей табеля по сотрудникам
    /// </summary>
    public class BaseTimeSheet
    {
        public int IdTimeSheet { get; set; }
        public Department Department { get; set; }
        public DateTime DateBegin { get; set; }
        public DateTime DateEnd { get; set; }
        public DateTime DateComposition { get; set; }
        public int EmployeesCount { get; set; }
        public int ApproveStep { get; set; }
        public bool IsFake { get; set; }
        public bool IsCorrection { get; set; }
        public IEnumerable<Approver> Approvers { get; set; }
        public IEnumerable<ApproverResult> ApprovalResults { get; set; }
        /// <summary>
        /// Возвращает последнего согласователя
        /// </summary>
        /// <returns></returns>
        //public Approver GetApprover(ApproveType approveType)
        //{
        //    if (ApproveStep <= (int) approveType)
        //    {
        //        return Approvers.SingleOrDefault(s=>s.DepartmentApprovers.)
        //    }
            
        //}
    }
}