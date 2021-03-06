﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Linq;
using System.Linq;
using System.Web.Providers.Entities;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Schema;

namespace TimeSheetMvc4WebApplication.ClassesDTO
{
    public static class DtoClassConstructor
    {
        private const int IdKadrDepartment = 154; //Отдел "Управления кадрами"
        private const int IdHoliday = 17; //праздничный день
        private const int IdTypeWorkPrimary = 1; //основной вид работы


        public static DtoEmployee DtoEmployee(Employee employee)
        {
            return new DtoEmployee
            {
                IdEmployee = employee.id,
                EmployeeLogin = employee.EmployeeLogin,
                Name = employee.FirstName,
                Surname = employee.LastName,
                Patronymic = employee.Otch,
                ItabN = employee.itab_n,
                SexBit = employee.SexBit
            };
        }

        public static DtoExceptionDay DtoExceptionDay(Exception exception)
        {
            var cult = System.Globalization.CultureInfo.GetCultureInfo("ru-Ru");
            return new DtoExceptionDay
            {
                IdExceptionDay = exception.id,
                DayStatus = DtoDayStatus(exception.DayStatus),
                WorkShedule = DtoWorkShedule(exception.WorkShedule),
                Name = exception.ExceptionName,
                DayAweek = exception.DateException.ToString("ddd.", cult),
                GNS = exception.KolHourGNS ?? 0,
                GPS = exception.KolHourGPS ?? 0,
                MNS = exception.KolHourMNS ?? 0,
                MPS = exception.KolHourMPS ?? 0,
                Date = exception.DateException
            }; 
        }

        public static DtoTimeSheetApprover DtoTimeSheetApprover(KadrDataContext db, int idTimeSheet, int approverNumber, int timeSheetApprovalStep)
        {
            var idDepartment = db.TimeSheet.Where(w => w.id == idTimeSheet).Select(s => s.idDepartment).FirstOrDefault();
            DateTime? approverDate=null;
            Approver approver;
            //Если текущий шаг не согласован то подтягиваем текущих согласователей cтруктурного подразделения
            if (timeSheetApprovalStep < approverNumber)
            {
                approver =
                    db.Approver.FirstOrDefault(
                        f =>
                        f.idDepartment == idDepartment && f.ApproverType.ApproveNumber == approverNumber &&
                        f.DateEnd == null);
            }
            //Если текущий шаг согласован то подтягиваем согласовавшее лицо.
            else
            {
                var timeSheetApproval =
                    db.TimeSheetApproval.Where(
                        f => f.idTimeSheet == idTimeSheet && f.Approver.ApproverType.ApproveNumber == approverNumber);

                if (timeSheetApproval.Any())
                {
                    var tsa = timeSheetApproval.First(f => f.ApprovalDate == timeSheetApproval.Max(m => m.ApprovalDate));
                    approver = tsa.Approver;
                    approverDate = tsa.ApprovalDate;
                }
                else approver = new Approver();
            }
            if (approver == null) return null;
            if (approverNumber == 3) idDepartment = IdKadrDepartment;

            //выбрать должность согласователя
            var fs = //сначала с учетом департамента
                db.FactStaffs.Where(
                    w =>
                        w.idEmployee == approver.idEmployee & w.PlanStaff.idDepartment == idDepartment &
                        ((w.DateEnd == null) || (w.DateEnd >= DateTime.Today))
                        & (Convert.ToInt32(w.PlanStaff.Post.ManagerBit) == db.FactStaffs
                                .Where(f => f.idEmployee == approver.idEmployee & f.PlanStaff.idDepartment == idDepartment &
                        ((f.DateEnd == null) || (f.DateEnd >= DateTime.Today))).Max(m=> Convert.ToInt32(m.PlanStaff.Post.ManagerBit)))
                       );

            if (!fs.Any()) //если согласователь не из текущего департамента, то грузим из другого
           {
                fs = db.FactStaffs.Where( w => w.idEmployee == approver.idEmployee 
                                                && ((w.DateEnd == null) || (w.DateEnd > DateTime.Today))
                                                && (Convert.ToInt32(w.PlanStaff.Post.ManagerBit) == db.FactStaffs.Where(f => f.idEmployee == approver.idEmployee 
                                                                                                                                && ((f.DateEnd == null) || (f.DateEnd > DateTime.Today)))
                                                                                                                 .Max(m => Convert.ToInt32(m.PlanStaff.Post.ManagerBit))));
            }

            //вытаскиваем основную должность в случае если factstaff'ов больше одного, странно что IdEmployee = id factstaff'а (зы: не исправил, мало ли так задумано)
            var factStaffs =
                    fs.Where(w => w.FactStaffWithHistory.idTypeWork == IdTypeWorkPrimary 
                    && (w.FactStaffWithHistory.DateEnd == null || w.FactStaffWithHistory.DateEnd >= DateTime.Today))
                    .Select(s => new DtoTimeSheetApprover
                    {
                        AppoverNumber = (int)approver.ApproverType.ApproveNumber,
                        EmployeeLogin = approver.Employee.EmployeeLogin,
                        IdEmployee = s.id,
                        IdFactStaff = s.id,
                        ItabN = s.Employee.itab_n,
                        Name = s.Employee.FirstName,
                        Surname = s.Employee.LastName,
                        Patronymic = s.Employee.Otch,
                        Post = DtoPost(s.PlanStaff.Post),
                        ApproverDate = approverDate
                    }).FirstOrDefault() 
                    ??
                    fs.Select(s => new DtoTimeSheetApprover
                    {
                        AppoverNumber = (int)approver.ApproverType.ApproveNumber,
                        EmployeeLogin = approver.Employee.EmployeeLogin,
                        IdEmployee = s.id,
                        IdFactStaff = s.id,
                        ItabN = s.Employee.itab_n,
                        Name = s.Employee.FirstName,
                        Surname = s.Employee.LastName,
                        Patronymic = s.Employee.Otch,
                        Post = DtoPost(s.PlanStaff.Post),
                        ApproverDate = approverDate
                    }).FirstOrDefault();

            return factStaffs;
        }

