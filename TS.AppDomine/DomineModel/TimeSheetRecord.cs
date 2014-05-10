using System;

namespace TS.AppDomine.DomineModel
{
    public class TimeSheetRecord
    {
        public Guid IdTimeSheetRecord;
        public double JobTimeCount;
        public DayStatus DayStays;
        public DateTime Date;
        //public string DayAweek;
        //public bool IsChecked;

        public TimeSheetRecord(DateTime date, DayStatus status, double jobTimeCount)
        {
            Date = date;
            DayStays = status;
            JobTimeCount = jobTimeCount;
            IdTimeSheetRecord = Guid.NewGuid();
        }
    }
}
