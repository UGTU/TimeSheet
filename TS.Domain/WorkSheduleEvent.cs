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
    
    public partial class WorkSheduleEvent
    {
        public int idEventSuperType { get; set; }
        public double KolHourMPS { get; set; }
        public double KolHourMNS { get; set; }
        public double KolHourGPS { get; set; }
        public double KolHourGNS { get; set; }
        public int idShedule { get; set; }
        public int idRepeatRate { get; set; }
        public Nullable<System.DateTime> Data_Agr { get; set; }
        public Nullable<bool> IsPPS { get; set; }
    
        public virtual EventSuperType EventSuperType { get; set; }
        public virtual RepeatRate RepeatRate { get; set; }
        public virtual WorkShedule WorkShedule { get; set; }
    }
}