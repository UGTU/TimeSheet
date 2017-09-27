using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Xml.Linq;
using TimeSheetMvc4WebApplication.ClassesDTO;

namespace TimeSheetMvc4WebApplication.Source
{
    public class TimeSheetManaget
    {
        private int? IdTimeSheet
        {
            get { return IsTimeSheetValid() ? _timeSheet.id : (int?)null; }
        }

        public DateTime? DateBegin
        {
            get { return IsTimeSheetValid() ? _timeSheet.DateBeginPeriod : (DateTime?)null; }
        }

        public DateTime? DateEnd
        {
            get { return IsTimeSheetValid() ? _timeSheet.DateEndPeriod : (DateTime?)null; }
        }

        public DateTime? DateComposition
        {
            get { return IsTimeSheetValid() ? _timeSheet.DateComposition : (DateTime?)null; }
        }

        readonly List<TimeSheetRecord> _timeSheetRecordLList = new List<TimeSheetRecord>();


        public TimeSheetManaget(int idDepartment, DateTime dateBeginPeriod, DateTime dateEndPeriod, string employeeLogin, KadrDataContext db)
        {
            employeeLogin = UserNameAdapter.Adapt(employeeLogin);
            SetDataContext(db);
            var approver =
                db.Approver.FirstOrDefault(f => f.Employee.EmployeeLogin.ToLower() == employeeLogin.ToLower());
            if (approver == null) throw new System.Exception("Указан недоступный создатель табеля.");
            _timeSheet = new TimeSheet
            {
                idDepartment = idDepartment,
                idCreater = approver.id,
                DateBeginPeriod = dateBeginPeriod,
                DateEndPeriod = dateEndPeriod,
                DateComposition = DateTime.Now,
                ApproveStep = 0
            };
        }

        public TimeSheetManaget(int idTimeSheet, KadrDataContext db)
        {
            SetDataContext(db);
            _timeSheet = _db.TimeSheet.First(f => f.id == idTimeSheet);
        }

        public bool RemoveTimeSheet()
        {
            if (!CanEditTimeSheet()) throw new System.Exception("Редактирование табеля невозможно в связи с тем, что табель согласован, либо находится в процессе согласования.");
            _db.TimeSheetApproval.DeleteAllOnSubmit(_db.TimeSheetApproval.Where(w => w.idTimeSheet == _timeSheet.id));
            _db.TimeSheetRecords.DeleteAllOnSubmit(_db.TimeSheetRecords.Where(w => w.idTimeSheet == _timeSheet.id));
            _db.TimeSheet.DeleteAllOnSubmit(_db.TimeSheet.Where(w => w.id == _timeSheet.id));
            _db.SubmitChanges();
            _timeSheet = null;
            return true;
        }

        /// <summary>
        /// Изменение табеля на авансовый, срезается пол табеля с 15 числа и до конца
        /// </summary>
        /// <param name="idTimeSheet"></param>
        /// <returns></returns>
        public bool remakeTSAdvance()
        {
            if (!CanEditTimeSheet()) throw new System.Exception("Редактирование табеля невозможно в связи с тем, что табель согласован, либо находится в процессе согласования.");
            var tsr = _db.TimeSheetRecords.Where(w => w.idTimeSheet == _timeSheet.id && w.RecordDate.Day>15).ToList();
             foreach (var item in tsr)
             {
                item.idDayStatus = IdX;
             }
            _db.SubmitChanges();
            return true;
        }

        public bool RemoveEmployee(int idFactStuffHistory)
        {
            if (!CanEditTimeSheet()) return false;
            _db.TimeSheetRecords.DeleteAllOnSubmit(_db.TimeSheetRecords.Where(w => w.idTimeSheet == _timeSheet.id && w.idFactStaffHistory==idFactStuffHistory));
            _db.SubmitChanges();
            return true;
        }

        public bool CanEditTimeSheet()
        {
            if (_timeSheet == null) return false;
            var approveStep = GetTimeSheetApproveStep();
            return approveStep == 0;
        }

