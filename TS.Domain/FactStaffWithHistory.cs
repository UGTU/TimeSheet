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
    
    public partial class FactStaffWithHistory
    {
        public int id { get; set; }
        public decimal StaffCount { get; set; }
        public Nullable<decimal> HourCount { get; set; }
        public Nullable<decimal> HourSalary { get; set; }
        public Nullable<int> idPlanStaff { get; set; }
        public int idEmployee { get; set; }
        public int idBeginPrikaz { get; set; }
        public Nullable<int> idEndPrikaz { get; set; }
        public int idTypeWork { get; set; }
        public System.DateTime DateBegin { get; set; }
        public Nullable<System.DateTime> DateEnd { get; set; }
        public bool IsReplacement { get; set; }
        public int idFactStaffHistory { get; set; }
        public Nullable<int> IDShedule { get; set; }
        public Nullable<int> idSalaryKoeff { get; set; }
        public Nullable<int> idlaborcontrakt { get; set; }
        public Nullable<int> idreason { get; set; }
        public Nullable<decimal> HourStaffCount { get; set; }
        public decimal CalcStaffCount { get; set; }
        public int IdWorkShedule { get; set; }
    }
}
