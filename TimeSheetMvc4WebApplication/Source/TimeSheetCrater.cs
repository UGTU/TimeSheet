﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using TimeSheetMvc4WebApplication.ClassesDTO;

namespace TimeSheetMvc4WebApplication.Source
{
    public class TimeSheetCrater
    {
        private readonly KadrDataContext _db;

        private const double Man5 = 8.0;
        private const double WomanFullDay5 = 7.5;
        private const double WomanNotFullDay5 = 7.2;
        private const double WomanFriday5 = 6.0;

        private const double Man6 = 7;
        private const double ManSaturday6 = 5;
        private const double ManNotFullDay6 = 6.6;
        private const double WomanFullDay6 = 6.25;
        private const double WomanSaturday6 = 4.75;
        private const double WomanNotFullDay6 = 6;


        private const double FullPps = 6.25;
        private const double NFullPps = 6;
        private const double SaturdayPps = 4.75;

        private const int IdYavka = 0;
        private const int IdVihodnoy = 17;
        private const int IdX = 21;
        private const int IdPp = 22;
        private const int Week5Days = 1;
        private const int Week6Days = 2;

        private const int IdWorkTypeSovmeshenie = 4;
        private const int IdWorkTypePochesovik = 16;

        private const int IdPps = 2;

        private readonly TimeSheet _timeSheet;

        public int? IdTimeSheet
        {
            get { return _timeSheet != null ? _timeSheet.id : (int?)null; }
        }

        public DateTime? DateBegin
        {
            get { return _timeSheet != null ? _timeSheet.DateBeginPeriod : (DateTime?)null; }
        }

        public DateTime? DateEnd
        {
            get { return _timeSheet != null ? _timeSheet.DateEndPeriod : (DateTime?)null; }
        }

        public DateTime? DateComposition
        {
            get { return _timeSheet != null ? _timeSheet.DateComposition : (DateTime?)null; }
        }

        readonly List<TimeSheetRecord> _timeSheetRecordLList = new List<TimeSheetRecord>();

        public TimeSheetCrater(int idDepartment, DateTime dateBeginPeriod, DateTime dateEndPeriod,DtoApproverDepartment dtoApproverDepartment, KadrDataContext db)
        {
            _db = db;
            if (dtoApproverDepartment == null) return;
            _timeSheet = new TimeSheet
            {
                idDepartment = idDepartment,
                idCreater = dtoApproverDepartment.IdApprover,
                DateBeginPeriod = dateBeginPeriod,
                DateEndPeriod = dateEndPeriod,
                DateComposition = DateTime.Now,
                ApproveStep = 0
            };
            _timeSheetRecordLList = new List<TimeSheetRecord>();
        }

        public TimeSheetCrater(int idTimeSheet, KadrDataContext db)
        {
            _db = db;
            _timeSheet = _db.TimeSheet.FirstOrDefault(f => f.id == idTimeSheet);
        }

        public bool RemoveTimeSheet()
        {
            if (!CanEditTimeSheet()) return false;
            _db.TimeSheetApproval.DeleteAllOnSubmit(_db.TimeSheetApproval.Where(w => w.idTimeSheet == _timeSheet.id));
            _db.TimeSheetRecord.DeleteAllOnSubmit(_db.TimeSheetRecord.Where(w => w.idTimeSheet == _timeSheet.id));
            _db.TimeSheet.DeleteAllOnSubmit(_db.TimeSheet.Where(w => w.id == _timeSheet.id));
            _db.SubmitChanges();
            return true;
        }

