//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Web;
//using TimeSheetMvc4WebApplication.ClassesDTO;

//namespace TimeSheetMvc4WebApplication.Source
//{
//    public class TimeSheetCrater
//    {
//        private readonly KadrDataContext _db;

//        private const double Man5 = 8.0;
//        private const double WomanFullDay5 = 7.5;
//        private const double WomanNotFullDay5 = 7.2;
//        private const double WomanFriday5 = 6.0;

//        private const double Man6 = 7;
//        private const double ManSaturday6 = 5;
//        private const double ManNotFullDay6 = 6.6;
//        private const double WomanFullDay6 = 6.25;
//        private const double WomanSaturday6 = 4.75;
//        private const double WomanNotFullDay6 = 6;


//        private const double FullPps = 6.25;
//        private const double NFullPps = 6;
//        private const double SaturdayPps = 4.75;

//        private const int IdYavka = 0;
//        private const int IdVihodnoy = 17;
//        private const int IdX = 21;
//        private const int Week5Days = 1;
//        private const int Week6Days = 2;

//        private const int IdWorkTypeSovmeshenie = 4;
//        private const int IdWorkTypePochesovik = 16;

//        private const int IdPps = 2;

//        private readonly TimeSheet _timeSheet;

//        public int? IdTimeSheet { get { return _timeSheet != null ? _timeSheet.id : (int?)null; } }
//        public DateTime? DateBegin { get { return _timeSheet != null ? _timeSheet.DateBeginPeriod : (DateTime?)null; } }
//        public DateTime? DateEnd { get { return _timeSheet != null ? _timeSheet.DateEndPeriod : (DateTime?)null; } }
//        public DateTime? DateComposition { get { return _timeSheet != null ? _timeSheet.DateComposition : (DateTime?)null; } }

//        public TimeSheetCrater(){}
//        public TimeSheetCrater(int idDepartment, DateTime dateBeginPeriod, DateTime dateEndPeriod,
//                               DtoApproverDepartment dtoApproverDepartment)
//        {
//            _db = new KadrDataContext();
//            if (dtoApproverDepartment == null) return;

//            _timeSheet = new TimeSheet
//                             {
//                                 idDepartment = idDepartment,
//                                 idCreater = dtoApproverDepartment.IdApprover,
//                                 DateBeginPeriod = dateBeginPeriod,
//                                 DateEndPeriod = dateEndPeriod,
//                                 DateComposition = DateTime.Now,
//                                 ApproveStep = 0
//                             };
//            _db.TimeSheet.InsertOnSubmit(_timeSheet);
//            _db.SubmitChanges();
//        }

//        public TimeSheetCrater(int idTimeSheet)
//        {
//            _db = new KadrDataContext();
//            var timeSheet = _db.TimeSheet.FirstOrDefault(f => f.id == idTimeSheet);
//            if (timeSheet == null) throw new NullReferenceException("Запрашиваемый табель не найден.");
//            else
//            {
//                _timeSheet = timeSheet;
//            }
//        }

//        private FactStaffCurrent[] GetAllEmployees()
//        {
//            //var employees =
//            //    _db.FactStaffCurrent.Where(
//            //        w =>
//            //        w.PlanStaff.idDepartment == _timeSheet.idDepartment && (w.DateEnd == null || w.DateEnd > _timeSheet.DateBeginPeriod) &&
//            //        w.DateBegin <= _timeSheet.DateEndPeriod && w.idTypeWork != IdWorkTypeSovmeshenie && w.idTypeWork != IdWorkTypePochesovik).
//            //        ToArray();
//            //return employees;

//            var employees =
//                _db.FactStaffCurrent.Where(
//                    w =>
//                        w.PlanStaff.idDepartment == _timeSheet.idDepartment &&
//                        (w.DateEnd == null || w.DateEnd >= _timeSheet.DateBeginPeriod) &&
//                        w.DateBegin <= _timeSheet.DateEndPeriod && w.idTypeWork != IdWorkTypeSovmeshenie &&
//                        w.idTypeWork != IdWorkTypePochesovik).
//                    ToArray();
//            return employees;
//        }

//        public DtoMessage GenerateTimeSheet()
//        {
//            return InsertEmployees(GetAllEmployees());
//        }

//        public DtoMessage RefreshTimeSheet()
//        {
//            var allEmployees = GetAllEmployees();
//            var insertedEmployees = _db.TimeSheetRecord.Where(w => w.idTimeSheet == IdTimeSheet).ToArray();
//            return InsertEmployees(allEmployees.Where(w => !insertedEmployees.Any(a => a.idFactStaff == w.id)).ToArray());
//        }




