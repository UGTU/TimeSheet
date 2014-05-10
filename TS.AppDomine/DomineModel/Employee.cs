using System.Collections.Generic;

namespace TS.AppDomine.DomineModel
{
    public class Employee:Persone
    {
        public int IdPersone { get; set; }
        public decimal Rate { get; set; }
        public IEnumerable<TimeSheetRecord> Records { get; set; }
    }
}