        public static DtoApprover DtoApprover(KadrDataContext db, int idEmployee, bool isAdmin = false)
        {
            var approver = db.Employees.Where(w => w.id == idEmployee).
                Select(s => new DtoApprover
                {
                    IdEmployee = s.id,
                    EmployeeLogin = s.EmployeeLogin,
                    Name = s.FirstName,
                    Surname = s.LastName,
                    Patronymic = s.Otch,
                    ItabN = s.itab_n,
                    SexBit = s.SexBit,
                    DtoApproverDepartments = db.Approver.Where(w => w.idEmployee == idEmployee && (w.DateEnd == null || w.DateEnd > DateTime.Now)).
                        Select(sa => DtoApproverDepartment(sa)).ToArray()
                }).FirstOrDefault();
            if (approver != null)
            {
                if (approver.DtoApproverDepartments != null)
                {
                    if (isAdmin)
                    {
                        var idApprover =
                            db.Approver.Where(w => w.idEmployee == approver.IdEmployee).Select(s => s.id).FirstOrDefault();
                        approver.DtoApproverDepartments = db.Approver.Where(w => (w.DateEnd == null || w.DateEnd > DateTime.Now) && w.Dep.HasTimeSheet).DistinctBy(d => d.idDepartment)
                                .Select(DtoApproverDepartment).OrderBy(o => o.DepartmentSmallName).ToArray();

                        foreach (var department in approver.DtoApproverDepartments)
                        {
                            department.ApproveNumber = 10;
                            department.ApproveTypeName = "Администратор";
                            department.IdApprover = idApprover;
                        }
                    }
                    return approver;
                }
                return null;
            }
            return null;
        }

