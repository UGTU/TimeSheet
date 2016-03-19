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
            var employees = _db.FactStaffWithHistory.Where(
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
            if (employee.PlanStaff.Post.Category.id == IdPps)
            {
                //PPS
                timeSheetRecordLList = PpsTimeSheetGenerate(employee);
            }
            else
            {
                timeSheetRecordLList = employee.PlanStaff.WorkShedule.id == Week5Days
                    //5 days week
                    ? FiveDayesTimeSheetGenerate(employee)
                    //6 days week
                    : SixDayesTimeSheetGenerate(employee);
            }


            // Добавляем информацию о отпусе 
            timeSheetRecordLList = AddHolidaysToTimeSheetRecords(employee, timeSheetRecordLList, otpuskList);    
            // Добавляем дни исключения
            timeSheetRecordLList = AddExceptoinDaysToTimeSheetRecords(employee, timeSheetRecordLList, exeptions);
            // Добавляем информацию о больничных 
            timeSheetRecordLList = AddHospitalsToTimeSheetRecords(employee, timeSheetRecordLList, inkapacities);
            //Добавляем информацию о командировках
            timeSheetRecordLList = AddBusinessTripToTimeSheetRecords(employee, timeSheetRecordLList, businesstrips);


            return timeSheetRecordLList;
        }

        //===========================   private Methods     ========================================================================

        private KadrDataContext _db;
        private const int IdYavka = 0;
        private const int IdVihodnoy = 17;
        private const int IdX = 21;
        private const int IdPp = 22;
        private const int Week5Days = 1;
        private const int Week6Days = 2;
        private const int IdWorkTypeSovmeshenie = 4;
        private const int IdWorkTypePochesovik = 19;
        private const int IdWorkTypeIspOb = 20;
        private const int IdPps = 2;
        private const int IdBusinessTripKind = 17;
        private const int BeginEvent = 1;
        private TimeSheet _timeSheet;

        //Генерация табеля для работников
        private void InsertEmployees(FactStaffWithHistory[] employees)
        {
            try
            {
                var exeptions = _db.Exception.Where(w => w.DateException >= _timeSheet.DateBeginPeriod && w.DateException <= _timeSheet.DateEndPeriod).ToArray();
                var factStaffIds = employees.Select(s => s.id).Distinct();
                var otpusk = _db.OK_Otpusk.Where(w => factStaffIds.Contains(w.idFactStaff) && w.DateBegin <= _timeSheet.DateEndPeriod &&
                             w.DateEnd >= _timeSheet.DateBeginPeriod).ToArray();
                var holspitals = _db.OK_Inkapacities.Where(w => employees.Select(s => s.idEmployee).Distinct().Contains(w.idEmployee) 
                            && w.DateBegin <= _timeSheet.DateEndPeriod && w.DateEnd >= _timeSheet.DateBeginPeriod);
                var trips = _db.Events.Where(w => employees.Select(x=>x.idFactStaffHistory).Contains(w.idFactStaffHistory)
                            && w.DateBegin <= _timeSheet.DateEndPeriod && w.idPrikazEnd == null &&
                             w.DateEnd >= _timeSheet.DateBeginPeriod && w.idEventKind == IdBusinessTripKind && w.idEventType == BeginEvent);

                foreach (var employee in employees)
                {
                    var employeeOtpusk = otpusk.Where(w => w.idFactStaff == employee.id && w.DateBegin <= _timeSheet.DateEndPeriod 
                        && w.DateEnd >= _timeSheet.DateBeginPeriod).ToArray();
                    var employeeHosp = holspitals.Where(w => w.idEmployee == employee.idEmployee && w.DateBegin <= _timeSheet.DateEndPeriod 
                        && w.DateEnd >= _timeSheet.DateBeginPeriod).ToArray();
                    var employeeTrips = trips.Where(w => w.FactStaffHistory.idFactStaff == employee.id && w.DateBegin <= _timeSheet.DateEndPeriod
                        && w.DateEnd >= _timeSheet.DateBeginPeriod).ToArray();
                    _timeSheetRecordLList.AddRange(InsertEmployee(employee, exeptions, employeeOtpusk, employeeHosp, employeeTrips));
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
        private List<TimeSheetRecord> FiveDayesTimeSheetGenerate(FactStaffWithHistory employee)
        {
            const double man5 = 8.0;
            const double womanFullDay5 = 7.5;
            const double womanNotFullDay5 = 7.2;
            const double womanFriday5 = 6.0;
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
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdYavka, man5, man5, womanFriday5,
                            womanNotFullDay5);
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
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdYavka, man5, man5, womanFullDay5,
                            womanNotFullDay5);
                        break;
                    }
                }
                timeSheetRecordLList.Add(timeSheetRecord);
            }
            return timeSheetRecordLList;
        }

        //Генерация табеля для шестидневной рабочей недели
        private List<TimeSheetRecord> SixDayesTimeSheetGenerate(FactStaffWithHistory employee)
        {
            const double man6 = 7;
            const double manSaturday6 = 5;
            const double manNotFullDay6 = 6.6;
            const double womanFullDay6 = 6.25;
            const double womanSaturday6 = 4.75;
            const double womanNotFullDay6 = 6;
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
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdYavka, manSaturday6, manNotFullDay6, womanSaturday6,
                            womanNotFullDay6);
                        break;
                    }
                    case DayOfWeek.Sunday:
                    {
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdVihodnoy);
                        break;
                    }
                    default:
                    {
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdYavka, man6, manNotFullDay6, womanFullDay6,
                            womanNotFullDay6);
                        break;
                    }
                }
                timeSheetRecordLList.Add(timeSheetRecord);
            }
            return timeSheetRecordLList;
        }

        private List<TimeSheetRecord> FlexibleTimeSheetGenerate(FactStaffWithHistory employee)
        {
            var timeSheetRecordLList = new List<TimeSheetRecord>();
            return timeSheetRecordLList;
        }


        //Генерация табеля для ППС
        private List<TimeSheetRecord> PpsTimeSheetGenerate(FactStaffWithHistory employee)
        {
            const double fullPps = 6.25;
            const double notFullPps = 6;
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
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdYavka, saturdayPps, notFullPps,
                            saturdayPps, notFullPps);
                        break;
                    }
                    case DayOfWeek.Sunday:
                    {
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdVihodnoy);
                        break;
                    }
                    default:
                    {
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdYavka, fullPps, notFullPps,
                            fullPps, notFullPps);
                        break;
                    }
                }
                timeSheetRecordLList.Add(timeSheetRecord);
            }
            return timeSheetRecordLList;
        }

        //Вносит в табель дни исключения и возвращает обновлённые записи табеля на работника
        List<TimeSheetRecord> AddExceptoinDaysToTimeSheetRecords(FactStaffWithHistory employee, IEnumerable<TimeSheetRecord> timeSheetRecords, IEnumerable<Exception> exeptions)
        {
            var timeSheetRecordLList = new List<TimeSheetRecord>(timeSheetRecords);
            foreach (var exception in exeptions.Where(w=>w.idWorkShedule==employee.PlanStaff.IdWorkShedule))
            {
                var day = timeSheetRecordLList.FirstOrDefault(f => f.RecordDate.Date == exception.DateException.Date);
                if (day == null || day.idDayStatus == IdX) continue;
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
                            exception.idDayStatus,
                            _timeSheet.id, employee.Employee.SexBit && employee.PlanStaff.Post.Category.id != IdPps
                                ? (double)exception.KolHourMPS
                                : (double)exception.KolHourGPS);
                    }
                    else
                    {
                        day = NewTimeSheetRecord(exception.DateException, employee.idFactStaffHistory,
                            exception.idDayStatus,
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
                    if (_timeSheet.DateBeginPeriod.Year == okOtpusk.DateBegin.Year &&
                        _timeSheet.DateBeginPeriod.Month == okOtpusk.DateBegin.Month)
                    {
                        beginDay = okOtpusk.DateBegin.Day;
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

        //Генерирукет запись в табеле
        private TimeSheetRecord GenerageTimeSheetRecord(FactStaffWithHistory employee, DateTime date, int idDayStatus,
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
                JobTimeCount = jobTimeCount
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
                _db.Employee.Where(w => w.EmployeeLogin.ToLower() == login.ToLower())
                    .Select(s => s.id)
                    .FirstOrDefault();
            //var idApprover = 
            //return DtoClassConstructor.DtoApprover(_db, idEmployee);
        }
    }
}