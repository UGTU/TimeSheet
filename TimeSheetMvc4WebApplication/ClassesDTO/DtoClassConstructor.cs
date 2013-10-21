using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeSheetMvc4WebApplication.ClassesDTO
{
    public static class DtoClassConstructor
    {
        private const int IdKadrDepartment = 154;

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
                    //approver =
                    //    timeSheetApproval.First(f => f.ApprovalDate == timeSheetApproval.Max(m => m.ApprovalDate)).
                    //        Approver;
                    var tsa = timeSheetApproval.First(f => f.ApprovalDate == timeSheetApproval.Max(m => m.ApprovalDate));
                    approver = tsa.Approver;
                    approverDate = tsa.ApprovalDate;
                }
                else approver = new Approver();
            }
            if (approverNumber == 3) idDepartment = IdKadrDepartment;
            var factStaffs =
                db.FactStaff.Where(w => w.idEmployee == approver.idEmployee & w.PlanStaff.idDepartment == idDepartment & w.DateEnd==null).
                    Select(s => new DtoTimeSheetApprover
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
                                        
                                        //,
                                        //StaffRate = db.FactStaffCurrent.Where(wf => wf.id == s.id).Select(sf => sf.StaffCount).FirstOrDefault(),
                                        //WorkShedule = new DtoWorkShedule
                                        //                  {
                                        //                      IdWorkShedule = s.WorkShedule.id,
                                        //                      WorkSheduleName = s.WorkShedule.NameShedule
                                        //                  }
                                    }).FirstOrDefault();
            return factStaffs;
        }

        public static DtoApprover DtoApprover(KadrDataContext db, int idEmployee)
        {
            const int administratorApproveNumber = 10;
            var approver = db.Employee.Where(w => w.id == idEmployee).
                Select(s => new DtoApprover
                {
                    IdEmployee = s.id,
                    EmployeeLogin = s.EmployeeLogin,
                    Name = s.FirstName,
                    Surname = s.LastName,
                    Patronymic = s.Otch,
                    ItabN = s.itab_n,
                    SexBit = s.SexBit,
                    DtoApproverDepartments = db.Approver.Where(w => w.idEmployee == idEmployee && w.DateEnd==null).
                        Select(sa => DtoApproverDepartment(db,sa.id)).ToArray()
                }).FirstOrDefault();
            if (approver != null)
            {
                if (approver.DtoApproverDepartments != null)
                {
                    approver.IsAdministrator = approver.DtoApproverDepartments.
                                                   FirstOrDefault(w => w.ApproveNumber == administratorApproveNumber) != null;
                    if (approver.IsAdministrator)
                    {
                        approver.DtoApproverDepartments =
                            db.Approver.Where(w => w.ApproverType.ApproveNumber == 1 & w.DateEnd==null).Select(
                                s => DtoApproverDepartment(db,s.id)).ToArray();
                        var idApprover =
                            db.Approver.Where(w => w.idEmployee == approver.IdEmployee).Select(s => s.id).FirstOrDefault();
                        foreach (var department in approver.DtoApproverDepartments)
                        {
                            department.ApproveNumber = 10;
                            department.ApproveTypeName = "Администратор";
                            department.IdApprover = idApprover;
                        }
                    }
                    return approver;
                }
                approver.IsAdministrator = false;
                return null;
            }
            return null;
        }

        public static DtoDepartment DtoDepartment(KadrDataContext db, int idDepartment)
        {
            return new DtoDepartment
            {
                IdDepartment = idDepartment,
                DepartmentFullName = db.Department.Where(w => w.id == idDepartment).Select(s => s.DepartmentName).FirstOrDefault(),
                DepartmentSmallName = db.Department.Where(w => w.id == idDepartment).Select(s => s.DepartmentSmallName).FirstOrDefault(),
                IdManagerDepartment = db.Department.Where(w => w.id == idDepartment).Select(s => s.idManagerDepartment).FirstOrDefault()
            };
        }

        public static DtoApproverDepartment DtoApproverDepartment(KadrDataContext db, int idApprover)
        {
            return db.Approver.Where(w => w.id == idApprover).Select(s => new DtoApproverDepartment
            {
                IdDepartment = s.idDepartment,
                ApproveNumber = (int)s.ApproverType.ApproveNumber,
                ApproveTypeName = s.ApproverType.ApproverTypeName,
                DepartmentFullName = db.Department.Where(w => w.id == s.idDepartment).Select(sd => sd.DepartmentName).FirstOrDefault(),
                DepartmentSmallName = db.Department.Where(w => w.id == s.idDepartment).Select(sd => sd.DepartmentSmallName).FirstOrDefault(),
                IdManagerDepartment = db.Department.Where(w => w.id == s.idDepartment).Select(sd => sd.idManagerDepartment).FirstOrDefault(),
                IdApprover = s.id
            }).FirstOrDefault();
        }

        public static DtoTimeSheet DtoTimeSheet(KadrDataContext db, int idTimeSheet,bool isEmpty=false)
        {
            var service = new TimeSheetService();
            var timeSheetApprovalStep = service.GetTimeSheetApproveStep(idTimeSheet);
            var approvers = new List<DtoTimeSheetApprover>();
            approvers.Add(DtoTimeSheetApprover(db, idTimeSheet, 1, timeSheetApprovalStep));
            approvers.Add(DtoTimeSheetApprover(db, idTimeSheet, 2, timeSheetApprovalStep));
            approvers.Add(DtoTimeSheetApprover(db, idTimeSheet, 3, timeSheetApprovalStep));
            DtoTimeSheet timeSheet = db.TimeSheet.Where(w => w.id == idTimeSheet).Select(s => new DtoTimeSheet
                {
                    IdTimeSheet = s.id,
                    DateBegin =
                        s.DateBeginPeriod,
                    DateEnd = s.DateEndPeriod,
                    DateComposition = timeSheetApprovalStep<3? DateTime.Now : db.TimeSheetApproval.Where(w=>w.idTimeSheet==idTimeSheet).Max(m=>m.ApprovalDate),
                    Department =
                        DtoDepartment(db, s.idDepartment),
                    Employees = !isEmpty?db.TimeSheetRecord.Where(we => we.idTimeSheet == idTimeSheet).Select(se => se.idFactStaffHistory).Distinct().Select(se => DtoTimeSheetEmployee(db, idTimeSheet, se)).ToArray():null,
                    Approvers = approvers.ToArray(),
                }).FirstOrDefault();
                if (timeSheet != null && timeSheet.Employees!=null)
                    timeSheet.Employees = timeSheet.Employees.
                        OrderByDescending(o => o.FactStaffEmployee.Post.IsMenager).
                        ThenBy(t => t.FactStaffEmployee.Post.Category.OrderBy).
                        ThenBy(o => o.FactStaffEmployee.Surname).
                        ToArray();
            timeSheet.ApproveStep = service.GetTimeSheetApproveStep(idTimeSheet);
            timeSheet.EmployeesCount =
                db.TimeSheetRecord.Where(we => we.idTimeSheet == idTimeSheet)
                    .Select(s => s.idFactStaffHistory)
                    .Distinct()
                    .Count();
            return timeSheet;
        }

        public static DtoTimeSheetEmployee DtoTimeSheetEmployee(KadrDataContext db, int idTimeSheet, int idFactStaffHistory)
        {
            var records =
                db.TimeSheetRecord.Where(w => w.idTimeSheet == idTimeSheet & w.idFactStaffHistory == idFactStaffHistory).Select(
                    s => DtoTimeSheetRecord(s)).ToArray();
            return new DtoTimeSheetEmployee
            {
                FactStaffEmployee = DtoFactStaffEmployee(db, idFactStaffHistory),
                Records = records,
                IsChecked = records.Any(w => w.IsChecked),
                IdTimeSheet = idTimeSheet
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
                           FullDayStatusName = dayStatus.DayStatusFullName
                       };
        }

        public static DtoFactStaffEmployee DtoFactStaffEmployee(KadrDataContext db, int idFactStaffHistory)
        {

            return db.FactStaffHistory.Where(w => w.id == idFactStaffHistory).Select(s => new DtoFactStaffEmployee
            {
                IdFactStaffHistiry = s.id,
                IdFactStaff = s.FactStaff.id,
                IdEmployee = s.FactStaff.idEmployee,
                EmployeeLogin = s.FactStaff.Employee.EmployeeLogin,
                Surname = s.FactStaff.Employee.LastName,
                Name = s.FactStaff.Employee.FirstName,
                Patronymic = s.FactStaff.Employee.Otch,
                ItabN = s.FactStaff.Employee.itab_n,
                StaffRate = s.StaffCount, //db.FactStaffHistory.Where(wf => wf.id == idFactStaffHistory).Select(sf => sf.StaffCount).FirstOrDefault(),
                Post = DtoPost(s.FactStaff.PlanStaff.Post),
                WorkShedule = DtoWorkShedule(s.FactStaff.PlanStaff.WorkShedule)
            }).FirstOrDefault();
        }

        public static DtoWorkShedule DtoWorkShedule(WorkShedule workShedule)
        {
            return new DtoWorkShedule
                       {
                           IdWorkShedule = workShedule.id,
                           WorkSheduleName = workShedule.NameShedule
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