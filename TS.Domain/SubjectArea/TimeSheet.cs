using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS.Domain.SubjectArea
{
    class TimeSheet
    {
        public int IdTimeSheet { get; set; }
        public TimeSheetEmployee[] Employees { get; set; }
        public Department Department { get; set; }
        public DateTime DateBegin { get; set; }
        public DateTime DateEnd { get; set; }
        public DateTime DateComposition { get; set; }
        public IEnumerable<Approver> Approvers { get; set; }
        public IEnumerable<ApproveRecord> ApproveHistoryRecords { get; set; } 
        public int EmployeesCount { get; set; }
        public int ApproveStep { get; set; }
        public bool IsFake { get; set; }
    }
}