//        private DtoMessage InsertEmployees(FactStaffCurrent[] employees)
//        {
//            var timeSheetRecordLList = new List<TimeSheetRecord>();
//            if (employees.Any(a => a.PlanStaff.IdWorkShedule == null))
//            {
//                var sb = new StringBuilder();
//                sb.AppendLine("Формирования табеля завершилось неудачей!");
//                sb.AppendLine();

//                foreach (var employee in employees.Where(w => w.FactStaff.IDShedule == null))
//                {
//                    sb.AppendLine(string.Format("   {0} {1} {2}", employee.FactStaff.Employee.LastName,
//                                                employee.FactStaff.Employee.FirstName, employee.FactStaff.Employee.Otch));
//                }
//                sb.AppendLine();
//                sb.AppendLine("Для устранения проблемы обратитесь в отдел управления кадрами.");

//                _db.TimeSheet.DeleteOnSubmit(_db.TimeSheet.FirstOrDefault(f => f.id == _timeSheet.id));
//                _db.SubmitChanges();
//                return new DtoMessage
//                           {
//                               Result = false,
//                               Message = sb.ToString()
//                           };
//            }

//            var exeptions =
//                _db.Exception.Where(w => w.DateException >= _timeSheet.DateBeginPeriod || w.DateException <= _timeSheet.DateEndPeriod).
//                    ToArray();


//            foreach (var employee in employees)
//            {
//                //var otpusk = _db.OK_Otpusk.Where(w =>
//                //        w.idFactStaff == employee.id && w.DateBegin <= _timeSheet.DateEndPeriod && w.DateEnd >= _timeSheet.DateEndPeriod);
//                var otpusk = _db.OK_Otpusk.Where(w =>
//                        w.idFactStaff == employee.id && w.DateBegin <= _timeSheet.DateEndPeriod && w.DateEnd >= _timeSheet.DateBeginPeriod);

//                for (int i = _timeSheet.DateBeginPeriod.Day - 1; i < _timeSheet.DateEndPeriod.Day; i++)
//                {
//                    TimeSheetRecord timeSheetRecord;
//                    var date = _timeSheet.DateBeginPeriod.AddDays(i);
//                    if (employee.DateEnd < date || employee.DateBegin > date)
//                    {
//                        timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdX, _timeSheet.id, 0);

//                        timeSheetRecordLList.Add(timeSheetRecord);
//                        continue;
//                    }
//                    var exeptionDay = exeptions.FirstOrDefault(f => f.DateException.Date == date.Date && f.idWorkShedule == employee.FactStaff.IDShedule);
//                    if (exeptionDay != null)
//                    {
//                        if (employee.StaffCount == 1)
//                        {
//                            timeSheetRecord = NewTimeSheetRecord(date, employee.id, exeptionDay.idDayStatus,
//                                                                 _timeSheet.id,
//                                                                 employee.FactStaff.Employee.SexBit && employee.PlanStaff.Post.Category.id != IdPps
//                                                                     ? (double)exeptionDay.KolHourMPS
//                                                                     : (double)exeptionDay.KolHourGPS);
//                        }
//                        else
//                        {
//                            timeSheetRecord = NewTimeSheetRecord(date, employee.id, exeptionDay.idDayStatus,
//                                                                 _timeSheet.id,
//                                                                 (double)employee.FactStaff.FactStaffCurrent.StaffCount *
//                                                                 (employee.FactStaff.Employee.SexBit &&
//                                                                  employee.PlanStaff.Post.Category.id != IdPps
//                                                                      ? (double)exeptionDay.KolHourMNS
//                                                                      : (double)exeptionDay.KolHourGNS));
//                        }

//                        timeSheetRecordLList.Add(timeSheetRecord);
//                        continue;
//                    }

//                    var previousExeptionDay = exeptions.FirstOrDefault(f => f.DateException.Date == date.Date.AddDays(1) && f.idWorkShedule == employee.FactStaff.IDShedule);

