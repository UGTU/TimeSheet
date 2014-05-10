using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS.AppDomine.DomineModel
{
    public class ExceptionDay
    {
        public ExceptionDay()
        {
            Date = DateTime.Today;
            Name = "Новый праздник";
        }
        public int IdExceptionDay { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public float Mps { get; set; }
        public float Mns { get; set; }
        public float Gps { get; set; }
        public float Gns { get; set; }
        public DayStatus DayStatus { get; set; }
        public WorkShedule WorkShedule { get; set; }
        public string DayAweek { get; set; }
    }
}
