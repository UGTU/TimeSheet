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
    
    public partial class Department
    {
        public int id { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentSmallName { get; set; }
        public Nullable<int> idDepartmentType { get; set; }
        public Nullable<int> idManagerDepartment { get; set; }
        public Nullable<int> KadrID { get; set; }
        public System.DateTime dateCreate { get; set; }
        public Nullable<System.DateTime> dateExit { get; set; }
        public Nullable<int> idManagerPlanStaff { get; set; }
        public int idBeginPrikaz { get; set; }
        public Nullable<int> idEndPrikaz { get; set; }
        public Nullable<int> SeverKoeff { get; set; }
        public Nullable<int> RayonKoeff { get; set; }
        public System.Guid DepartmentGUID { get; set; }
        public Nullable<int> idFundingCenter { get; set; }
        public string DepartmentIndex { get; set; }
        public string DepPhoneNumber { get; set; }
        public bool HasTimeSheet { get; set; }
    }
}