//                    switch (date.DayOfWeek)
//                    {
//                        case DayOfWeek.Friday:
//                            {
//                                if (employee.PlanStaff.Post.Category.id == IdPps)
//                                {
//                                    if (employee.StaffCount == 1)
//                                    {
//                                        timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdYavka,
//                                                                             _timeSheet.id, FullPps);
//                                    }
//                                    else
//                                    {
//                                        timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdYavka,
//                                                                             _timeSheet.id,
//                                                                             (double)
//                                                                             employee.StaffCount *
//                                                                             NFullPps);
//                                    }
//                                }
//                                else
//                                {
//                                    if (employee.PlanStaff.WorkShedule.id == Week5Days)
//                                    {
//                                        if (employee.StaffCount == 1)
//                                        {
//                                            timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdYavka,
//                                                                                 _timeSheet.id,
//                                                                                 employee.FactStaff.Employee.SexBit
//                                                                                     ? Man5
//                                                                                     : WomanFriday5);
//                                        }
//                                        else
//                                        {
//                                            timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdYavka,
//                                                                                 _timeSheet.id,
//                                                                                 (double)
//                                                                                 employee.FactStaff.FactStaffCurrent
//                                                                                     .StaffCount *
//                                                                                 (employee.FactStaff.Employee.SexBit
//                                                                                      ? Man5
//                                                                                      : WomanNotFullDay5));
//                                        }
//                                    }
//                                    else
//                                    {
//                                        if (employee.StaffCount == 1)
//                                        {
//                                            timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdYavka,
//                                                                                 _timeSheet.id,
//                                                                                 employee.FactStaff.Employee.SexBit
//                                                                                     ? Man6
//                                                                                     : WomanFullDay6);
//                                        }
//                                        else
//                                        {
//                                            timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdYavka,
//                                                                                 _timeSheet.id,
//                                                                                 (double)
//                                                                                 employee.StaffCount *
//                                                                                 (employee.FactStaff.Employee.SexBit
//                                                                                      ? ManNotFullDay6
//                                                                                      : WomanNotFullDay6));
//                                        }
//                                    }
//                                }
//                                break;
//                            }
//                        case DayOfWeek.Saturday:
//                            {
//                                if (employee.PlanStaff.Post.Category.id == IdPps)
//                                {
//                                    if (employee.StaffCount == 1)
//                                    {
//                                        timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdYavka,
//                                                                             _timeSheet.id, SaturdayPps);
//                                    }
//                                    else
//                                    {
//                                        timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdYavka,
//                                                                             _timeSheet.id,
//                                                                             (double)
//                                                                             employee.StaffCount *
//                                                                             NFullPps);
//                                    }
//                                }
//                                else
//                                {
//                                    if (employee.FactStaff.PlanStaff.WorkShedule.id == Week5Days)
//                                    {
//                                        if (employee.StaffCount == 1)
//                                        {
//                                            timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdVihodnoy,
//                                                                                 _timeSheet.id, 0);
//                                        }
//                                        else
//                                        {
//                                            timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdVihodnoy,
//                                                                                 _timeSheet.id, 0);
//                                        }
//                                    }
//                                    else
//                                    {
//                                        if (employee.StaffCount == 1)
//                                        {
//                                            timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdYavka,
//                                                                                 _timeSheet.id,
//                                                                                 employee.FactStaff.Employee.SexBit
//                                                                                     ? ManSaturday6
//                                                                                     : WomanSaturday6);
//                                        }
//                                        else
//                                        {
//                                            timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdYavka,
//                                                                                 _timeSheet.id,
//                                                                                 (double)
//                                                                                 employee.FactStaff.FactStaffCurrent
//                                                                                     .StaffCount *
//                                                                                 (employee.FactStaff.Employee.SexBit
//                                                                                      ? ManNotFullDay6
//                                                                                      : WomanNotFullDay6));
//                                        }
//                                    }
//                                }
//                                break;
//                            }
//                        case DayOfWeek.Sunday:
//                            {
//                                timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdVihodnoy, _timeSheet.id, 0);
//                                break;
//                            }
//                        default:
//                            {
//                                if (employee.PlanStaff.Post.Category.id == IdPps)
//                                {
//                                    if (employee.StaffCount == 1)
//                                    {
//                                        timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdYavka,
//                                                                             _timeSheet.id, FullPps);
//                                    }
//                                    else
//                                    {
//                                        timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdYavka,
//                                                                             _timeSheet.id,
//                                                                             (double)
//                                                                             employee.StaffCount *
//                                                                             NFullPps);
//                                    }
//                                }
//                                else
//                                {
//                                    if (employee.FactStaff.PlanStaff.WorkShedule.id == Week5Days)
//                                    {
//                                        if (employee.StaffCount == 1)
//                                        {
//                                            timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdYavka,
//                                                                                 _timeSheet.id,
//                                                                                 employee.FactStaff.Employee.SexBit
//                                                                                     ? Man5
//                                                                                     : WomanFullDay5);
//                                        }
//                                        else
//                                        {
//                                            timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdYavka,
//                                                                                 _timeSheet.id,
//                                                                                 (double)
//                                                                                 employee.StaffCount *
//                                                                                 (employee.FactStaff.Employee.SexBit
//                                                                                      ? Man5
//                                                                                      : WomanNotFullDay5));
//                                        }
//                                    }
//                                    else
//                                    {
//                                        if (employee.StaffCount == 1)
//                                        {
//                                            timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdYavka,
//                                                                                 _timeSheet.id,
//                                                                                 employee.FactStaff.Employee.SexBit
//                                                                                     ? Man6
//                                                                                     : WomanFullDay6);
//                                        }
//                                        else
//                                        {
//                                            timeSheetRecord = NewTimeSheetRecord(date, employee.id, IdYavka,
//                                                                                 _timeSheet.id,
//                                                                                 (double)
//                                                                                 employee.StaffCount *
//                                                                                 (employee.FactStaff.Employee.SexBit
//                                                                                      ? ManNotFullDay6
//                                                                                      : WomanNotFullDay6));
//                                        }
//                                    }
//                                }
//                                break;
//                            }
//                    }
//                    //if(previousExeptionDay!=null && employee.StaffCount==1 && timeSheetRecord.JobTimeCount>0)
//                    if (previousExeptionDay != null && timeSheetRecord.JobTimeCount > 0)
//                    {
//                        timeSheetRecord.JobTimeCount -= 1;
//                    }

