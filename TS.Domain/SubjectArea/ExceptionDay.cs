using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS.Domain.SubjectArea
{
    class ExceptionDay
    {
        public ExceptionDay()
        {
            IdExceptionDay = int.MinValue;
            Date = DateTime.Now;
            Name = "Новый праздник";
        }
        public int IdExceptionDay { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public float MPS { get; set; }
        public float MNS { get; set; }
        public float GPS { get; set; }
        public float GNS { get; set; }
        public DayStatus DayStatus { get; set; }
        public WorkShedule WorkShedule { get; set; }
        public string DayAweek { get; set; }
    }
}
