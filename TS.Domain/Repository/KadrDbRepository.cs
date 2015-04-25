using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using TS.Domain.SubjectArea;

namespace TS.Domain.Repository
{
    class KadrDbRepository:ITimeSheetRepository
    {
        public SubjectArea.Approver GetCurrentApprover(string employeeLogin, bool isAdmin = false)
        {
            using (var db = new KadrEntities())
            {
                if (!string.IsNullOrWhiteSpace(employeeLogin))
                {
                    var empl = db.Employee.SingleOrDefault(s => s.EmployeeLogin.ToLower() == employeeLogin.ToLower());
                    if (empl != null)
                    {

                        var appr = db.Approver.Where(w => w.idEmployee == empl.id).ToArray();

                        //var depsId = appr.Select(s => s.idDepartment).Distinct();


                        
                        //var deps =
                        //    db.Department.Where(w => depsId.Contains(w.id)).Select(s => new SubjectArea.Department
                        //    {
                        //        IdDepartment = s.id,
                        //        IdManagerDepartment = s.idManagerDepartment,
                        //        Name = s.DepartmentName,
                        //        DepartmentSmallName = s.DepartmentSmallName,
                        //        HasTimeSheet = s.HasTimeSheet,
                        //    });

                        var approver = new SubjectArea.Approver
                        {
                            IdEmployee = empl.id,
                            ItabNo = empl.itab_n,
                            Login = empl.EmployeeLogin,
                            Name = empl.FirstName,
                            Surname = empl.LastName,
                            Patronymic = empl.Otch,
                            SexBit = empl.SexBit,
                            ApproveDepartments = appr.Select(s=>new ApproveDepartment
                            {
                                IdApproveDepartment = s.id,
                                IdDepartment = s.idDepartment,
                                
                            })
                        };





                        return approver;
                    }
                }
            }
            throw new System.Exception("В системе отсутствует согласователь сотвествующий текущей учётной записи.");
            //throw new NotImplementedException();
        }

        IEnumerable<SubjectArea.Department> ITimeSheetRepository.DepartmentsList()
        {
            throw new NotImplementedException();
        }

        public bool UpdateDepartment(SubjectArea.Department department)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Department> DepartmentsList()
        {
            throw new NotImplementedException();
        }

        public bool UpdateDepartment(Department department)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DepartmentEmployee> GetDepartmentEmployees(int idDepartment, DateTime? dateStartPeriod = null, DateTime? dateEndPeriod = null)
        {
            throw new NotImplementedException();
        }

        public TimeSheet GetTimeSheet(int idTimeSheet, bool isEmpty = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TimeSheet> GetTimeSheetList(int idDepartment, int koll = 0, bool isEmpty = false)
        {
            throw new NotImplementedException();
        }

        public bool InsertTimeSheet(TimeSheet timeSheet)
        {
            throw new NotImplementedException();
        }

        public bool DeleteTimeSheet(int id)
        {
            throw new NotImplementedException();
        }

        public bool UpdateTimeSheetRecords(IEnumerable<TimeSheetEmployee> recordsForEdit)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ApproveRecord> GetTimeSheetApproveHistory(int idTimeSheet)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ApproveDepartment> GetDepartmentApprover(int idDepartment, ApproverType approverType = null)
        {
            throw new NotImplementedException();
        }

        public bool AddApproverForDepartment(int idEmployee, int idDepartment, ApproverType approverType)
        {
            throw new NotImplementedException();
        }

        public bool DellApproverForDepartment(ApproveDepartment approver)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ExceptionDay> ExeptionsDays()
        {
            throw new NotImplementedException();
        }

        public bool InsertExeptionsDay(ExceptionDay exceptionDay)
        {
            throw new NotImplementedException();
        }

        public bool UpdateExeptionsDay(ExceptionDay exceptionDay)
        {
            throw new NotImplementedException();
        }

        public bool DeleteExeptionsDay(int idExceptionDay)
        {
            throw new NotImplementedException();
        }

        public bool ApprovTimeSheet(int idTimeSheet, ApproveRecord record)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DayStatus> GetDayStatusList()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<WorkShedule> GetWorkScheduleList()
        {
            throw new NotImplementedException();
        }
    }
}
