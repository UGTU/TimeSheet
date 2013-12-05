using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Net.Mail;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
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
        /// <returns></returns>
        [OperationContract]
        [Authorize]
        public DtoApprover GetCurrentApproverByLogin(string employeeLogin)
        {
            if (string.IsNullOrWhiteSpace(employeeLogin)) return null;
            if (employeeLogin == "ALEXEY-PC\\Alexey") employeeLogin = "atipunin@ugtu.net";
            if (employeeLogin.ToLower().StartsWith(@"ugtu\".ToLower()))
            {
                employeeLogin =
                    string.Format("{0}@{1}.NET", employeeLogin.Substring(5, employeeLogin.Length - 5),
                        employeeLogin.Substring(0, 4)).ToLower();
            }
            using (var db = new KadrDataContext())
            {
                var idEmployee =
                    db.Employee.Where(w => w.EmployeeLogin.ToLower() == employeeLogin.ToLower())
                        .Select(s => s.id).FirstOrDefault();
                return DtoClassConstructor.DtoApprover(db, idEmployee);
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
                    db.Department.Where(w => w.dateExit == null)
                        .Select(s => DtoClassConstructor.DtoDepartment(db, s.id))
                        .ToArray();
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
                    db.FactStaff.Where(w => w.PlanStaff.idDepartment == idDepartment & w.DateEnd == null).Select(
                        s => DtoClassConstructor.DtoEmployee(s.Employee)).Distinct().ToArray();
            }
        }


        [OperationContract]
        public DtoFactStaffEmployee[] GetEmployeesForTimeSheet(int idDepartment, string employeeLogin,
            DateTime dateStart, DateTime dateEnd)
        {
            using (var db = new KadrDataContext())
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith((FactStaffWithHistory fswh) => fswh.PlanStaff);
                loadOptions.LoadWith((PlanStaff ps) => ps.Post);
                loadOptions.LoadWith((Post p) => p.Category);
                loadOptions.LoadWith((PlanStaff ps) => ps.WorkShedule);
                loadOptions.LoadWith((FactStaffWithHistory fswh) => fswh.Employee);
                loadOptions.LoadWith((OK_Otpusk oko) => oko.OK_Otpuskvid);
                db.LoadOptions = loadOptions;
                var dtoApproverDepartment =
                    GetCurrentApproverByLogin(employeeLogin)
                        .DtoApproverDepartments.FirstOrDefault(w => w.IdDepartment == idDepartment);
                var ts = new TimeSheetCraterNew(idDepartment, dateStart, dateEnd, dtoApproverDepartment, db);
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
                            db.TimeSheetRecord.FirstOrDefault(
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
        /// <returns>Согласователь структурного подразделения</returns>
        [OperationContract]
        public DtoApprover GetDepartmentApprover(int idDepartment, int approveNumber)
        {
            using (var db = new KadrDataContext())
            {
                return
                    db.Approver.Where(
                        w => w.idDepartment == idDepartment & w.ApproverType.ApproveNumber == approveNumber &
                            w.DateEnd == null).
                        Select(s => DtoClassConstructor.DtoApprover(db, s.Employee.id)).FirstOrDefault();
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
                    var itabN = db.Employee.Where(w => w.id == idEmployee).Select(s => s.itab_n).FirstOrDefault();
                    if (itabN != null)
                        db.add_EmplLogin(itabN, new Binary(new[] { Convert.ToByte(true) }), login);
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
        public bool TimeSheetApproval(int idTimeSheet, string employeeLogin, bool result, string comments)
        {
            using (var db = new KadrDataContext())
            {
                if (!CanApprove(idTimeSheet, employeeLogin)) return false;
                var approvalStep = GetTimeSheetApproveStep(idTimeSheet);
                var timeSheet = GetTimeSheet(idTimeSheet, true);
                var idDepartment = timeSheet.Department.IdDepartment;
                var approver = GetCurrentApproverByLogin(employeeLogin)
                    .GetDepartmentApproverNumbers(idDepartment)
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
                    if (result && CanApprove(idTimeSheet, employeeLogin))
                    {
                        TimeSheetApproval(idTimeSheet, employeeLogin, true, comments);
                    }
                    EmailSending(idTimeSheet, result, comments, approvalStep);
                    return true;
                }
                catch (System.Exception)
                {
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
                        f => f.idDepartment == idDepatrment && f.ApproverType.ApproveNumber == approverNum + 1 && f.DateEnd == null);
                return approver != null ? DtoClassConstructor.DtoApprover(db, approver.Employee.id) : null;
            }
        }


        /// <summary>
        /// Определяет доступен ли для согласования табельщиком табель
        /// </summary>
        /// <param name="idTimeSheet">Идентификатор табеля</param>
        /// <param name="employeeLogin">Логин согласователя</param>
        /// <returns></returns>
        [OperationContract]
        public bool CanApprove(int idTimeSheet, string employeeLogin)
        {
            using (var db = new KadrDataContext())
            {
                var approver = GetCurrentApproverByLogin(employeeLogin);
                var timeSheet = db.TimeSheet.FirstOrDefault(f => f.id == idTimeSheet);
                var timeSheetApprovalStep = GetTimeSheetApproveStep(idTimeSheet) + 1;
                if (timeSheet == null) return false;
                var approveDepartment =
                    approver.GetDepartmentApproverNumbers(timeSheet.idDepartment).FirstOrDefault(f => f.ApproveNumber == timeSheetApprovalStep);
                return approveDepartment != null &&
                       approveDepartment.ApproveNumber == timeSheetApprovalStep;
            }
        }

        /// <summary>
        /// Возвращает шаг согласования для табеля
        /// </summary>
        /// <param name="idTimeSheet">Идентификатор табеля</param>
        /// <returns></returns>
        [OperationContract]
        public int GetTimeSheetApproveStep(int idTimeSheet)
        {
            using (var db = new KadrDataContext())
            {
                var lastDateOfTimeSheetApproval =
                    db.TimeSheetApproval.Where(w => w.idTimeSheet == idTimeSheet)
                        .OrderByDescending(o => o.ApprovalDate)
                        .FirstOrDefault();
                if (lastDateOfTimeSheetApproval != null)
                {
                    var lastApproval =
                        db.TimeSheetApproval.FirstOrDefault(
                            w =>
                                w.idTimeSheet == idTimeSheet &
                                w.ApprovalDate == lastDateOfTimeSheetApproval.ApprovalDate);

                    if (lastApproval != null)
                    {
                        if (lastApproval.Result)
                        {
                            if (lastApproval.Approver.ApproverType.ApproveNumber != null)
                                return (int)lastApproval.Approver.ApproverType.ApproveNumber;
                        }
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
            string employeeLogin, IEnumerable<DtoFactStaffEmployee> employees = null)
        {
            using (var db = new KadrDataContext())
            //using (var dbloger = new DataContextLoger("CreateTimeSheetLog.txt", FileMode.OpenOrCreate, db))
            {
                try
                {
                    var dtoApproverDepartment =
                        GetCurrentApproverByLogin(employeeLogin)
                            .DtoApproverDepartments.FirstOrDefault(w => w.IdDepartment == idDepartment);
                    var timeSheet = new TimeSheetManaget(idDepartment, dateBeginPeriod, dateEndPeriod,
                        dtoApproverDepartment, db);
                    timeSheet.GenerateTimeSheet(employees);
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

        private bool EmailSending(int idTimeSheet, bool result, string comments, int approvalStep)
        {
            //var approvalStep = GetTimeSheetApproveStep(idTimeSheet);
            if (result)
            {
                approvalStep++;
                if (approvalStep < 3)
                {
                    SendMail(GetApproverForTimeSheet(idTimeSheet, approvalStep + 1), idTimeSheet, true,
                        comments);
                }
                else
                {
                    for (int i = approvalStep; i > 0; i--)
                    {
                        SendMail(GetApproverForTimeSheet(idTimeSheet, i), idTimeSheet, true, comments, true);
                    }
                }
            }
            else
            {
                //=== На данном этапе шаг согласования 0 =============================
                for (int i = approvalStep; i > 0; i--)
                {
                    SendMail(GetApproverForTimeSheet(idTimeSheet, i), idTimeSheet, false, comments);
                }
            }
            return true;
        }

        //todo:Вот это вот надо убрать в отдельный класс
        //[OperationContract]
        private bool SendMail(DtoApprover approver, int idTimeSheet, bool approveResult, string comment,
            bool isApproveFinished = false)
        {
            var requestUrl = System.Web.HttpContext.Current.Request.Url.Authority;
            Action<object> mailSending = (object urlAuth) =>
            {
                var url = "http:/" + urlAuth;
                var timeSheet = "<a href=\"" + url + "\">ИС \"Табель\"</a>";
                var timeSheetShow =
                    String.Format("<a href=\"" + url + "/Main/TimeSheetShow?idTimeSheet={0}\">ссылке</a>",
                        idTimeSheet);
                var timeSheetPrint =
                    String.Format("<a href=\"" + url + "/tabel/{0}\">печать</a>",
                        idTimeSheet);
                var timeSheetApproval =
                    String.Format(
                        "<a href=\"" + url + "/Main/TimeSheetApprovalNew?idTimeSheet={0}\">ссылке</a>",
                        idTimeSheet);
                var stringBuilder = new StringBuilder();
                if (isApproveFinished)
                {
                    stringBuilder.AppendLine("<br/><br/>");
                    stringBuilder.AppendFormat("Здравствуйте, {0} {1}.", approver.Name, approver.Patronymic);
                    stringBuilder.AppendLine("<br/><br/>");
                    stringBuilder.Append("Табель успешно согласован. ");
                    stringBuilder.AppendFormat("Вы пожете просмотреть табель перейдя по {0}, ", timeSheetShow);
                    stringBuilder.AppendFormat(" или вывести табель на {0}.", timeSheetPrint);
                    stringBuilder.AppendFormat(" Так же вы можете посетить {0}.", timeSheet);
                }
                else
                {
                    if (approveResult)
                    {
                        stringBuilder.AppendLine("<br/><br/>");
                        stringBuilder.AppendFormat("Здравствуйте {0} {1}.", approver.Name, approver.Patronymic);
                        stringBuilder.AppendLine("<br/><br/>");
                        stringBuilder.Append("Вам на согласование был направлен табель рабочего времени. ");
                        stringBuilder.AppendFormat(
                            "Для того, чтоб приступить к согласованию тебеля перейдите по {0}, ", timeSheetApproval);
                        stringBuilder.AppendFormat(" или посетите {0}.", timeSheet);
                    }
                    else
                    {

                        stringBuilder.AppendLine("<br/><br/>");
                        stringBuilder.AppendFormat("Здравствуйте {0} {1}.", approver.Name, approver.Patronymic);
                        stringBuilder.AppendLine("<br/><br/>");
                        stringBuilder.AppendFormat("Согласование табеля было отклонено по причине: {0}", comment);
                    }
                }
                var mm = new MailMessage("tabel-no-reply@ugtu.net", approver.EmployeeLogin,
                    "ИС Табель рабочего времени", stringBuilder.ToString()) { IsBodyHtml = true };
                var client = new SmtpClient("mail.ugtu.net");
                client.Send(mm);
            };
            var t1 = new Task(mailSending, requestUrl);
            t1.Start();
            return true;
        }

        ////todo:Вот это вот надо убрать в отдельный класс
        ////[OperationContract]
        //private bool SendMail(DtoApprover approver, int idTimeSheet, bool approveResult, string comment,
        //    bool isApproveFinished = false)
        //{
        //    try
        //    {
        //        //var r = System.Web.HttpContext.Current.Request.Url;
        //        var url = "http:/" + System.Web.HttpContext.Current.Request.Url.Authority;
        //        var timeSheet = "<a href=\"" + url + "\">ИС \"Табель\"</a>";
        //        var timeSheetShow =
        //            String.Format("<a href=\"" + url + "/Main/TimeSheetShow?idTimeSheet={0}\">ссылке</a>",
        //                idTimeSheet);
        //        var timeSheetPrint =
        //            String.Format("<a href=\"" + url + "/tabel/{0}\">печать</a>",
        //                idTimeSheet);
        //        var timeSheetApproval =
        //            String.Format(
        //                "<a href=\"" + url + "/Main/TimeSheetApprovalNew?idTimeSheet={0}\">ссылке</a>",
        //                idTimeSheet);
        //        var stringBuilder = new StringBuilder();
        //        if (isApproveFinished)
        //        {
        //            stringBuilder.AppendLine("<br/><br/>");
        //            stringBuilder.AppendFormat("Здравствуйте, {0} {1}.", approver.Name, approver.Patronymic);
        //            stringBuilder.AppendLine("<br/><br/>");
        //            stringBuilder.Append("Табель успешно согласован. ");
        //            stringBuilder.AppendFormat("Вы пожете просмотреть табель перейдя по {0}, ", timeSheetShow);
        //            stringBuilder.AppendFormat(" или вывести табель на {0}.", timeSheetPrint);
        //            stringBuilder.AppendFormat(" Так же вы можете посетить {0}.", timeSheet);
        //        }
        //        else
        //        {
        //            if (approveResult)
        //            {
        //                stringBuilder.AppendLine("<br/><br/>");
        //                stringBuilder.AppendFormat("Здравствуйте {0} {1}.", approver.Name, approver.Patronymic);
        //                stringBuilder.AppendLine("<br/><br/>");
        //                stringBuilder.Append("Вам на согласование был направлен табель рабочего времени. ");
        //                stringBuilder.AppendFormat(
        //                    "Для того, чтоб приступить к согласованию тебеля перейдите по {0}, ", timeSheetApproval);
        //                stringBuilder.AppendFormat(" или посетите {0}.", timeSheet);
        //            }
        //            else
        //            {

        //                stringBuilder.AppendLine("<br/><br/>");
        //                stringBuilder.AppendFormat("Здравствуйте {0} {1}.", approver.Name, approver.Patronymic);
        //                stringBuilder.AppendLine("<br/><br/>");
        //                stringBuilder.AppendFormat("Согласование табеля было отклонено по причине: {0}", comment);
        //            }
        //        }
        //        var mm = new MailMessage("tabel-no-reply@ugtu.net", approver.EmployeeLogin,
        //            "ИС Табель рабочего времени", stringBuilder.ToString()) { IsBodyHtml = true };
        //        var client = new SmtpClient("mail.ugtu.net");
        //        client.Send(mm);
        //        return true;
        //    }
        //    catch (System.Exception ex)
        //    {
        //        var r = ex.Message;
        //        return false;
        //    }
        //}

        //==========        Не используемые методы, возможно будут реализованы позже

        //private bool RemoveTimeSheet(int idTimeSheet)
        //{
        //    using (var db = new KadrDataContext())
        //    {
        //        try
        //        {
        //            var ts = new TimeSheetCraterNew(idTimeSheet, db);
        //            return ts.RemoveTimeSheet();
        //        }
        //        catch (System.Exception)
        //        {
        //            return false;
        //        }
        //    }
        //}

        //private bool RemoveTimeSheetEmployee(int idTimeSheet, int idFactStuffHistory)
        //{
        //    using (var db = new KadrDataContext())
        //    {
        //        try
        //        {
        //            var ts = new TimeSheetCraterNew(idTimeSheet, db);
        //            return ts.RemoveEmployee(idFactStuffHistory);
        //        }
        //        catch (System.Exception)
        //        {
        //            return false;
        //        }
        //    }
        //}

        //[OperationContract]
        //public DtoMessage RefreshTimeSheet(int idTimeSheet)
        //{
        //    try
        //    {
        //        return new DtoMessage();
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return new DtoMessage
        //        {
        //            Result = false,
        //            Message = ex.Message
        //        };
        //    }
        //}
    }
}