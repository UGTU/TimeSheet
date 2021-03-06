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
    
    public partial class Employee
    {
        public Employee()
        {
            this.Approver = new HashSet<Approver>();
            this.OK_Inkapacity = new HashSet<OK_Inkapacity>();
        }
    
        public int id { get; set; }
        public string itab_n { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Otch { get; set; }
        public Nullable<System.DateTime> BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public bool SexBit { get; set; }
        public Nullable<int> idGrazd { get; set; }
        public Nullable<int> idSemPol { get; set; }
        public int SeverKoeff { get; set; }
        public int RayonKoeff { get; set; }
        public System.Guid GUID { get; set; }
        public byte[] EmployeeSid { get; set; }
        public string EmployeeLogin { get; set; }
        public bool AllowBirthdate { get; set; }
        public string paspser { get; set; }
        public string paspnomer { get; set; }
        public Nullable<System.DateTime> paspdate { get; set; }
        public string paspkem { get; set; }
        public string inn { get; set; }
        public string ssgps { get; set; }
        public string medpolis { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeSmallName { get; set; }
        public Nullable<System.DateTime> OtpReducedFareDateBegin { get; set; }
    
        public virtual ICollection<Approver> Approver { get; set; }
        public virtual ICollection<OK_Inkapacity> OK_Inkapacity { get; set; }
    }
}
