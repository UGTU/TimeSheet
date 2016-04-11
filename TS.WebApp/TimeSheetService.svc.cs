using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Threading.Tasks;
using System.Web.Mvc;
using TimeSheetMvc4WebApplication.ClassesDTO;
using TimeSheetMvc4WebApplication.Source;

namespace TimeSheetMvc4WebApplication
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class TimeSheetService
    {
        /// <summary>
        /// Розвращает текущего пользователя системы
        /// </summary>
        /// <param name="employeeLogin">Логин пользователя в системе</param>
        /// <param name="isAdmin"></param>
        /// <returns></returns>
        [OperationContract]
        [Authorize]
        public DtoApprover GetCurrentApproverByLogin(string employeeLogin, bool isAdmin = false)
        {
            employeeLogin = UserNameAdapter.Adapt(employeeLogin);
            if (string.IsNullOrWhiteSpace(employeeLogin)) return null;
            using (var db = new KadrDataContext())
            {
                var idEmployee =
                    db.Employees.FirstOrDefault(w => w.EmployeeLogin.ToLower() == employeeLogin.ToLower());
                return idEmployee != null ? DtoClassConstructor.DtoApprover(db, idEmployee.id, isAdmin):
                    DtoClassConstructor.DtoApproverIfNullAndAdmin(db,isAdmin);
            }
        }

        //==========        Работа с структурными подразделениями

        /// <summary>
        /// Возвращает список всех структурных подразделений
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public DtoDepartment[] GetDepartmentsList()
        {
            using (var db = new KadrDataContext())
            {
                return
                    db.Department.Where(w => w.dateExit == null | w.dateExit> DateTime.Now)
                        .Select(s => DtoClassConstructor.DtoDepartment(db, s.id))
                        .ToArray();
            }
        }

        public bool UpdateDepartment(DtoDepartment department)
        {
            using (var db = new KadrDataContext())
            {
                try
                {
                    var dep = db.Dep.FirstOrDefault(f => f.id == department.IdDepartment);
                    if (dep != null)
                    {
                        dep.HasTimeSheet = department.HasTimeSheet;
                        db.SubmitChanges();
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Возвращает список всех текущих сотрудников структурного подразделения
        /// </summary>
        /// <param name="idDepartment">Идентификатор структурного подразделения</param>
        /// <returns></returns>
        [OperationContract]
        public DtoEmployee[] GetDepartmentEmployees(int idDepartment)
        {
            using (var db = new KadrDataContext())
            {
                return
                    db.FactStaffs.Where(w => w.PlanStaff.idDepartment == idDepartment & w.DateEnd == null).Select(
                        s => DtoClassConstructor.DtoEmployee(s.Employee)).Distinct().ToArray();
            }
        }

        /// <summary>
        /// Возвращает сотрудников структурного подразделения с их рабочими режимами
        /// </summary>
        [OperationContract]
        public DtoFactStaffEmployee[] GetDepartmentFactStaffs(int idDepartment)
        {
            using (var db = new KadrDataContext())
            {
                return
                    db.FactStaffs.Where(w => w.PlanStaff.idDepartment == idDepartment & w.DateEnd == null).Select(
                        s => DtoClassConstructor.DtoFactStaffEmployee(s.CurrentChange)).ToArray();
            }
        }

        /// <summary>
        /// Возвращает список режимов работы
        /// </summary>
        [OperationContract]
        public DtoWorkShedule[] GetWorkShedules()
        {
            using (var db = new KadrDataContext())
            {
                return db.WorkShedule.Select(s=> DtoClassConstructor.DtoWorkShedule(s)).ToArray();
            }
        }

        [OperationContract]
        public DtoFactStaffEmployee[] GetEmployeesForTimeSheet(int idDepartment, DtoApprover approver, DateTime dateStart, DateTime dateEnd)
        {
            using (var db = new KadrDataContext())
            {
                //var depsId = GetDepartmentsIdList(idDepartment);
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith((FactStaffWithHistory fswh) => fswh.PlanStaff);
                loadOptions.LoadWith((PlanStaff ps) => ps.Post);
                loadOptions.LoadWith((Post p) => p.Category);
                loadOptions.LoadWith((PlanStaff ps) => ps.WorkShedule);
                loadOptions.LoadWith((FactStaffWithHistory fswh) => fswh.Employee);
                loadOptions.LoadWith((OK_Otpusk oko) => oko.OK_Otpuskvid);
                db.LoadOptions = loadOptions;
                var ts = new TimeSheetManaget(idDepartment, dateStart, dateEnd, approver.EmployeeLogin, db);
                return
                    ts.GetAllEmployees()
                        .Select(s => DtoClassConstructor.DtoFactStaffEmployee(db, s.idFactStaffHistory))
                        .ToArray();
            }
        }

        //==========    Работа с табелем

        /// <summary>
        /// Определяет есть ли системе табель с таким идентификатором
        /// </summary>
        /// <param name="idTimeSheet">Идентификатор табеля</param>
        /// <returns>true если в системе есть табель с таким идентификатором</returns>
        [OperationContract]
        public bool IsAnyTimeSheetWithThisId(int idTimeSheet)
        {
            using (var db = new KadrDataContext())
                //using (var dbloger = new DataContextLoger("GetTimeSheetLog.txt", FileMode.OpenOrCreate, db))
            {
                return db.TimeSheet.Any(a => a.id == idTimeSheet);
            }
        }


        /// <summary>
        /// Возврвщвет табель рабочего времени
        /// </summary>
        /// <param name="idTimeSheet">Идентификатор табеля</param>
        /// <param name="isEmpty">Флаг указывающий возврвщвть ли табель пустым (только реквизиты табеля без записей) по умолчанию false</param>
        /// <returns>Табель рабочего времени</returns>
        [OperationContract]
        public DtoTimeSheet GetTimeSheet(int idTimeSheet, bool isEmpty = false)
        {
            using (var db = new KadrDataContext())
            //using (var dbloger = new DataContextLoger("GetTimeSheetLog.txt", FileMode.OpenOrCreate, db))
            {
                return DtoClassConstructor.DtoTimeSheet(db, idTimeSheet, isEmpty);
            }
        }

        /// <summary>
        /// Возвращает список табелей
        /// </summary>
        /// <param name="idDepartment">Идентификатор структурного подразделения</param>
        /// <param name="koll">Колличество возвращаемых табелей, при значений переметра равным или меньше 0 возвращаются все</param>
        /// <param name="isEmpty">Флаг указывающий возврвщвть ли табель пустым (только реквизиты табеля без записей) по умолчанию false</param>
        /// <returns>Список табелей</returns>
        [OperationContract]
        public DtoTimeSheet[] GetTimeSheetList(int idDepartment, int koll = 0, bool isEmpty = false)
        {
            using (var db = new KadrDataContext())
                //using (var dbloger = new DataContextLoger("GetTimeSheetListLog.txt", FileMode.OpenOrCreate, db))
            {
                return koll <= 0
                    ? db.TimeSheet.Where(w => w.idDepartment == idDepartment)
                        .Select(s => DtoClassConstructor.DtoTimeSheet(db, s.id, isEmpty)).ToArray()
                    : db.TimeSheet.Where(w => w.idDepartment == idDepartment)
                        .OrderByDescending(o => o.DateBeginPeriod).Take(koll)
                        .Select(s => DtoClassConstructor.DtoTimeSheet(db, s.id, isEmpty)).ToArray();
            }
        }



        public DtoTimeSheet CreateFakeTimeSheet(int idDepartment, DateTime dateStart, DtoApprover approver)
        {
            using (var db = new KadrDataContext())
            {
                if(db.TimeSheet.Any(a=>a.idDepartment==idDepartment&& a.DateBeginPeriod==dateStart))
                    throw new System.Exception("Табель на этот месяц уже сформирован.");
                var dtoApproverDepartment = approver.DtoApproverDepartments.FirstOrDefault(w => w.IdDepartment == idDepartment);
                var ts = new TimeSheet
                {
                    idDepartment = idDepartment,
                    DateComposition = DateTime.Now,
                    DateBeginPeriod = dateStart,
                    DateEndPeriod = dateStart.AddMonths(1).AddDays(-1),
                    ApproveStep = 0,
                    IsFake = true,
                    idCreater = dtoApproverDepartment.IdApprover
                };
                db.TimeSheet.InsertOnSubmit(ts);
                db.SubmitChanges();
                return DtoClassConstructor.DtoTimeSheet(db.TimeSheetView.Single(s => s.id == ts.id));
            }
        }

        public void DelFakeTimeSheet(int id)
        {
            using (var db = new KadrDataContext())
            {
                var ts = db.TimeSheet.FirstOrDefault(f => f.id == id);
                if (ts == null || !ts.IsFake) throw new System.Exception("Fake timeSheet delition proplems");
                db.TimeSheet.DeleteOnSubmit(ts);
                db.SubmitChanges();
            }
        }


        public DtoTimeSheet[] GetTimeSheetListForDepartments(int[] idDepartment,DateTime dateStart, int koll = 0, bool isEmpty = false)
        {
            using (var db = new KadrDataContext())
            {
                return koll <= 0
                    ? db.TimeSheet.Where(w => idDepartment.Contains(w.idDepartment) && w.DateBeginPeriod.Year == dateStart.Year && w.DateBeginPeriod.Month == dateStart.Month)
                        .Select(s => DtoClassConstructor.DtoTimeSheet(db, s.id, isEmpty)).ToArray()
                    : db.TimeSheet.Where(w => idDepartment.Contains(w.idDepartment) && w.DateBeginPeriod.Year == dateStart.Year && w.DateBeginPeriod.Month == dateStart.Month)
                        .Select(s => DtoClassConstructor.DtoTimeSheet(db, s.id, isEmpty)).ToArray();
            }
        }

        /// <summary>
        /// Редактирует записи в табеле
        /// </summary>
        /// <param name="recordsForEdit">Изменённые записи в табеле</param>
        /// <returns>true если записи успешно обновлены</returns>
        [OperationContract]
        public bool EditTimeSheetRecords(DtoTimeSheetRecord[] recordsForEdit)
        {
            using (var db = new KadrDataContext())
            {
                try
                {
                    foreach (var recordForEdit in recordsForEdit)
                    {
                        var updeteItem =
                            db.TimeSheetRecords.FirstOrDefault(
                                f => f.IdTimeSheetRecord == recordForEdit.IdTimeSheetRecord);
                        if (updeteItem == null) continue;
                        updeteItem.JobTimeCount = recordForEdit.JobTimeCount;
                        updeteItem.idDayStatus = recordForEdit.DayStays.IdDayStatus;
                    }
                    db.SubmitChanges();
                }
                catch (System.Exception)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// История согласования табеля
        /// </summary>
        /// <param name="idTimeSheet">Идентификатор табеля</param>
        /// <returns>История согласования</returns>
        [OperationContract]
        public DtoTimeSheetApproveHistiry[] GetTimeSheetApproveHistory(int idTimeSheet)
        {
            using (var db = new KadrDataContext())
            {
                return db.TimeSheetApproval.Where(w => w.idTimeSheet == idTimeSheet).Select(
                    s => new DtoTimeSheetApproveHistiry
                    {
                        AppoverComment = s.Comment,
                        AppoverDate = s.ApprovalDate,
                        AppoverName =
                            string.Format("{1} {0} {2}", s.Approver.Employee.FirstName,
                                s.Approver.Employee.LastName, s.Approver.Employee.Otch),
                        AppoverStatus = s.Approver.ApproverType.ApproverTypeName,
                        AppoverVisa = s.Result ? "Согласовано" : "Отклонено",
                        AppoverResult = s.Result
                    }).OrderBy(o => o.AppoverDate).ToArray();
            }
        }


        //==========        Согласователи

        /// <summary>
        /// Возвращает согласоватебя для структурноо подразделения
        /// </summary>
        /// <param name="idDepartment">Идентификатор структурного подразделения</param>
        /// <param name="approveNumber">Номер согласования 1-3</param>
        /// <param name="isAdmin">Является ли пользователь администратором (для администратора выбираются все подразделения а не только закреплённые за ним)</param>
        /// <returns>Согласователь структурного подразделения</returns>
        [OperationContract]
        public DtoApprover GetDepartmentApprover(int idDepartment, int approveNumber,bool isAdmin=false)
        {
            using (var db = new KadrDataContext())
            {
                return
                    db.Approver.Where(
                        w => w.idDepartment == idDepartment & w.ApproverType.ApproveNumber == approveNumber &
                             ((w.DateEnd == null) ||(w.DateEnd >= DateTime.Today))).
                        Select(s => DtoClassConstructor.DtoApprover(db, s.Employee.id,isAdmin)).FirstOrDefault();
            }
        }

        /// <summary>
        /// Добавляет логин сотруднику
        /// </summary>
        /// <param name="idEmployee">Идентификатор сотрудника</param>
        /// <param name="login">Логин</param>
        /// <returns>true в случае успешного добавления</returns>
        [OperationContract]
        public bool AddEmployeeLogin(int idEmployee, string login)
        {
            using (var db = new KadrDataContext())
            {
                try
                {
                    var itabN = db.Employees.Where(w => w.id == idEmployee).Select(s => s.itab_n).FirstOrDefault();
                    if (itabN != null)
                        db.add_EmplLogin(itabN, new Binary(new[] {Convert.ToByte(true)}), login);
                    return true;
                }
                catch (System.Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Добавляет согласователя для структурного подразделения
        /// </summary>
        /// <param name="idEmployee">Идентификатор работника</param>
        /// <param name="idDepartment">Идентификатор структурного подразделения</param>
        /// <param name="approveNumber">Номер согласователя</param>
        /// <returns>true в случае успешного добавления</returns>
        [OperationContract]
        public bool AddApproverForDepartment(int idEmployee, int idDepartment, int approveNumber)
        {
            using (var db = new KadrDataContext())
            {
                try
                {
                    var currentApprover =
                        db.Approver.Where(
                            f =>
                                f.idDepartment == idDepartment & f.ApproverType.ApproveNumber == approveNumber &
                                f.DateEnd == null).ToArray();
                    foreach (var approver in currentApprover)
                    {
                        approver.DateEnd = DateTime.Now;
                    }
                    var newApprover = new Approver
                    {
                        idApproverType =
                            db.ApproverType.Where(w => w.ApproveNumber == approveNumber).Select(
                                s => s.id).FirstOrDefault(),
                        idDepartment = idDepartment,
                        idEmployee = idEmployee,
                        DateBegin = DateTime.Now,
                        DateEnd = null
                    };
                    db.Approver.InsertOnSubmit(newApprover);
                    db.SubmitChanges();
                    return true;
                }
                catch (System.Exception)
                {
                    return false;
                }
            }
        }

        //=========================  Дни исключения ==================================================================

        /// <summary>
        /// Возвращает список дней исключений
        /// </summary>
        /// <returns></returns>
        public DtoExceptionDay[] GetExeptionsDays()
        {
            using (var db = new KadrDataContext())
            {
                return
                    db.Exception.OrderBy(o => o.DateException)
                        .Select(s => DtoClassConstructor.DtoExceptionDay(s))
                        .ToArray();
            }
        }

        /// <summary>
        /// Добавляет день исключение
        /// </summary>
        /// <param name="exceptionDay">День исключение</param>
        /// <returns></returns>
        [OperationContract]
        public DtoMessage InsertExeptionsDay(DtoExceptionDay exceptionDay)
        {
            using (var db = new KadrDataContext())
            {
                try
                {
                    var exception = new Exception
                    {
                        DateException = exceptionDay.Date,
                        ExceptionName = exceptionDay.Name,
                        idDayStatus = exceptionDay.DayStatus.IdDayStatus,
                        KolHourGNS = exceptionDay.GNS,
                        KolHourGPS = exceptionDay.GPS,
                        KolHourMNS = exceptionDay.MNS,
                        KolHourMPS = exceptionDay.MPS,
                        idPrikaz = 1,
                        idWorkShedule = exceptionDay.WorkShedule.IdWorkShedule
                    };
                    db.Exception.InsertOnSubmit(exception);
                    db.SubmitChanges();
                    return new DtoMessage
                    {
                        Result = true
                    };
                }
                catch (System.Exception ex)
                {
                    return new DtoMessage
                    {
                        Result = false,
                        Message = ex.Message + " " + ex.TargetSite
                    };
                }
            }
        }

        /// <summary>
        /// Редактирует день исключение
        /// </summary>
        /// <param name="exceptionDay">День исключение</param>
        /// <returns></returns>
        [OperationContract]
        public DtoMessage EditExeptionsDay(DtoExceptionDay exceptionDay)
        {
            using (var db = new KadrDataContext())
            {
                try
                {
                    var exception = db.Exception.FirstOrDefault(f => f.id == exceptionDay.IdExceptionDay);
                    if (exception != null)
                    {
                        exception.DateException = exceptionDay.Date;
                        exception.ExceptionName = exceptionDay.Name;
                        exception.idDayStatus = exceptionDay.DayStatus.IdDayStatus;
                        exception.KolHourGNS = exceptionDay.GNS;
                        exception.KolHourGPS = exceptionDay.GPS;
                        exception.KolHourMNS = exceptionDay.MNS;
                        exception.KolHourMPS = exceptionDay.MPS;
                        exception.idPrikaz = 1;
                        exception.idWorkShedule = exceptionDay.WorkShedule.IdWorkShedule;
                    }
                    db.SubmitChanges();
                    return new DtoMessage
                    {
                        Result = true
                    };
                }
                catch (System.Exception ex)
                {
                    return new DtoMessage
                    {
                        Result = false,
                        Message = ex.Message
                    };
                }
            }
        }

        /// <summary>
        /// Удаляет день исключение
        /// </summary>
        /// <param name="idExceptionDay">Идентификатор дня исключения</param>
        /// <returns></returns>
        [OperationContract]
        public DtoMessage DeleteExeptionsDay(int idExceptionDay)
        {
            using (var db = new KadrDataContext())
            {
                try
                {
                    var exceptionDay = db.Exception.FirstOrDefault(f => f.id == idExceptionDay);

                    if (exceptionDay != null) db.Exception.DeleteOnSubmit(exceptionDay);
                    db.SubmitChanges();
                    return new DtoMessage
                    {
                        Result = true
                    };
                }
                catch (System.Exception ex)
                {
                    return new DtoMessage
                    {
                        Result = false,
                        Message = ex.Message
                    };
                }
            }
        }

        //=========================Согласование==========================================================

        /// <summary>
        /// Вносит в систему отместку о согласовании
        /// </summary>
        /// <param name="idTimeSheet">Идентификатор табеля</param>
        /// <param name="employeeLogin">Логин согласователя</param>
        /// <param name="result">Результат согласования</param>
        /// <param name="comments">Комментарии</param>
        /// <returns>true случае успешного добавления отметки</returns>
        [OperationContract]
        public bool TimeSheetApproval(int idTimeSheet, DtoApprover employeeLogin, bool result, string comments, string appDominUrl)
        {
            using (var db = new KadrDataContext())
            {
                if (!CanApprove(idTimeSheet, employeeLogin)) return false;
                var approvalStep = GetTimeSheetApproveStep(idTimeSheet);
                var timeSheet = GetTimeSheet(idTimeSheet, true);
                var idDepartment = timeSheet.Department.IdDepartment;
                var departmentName = db.Department.First(f => f.id == idDepartment).DepartmentSmallName;
                //var approver = GetCurrentApproverByLogin(employeeLogin)
                //    .GetDepartmentApproverNumbers(idDepartment)
                //    .First(w => w.ApproveNumber == approvalStep + 1);
                var approver = employeeLogin.GetDepartmentApproverNumbers(idDepartment)
                    .First(w => w.ApproveNumber == approvalStep + 1);
                try
                {
                    var timeSheetApproval = new TimeSheetApproval
                    {
                        ApprovalDate = DateTime.Now,
                        idTimeSheet = idTimeSheet,
                        idApprover = approver.IdApprover,
                        Result = result,
                        Comment = comments
                    };
                    db.TimeSheetApproval.InsertOnSubmit(timeSheetApproval);
                    db.SubmitChanges();
                    Task.Run(
                        () => EmailSending(employeeLogin.EmployeeLogin, idTimeSheet, result, comments, approvalStep, departmentName, appDominUrl ));
                    return true;
                }
                catch (System.Exception e)
                {
                    var s = e.Message;
                    return false;
                }
            }
        }

        /// <summary>
        /// Возвращает согласователей для табеля
        /// </summary>
        /// <param name="idTimeSheet">Идентификатор табеля</param>
        /// <param name="approverNum">Номер согласователя</param>
        /// <returns></returns>
        [OperationContract]
        public DtoApprover GetApproverForTimeSheet(int idTimeSheet, int approverNum)
        {
            using (var db = new KadrDataContext())
            {
                var timeSheet = db.TimeSheet.FirstOrDefault(f => f.id == idTimeSheet);
                if (timeSheet == null) return null;
                var idDepatrment = timeSheet.idDepartment;
                return GetDepartmentApprover(idDepatrment, approverNum);
            }
        }

        /// <summary>
        /// Возвращает следеющего согласователя для табеля
        /// </summary>
        /// <param name="idTimeSheet">Идентификатор табеля</param>
        /// <returns></returns>
        [OperationContract]
        public DtoApprover GetNextApproverForTimeSheet(int idTimeSheet)
        {
            using (var db = new KadrDataContext())
            {
                var timeSheet = db.TimeSheet.FirstOrDefault(f => f.id == idTimeSheet);
                var approverNum = GetTimeSheetApproveStep(idTimeSheet);
                if (timeSheet == null || approverNum >= 3) return null;
                var idDepatrment = timeSheet.idDepartment;
                var approver =
                    db.Approver.FirstOrDefault(
                        f =>
                            f.idDepartment == idDepatrment && f.ApproverType.ApproveNumber == approverNum + 1 &&
                            f.DateEnd == null);
                return approver != null ? DtoClassConstructor.DtoApprover(db, approver.Employee.id) : null;
            }
        }


        /// <summary>
        /// Определяет доступен ли для согласования табельщиком табель
        /// </summary>
        /// <param name="idTimeSheet">Идентификатор табеля</param>
        /// <param name="employeeLogin">Логин согласователя</param>
        /// <returns></returns>
        //[OperationContract]
        //public bool CanApprove(int idTimeSheet, string employeeLogin)
        //{
        //    using (var db = new KadrDataContext())
        //    {
        //        var approver = GetCurrentApproverByLogin(employeeLogin);
        //        var timeSheet = db.TimeSheet.FirstOrDefault(f => f.id == idTimeSheet);
        //        var timeSheetApprovalStep = GetTimeSheetApproveStep(idTimeSheet) + 1;
        //        if (timeSheet == null) return false;
        //        var approveDepartment =
        //            approver.GetDepartmentApproverNumbers(timeSheet.idDepartment)
        //                .FirstOrDefault(f => f.ApproveNumber == timeSheetApprovalStep);
        //        return approveDepartment != null &&
        //               approveDepartment.ApproveNumber == timeSheetApprovalStep;
        //    }
        //}

        [OperationContract]
        public bool CanApprove(int idTimeSheet, DtoApprover approver)
        {
            using (var db = new KadrDataContext())
            {
                //var approver = GetCurrentApproverByLogin(employeeLogin);
                var timeSheet = db.TimeSheet.FirstOrDefault(f => f.id == idTimeSheet);
                var timeSheetApprovalStep = GetTimeSheetApproveStep(idTimeSheet) + 1;
                if (timeSheet == null) return false;
                var approveDepartment =
                    approver.GetDepartmentApproverNumbers(timeSheet.idDepartment)
                        .FirstOrDefault(f => f.ApproveNumber == timeSheetApprovalStep);
                return approveDepartment != null &&
                       approveDepartment.ApproveNumber == timeSheetApprovalStep;
            }
        }

        public int GetTimeSheetApproveStep(int idTimeSheet)
        {
            using (var db = new KadrDataContext())
            {
                var lastDateOfTimeSheetApproval =
                    db.TimeSheetApproval.Where(w => w.idTimeSheet == idTimeSheet)
                        .OrderByDescending(o => o.ApprovalDate).FirstOrDefault();
                if (lastDateOfTimeSheetApproval != null)
                {
                    if (lastDateOfTimeSheetApproval.Result)
                    {
                        if (lastDateOfTimeSheetApproval.Approver.ApproverType.ApproveNumber != null)
                            return (int) lastDateOfTimeSheetApproval.Approver.ApproverType.ApproveNumber;
                    }
                }
                return 0;
            }
        }

        //==================================================================================================================================================================================

        /// <summary>
        /// Создаёт табель рабочего времени
        /// </summary>
        /// <param name="idDepartment">Идентификатор отдела</param>
        /// <param name="dateBeginPeriod">Дата начала периода</param>
        /// <param name="dateEndPeriod">Дата окончания периода</param>
        /// <param name="employeeLogin">Логин пользователя от имени которого создаётся табель</param>
        /// <param name="employees">Сотрудники для которых формируется табель, не обязательный параметр, по умолчания формируется для всего отдела</param>
        /// <returns>Сообщение о результате создания табеля</returns>
        [OperationContract]
        public DtoMessage CreateTimeSheet(int idDepartment, DateTime dateBeginPeriod, DateTime dateEndPeriod,
            DtoApprover approver, IEnumerable<DtoFactStaffEmployee> employees = null)
        {
            using (var db = new KadrDataContext())
            {
                try
                {
                    //todo:тут надо корректно вытаскивать согласователя
                    var timeSheet = new TimeSheetManaget(idDepartment, dateBeginPeriod, dateEndPeriod,
                        approver.EmployeeLogin, db);
                    timeSheet.GenerateTimeSheet(employees.ToArray());
                    return new DtoMessage
                    {
                        Result = true
                    };
                }
                catch (System.Exception ex)
                {
                    return new DtoMessage
                    {
                        Message = ex.Message,
                        Result = false
                    };
                }
            }
        }

        public DtoMessage RemoveTimeSheet(int idTimeSheet)
        {
            using (var db = new KadrDataContext())
            {
                try
                {
                    var timeSheet = new TimeSheetManaget(idTimeSheet, db);
                    return new DtoMessage
                    {
                        Result = timeSheet.RemoveTimeSheet()
                    };
                }
                catch (System.Exception ex)
                {
                    return new DtoMessage
                    {
                        Result = false,
                        Message = ex.Message
                    };
                }
            }
        }

        //==========        Справочники
        /// <summary>
        /// Возвращает статусы дней
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public DtoDayStatus[] GetDayStatusList()
        {
            using (var db = new KadrDataContext())
            {
                return db.DayStatus.Select(s => DtoClassConstructor.DtoDayStatus(s)).ToArray();
            }
        }

        /// <summary>
        /// Возвращает графики работ
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public DtoWorkShedule[] GetWorkScheduleList()
        {
            var db = new KadrDataContext();
            return db.WorkShedule.Select(s => DtoClassConstructor.DtoWorkShedule(s)).ToArray();
        }

        //==========        Приватные методы

        //==========        Рассылка писем

        private async void EmailSending(string userName, int idTimeSheet, bool result, string comments, int approvalStep,
            string departmentName,string appDominUrl)
        {
            await Task.Run(() =>
            {
                var approverList = new List<DtoApprover>();
                if (result)
                {
                    approvalStep++;
                    if (approvalStep < 3)
                    {
                        approverList.Add(GetApproverForTimeSheet(idTimeSheet, approvalStep + 1));
                    }
                    else
                    {
                        for (int i = approvalStep; i > 0; i--)
                        {
                            approverList.Add(GetApproverForTimeSheet(idTimeSheet, i));
                        }
                    }
                }
                else
                {
                    //=== На данном этапе шаг согласования 0 =============================
                    for (int i = approvalStep; i > 0; i--)
                    {
                        approverList.Add(GetApproverForTimeSheet(idTimeSheet, i));
                    }
                }
                return approverList;
            }).ContinueWith(async task =>
            {
                try
                {
                    var approverList = await task;
                    var emailSender = new TimeEmailSender("mail.ugtu.net",
                        appDominUrl);
                    await emailSender.TimeSheetApproveEmailSending(userName, approverList, idTimeSheet, result, comments,
                        approvalStep, departmentName, approvalStep >= 3);
                }
                catch (System.Exception e)
                {
                    var ex = e;
                }
            });
        }
    }
}