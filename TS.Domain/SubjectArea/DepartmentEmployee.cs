using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS.Domain.SubjectArea
{
    class DepartmentEmployee:Employee
    {
        public int IdTimeSheetEmployee { get; set; } //IdFactStuffHistory
        public int IdDepartment { get; set; }
        public Post Post { get; set; }
        public decimal StaffRate { get; set; }

    }
}
