using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS.Domain.SubjectArea
{
    class TimeSheetRecord
    {
        public Guid IdTimeSheetRecord;
        public double JobTimeCount;
        public DayStatus DayStays;
        public DateTime Date;
        public string DayAweek;
        public bool IsChecked;
    }
}
