using System.Collections.Generic;

namespace TS.AppDomine.DomineModel
{
    public class DepartmentApprover
    {
        public Department Department { get; set; }
        public Dictionary<int, ApproveType> ApproveType { get; set; }
    }
}