        //Для пользователей на AD
        public static DtoApprover DtoApproverIfNullAndAdmin(KadrDataContext db, bool isAdmin = false)
        {
            var approver = new DtoApprover();
            if (!isAdmin) return approver;
            approver.DtoApproverDepartments =
                db.Approver.Where(w => w.DateEnd == null && w.Dep.HasTimeSheet).DistinctBy(d => d.idDepartment)
                    .Select(DtoApproverDepartment).OrderBy(o => o.DepartmentSmallName).ToArray();

            foreach (var department in approver.DtoApproverDepartments)
            {
                department.ApproveNumber = 10;
                department.ApproveTypeName = "Администратор";
            }
            return approver;
        }

        public static DtoDepartment DtoDepartment(KadrDataContext db, int idDepartment)
        {
            var dep = db.Department.FirstOrDefault(w => w.id == idDepartment);
            if(dep==null) throw new System.Exception("Запрашиваемый отдел не найден. IdDep="+idDepartment);
            return new DtoDepartment
            {
                IdDepartment = dep.id,
                DepartmentFullName = dep.DepartmentName,
                DepartmentSmallName = dep.DepartmentSmallName,
                IdManagerDepartment = dep.idManagerDepartment,
                HasTimeSheet = dep.HasTimeSheet
            };
        }

        public static DtoDepartment DtoDepartment(Department dep)
        {
            return new DtoDepartment
            {
                IdDepartment = dep.id,
                DepartmentFullName = dep.DepartmentName,
                DepartmentSmallName = dep.DepartmentSmallName,
                IdManagerDepartment = dep.idManagerDepartment,
                HasTimeSheet = dep.HasTimeSheet
            };
        }

        //Old version
        public static DtoApproverDepartment DtoApproverDepartment(KadrDataContext db, int idApprover)
        {
            return db.Approver.Where(w => w.id == idApprover).Select(s => new DtoApproverDepartment
            {
                IdDepartment = s.idDepartment,
                ApproveNumber = (int)s.ApproverType.ApproveNumber,
                ApproveTypeName = s.ApproverType.ApproverTypeName,
                DepartmentFullName = s.Dep.Department.DepartmentName,
                DepartmentSmallName = s.Dep.Department.DepartmentSmallName,
                IdManagerDepartment = s.Dep.Department.idManagerDepartment,
                IdApprover = s.id
            }).FirstOrDefault();
        }

        public static DtoApproverDepartment DtoApproverDepartment(Approver approver)
        {
            return new DtoApproverDepartment
            {
                IdDepartment = approver.idDepartment,
                ApproveNumber = approver.ApproverType.ApproveNumber,
                ApproveTypeName = approver.ApproverType.ApproverTypeName,
                DepartmentFullName = approver.Dep.Department.DepartmentName,
                DepartmentSmallName = approver.Dep.Department.DepartmentSmallName,
                IdManagerDepartment = approver.Dep.Department.idManagerDepartment,
                IdApprover = approver.id
            };
        }

        public static DtoTimeSheet DtoTimeSheet(KadrDataContext db, int idTimeSheet, bool isEmpty = false)
        {
            var service = new TimeSheetService();
            var timeSheet = db.TimeSheet.Where(w => w.id == idTimeSheet).Select(s => new DtoTimeSheet
            {
                IdTimeSheet = s.id,
                DateBegin = s.DateBeginPeriod,
                DateEnd = s.DateEndPeriod,
                DateComposition = s.DateComposition,
                Department = DtoDepartment(db, s.idDepartment),
                ApproveStep = service.GetTimeSheetApproveStep(idTimeSheet),
                IsFake = s.IsFake,
                Holidays = db.Exception.Where(e=>(e.DateException >= s.DateBeginPeriod) && (e.DateException <= s.DateEndPeriod)
                            && (e.WorkShedule.AllowNight)
                            && (e.idDayStatus == IdHoliday)).Select(t=> DtoExceptionDay(t)).ToArray(),
                EmployeesCount = db.TimeSheetRecords.Where(we => we.idTimeSheet == idTimeSheet)
                        .Select(ec => ec.idFactStaffHistory)
                        .Distinct()
                        .Count()
            }).FirstOrDefault();
            if (timeSheet == null) return null;
            if (isEmpty) return timeSheet;
            timeSheet.Approvers =
                Enumerable.Range(1, 3)
                    .Select(s => DtoTimeSheetApprover(db, idTimeSheet, s, timeSheet.ApproveStep))
                    .ToArray();
            timeSheet.Employees = db.TimeSheetRecords.Where(we => we.idTimeSheet == idTimeSheet)
                .Select(se => se.idFactStaffHistory)
                .Distinct()
                .Select(se => DtoTimeSheetEmployee(db, idTimeSheet, se))
                .ToArray().OrderByDescending(o => o.FactStaffEmployee.Post.IsMenager).
                ThenBy(t => t.FactStaffEmployee.Post.Category.OrderBy).
                ThenBy(o => o.FactStaffEmployee.Surname).
                ToArray();
            return timeSheet;
        }

