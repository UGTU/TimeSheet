using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS.AppDomine.DomineModel
{
    public class WorkShedule
    {
        public int Id { get; set; }
        public string WorkSheduleName { get; set; }
        public static bool operator == (WorkShedule a, WorkShedule b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }
            // Return true if the fields match:
            //return a.Id == b.Id;
            return a.Equals(b);
        }

        public static bool operator !=(WorkShedule a, WorkShedule b)
        {
            return !(a == b);
        }

        protected bool Equals(WorkShedule other)
        {
            return Id == other.Id && string.Equals(WorkSheduleName, other.WorkSheduleName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WorkShedule)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id * 397) ^ (WorkSheduleName != null ? WorkSheduleName.GetHashCode() : 0);
            }
        }
    }
}
