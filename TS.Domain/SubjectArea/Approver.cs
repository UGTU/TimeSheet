using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS.Domain.SubjectArea
{
    class Approver:Employee
    {
        public IEnumerable<ApproveDepartment> ApproveDepartments { get; set; }  
    }
}
