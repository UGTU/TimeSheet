using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace TS.AppDomine.DomineModel
{
    public class Employee:Persone
    {
        public int IdPersone { get; set; }
        public decimal Rate { get; set; }
        public WorkShedule WorkShedule { get; set; }
        public IEnumerable<TimeSheetRecord> Records { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public bool IsPps { get; set; }
    }
}
