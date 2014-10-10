using System;
using System.Collections.Generic;
using TS.Domain.SubjectArea;

namespace TS.Domain.Repository
{
    interface ITimeSheetRepository
    {
        //
        SubjectArea.Approver GetCurrentApprover(string employeeLogin, bool isAdmin = false);
        
        //
        IEnumerable<SubjectArea.Department> DepartmentsList();
        bool UpdateDepartment(SubjectArea.Department department);
        IEnumerable<DepartmentEmployee> GetDepartmentEmployees(int idDepartment, DateTime? dateStartPeriod=null, DateTime? dateEndPeriod=null);
        
        //
        TimeSheet GetTimeSheet(int idTimeSheet, bool isEmpty = false);
        IEnumerable<TimeSheet> GetTimeSheetList(int idDepartment, int koll = 0, bool isEmpty = false);
        bool InsertTimeSheet(TimeSheet timeSheet);
        bool DeleteTimeSheet(int id);
        bool UpdateTimeSheetRecords(IEnumerable<TimeSheetEmployee> recordsForEdit);
        IEnumerable<ApproveRecord> GetTimeSheetApproveHistory(int idTimeSheet);
        
        //
        //IEnumerable<ApproveDepartment> GetDepartmentApprover(int idDepartment);
        IEnumerable<ApproveDepartment> GetDepartmentApprover(int idDepartment, ApproverType approverType = null);
        bool AddApproverForDepartment(int idEmployee, int idDepartment, ApproverType approverType);
        bool DellApproverForDepartment(ApproveDepartment approver);
       
        //
        IEnumerable<ExceptionDay> ExeptionsDays();
        bool InsertExeptionsDay(ExceptionDay exceptionDay);
        bool UpdateExeptionsDay(ExceptionDay exceptionDay);
        bool DeleteExeptionsDay(int idExceptionDay);
        
        //
        bool ApprovTimeSheet(int idTimeSheet, ApproveRecord record);
        
        //
        IEnumerable<DayStatus> GetDayStatusList();
        IEnumerable<WorkShedule> GetWorkScheduleList();
    }
}
