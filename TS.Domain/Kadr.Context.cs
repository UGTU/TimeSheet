﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class KadrEntities : DbContext
    {
        public KadrEntities()
            : base("name=KadrEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<Employee> Employee { get; set; }
        public virtual DbSet<Post> Post { get; set; }
        public virtual DbSet<PostType> PostType { get; set; }
        public virtual DbSet<ApperoveDepartment> ApperoveDepartment { get; set; }
        public virtual DbSet<Approver> Approver { get; set; }
        public virtual DbSet<ApproverType> ApproverType { get; set; }
        public virtual DbSet<DayStatus> DayStatus { get; set; }
        public virtual DbSet<EmployeeEvent> EmployeeEvent { get; set; }
        public virtual DbSet<EventSuperType> EventSuperType { get; set; }
        public virtual DbSet<Exception> Exception { get; set; }
        public virtual DbSet<RepeatRate> RepeatRate { get; set; }
        public virtual DbSet<TimeSheet> TimeSheet { get; set; }
        public virtual DbSet<TimeSheetApproval> TimeSheetApproval { get; set; }
        public virtual DbSet<TimeSheetRecord> TimeSheetRecord { get; set; }
        public virtual DbSet<WorkShedule> WorkShedule { get; set; }
        public virtual DbSet<WorkSheduleEvent> WorkSheduleEvent { get; set; }
        public virtual DbSet<Department> Department { get; set; }
        public virtual DbSet<FactStaffCurrent> FactStaffCurrent { get; set; }
        public virtual DbSet<FactStaffCurrentMainData> FactStaffCurrentMainData { get; set; }
        public virtual DbSet<FactStaffWithHistory> FactStaffWithHistory { get; set; }
        public virtual DbSet<TimeSheetView> TimeSheetView { get; set; }
        public virtual DbSet<OK_Inkapacity> OK_Inkapacity { get; set; }
    
        public virtual int AddEmployeeLogin(Nullable<int> id, string login)
        {
            var idParameter = id.HasValue ?
                new ObjectParameter("id", id) :
                new ObjectParameter("id", typeof(int));
    
            var loginParameter = login != null ?
                new ObjectParameter("login", login) :
                new ObjectParameter("login", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("AddEmployeeLogin", idParameter, loginParameter);
        }
    
        public virtual int TimeSheetRecordInsert(string validXMLInput)
        {
            var validXMLInputParameter = validXMLInput != null ?
                new ObjectParameter("ValidXMLInput", validXMLInput) :
                new ObjectParameter("ValidXMLInput", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("TimeSheetRecordInsert", validXMLInputParameter);
        }
    }
}
