using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using TimeSheetMvc4WebApplication.ClassesDTO;

namespace TimeSheetMvc4WebApplication.Models
{
    public class MessageModel
    {
        public string MessageTitile { get; set; }
        public string Message { get; set; }
    }

    public class TimeSheetModel
    {
        public int DocumentNumber { get; set; }
        public string DepartmentName { get; set; }
        public DateTime DateComposition { get; set; }
        public DateTime DateBeginPeriod { get; set; }
        public DateTime DateEndPeriod { get; set; }
        public PaperModel[] Papers { get; set; }
    }

    public class PaperModel
    {
        public EmployeeModel[] Employees { get; set; }
        public HeaderStyle[] HeaderStyle { get; set; }
        public bool IsFirst { get; set; }
        public bool IsLast { get; set; }
        public ApproverModel[] Approvers { get; set; }
        public int PaperNum { get; set; }
        public int PaperOf { get; set; }
        public int TimeSheetNum { get; set; }
    }

    public class HeaderStyle
    {
        public int Day { get; set; }
        public string CSS { get; set; }
        public string DayText { get; set; }
    }


    public class ApproverModel
    {
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string Post { get; set; }
        public int ApproveStep { get; set; }
        public DateTime? ApproveTime { get; set; }
        public string Login { get; set; }
    }

    public class EmployeeModel
    {
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public int EmployeeNumber { get; set; }
        public string Post { get; set; }
        public decimal StaffRate { get; set; }
        public EmployeeRecordModel[] Records { get; set; }
        public int FirstHalfMonthDays { get; set; }
        public double FirstHalfMonthHours { get; set; }
        public int SecondHalfMonthDays { get; set; }
        public double SecondHalfMonthHours { get; set; }
        //public int Days { get; set; }
        public int Days { get { return FirstHalfMonthDays + SecondHalfMonthDays; }}
        //public int MounthDays { get; set; }
        public int MounthDays { get { return Days + NonWorkedDays.Sum(s => s.Count); } }
        //public double Hours { get; set; }
        public double Hours { get { return FirstHalfMonthHours + SecondHalfMonthHours; } }
        //public EmployeeRecordModel В { get; set; }
        //public EmployeeRecordModel Б { get; set; }
        //public EmployeeRecordModel О { get; set; }
        //public EmployeeRecordModel ОЖ { get; set; }
        //public EmployeeRecordModel ОЗ { get; set; }
        //public EmployeeRecordModel ДО { get; set; }
        //public EmployeeRecordModel У { get; set; }
        //public EmployeeRecordModel К { get; set; }
        //public EmployeeRecordModel НН { get; set; }
        //public EmployeeRecordModel Р { get; set; }
        //public EmployeeRecordModel ПР { get; set; }
        //public EmployeeRecordModel УД { get; set; }
        //public EmployeeRecordModel ОВ { get; set; }
        public EmployeeRecordModel[] NonWorkedDays { get; set; }
    }

    public class EmployeeRecordModel
    {
        public int Day { get; set; }
        public string DayStatus { get; set; }
        public string Value { get; set; }
        
        //public string Value { get { return Count.ToString(CultureInfo.InvariantCulture); } }
        public int Count { get; set; } 
        public string CSS { get; set; }
    }

    public class TimeSheetAprovalModel
    {
        public int IdApprover { get; set; }
        public int IdTimeSheet { get; set; }
        public DateTime ApprovalDate { get; set; }
        [Required(ErrorMessage = "Укажите согласован ли табель!")]
        public bool? ApprovalResult { get; set; }
        public string Comment { get; set; }
    }
}