        public int GetTimeSheetApproveStep()
        {
            var lastDateOfTimeSheetApproval =
                _db.TimeSheetApproval.Where(w => w.idTimeSheet == _timeSheet.id)
                    .OrderByDescending(o => o.ApprovalDate)
                    .FirstOrDefault();
            if (lastDateOfTimeSheetApproval != null)
            {
                var lastApproval =
                    _db.TimeSheetApproval.FirstOrDefault(
                        w =>
                            w.idTimeSheet == _timeSheet.id &
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


        public void GenerateTimeSheet(IEnumerable<DtoFactStaffEmployee> employees=null)
        {
            if (employees == null || !employees.Any())
            {
                InsertEmployees(GetAllEmployees());
            }
            else
            {
                var factStuffHistiryIdList = employees.Select(s => s.IdFactStaffHistiry).ToArray();
                InsertEmployees(GetAllEmployees().Where(w => factStuffHistiryIdList.Contains(w.idFactStaffHistory)).ToArray());
            }
            SubmitTimeSheet();
        }


        private IEnumerable<int> GetInnerDepartmentsIdForTS(KadrDataContext db, IEnumerable<int> idsDepartment)
        {
            var list = new List<int>();
            foreach (var id in idsDepartment)
            {
                list.Add(id);
                var temp = db.Department.Where(w => w.idManagerDepartment == id && w.HasTimeSheet == false).Select(s => s.id);
                list.AddRange(GetInnerDepartmentsIdForTS(db, temp));
            }
            return list.ToArray();
        }

        public FactStaffWithHistory[] GetAllEmployees()
        {
            var deps = GetInnerDepartmentsIdForTS(_db, new int[] { _timeSheet.idDepartment });
            var employees = _db.FactStaffWithHistories.Where(
                w => deps.Contains(w.PlanStaff.idDepartment) &&
                     (w.DateEnd == null || w.DateEnd >= _timeSheet.DateBeginPeriod) &&
                     w.DateBegin <= _timeSheet.DateEndPeriod && w.idTypeWork != IdWorkTypeSovmeshenie &&
                     w.idTypeWork != IdWorkTypePochesovik && w.idTypeWork!= IdWorkTypeIspOb).ToArray();
            return employees;
        }


        private IEnumerable<TimeSheetRecord> InsertEmployee(FactStaffWithHistory employee, IEnumerable<Exception> exeptions, 
            IEnumerable<OK_Otpusk> otpuskList, IEnumerable<OK_Inkapacity> inkapacities, IEnumerable<Event> businesstrips)
        {
            
            List<TimeSheetRecord> timeSheetRecordLList;
            // Генерируем табель
            //if (employee.PlanStaff.Post.Category.id == IdPps)
            if (employee.PlanStaff.Post.Category.IsPPS.Value)
            {
                var FullEmployee = employee.WorkHoursInWeek == FullPpsHours;
                //PPS
                timeSheetRecordLList = PpsTimeSheetGenerate(employee, FullEmployee);
            }
            else
            {
                var fullEmployee = (employee.Employee.SexBit && employee.WorkHoursInWeek == FullManHours)||
                                   (!employee.Employee.SexBit && employee.WorkHoursInWeek == FullWomanHours);

                timeSheetRecordLList = employee.PlanStaff.WorkShedule.id == Week5Days
                    //5 days week
                    ? FiveDayesTimeSheetGenerate(employee, fullEmployee)
                    //6 days week
                    : (employee.PlanStaff.WorkShedule.id == Week6Days
                    ? SixDayesTimeSheetGenerate(employee, fullEmployee)
                    //flexible week
                    : FlexibleTimeSheetGenerate(employee)
                    );
            }


            // Добавляем информацию о отпусе 
            timeSheetRecordLList = AddHolidaysToTimeSheetRecords(employee, timeSheetRecordLList, otpuskList);    
            // Добавляем дни исключения
            timeSheetRecordLList = AddExceptoinDaysToTimeSheetRecords(employee, timeSheetRecordLList, exeptions, otpuskList);
            // Добавляем информацию о больничных 
            timeSheetRecordLList = AddHospitalsToTimeSheetRecords(employee, timeSheetRecordLList, inkapacities);
            //Добавляем информацию о командировках
            timeSheetRecordLList = AddBusinessTripToTimeSheetRecords(employee, timeSheetRecordLList, businesstrips);


            return timeSheetRecordLList;
        }

 //===============================================Константы=================================================================      

        public static int IdYavka = 0;
        public static int IdVihodnoy = 17;
        public static int IdTransferVihodnoy = 27; // перенесенный выходной
        public static int IdOtpusk = 6; // основной отпуск
        public static int IdOtpuskFemale = 10; // отпуск женский
        public static int IdX = 21;
        public static int IdPp = 22;
        public static int Week5Days = 1;
        public static int Week6Days = 2;
        public static int IdWorkTypeSovmeshenie = 4;
        public static int IdWorkTypePochesovik = 19;
        public static int IdWorkTypeIspOb = 20;
        public static int IdPps = 2;
        public static int IdBusinessTripKind = 17;
        public static int BeginEvent = 1;
        public static int FullManHours = 40;
        public static int FullWomanHours = 36;
        public static int FullPpsHours = 36;

        //===========================   private Methods     ========================================================================
        private KadrDataContext _db;
        private TimeSheet _timeSheet;

        //Генерация табеля для работников
        private void InsertEmployees(FactStaffWithHistory[] employees)
        {
            try
            {
                FactStaffWithHistory previousEmpl = null; // переменная для хранения предыдущей FactStaffWithHistory
                var exeptions = _db.Exception.Where(w => w.DateException >= _timeSheet.DateBeginPeriod && w.DateException <= _timeSheet.DateEndPeriod).ToArray();
                var factStaffIds = employees.Select(s => s.id).Distinct();
                var otpusk = _db.OK_Otpusks.Where(w => factStaffIds.Contains(w.idFactStaff) && w.DateBegin <= _timeSheet.DateEndPeriod &&
                             w.DateEnd >= _timeSheet.DateBeginPeriod).ToArray();
                var holspitals = _db.OK_Inkapacities.Where(w => employees.Select(s => s.idEmployee).Distinct().Contains(w.idEmployee) 
                            && w.DateBegin <= _timeSheet.DateEndPeriod && w.DateEnd >= _timeSheet.DateBeginPeriod);
                var trips = _db.Events.Where(w => employees.Select(x=>x.idFactStaffHistory).Contains(w.idFactStaffHistory)
                            && w.DateBegin <= _timeSheet.DateEndPeriod && w.idPrikazEnd == null &&
                             w.DateEnd >= _timeSheet.DateBeginPeriod && w.idEventKind == IdBusinessTripKind && w.idEventType == BeginEvent);

                foreach (var employee in employees)
                {
                    var employeeOtpusk = otpusk.Where(w => w.DateBegin.HasValue && w.idFactStaff == employee.id && 
                        w.DateBegin <= _timeSheet.DateEndPeriod && w.DateEnd >= _timeSheet.DateBeginPeriod).ToArray();
                    var employeeHosp = holspitals.Where(w => w.idEmployee == employee.idEmployee && w.DateBegin <= _timeSheet.DateEndPeriod 
                        && w.DateEnd >= _timeSheet.DateBeginPeriod).ToArray();
                    var employeeTrips = trips.Where(w => w.FactStaffHistory.idFactStaff == employee.id && w.DateBegin <= _timeSheet.DateEndPeriod
                        && w.DateEnd >= _timeSheet.DateBeginPeriod).ToArray();
           
                    _timeSheetRecordLList.AddRange(InsertEmployee(employee, exeptions, employeeOtpusk, employeeHosp, employeeTrips));

                    if (previousEmpl != null                                    //если имеются FactStaffWithHistory с одинаковыми idEmployee, StaffCount, idPlanStaff, id(idFactStaff)
                          && previousEmpl.idEmployee == employee.idEmployee
                          && previousEmpl.StaffCount == employee.StaffCount
                          && previousEmpl.idPlanStaff == employee.idPlanStaff
                          && previousEmpl.id == employee.id)
                    {
                        var previousItems = _timeSheetRecordLList.Where(p => p.idFactStaffHistory == previousEmpl.idFactStaffHistory && p.idTimeSheet == _timeSheet.id).ToList(); // берем записи табеля с 1ым FactStaffWithHistory
                        var currentItems = _timeSheetRecordLList.Where(p => p.idFactStaffHistory == employee.idFactStaffHistory && p.idTimeSheet == _timeSheet.id).ToList(); // берем записи табеля с 2ым FactStaffWithHistory
                        foreach (var deletepreviousItem in previousItems)       //удаляем первые FactStaffWithHistory из общей колекции записей _timeSheetRecordLList
                        {
                            _timeSheetRecordLList.Remove(deletepreviousItem);
                        }
                        foreach (var deletecurrentItem in currentItems)         //удаляем вторые FactStaffWithHistory из общей колекции записей _timeSheetRecordLList
                        {
                            _timeSheetRecordLList.Remove(deletecurrentItem);
                        }
                        foreach (var currentItem in currentItems)               //проходим по всем записям табеля с 2ым FactStaffWithHistory 
                                                                                //и делаем слияние 1 и 2 FactStaffWithHistory записей табеля в общую
                        {                                                       //также меняем idFactStaffHistory у записей табеля 1 на dFactStaffHistory записей табеля 2 
                                                                                //"чтобы не было 2ух строк с одинаковыми человеками"(актуально при продлении трудового договора)
                            if (currentItem.RecordDate <= previousEmpl.DateEnd) {
                                var previousItemWithDay = previousItems.Where(m => m.RecordDate == currentItem.RecordDate).SingleOrDefault();
                                previousItemWithDay.idFactStaffHistory = currentItem.idFactStaffHistory;
                                _timeSheetRecordLList.Add(previousItemWithDay);
                            }
                            if (currentItem.RecordDate > previousEmpl.DateEnd)
                            {
                                _timeSheetRecordLList.Add(currentItem);
                            }
                        }
                    }
                    previousEmpl = employee; //сохраняем текущий элемент как предыдущую
                }
            }
            catch (System.Exception ex)
            {
                _db.TimeSheet.DeleteAllOnSubmit(_db.TimeSheet.Where(w => w.id == IdTimeSheet));
                _db.SubmitChanges();
                var sb = new StringBuilder();
                sb.AppendLine("Формирование табеля завершилось неудачей");
                if (employees.Any(a => a.PlanStaff.IdWorkShedule == null))
                {
                    foreach (var employee in employees.Where(w => w.PlanStaff.IdWorkShedule == null))
                    {
                        sb.AppendLine(string.Format("   {0} {1} {2}", employee.Employee.LastName,
                            employee.Employee.FirstName, employee.Employee.Otch));
                    }
                    sb.AppendLine();
                    sb.AppendLine("Для устранения проблемы обратитесь в отдел управления кадрами.");
                }
                sb.AppendLine(ex.Message);
                throw new System.Exception(sb.ToString());
            }
        }

        //Генерация табеля для пятидневной рабочей недели
        private List<TimeSheetRecord> FiveDayesTimeSheetGenerate(FactStaffWithHistory employee, bool fullEmployee)
        {
            var employee5 = (double) (employee.WorkHoursInWeek/5);
            const int womanFriday = 6;
            const double womanDaily = 7.5;

            var timeSheetRecordLList = new List<TimeSheetRecord>();
            for (int i = _timeSheet.DateBeginPeriod.Day - 1; i < _timeSheet.DateEndPeriod.Day; i++)
            {
                TimeSheetRecord timeSheetRecord;
                var date = _timeSheet.DateBeginPeriod.AddDays(i);
                if ((employee.DateEnd != null && employee.DateEnd < date) || employee.DateBegin > date)
                {
                    timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdX, _timeSheet.id, 0);
                    timeSheetRecordLList.Add(timeSheetRecord);
                    continue;
                }
                switch (date.DayOfWeek)
                {
                    case DayOfWeek.Friday:
                    {
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdYavka, employee5, fullEmployee ? womanFriday : employee5);
                        break;
                    }
                    case DayOfWeek.Saturday:
                    case DayOfWeek.Sunday:
                    {
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdVihodnoy);
                        break;
                    }
                    default:
                    {
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdYavka, employee5, fullEmployee ? womanDaily : employee5);
                        break;
                    }
                }
                timeSheetRecordLList.Add(timeSheetRecord);
            }
            return timeSheetRecordLList;
        }

