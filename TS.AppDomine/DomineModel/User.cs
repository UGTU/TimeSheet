using System.Collections.Generic;

namespace TS.AppDomine.DomineModel
{
    public class User:Persone
    {
        public IEnumerable<DepartmentApprover> Departments { get; set; } 
    }
}
