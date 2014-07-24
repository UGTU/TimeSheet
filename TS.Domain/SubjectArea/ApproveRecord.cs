using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS.Domain.SubjectArea
{
    class ApproveRecord
    {
        public DateTime Date { get; set; }
        public bool Result { get; set; }
        public ApproveDepartment ApproveDepartment { get; set; }
        public Employee Employee { get; set; }
    }
}