        //Генерация табеля для шестидневной рабочей недели
        private List<TimeSheetRecord> SixDayesTimeSheetGenerate(FactStaffWithHistory employee, bool fullEmployee)
        {
            const double man6 = 7;
            const double woman6 = 6.25;
            const double manSaturday = 5;
            const double womanSaturday = 4.75;
            var employee6 = (double)(employee.WorkHoursInWeek / 6);
            var timeSheetRecordLList = new List<TimeSheetRecord>();
            for (int i = _timeSheet.DateBeginPeriod.Day - 1; i < _timeSheet.DateEndPeriod.Day; i++)
            {
                TimeSheetRecord timeSheetRecord;
                var date = _timeSheet.DateBeginPeriod.AddDays(i);
                if ((employee.DateEnd != null && employee.DateEnd < date) || employee.DateBegin > date)
                {
                    timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdX, _timeSheet.id, 0);
                    timeSheetRecordLList.Add(timeSheetRecord);
                    continue;
                }
                switch (date.DayOfWeek)
                {
                    case DayOfWeek.Saturday:
                    {
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdYavka, fullEmployee ? manSaturday : employee6,     //мужчины
                                                                                           fullEmployee ? womanSaturday : employee6); //женщины
                        break;
                    }
                    case DayOfWeek.Sunday:
                    {
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdVihodnoy);
                        break;
                    }
                    default:
                    {
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdYavka, fullEmployee ? man6 : employee6,
                                                                                            fullEmployee ? woman6 : employee6);
                        break;
                    }
                }
                timeSheetRecordLList.Add(timeSheetRecord);
            }
            return timeSheetRecordLList;
        }

        //Генерация табеля для гибкой недели
        private List<TimeSheetRecord> FlexibleTimeSheetGenerate(FactStaffWithHistory employee)
        {
            var timeSheetRecordLList = new List<TimeSheetRecord>();
            for (int i = _timeSheet.DateBeginPeriod.Day - 1; i < _timeSheet.DateEndPeriod.Day; i++)
            {
                TimeSheetRecord timeSheetRecord;
                var date = _timeSheet.DateBeginPeriod.AddDays(i);
                if ((employee.DateEnd != null && employee.DateEnd < date) || employee.DateBegin > date)
                {
                    timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdX, _timeSheet.id, 0);
                    timeSheetRecordLList.Add(timeSheetRecord);
                    continue;
                }
                switch (date.DayOfWeek)
                {
                    default:
                        {
                            timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdVihodnoy);
                            break;
                        }
                }
                timeSheetRecordLList.Add(timeSheetRecord);
            }
            return timeSheetRecordLList;
        }

        //Генерация табеля для ППС
        private List<TimeSheetRecord> PpsTimeSheetGenerate(FactStaffWithHistory employee, bool fullEmployee)
        {
            var employeePps = (double)(employee.WorkHoursInWeek / 6);
            const double fullPps = 6.25;
            const double saturdayPps = 4.75;

            var timeSheetRecordLList = new List<TimeSheetRecord>();
            for (int i = _timeSheet.DateBeginPeriod.Day - 1; i < _timeSheet.DateEndPeriod.Day; i++)
            {
                TimeSheetRecord timeSheetRecord;
                var date = _timeSheet.DateBeginPeriod.AddDays(i);
                if ((employee.DateEnd != null && employee.DateEnd < date) || employee.DateBegin > date)
                {
                    timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdX, _timeSheet.id, 0);
                    timeSheetRecordLList.Add(timeSheetRecord);
                    continue;
                }
                switch (date.DayOfWeek)
                {
                    case DayOfWeek.Saturday:
                    {
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdYavka, (employee.StaffCount == 1) ? saturdayPps : employeePps,
                                                                                           (employee.StaffCount == 1) ? saturdayPps : employeePps);
                        break;
                    }
                    case DayOfWeek.Sunday:
                    {
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdVihodnoy);
                        break;
                    }
                    default:
                    {
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdYavka, (employee.StaffCount == 1) ? fullPps : employeePps,
                                                                                           (employee.StaffCount == 1) ? fullPps : employeePps);
                        break;
                    }
                }
                timeSheetRecordLList.Add(timeSheetRecord);
            }
            return timeSheetRecordLList;
        }

        //Вносит в табель дни исключения и возвращает обновлённые записи табеля на работника
        List<TimeSheetRecord> AddExceptoinDaysToTimeSheetRecords(FactStaffWithHistory employee, IEnumerable<TimeSheetRecord> timeSheetRecords, 
            IEnumerable<Exception> exeptions, IEnumerable<OK_Otpusk> otpusk)
        {
            var timeSheetRecordLList = new List<TimeSheetRecord>(timeSheetRecords);
            foreach (var exception in exeptions.Where(w=>w.idWorkShedule==employee.PlanStaff.IdWorkShedule))
            {
                var day = timeSheetRecordLList.FirstOrDefault(f => f.RecordDate.Date == exception.DateException.Date);
                if (day == null || day.idDayStatus == IdX) continue;
                var list = otpusk.Select(s => s.OK_Otpuskvid.idDayStatus);
               
                if ((list.Any(x => !x.HasValue ||x.Value == day.idDayStatus) && exception.idDayStatus == IdTransferVihodnoy)
                    || (list.Contains(IdOtpuskFemale) && exception.idDayStatus == IdVihodnoy))
                {       // если у человека отпуск и это перенесенный выходной (праздник) -> то оставляем отпуск
                        // если женский отпуск и это выходной(праздник) -> то оставляем отпуск
                    continue;
                }

                if (exception.idDayStatus == IdPp)
                {
                    day.JobTimeCount = day.JobTimeCount > 1 ? day.JobTimeCount - 1 : 0;
                }
                else
                {
                    timeSheetRecordLList.Remove(day);
                    if (employee.StaffCount == 1)
                    {
                        day = NewTimeSheetRecord(exception.DateException, employee.idFactStaffHistory,
                            (exception.idDayStatus == IdTransferVihodnoy) ? IdVihodnoy: exception.idDayStatus, // перенесенный выходной менякм на обычный выходной
                            _timeSheet.id, employee.Employee.SexBit && employee.PlanStaff.Post.Category.id != IdPps
                                ? (double)exception.KolHourMPS
                                : (double)exception.KolHourGPS);
                    }
                    else
                    {
                        day = NewTimeSheetRecord(exception.DateException, employee.idFactStaffHistory,
                             (exception.idDayStatus == IdTransferVihodnoy) ? IdVihodnoy : exception.idDayStatus, // перенесенный выходной менякм на обычный выходной
                            _timeSheet.id, (double)employee.StaffCount *
                                           (employee.Employee.SexBit &&
                                            employee.PlanStaff.Post.Category.id != IdPps
                                               ? (double)exception.KolHourMNS
                                               : (double)exception.KolHourGNS));
                    }
                    timeSheetRecordLList.Add(day);
                }
            }
            return timeSheetRecordLList;
        }

        //Вносит в табель информацию о отпуске и возвращает обновлённые записи табеля на работника
        List<TimeSheetRecord> AddHolidaysToTimeSheetRecords(FactStaffWithHistory employee, IEnumerable<TimeSheetRecord> timeSheetRecords, IEnumerable<OK_Otpusk> otpusk)
        {
            var timeSheetRecordLList = new List<TimeSheetRecord>(timeSheetRecords);
            var okOtpusks = otpusk as OK_Otpusk[] ?? otpusk.ToArray();
            if (okOtpusks.Any())
            {
                foreach (var okOtpusk in okOtpusks)
                {
                    int beginDay;
                    if (_timeSheet.DateBeginPeriod.Year == okOtpusk.DateBegin.Value.Year &&
                        _timeSheet.DateBeginPeriod.Month == okOtpusk.DateBegin.Value.Month)
                    {
                        beginDay = okOtpusk.DateBegin.Value.Day;
                    }
                    else
                    {
                        beginDay = _timeSheet.DateBeginPeriod.Day;
                    }

                    int endDay;
                    if (okOtpusk.DateEnd.HasValue && _timeSheet.DateEndPeriod.Year == okOtpusk.DateEnd.Value.Year &&
                        _timeSheet.DateEndPeriod.Month == okOtpusk.DateEnd.Value.Month)
                    {
                        endDay = okOtpusk.DateEnd.Value.Day;
                    }
                    else
                    {
                        endDay = _timeSheet.DateEndPeriod.Day;
                    }

                    for (int i = beginDay; i <= endDay; i++)
                    {
                        var otpuskDay = new DateTime(_timeSheet.DateBeginPeriod.Year, _timeSheet.DateBeginPeriod.Month,
                            i);
                        var record =
                            timeSheetRecordLList.FirstOrDefault(
                                f => f.RecordDate == otpuskDay && f.idFactStaffHistory == employee.idFactStaffHistory);
                        if (record == null || record.idDayStatus == IdX) continue;
                        if (okOtpusk.OK_Otpuskvid.idDayStatus != null)
                        {
                            record.idDayStatus = (int)okOtpusk.OK_Otpuskvid.idDayStatus;
                            record.JobTimeCount = 0;
                        }
                        else
                        {
                            record.idDayStatus = 6;
                            record.JobTimeCount = 0;
                        }
                    }
                }
            }
            return timeSheetRecordLList;
        }

        //Вносит в табель информацию о больничном и возвращает обновлённые записи табеля на работника
        List<TimeSheetRecord> AddHospitalsToTimeSheetRecords(FactStaffWithHistory employee, IEnumerable<TimeSheetRecord> timeSheetRecords, IEnumerable<OK_Inkapacity> inkapacities)
        {
            var timeSheetRecordLList = new List<TimeSheetRecord>(timeSheetRecords);
            var okInkapacities = inkapacities as OK_Inkapacity[] ?? inkapacities.ToArray();
            if (okInkapacities.Any())
            {
                foreach (var inkapacity in okInkapacities)
                {
                    int beginDay;
                    if (_timeSheet.DateBeginPeriod.Year == inkapacity.DateBegin.Year &&
                        _timeSheet.DateBeginPeriod.Month == inkapacity.DateBegin.Month)
                    {
                        beginDay = inkapacity.DateBegin.Day;
                    }
                    else
                    {
                        beginDay = _timeSheet.DateBeginPeriod.Day;
                    }

                    int endDay;
                    if (inkapacity.DateEnd.HasValue && _timeSheet.DateEndPeriod.Year == inkapacity.DateEnd.Value.Year &&
                        _timeSheet.DateEndPeriod.Month == inkapacity.DateEnd.Value.Month)
                    {
                        endDay = inkapacity.DateEnd.Value.Day;
                    }
                    else
                    {
                        endDay = _timeSheet.DateEndPeriod.Day;
                    }

                    for (int i = beginDay; i <= endDay; i++)
                    {
                        var day = new DateTime(_timeSheet.DateBeginPeriod.Year, _timeSheet.DateBeginPeriod.Month,i);
                        var record = timeSheetRecordLList.FirstOrDefault(f => f.RecordDate.Date == day.Date && f.idFactStaffHistory == employee.idFactStaffHistory);
                        if (record == null || record.idDayStatus == IdX) continue;
                        record.idDayStatus = (int) Models.DayStatus.Б;
                        record.JobTimeCount = 0;
                    }
                }
            }
            return timeSheetRecordLList;
        }

        List<TimeSheetRecord> AddBusinessTripToTimeSheetRecords(FactStaffWithHistory employee,
            IEnumerable<TimeSheetRecord> timeSheetRecords, IEnumerable<Event> businesstrips)
        {
            var timeSheetRecordList = new List<TimeSheetRecord>(timeSheetRecords);
            var okBusinesstrips = businesstrips as Event[] ?? businesstrips.ToArray();
            if (okBusinesstrips.Any())
            {
                foreach (var businesstrip in okBusinesstrips)
                {
                    int beginDay;
                    if (_timeSheet.DateBeginPeriod.Year == businesstrip.DateBegin.Value.Year &&
                        _timeSheet.DateBeginPeriod.Month == businesstrip.DateBegin.Value.Month)
                    {
                        beginDay = businesstrip.DateBegin.Value.Day;
                    }
                    else
                    {
                        beginDay = _timeSheet.DateBeginPeriod.Day;
                    }
                    int endDay;
                    if (businesstrip.DateEnd.HasValue && _timeSheet.DateEndPeriod.Year == businesstrip.DateEnd.Value.Year &&
                        _timeSheet.DateEndPeriod.Month == businesstrip.DateEnd.Value.Month)
                    {
                        endDay = businesstrip.DateEnd.Value.Day;
                    }
                    else
                    {
                        endDay = _timeSheet.DateEndPeriod.Day;
                    }

                    for (int i = beginDay; i <= endDay; i++)
                    {
                        var day = new DateTime(_timeSheet.DateBeginPeriod.Year, _timeSheet.DateBeginPeriod.Month, i);
                        var record = timeSheetRecordList.FirstOrDefault(f => f.RecordDate.Date == day.Date && f.idFactStaffHistory == employee.idFactStaffHistory);
                        if (record == null || record.idDayStatus == IdX) continue;
                        record.idDayStatus = (int)Models.DayStatus.К;
                        record.JobTimeCount = 0;
                    }
                }
            }
            return timeSheetRecordList;
        }

        //Генерирует запись в табеле
      /*  private TimeSheetRecord GenerageTimeSheetRecord(FactStaffWithHistory employee, DateTime date, int idDayStatus,
            double manFullDay=0, double manNotFullDay=0, double womanFullDay=0, double womanNotFullDay=0)
        {
            if (employee.StaffCount == 1)
            {
                return NewTimeSheetRecord(date, employee.idFactStaffHistory, idDayStatus,
                    _timeSheet.id, employee.Employee.SexBit ? manFullDay : womanFullDay);
            }
            return NewTimeSheetRecord(date, employee.idFactStaffHistory, idDayStatus,
                _timeSheet.id,
                (double) employee.StaffCount*
                (employee.Employee.SexBit ? manNotFullDay : womanNotFullDay));
        }*/

        private TimeSheetRecord GenerageTimeSheetRecord(FactStaffWithHistory employee, DateTime date, int idDayStatus,
            double manDay = 0, double womanDay = 0)
        {

                return NewTimeSheetRecord(date, employee.idFactStaffHistory, idDayStatus,
                    _timeSheet.id, employee.Employee.SexBit ? manDay: womanDay);
            
        }

        //Создаёт запись в табеле
        private TimeSheetRecord NewTimeSheetRecord(DateTime recordDate, int idFactStaffHistory, int idDayStatus,
            int idTimeSheet, double jobTimeCount)
        {
            return new TimeSheetRecord
            {
                IdTimeSheetRecord = Guid.NewGuid(),
                idFactStaffHistory = idFactStaffHistory,
                idDayStatus = idDayStatus,
                idTimeSheet = idTimeSheet,
                IsChecked = false,
                RecordDate = recordDate,
                JobTimeCount = Math.Round(jobTimeCount,2)
            };
        }

        //Сериализует записи табеля в XML
        private XElement SerializeTimeSheetRecordsToXml(IEnumerable<TimeSheetRecord> records)
        {
            var culture = new CultureInfo("en-US");     
            return new XElement("TimeSheetRecords",records.Select(record => new XElement("Record",
                    new XElement("IdTimeSheetRecord", record.IdTimeSheetRecord),
                    new XElement("JobTimeCount", record.JobTimeCount),
                    new XElement("idTimeSheet", record.idTimeSheet),
                    new XElement("idDayStatus", record.idDayStatus),
                    new XElement("IsChecked", record.IsChecked ?? false),
                    new XElement("idFactStaffHistory", record.idFactStaffHistory),
                    new XElement("RecordDate", record.RecordDate)))
                );
        }

        //Сохраняет записи табеля в базу
        private void SubmitTimeSheet()
        {
            try
            {
                _db.TimeSheet.InsertOnSubmit(_timeSheet);
                _db.SubmitChanges();
                foreach (var timeSheetRecord in _timeSheetRecordLList)
                {
                    timeSheetRecord.idTimeSheet = _timeSheet.id;
                }
                var r = SerializeTimeSheetRecordsToXml(_timeSheetRecordLList);
                _db.TimeSheetRecordInsert(r);
            }
            catch (System.Exception ex)
            {
                _db.TimeSheetRecords.DeleteAllOnSubmit(_db.TimeSheetRecords.Where(w => w.idTimeSheet == _timeSheet.id));
                _db.TimeSheet.DeleteOnSubmit(_timeSheet);
                _db.SubmitChanges();
                throw;
            }
        }

        private bool IsTimeSheetValid()
        {
            return _timeSheet != null && _timeSheet.id != 0 ? true : false;
        }

        private void SetDataContext(KadrDataContext db)
        {
            _db = db;
            var loadOptions = new DataLoadOptions();
            loadOptions.LoadWith((FactStaffWithHistory fswh) => fswh.PlanStaff);
            loadOptions.LoadWith((PlanStaff ps) => ps.Post);
            loadOptions.LoadWith((Post p) => p.Category);
            loadOptions.LoadWith((PlanStaff ps) => ps.WorkShedule);
            loadOptions.LoadWith((FactStaffWithHistory fswh) => fswh.Employee);
            loadOptions.LoadWith((OK_Otpusk oko) => oko.OK_Otpuskvid);
            _db.LoadOptions = loadOptions;
        }

        private void GetEmployeeByLogin(string login)
        {
            var idEmployee =
                _db.Employees.Where(w => w.EmployeeLogin.ToLower() == login.ToLower())
                    .Select(s => s.id)
                    .FirstOrDefault();
            //var idApprover = 
            //return DtoClassConstructor.DtoApprover(_db, idEmployee);
        }

    }
}