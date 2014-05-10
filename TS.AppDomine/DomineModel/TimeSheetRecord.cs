using System;

namespace TS.AppDomine.DomineModel
{
    public class TimeSheetRecord
    {
        public Guid IdTimeSheetRecord;
        public double JobTimeCount;
        public DayStatus DayStays;
        public DateTime Date;
        public string DayAweek;
        public bool IsChecked;
    }
}
