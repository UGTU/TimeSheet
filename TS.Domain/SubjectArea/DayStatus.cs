using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS.Domain.SubjectArea
{
    class DayStatus
    {
        public int IdDayStatus { get; set; }
        public string SmallDayStatusName { get; set; }
        public string FullDayStatusName { get; set; }
        public bool IsVisible { get; set; }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }
            // If parameter cannot be cast to Point return false.
            var p = obj as DayStatus;
            if (p == null)
            {
                return false;
            }
            // Return true if the fields match:
            return (IdDayStatus == p.IdDayStatus) && (SmallDayStatusName == p.SmallDayStatusName) && (FullDayStatusName == p.FullDayStatusName);
        }

        public override int GetHashCode()
        {
            return IdDayStatus;
        }
    }
}
