using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS.Domain.SubjectArea
{
    class ApproveDepartment
    {
        public int IdApproveDepartment { get; set; }
        public int IdDepartment { get; set; }
        public ApproverType Type { get; set; }
    }
}
