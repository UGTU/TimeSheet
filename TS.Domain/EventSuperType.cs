//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TS.Domain
{
    using System;
    using System.Collections.Generic;
    
    public partial class EventSuperType
    {
        public EventSuperType()
        {
            this.WorkSheduleEvent = new HashSet<WorkSheduleEvent>();
        }
    
        public int id { get; set; }
        public int idDayStatus { get; set; }
        public System.DateTime BeginDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public System.DateTime FirstEventDate { get; set; }
    
        public virtual DayStatus DayStatus { get; set; }
        public virtual EmployeeEvent EmployeeEvent { get; set; }
        public virtual ICollection<WorkSheduleEvent> WorkSheduleEvent { get; set; }
    }
}