        public static DtoTimeSheet DtoTimeSheet(TimeSheetView ts, bool isEmpty = false)
        {
            var timeSheet = new DtoTimeSheet
            {
                IdTimeSheet = ts.id,
                DateBegin = ts.DateBeginPeriod,
                DateEnd = ts.DateEndPeriod,
                DateComposition = ts.DateComposition,
                Department = DtoDepartment(ts.Dep.Department),
                ApproveStep = ts.ApproveStep,
                IsFake = ts.IsFake,
                EmployeesCount = ts.EmployeeCount
            };
            if (isEmpty) return timeSheet;
            var rec = ts.TimeSheetRecord.ToArray();
            var employees = rec.DistinctBy(d => d.idFactStaffHistory);
            timeSheet.Employees = employees.Select(s => DtoTimeSheetEmployee(s.FactStaffHistory,rec.Where(w=>w.idFactStaffHistory==s.idFactStaffHistory))).ToArray();

            return timeSheet;
        }

        public static DtoTimeSheet DtoTimeSheet(TimeSheetView ts)
        {
            var timeSheet = new DtoTimeSheet
            {
                IdTimeSheet = ts.id,
                DateBegin = ts.DateBeginPeriod,
                DateEnd = ts.DateEndPeriod,
                DateComposition = ts.DateComposition,
                Department = DtoDepartment(ts.Dep.Department),
                IsFake = ts.IsFake,
                EmployeesCount = ts.EmployeeCount,
                ApproveStep = ts.ApproveStep
            };
            return timeSheet;
        }

        public static DtoTimeSheetEmployee DtoTimeSheetEmployee(KadrDataContext db, int idTimeSheet, int idFactStaffHistory)
        {
            var records =
                db.TimeSheetRecords.Where(w => w.idTimeSheet == idTimeSheet & w.idFactStaffHistory == idFactStaffHistory).Select(
                    s => DtoTimeSheetRecord(s)).ToArray();

           // records
            var emp = new DtoTimeSheetEmployee
            {
                FactStaffEmployee = DtoFactStaffEmployee(db, idFactStaffHistory),
                Records = records,
                IdTimeSheet = idTimeSheet
            };
            return emp;
        }

        public static DtoTimeSheetEmployee DtoTimeSheetEmployee(FactStaffHistory staff, IEnumerable<TimeSheetRecord> records)
        {
            var singleRecord = records.First();
            return new DtoTimeSheetEmployee
            {
                FactStaffEmployee = DtoFactStaffEmployee(staff),
                Records = records.Select(s => DtoTimeSheetRecord(s)).ToArray(),
                IdTimeSheet = singleRecord.idTimeSheet
            };
        }

        public static DtoTimeSheetRecord DtoTimeSheetRecord(TimeSheetRecord timeSheetRecord)
        {
            var cult = System.Globalization.CultureInfo.GetCultureInfo("ru-Ru");
            return new DtoTimeSheetRecord
                       {
                           IdTimeSheetRecord = timeSheetRecord.IdTimeSheetRecord,
                           Date = timeSheetRecord.RecordDate,
                           JobTimeCount = timeSheetRecord.JobTimeCount,
                           NightTimeCount = (timeSheetRecord.NightTimeCount > 0) ? timeSheetRecord.NightTimeCount.ToString() : "" ,
                           DayAweek = timeSheetRecord.RecordDate.ToString("ddd.", cult),
                           DayStays = DtoDayStatus(timeSheetRecord.DayStatus),
                           IsChecked = timeSheetRecord.IsChecked != null && (bool)timeSheetRecord.IsChecked
                       };
        }

