using System;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Web.Mvc;
using TimeSheetMvc4WebApplication.ClassesDTO;
using TimeSheetMvc4WebApplication.Models.Main;
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
            using (var dbloger = new DataContextLoger("GetTimeSheetLog.txt", FileMode.OpenOrCreate, db))
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
            using (var dbloger = new DataContextLoger("GetTimeSheetLog.txt", FileMode.OpenOrCreate, db))
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
            {
                using (var dbloger = new DataContextLoger("GetTimeSheetListLog.txt", FileMode.OpenOrCreate, db))
                {
                    return koll <= 0
                        ? db.TimeSheet.Where(w => w.idDepartment == idDepartment)
                            .Select(s => DtoClassConstructor.DtoTimeSheet(db, s.id, isEmpty)).ToArray()
                        : db.TimeSheet.Where(w => w.idDepartment == idDepartment)
                            .OrderByDescending(o => o.DateBeginPeriod).Take(koll)
                            .Select(s => DtoClassConstructor.DtoTimeSheet(db, s.id, isEmpty)).ToArray();
                }
            }
        }

















        [OperationContract]
        public DtoApprover GetDepartmentApprover(int idDepartment, int approveNumber)
        {
            using (var db = new KadrDataContext())
            {
                return
                    db.Approver.Where(
                        w =>
                            w.idDepartment == idDepartment & w.ApproverType.ApproveNumber == approveNumber &
                            w.DateEnd == null).
                        Select(s => DtoClassConstructor.DtoApprover(db, s.Employee.id)).FirstOrDefault();
            }
        }

        [OperationContract]
        public bool AddEmployeeLogin(int idEmployee, string login)
        {
            using (var db = new KadrDataContext())
            {
                try
                {
                    var itabN = db.Employee.Where(w => w.id == idEmployee).Select(s => s.itab_n).FirstOrDefault();
                    if (itabN != null)
                    {
                        db.add_EmplLogin(itabN, new Binary(new[] {Convert.ToByte(true)}), login);
                    }
                    return true;
                }
                catch (System.Exception)
                {
                    return false;
                }
            }
        }

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

        [OperationContract]
        public DtoDayStatus[] GetDayStatusList()
        {
            using (var db = new KadrDataContext())
            {
                return db.DayStatus.Select(s => DtoClassConstructor.DtoDayStatus(s)).ToArray();
            }
        }

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
                        if (updeteItem != null)
                        {
                            updeteItem.JobTimeCount = recordForEdit.JobTimeCount;
                            updeteItem.idDayStatus = recordForEdit.DayStays.IdDayStatus;
                        }
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


        public bool EditTimeSheetRecords(JsTimeSheetRecordModel[] recordsForEdit)
        {
            using (var db = new KadrDataContext())
            {
                try
                {
                    if (recordsForEdit != null && recordsForEdit.Any() &&
                        CanTimeSheeEdit(recordsForEdit.First().IdTimeSheetRecord))
                    {
                        foreach (var recordForEdit in recordsForEdit)
                        {
                            var updeteItem =
                                db.TimeSheetRecord.FirstOrDefault(
                                    f => f.IdTimeSheetRecord == recordForEdit.IdTimeSheetRecord);
                            if (updeteItem != null)
                            {
                                updeteItem.JobTimeCount = recordForEdit.JobTimeCount;
                                updeteItem.idDayStatus = recordForEdit.IdDayStatus;
                            }
                        }
                        db.SubmitChanges();
                        return true;
                    }
                    return false;
                }
                catch (System.Exception)
                {
                    return false;
                }
            }
        }

        private bool RemoveTimeSheet(int idTimeSheet)
        {
            using (var db = new KadrDataContext())
            {
                try
                {
                    var ts = new TimeSheetCraterNew(idTimeSheet, db);
                    return ts.RemoveTimeSheet();
                }
                catch (System.Exception)
                {
                    return false;
                }
            }
        }

        private bool RemoveTimeSheetEmployee(int idTimeSheet, int idFactStuffHistory)
        {
            using (var db = new KadrDataContext())
            {
                try
                {
                    var ts = new TimeSheetCraterNew(idTimeSheet, db);
                    return ts.RemoveEmployee(idFactStuffHistory);
                }
                catch (System.Exception)
                {
                    return false;
                }
            }
        }

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

        [OperationContract]
        public bool CanTimeSheeEdit(Guid idRecord)
        {
            using (var db = new KadrDataContext())
            {
                var timeSheet = db.TimeSheetRecord.FirstOrDefault(f => f.IdTimeSheetRecord == idRecord);
                if (timeSheet == null) return false;
                var approveStep = GetTimeSheetApproveStep(timeSheet.idTimeSheet);
                return approveStep == 0;
            }
        }


        [OperationContract]
        public DtoMessage TimeSheetByName(DtoTimeSheet timeSheet)
        {
            using (var db = new KadrDataContext())
            {
                try
                {
                    var rec = db.TimeSheetRecord.Where(w => w.idTimeSheet == timeSheet.IdTimeSheet);
                    foreach (var timeSheetRecord in rec)
                    {
                        timeSheetRecord.IsChecked = false;
                    }
                    foreach (var emploeye in timeSheet.Employees.Where(w => w.IsChecked))
                    {
                        DtoTimeSheetEmployee empl = emploeye;
                        var records =
                            rec.Where(
                                w =>
                                    empl != null && (w.idTimeSheet == timeSheet.IdTimeSheet &&
                                                     w.idFactStaffHistory == empl.FactStaffEmployee.IdFactStaff));
                        foreach (var record in records)
                        {
                            record.IsChecked = true;
                        }
                    }
                    db.SubmitChanges();
                    return new DtoMessage
                    {
                        Result = true,
                        Message = ""
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


        //=========================  Дни исключения ==================================================================

        [OperationContract]
        public int[] GetYearsForExeptionDays()
        {
            using (var db = new KadrDataContext())
            {
                return db.Exception.Select(s => s.DateException.Year).Distinct().ToArray();
            }
        }

        [OperationContract]
        public DtoWorkShedule[] GetWorkScheduleList()
        {
            var db = new KadrDataContext();
            return db.WorkShedule.Select(s => DtoClassConstructor.DtoWorkShedule(s)).ToArray();
        }

        [OperationContract]
        public DtoExceptionDay[] GetExeptionsDaysInYear(int year, int idWprkSchedule)
        {
            using (var db = new KadrDataContext())
            {
                return
                    db.Exception.Where(w => w.DateException.Year == year & w.idWorkShedule == idWprkSchedule)
                        .OrderBy(o => o.DateException)
                        .Select(s => DtoClassConstructor.DtoExceptionDay(s))
                        .ToArray();
            }
        }

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

        [OperationContract]
        public bool TimeSheetApproval(int idTimeSheet, string employeeLogin, bool result, string comments)
        {
            using (var db = new KadrDataContext())
            {
                if (CanApprove(idTimeSheet, employeeLogin))
                {
                    var approvalStep = GetTimeSheetApproveStep(idTimeSheet);
                    //todo: тут надо вытаскивать пустой табель, без записей
                    var timeSheet = GetTimeSheet(idTimeSheet);
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
                        //Отправка письма
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
                    catch (System.Exception)
                    {
                        return false;
                    }
                }
                return false;
            }
        }



        [OperationContract]
        public DtoApprover GetApproverForTimeSheet(int idTimeSheet, int approverNum)
        {
            using (var db = new KadrDataContext())
            {
                var timeSheet = db.TimeSheet.FirstOrDefault(f => f.id == idTimeSheet);
                if (timeSheet != null)
                {
                    var idDepatrment = timeSheet.idDepartment;
                    return GetDepartmentApprover(idDepatrment, approverNum);
                }
                return null;
            }
        }

        [OperationContract]
        public DtoApprover GetNextApproverForTimeSheet(int idTimeSheet)
        {
            using (var db = new KadrDataContext())
            {
                var timeSheet = db.TimeSheet.FirstOrDefault(f => f.id == idTimeSheet);
                if (timeSheet == null) throw new System.Exception("Запрашиваемый табель не найден");
                var idDepatrment = timeSheet.idDepartment;
                var approverNum = GetTimeSheetApproveStep(idTimeSheet);
                if (approverNum >= 3) return null;
                var approver =
                    db.Approver.FirstOrDefault(
                        f => f.idDepartment == idDepatrment & f.ApproverType.ApproveNumber == approverNum + 1);
                if (approver != null) return DtoClassConstructor.DtoApprover(db, approver.Employee.id);
                return null;
            }
        }



        [OperationContract]
        public bool CanApprove(int idTimeSheet, string employeeLogin)
        {
            using (var db = new KadrDataContext())
            {
                var approver = GetCurrentApproverByLogin(employeeLogin);
                var timeSheet = db.TimeSheet.FirstOrDefault(f => f.id == idTimeSheet);
                var timeSheetApprovalStep = GetTimeSheetApproveStep(idTimeSheet) + 1;
                if (timeSheet != null)
                {
                    var approveDepartment =
                        approver.GetDepartmentApproverNumbers(timeSheet.idDepartment).FirstOrDefault(f=>f.ApproveNumber==timeSheetApprovalStep);
                    //var approveDepartment =
                    //    approver.DtoApproverDepartments.FirstOrDefault(f => f.IdDepartment == timeSheet.idDepartment);
                    if (approveDepartment != null &&
                        approveDepartment.ApproveNumber == timeSheetApprovalStep)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

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
                                return (int) lastApproval.Approver.ApproverType.ApproveNumber;
                        }
                    }
                }
                return 0;
            }
        }

        [OperationContract]
        public bool SendMail(DtoApprover approver, int idTimeSheet, bool approveResult, string comment,
            bool isApproveFinished = false)
        {
            try
            {
                //var r = System.Web.HttpContext.Current.Request.Url;
                var url = "http:/" + System.Web.HttpContext.Current.Request.Url.Authority;
                var timeSheet = "<a href=\"" + url + "\">ИС \"Табель\"</a>";
                var timeSheetShow =
                    String.Format("<a href=\"" + url + "/Home/TimeSheetShow?idTimeSheet={0}\">ссылке</a>",
                        idTimeSheet);
                var timeSheetPrint =
                    String.Format("<a href=\"" + url + "/Home/TimeSheetPdf?idTimeSheet={0}\">печать</a>",
                        idTimeSheet);
                var timeSheetApproval =
                    String.Format(
                        "<a href=\"" + url + "/Home/TimeSheetApproval?idTimeSheet={0}\">ссылке</a>",
                        idTimeSheet);
                var stringBuilder = new StringBuilder();
                if (isApproveFinished)
                {
                    stringBuilder.AppendLine("<br/><br/>");
                    stringBuilder.AppendFormat("Здравствуйте {0} {1}.", approver.Name, approver.Patronymic);
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
                    "ИС Табель рабочего времени", stringBuilder.ToString());
                mm.IsBodyHtml = true;
                var client = new SmtpClient("mail.ugtu.net");
                client.Send(mm);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        [OperationContract]
        public string FioToDat(string fio, int? sex)
        {
            using (var db = new KadrDataContext())
            {
                var arr = fio.Trim().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Any())
                {
                    var fioDat = arr.Aggregate("", (current, s) => current + (db.FIOToDat(s, sex) + " "));
                    return fioDat.Trim();
                }
                return fio;
            }
        }

        //==================================================================================================================================================================================

        [OperationContract]
        public DtoMessage CreateTimeSheet(int idDepartment, DateTime dateBeginPeriod, DateTime dateEndPeriod,
            string employeeLogin)
        {
            using (var db = new KadrDataContext())
            using (var dbloger = new DataContextLoger("CreateTimeSheetLog.txt", FileMode.OpenOrCreate, db))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith((FactStaffWithHistory fswh) => fswh.PlanStaff);
                loadOptions.LoadWith((PlanStaff ps) => ps.Post);
                loadOptions.LoadWith((Post p) => p.Category);
                loadOptions.LoadWith((PlanStaff ps) => ps.WorkShedule);
                loadOptions.LoadWith((FactStaffWithHistory fswh) => fswh.Employee);
                loadOptions.LoadWith((OK_Otpusk oko) => oko.OK_Otpuskvid);
                db.LoadOptions = loadOptions;

                try
                {
                    var dtoApproverDepartment =
                        GetCurrentApproverByLogin(employeeLogin)
                            .DtoApproverDepartments.FirstOrDefault(w => w.IdDepartment == idDepartment);
                    var timeSheet = new TimeSheetCraterNew(idDepartment, dateBeginPeriod, dateEndPeriod,
                        dtoApproverDepartment, db);
                    timeSheet.GenerateTimeSheet();
                    timeSheet.SubmitTimeSheet();
                    return new DtoMessage()
                    {
                        Result = true
                    };
                }
                catch (System.Exception ex)
                {
                    return new DtoMessage()
                    {
                        Message = ex.Message,
                        Result = false
                    };
                }
            }
        }


        [OperationContract]
        public DtoMessage CreateTimeSheetByName(int idDepartment, DateTime dateBeginPeriod, DateTime dateEndPeriod,
            string employeeLogin, DtoFactStaffEmployee[] employees)
        {
            using (var db = new KadrDataContext())
            using (var dbloger = new DataContextLoger("CreateTimeSheetLog.txt", FileMode.OpenOrCreate, db))
            {
                try
                {
                    var dtoApproverDepartment =
                        GetCurrentApproverByLogin(employeeLogin)
                            .DtoApproverDepartments.FirstOrDefault(w => w.IdDepartment == idDepartment);
                    var timeSheet = new TimeSheetCraterNew(idDepartment, dateBeginPeriod, dateEndPeriod,
                        dtoApproverDepartment, db);
                    timeSheet.GenerateTimeSheet(employees.Where(w => w.IsCheked).ToArray());
                    timeSheet.SubmitTimeSheet();
                    return new DtoMessage()
                    {
                        Result = true
                    };
                }
                catch (System.Exception ex)
                {
                    return new DtoMessage()
                    {
                        Message = ex.Message,
                        Result = false
                    };
                }
            }
        }


        [OperationContract]
        public DtoMessage RefreshTimeSheet(int idTimeSheet)
        {
            try
            {
                return new DtoMessage();
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
}