        public bool RemoveEmployee(int idFactStuffHistory)
        {
            if (!CanEditTimeSheet()) return false;
            _db.TimeSheetRecord.DeleteAllOnSubmit(_db.TimeSheetRecord.Where(w => w.idTimeSheet == _timeSheet.id && w.idFactStaffHistory==idFactStuffHistory));
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
            using (var db = new KadrDataContext())
            {
                var lastDateOfTimeSheetApproval =
                    db.TimeSheetApproval.Where(w => w.idTimeSheet == _timeSheet.id)
                        .OrderByDescending(o => o.ApprovalDate)
                        .FirstOrDefault();
                if (lastDateOfTimeSheetApproval != null)
                {
                    var lastApproval =
                        db.TimeSheetApproval.FirstOrDefault(
                            w =>
                                w.idTimeSheet == _timeSheet.id &
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



        public void GenerateTimeSheet()
        {
            InsertEmployees(GetAllEmployees());
        }

        public void GenerateTimeSheet(DtoFactStaffEmployee[] employees)
        {
            var factStuffHistiryIdList = employees.Select(s => s.IdFactStaffHistiry).ToArray();
            InsertEmployees(
                GetAllEmployees().Where(w => factStuffHistiryIdList.Contains(w.idFactStaffHistory)).ToArray());
        }

        public bool SubmitTimeSheet()
        {
            try
            {
                _db.TimeSheet.InsertOnSubmit(_timeSheet);
                _db.SubmitChanges();
                foreach (var timeSheetRecord in _timeSheetRecordLList)
                {
                    timeSheetRecord.idTimeSheet = _timeSheet.id;
                }
                _db.TimeSheetRecordInsert(SerializeTimeSheetRecordsToXml(_timeSheetRecordLList));
                return true;
            }
            catch (System.Exception ex)
            {
                _db.TimeSheet.DeleteOnSubmit(_timeSheet);
                _db.TimeSheetRecord.DeleteAllOnSubmit(_db.TimeSheetRecord.Where(w=>w.idTimeSheet==_timeSheet.id));
                _db.SubmitChanges();
                return false;
            }
        }

        public FactStaffWithHistory[] GetAllEmployees()
        {
            var employees = _db.FactStaffWithHistory.Where(
                w => w.PlanStaff.idDepartment == _timeSheet.idDepartment &&
                     (w.DateEnd == null || w.DateEnd >= _timeSheet.DateBeginPeriod) &&
                     w.DateBegin <= _timeSheet.DateEndPeriod && w.idTypeWork != IdWorkTypeSovmeshenie &&
                     w.idTypeWork != IdWorkTypePochesovik).ToArray();
            return employees;
        }


        private void InsertEmployees(FactStaffWithHistory[] employees)
        {
            try
            {
                var exeptions =
                    _db.Exception.Where(
                        w =>
                            w.DateException >= _timeSheet.DateBeginPeriod && w.DateException <= _timeSheet.DateEndPeriod)
                        .
                        ToArray();

                var factStaffIds = employees.Select(s => s.id).Distinct();

                var otpusk =
                    _db.OK_Otpusk.Where(
                        w => factStaffIds.Contains(w.idFactStaff) && w.DateBegin <= _timeSheet.DateEndPeriod &&
                             w.DateEnd >= _timeSheet.DateBeginPeriod).ToArray();

                
                foreach (var employee in employees)
                {
                    _timeSheetRecordLList.AddRange(InsertEmployee(employee, exeptions, otpusk));
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

        private IEnumerable<TimeSheetRecord> InsertEmployee(FactStaffWithHistory employee, IEnumerable<Exception> exeptions, IEnumerable<OK_Otpusk> otpuskList)
        {
            var timeSheetRecordLList = new List<TimeSheetRecord>();
            //===============  Шаблонный табель  ===============
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
                            if (employee.PlanStaff.Post.Category.id == IdPps)
                            {
                                if (employee.StaffCount == 1)
                                {
                                    timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdYavka,
                                        _timeSheet.id, FullPps);
                                }
                                else
                                {
                                    timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdYavka,
                                        _timeSheet.id, (double)employee.StaffCount * NFullPps);
                                }
                            }
                            else
                            {
                                if (employee.PlanStaff.WorkShedule.id == Week5Days)
                                {
                                    if (employee.StaffCount == 1)
                                    {
                                        timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdYavka,
                                            _timeSheet.id, employee.Employee.SexBit ? Man5 : WomanFriday5);
                                    }
                                    else
                                    {
                                        timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdYavka,
                                            _timeSheet.id,
                                            (double)employee.StaffCount *
                                            (employee.Employee.SexBit ? Man5 : WomanNotFullDay5));
                                    }
                                }
                                else
                                {
                                    if (employee.StaffCount == 1)
                                    {
                                        timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdYavka,
                                            _timeSheet.id, employee.Employee.SexBit ? Man6 : WomanFullDay6);
                                    }
                                    else
                                    {
                                        timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdYavka,
                                            _timeSheet.id,
                                            (double)employee.StaffCount *
                                            (employee.Employee.SexBit ? ManNotFullDay6 : WomanNotFullDay6));
                                    }
                                }
                            }
                            break;
                        }
                    case DayOfWeek.Saturday:
                        {
                            if (employee.PlanStaff.Post.Category.id == IdPps)
                            {
                                if (employee.StaffCount == 1)
                                {
                                    timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdYavka,
                                        _timeSheet.id, SaturdayPps);
                                }
                                else
                                {
                                    timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdYavka,
                                        _timeSheet.id, (double)employee.StaffCount * NFullPps);
                                }
                            }
                            else
                            {
                                if (employee.PlanStaff.WorkShedule.id == Week5Days)
                                {
                                    if (employee.StaffCount == 1)
                                    {
                                        timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdVihodnoy,
                                            _timeSheet.id, 0);
                                    }
                                    else
                                    {
                                        timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdVihodnoy,
                                            _timeSheet.id, 0);
                                    }
                                }
                                else
                                {
                                    if (employee.StaffCount == 1)
                                    {
                                        timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdYavka,
                                            _timeSheet.id, employee.Employee.SexBit ? ManSaturday6 : WomanSaturday6);
                                    }
                                    else
                                    {
                                        timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdYavka,
                                            _timeSheet.id,
                                            (double)employee.StaffCount *
                                            (employee.Employee.SexBit ? ManNotFullDay6 : WomanNotFullDay6));
                                    }
                                }
                            }
                            break;
                        }
                    case DayOfWeek.Sunday:
                        {
                            timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdVihodnoy,
                                _timeSheet.id, 0);
                            break;
                        }
                    default:
                        {
                            if (employee.PlanStaff.Post.Category.id == IdPps)
                            {
                                if (employee.StaffCount == 1)
                                {
                                    timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdYavka,
                                        _timeSheet.id, FullPps);
                                }
                                else
                                {
                                    timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdYavka,
                                        _timeSheet.id, (double)employee.StaffCount * NFullPps);
                                }
                            }
                            else
                            {
                                if (employee.PlanStaff.WorkShedule.id == Week5Days)
                                {
                                    if (employee.StaffCount == 1)
                                    {
                                        timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdYavka,
                                            _timeSheet.id, employee.Employee.SexBit ? Man5 : WomanFullDay5);
                                    }
                                    else
                                    {
                                        timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdYavka,
                                            _timeSheet.id,
                                            (double)employee.StaffCount *
                                            (employee.Employee.SexBit ? Man5 : WomanNotFullDay5));
                                    }
                                }
                                else
                                {
                                    if (employee.StaffCount == 1)
                                    {
                                        timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdYavka,
                                            _timeSheet.id, employee.Employee.SexBit ? Man6 : WomanFullDay6);
                                    }
                                    else
                                    {
                                        timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdYavka,
                                            _timeSheet.id,
                                            (double)employee.StaffCount *
                                            (employee.Employee.SexBit ? ManNotFullDay6 : WomanNotFullDay6));
                                    }
                                }
                            }
                            break;
                        }
                }
                timeSheetRecordLList.Add(timeSheetRecord);
            }

            //===============  Праздники  ===============



            foreach (var exception in exeptions)
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



            //===============  Отпуск  ===============
            var otpusk = otpuskList.Where(w => w.idFactStaff == employee.id && w.DateBegin <= _timeSheet.DateEndPeriod && w.DateEnd >= _timeSheet.DateBeginPeriod).ToArray();
            if (otpusk.Any())
            {
                foreach (var okOtpusk in otpusk)
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
                JobTimeCount = jobTimeCount,
                //TimeSheet = _timeSheet
            };
        }

        private XElement SerializeTimeSheetRecordsToXml(IEnumerable<TimeSheetRecord> records)
        {
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
    }
}