//                    timeSheetRecordLList.Add(timeSheetRecord);
//                }
//                foreach (var okOtpusk in otpusk)
//                {
//                    int beginDay;
//                    if (_timeSheet.DateBeginPeriod.Year == okOtpusk.DateBegin.Year &&
//                        _timeSheet.DateBeginPeriod.Month == okOtpusk.DateBegin.Month)
//                    {
//                        beginDay = okOtpusk.DateBegin.Day;
//                    }
//                    else
//                    {
//                        beginDay = _timeSheet.DateBeginPeriod.Day;
//                    }

//                    int endDay;
//                    if (okOtpusk.DateEnd.HasValue && _timeSheet.DateEndPeriod.Year == okOtpusk.DateEnd.Value.Year &&
//                        _timeSheet.DateEndPeriod.Month == okOtpusk.DateEnd.Value.Month)
//                    {
//                        endDay = okOtpusk.DateEnd.Value.Day;
//                    }
//                    else
//                    {
//                        endDay = _timeSheet.DateEndPeriod.Day;
//                    }
//                    //var beginDay = okOtpusk.DateBegin.Day;
//                    //var endDay = (okOtpusk.DateEnd != null ? (DateTime) okOtpusk.DateEnd : _timeSheet.DateEndPeriod).Day;


//                    for (int i = beginDay; i <= endDay; i++)
//                    {
//                        //var otpuskDay = okOtpusk.DateBegin.AddDays(i - okOtpusk.DateBegin.Day);
//                        var otpuskDay = new DateTime(_timeSheet.DateBeginPeriod.Year, _timeSheet.DateBeginPeriod.Month, i);
//                        var record =
//                            timeSheetRecordLList.FirstOrDefault(
//                                f => f.RecordDate == otpuskDay && f.idFactStaff == employee.id);
//                        //if (record != null || exeptions.FirstOrDefault(f => f.DateException == record.RecordDate) != null)
//                        if (record != null && !exeptions.Any(f => record != null && f.DateException == record.RecordDate))
//                            if (okOtpusk.OK_Otpuskvid.idDayStatus != null)
//                            {
//                                record.idDayStatus = (int)okOtpusk.OK_Otpuskvid.idDayStatus;
//                                record.JobTimeCount = 0;
//                            }
//                            else
//                            {
//                                record.idDayStatus = 6;
//                                record.JobTimeCount = 0;
//                            }
//                    }
//                }
//            }
//            _db.TimeSheetRecord.InsertAllOnSubmit(timeSheetRecordLList);
//            _db.SubmitChanges();
//            return new DtoMessage
//            {
//                Message = "Табель успешно сформирован.",
//                Result = true
//            };
//        }

//        TimeSheetRecord NewTimeSheetRecord(DateTime recordDate, int idFactStaff, int idDayStatus, int idTimeSheet, double jobTimeCount)
//        {
//            return new TimeSheetRecord
//                       {
//                           idFactStaff = idFactStaff,
//                           idDayStatus = idDayStatus,
//                           idTimeSheet = idTimeSheet,
//                           IsChecked = false,
//                           RecordDate = recordDate,
//                           JobTimeCount = jobTimeCount
//                       };
//        }
//    }
//}