using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TimeSheetMvc4WebApplication.ClassesDTO
{
    [DataContract]
    public class DtoExceptionDay
    {
        [DataMember]
        public int IdExceptionDay { get; set; }
        [DataMember]
        public DateTime Date { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public float MPS { get; set; }
        [DataMember]
        public float MNS { get; set; }
        [DataMember]
        public float GPS { get; set; }
        [DataMember]
        public float GNS { get; set; }
        [DataMember]
        public DtoDayStatus DayStatus { get; set; }
        [DataMember]
        public DtoWorkShedule WorkShedule { get; set; }
        [DataMember]
        public string DayAweek { get; set; }

    }

    [DataContract]
    public class DtoEmployee
    {
        [DataMember]
        public int IdEmployee { get; set; }
        [DataMember]
        public string ItabN { get; set; }
        [DataMember]
        public string Surname { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Patronymic { get; set; }
        [DataMember]
        public string EmployeeLogin { get; set; }
        [DataMember]
        public bool SexBit { get; set; }
    }

    [DataContract]
    public class DtoApprover:DtoEmployee
    {
        [DataMember]
        public bool IsAdministrator { get; set; }

        [DataMember]
        public DtoApproverDepartment[] DtoApproverDepartments { get; set; }
    }

    [DataContract]
    public class DtoDepartment
    {
        [DataMember]
        public int IdDepartment { get; set; }
        [DataMember]
        public string DepartmentSmallName { get; set; }
        [DataMember]
        public string DepartmentFullName { get; set; }
        [DataMember]
        public int? IdManagerDepartment { get; set; }
    }

    [DataContract]
    public class DtoApproverDepartment:DtoDepartment
    {
        [DataMember]
        public int IdApprover { get; set; }
        [DataMember]
        public int ApproveNumber { get; set; }
        [DataMember]
        public string ApproveTypeName { get; set; }
    }

    [DataContract]
    public class DtoWorkShedule
    {
        [DataMember]
        public int IdWorkShedule { get; set; }
        [DataMember]
        public string WorkSheduleName { get; set; }
    }

    [DataContract]
    public class DtoCategory
    {
        [DataMember]
        public int IdCategory { get; set; }
        [DataMember]
        public string CategorySmallName { get; set; }
        [DataMember]
        public string CategoryFullName { get; set; }
        [DataMember]
        public int? OrderBy { get; set; }
        [DataMember]
        public bool IsPPS { get; set; }
    }

    [DataContract]
    public class DtoPost
    {
        [DataMember]
        public int IdPost { get; set; }
        [DataMember]
        public string PostFullName { get; set; }
        [DataMember]
        public string PostSmallName { get; set; }
        [DataMember]
        public bool IsMenager { get; set; }
        [DataMember]
        public DtoCategory Category { get; set; }  
    }

    [DataContract]
    public class DtoTimeSheet
    {
        [DataMember]
        public int IdTimeSheet { get; set; }
        [DataMember]
        public DtoTimeSheetEmployee[] Employees { get; set; }
        [DataMember]
        public DtoDepartment Department { get; set; }
        [DataMember]
        public DateTime DateBegin { get; set; }
        [DataMember]
        public DateTime DateEnd { get; set; }
        [DataMember]
        public DateTime DateComposition { get; set; }
        [DataMember]
        public DtoTimeSheetApprover[] Approvers { get; set; }
        [DataMember]
        public int EmployeesCount { get; set; }
        [DataMember]
        public int ApproveStep { get; set; }
    }

    [DataContract]
    public class DtoFactStaffEmployee:DtoEmployee
    {
        [DataMember]
        public int IdFactStaff { get; set; }

        [DataMember]
        public int IdFactStaffHistiry { get; set; } 
        [DataMember]
        public decimal StaffRate { get; set; }
        [DataMember]
        public DtoWorkShedule WorkShedule { get; set; }
        [DataMember]
        public DtoPost Post { get; set; }
        [DataMember]
        public bool IsCheked { get; set; }

    }

    [DataContract]
    public class DtoTimeSheetRecord
    {
        [DataMember]
        public Guid IdTimeSheetRecord;
        [DataMember]
        public double JobTimeCount;
        [DataMember]
        public DtoDayStatus DayStays;
        [DataMember]
        public DateTime Date;
        [DataMember]
        public string DayAweek;
        [DataMember]
        public bool IsChecked;

    }

    [DataContract]
    public class DtoDayStatus
    {
        [DataMember]
        public int IdDayStatus { get; set; }
        [DataMember]
        public string SmallDayStatusName { get; set; }
        [DataMember]
        public string FullDayStatusName { get; set; }
    }

    [DataContract]
    public class DtoTimeSheetEmployee
    {
        [DataMember]
        public DtoFactStaffEmployee FactStaffEmployee { get; set; }

        [DataMember]
        public DtoTimeSheetRecord[] Records { get; set; }

        [DataMember]
        public bool IsChecked;

        [DataMember]
        public int IdTimeSheet;
    }

    [DataContract]
    public class DtoTimeSheetApprover:DtoFactStaffEmployee
    {
        [DataMember]
        public int AppoverNumber { get; set; }
        [DataMember]
        public DateTime? ApproverDate { get; set; }
    }

    [DataContract]
    public class DtoTimeSheetApproveHistiry
    {
        [DataMember]
        public string AppoverName { get; set; }
        [DataMember]
        public string AppoverStatus { get; set; }
        [DataMember]
        public string AppoverComment { get; set; }
        [DataMember]
        public string AppoverVisa { get; set; }
        [DataMember]
        public DateTime AppoverDate { get; set; }
    }

    [DataContract]
    public class DtoMessage
    {
        [DataMember]
        public bool Result { get; set; }

        [DataMember]
        public string Message { get; set; }
    }

}