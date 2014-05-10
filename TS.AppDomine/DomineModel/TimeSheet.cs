using System.Collections.Generic;

namespace TS.AppDomine.DomineModel
{
    /// <summary>
    /// Табель с записями
    /// </summary>
    public class TimeSheet:BaseTimeSheet
    {
        public IEnumerable<Employee> Employees { get; set; }
    }
}