        public static DtoDayStatus DtoDayStatus(DayStatus dayStatus)
        {
            return new DtoDayStatus
                       {
                           IdDayStatus = dayStatus.id,
                           SmallDayStatusName = dayStatus.DayStatusName,
                           FullDayStatusName = dayStatus.DayStatusFullName,
                           IsVisible = dayStatus.IsVisible
                       };
        }

        public static DtoFactStaffEmployee DtoFactStaffEmployee(KadrDataContext db, int idFactStaffHistory)
        {
            var fsE = 
            db.FactStaffHistories.Where(w => w.id == idFactStaffHistory).Select(s => new DtoFactStaffEmployee
            {
                IdFactStaffHistiry = s.id,
                IdFactStaff = s.FactStaff.id,
                IdEmployee = (int) s.FactStaff.idEmployee,
                EmployeeLogin = s.FactStaff.Employee.EmployeeLogin,
                Surname = s.FactStaff.Employee.LastName,
                Name = s.FactStaff.Employee.FirstName,
                Patronymic = s.FactStaff.Employee.Otch,
                ItabN = s.FactStaff.Employee.itab_n,
                StaffRate = s.StaffCount,
                Post = DtoPost(s.FactStaff.PlanStaff.Post),
                WorkShedule = DtoWorkShedule(s.FactStaff.WorkShedule ?? s.FactStaff.PlanStaff.WorkShedule),
                IsPersonalShedule = s.FactStaff.WorkShedule != null
            }).FirstOrDefault();
            return fsE;
        }

        public static DtoFactStaffEmployee DtoFactStaffEmployee(FactStaffHistory factStaff)
        {
                return new DtoFactStaffEmployee
                {
                    IdFactStaffHistiry = factStaff.id,
                    IdFactStaff = factStaff.FactStaff.id,
                    IdEmployee = (int) factStaff.FactStaff.idEmployee,
                    EmployeeLogin = factStaff.FactStaff.Employee.EmployeeLogin,
                    Surname = factStaff.FactStaff.Employee.LastName,
                    Name = factStaff.FactStaff.Employee.FirstName,
                    Patronymic = factStaff.FactStaff.Employee.Otch,
                    ItabN = factStaff.FactStaff.Employee.itab_n,
                    StaffRate = factStaff.StaffCount,
                    Post = DtoPost(factStaff.FactStaff.PlanStaff.Post),
                    WorkShedule = DtoWorkShedule(factStaff.FactStaff.WorkShedule ?? factStaff.FactStaff.PlanStaff.WorkShedule),
                    IsPersonalShedule = factStaff.FactStaff.WorkShedule != null,
                    IdPlanStaff = factStaff.FactStaff.idPlanStaff,
                    HoursWeek = (double)( factStaff.WorkHoursInWeek ?? 0)
                };
        }

        public static DtoWorkShedule DtoWorkShedule(WorkShedule workShedule)
        {
            return new DtoWorkShedule
                       {
                           IdWorkShedule = workShedule.id,
                           WorkSheduleName = workShedule.NameShedule,
                           AllowNight = workShedule.AllowNight
            };
        }

        public static DtoPost DtoPost(Post post)
        {
            return new DtoPost
                       {
                           IdPost = post.id,
                           PostSmallName = post.PostShortName,
                           PostFullName = post.PostName,
                           IsMenager = post.ManagerBit,
                           Category = DtoCategory(post.Category)
                       };
        }

        public static DtoCategory DtoCategory(Category category)
        {
            return new DtoCategory
                       {
                           IdCategory = category.id,
                           CategoryFullName = category.CategoryName,
                           CategorySmallName = category.CategorySmallName,
                           IsPPS = category.IsPPS ?? false,
                           OrderBy = category.OrderBy
                       };
        }

    }
}