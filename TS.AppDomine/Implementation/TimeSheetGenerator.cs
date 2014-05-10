using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TS.AppDomine.Abstract;
using TS.AppDomine.DomineModel;

namespace TS.AppDomine.Implementation
{
    public class TimeSheetGenerator : ITimeSheetGenerator
    {
        private readonly IDataProvider _provider;
        private readonly TimeSheet _timeSheet;
        //public BaseTimeSheet TimeSheet { get { return _timeSheet; } }

        public TimeSheetGenerator(IDataProvider provider, Department department, DateTime dateBeginPeriod, DateTime dateEndPeriod, int idApprover, bool isCorrection = false, bool isFake = false)
        {
            _provider = provider;
            _timeSheet = new TimeSheet
            {
                DateBegin = dateBeginPeriod,
                DateEnd = dateEndPeriod,
                DateComposition = DateTime.Now,
                Department = department,
                IsCorrection = isCorrection,
                IsFake = isFake,
            };
        }

        public void GenerateTimeSheet(IEnumerable<Employee> employees = null)
        {
            if(_timeSheet.IsFake) return;
            if (employees == null)
                employees = _provider.GetEmployeesForTimeSheet(_timeSheet.Department.IdDepartment, _timeSheet.DateBegin,_timeSheet.DateEnd);
            _timeSheet.Employees = employees;
            var exeptionDays = _provider.GetExeptionsDays(_timeSheet.DateBegin, _timeSheet.DateEnd);
            foreach (var employee in _timeSheet.Employees)
            {
                GenerateTimeSheetForEmployee(employee, exeptionDays.Where(w=>w.WorkShedule==employee.WorkShedule));
            }
            //throw new NotImplementedException();
        }

        //===========================   private Methods     ========================================================================
        struct WorkingHours
        {
            public double ManFullDay;
            public double ManNotFullDay;
            public double WomanFullDay;
            public double WomanNotFullDay;
        }

        private DayStatus dayStatusX;
        private DayStatus dayStatusPp;
        private DayStatus dayStatusYavka;
        private DayStatus dayStatusHolidays;

        private void GenerateTimeSheetForEmployee(Employee employee, IEnumerable<ExceptionDay> exceptionDays)
        {
            var timeSheetRecordLList = new List<TimeSheetRecord>();
            for (var i = _timeSheet.DateBegin.Day - 1; i < _timeSheet.DateBegin.Day; i++)
            {
                var date = _timeSheet.DateBegin.AddDays(i);
                if ((employee.DateEnd != null && employee.DateEnd < date) || employee.DateStart > date)
                {
                    timeSheetRecordLList.Add(new TimeSheetRecord(date, dayStatusX, 0));
                }
                if(employee.IsPps)

            }
        }

        private TimeSheetRecord FiveDayesTimeSheetGenerate1(Employee employee, DateTime date)
        {
            var workingHours = new WorkingHours
            {
                ManFullDay = 8,
                ManNotFullDay = 8,
                WomanFullDay = 7.5,
                WomanNotFullDay = 7.2
            };
            TimeSheetRecord timeSheetRecord;
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Friday:
                {
                    var fridayWorkingHours = workingHours;
                    fridayWorkingHours.WomanFullDay = 6;
                    timeSheetRecord = GenerageTimeSheetRecord(employee, date, dayStatusYavka, fridayWorkingHours);
                    break;
                }
                case DayOfWeek.Saturday:
                case DayOfWeek.Sunday:
                {
                    timeSheetRecord = GenerageTimeSheetRecord(employee, date, dayStatusHolidays, new WorkingHours());
                    break;
                }
                default:
                {
                    timeSheetRecord = GenerageTimeSheetRecord(employee, date, dayStatusYavka, workingHours);
                    break;
                }
            }
            return timeSheetRecord;
        }



        private List<TimeSheetRecord> FiveDayesTimeSheetGenerate(Employee employee)
        {
            var workingHours = new WorkingHours
            {
                ManFullDay = 8,
                ManNotFullDay = 8,
                WomanFullDay = 7.5,
                WomanNotFullDay = 7.2
            };
            var timeSheetRecordLList = new List<TimeSheetRecord>();
            for (var i = _timeSheet.DateBegin.Day - 1; i < _timeSheet.DateBegin.Day; i++)
            {
                TimeSheetRecord timeSheetRecord;
                var date = _timeSheet.DateBegin.AddDays(i);
                if ((employee.DateEnd != null && employee.DateEnd < date) || employee.DateStart > date)
                {
                    timeSheetRecord = new TimeSheetRecord(date,dayStatusX, 0);
                    timeSheetRecordLList.Add(timeSheetRecord);
                    continue;
                }
                switch (date.DayOfWeek)
                {
                    case DayOfWeek.Friday:
                    {
                        var fridayWorkingHours = workingHours;
                        fridayWorkingHours.WomanFullDay = 6;
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, dayStatusYavka, fridayWorkingHours);
                        break;
                    }
                    case DayOfWeek.Saturday:
                    case DayOfWeek.Sunday:
                    {
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, dayStatusHolidays,new WorkingHours());
                        break;
                    }
                    default:
                    {
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, dayStatusYavka,workingHours);
                        break;
                    }
                }
                timeSheetRecordLList.Add(timeSheetRecord);
            }
            return timeSheetRecordLList;
        }

        private List<TimeSheetRecord> SixDayesTimeSheetGenerate(Employee employee)
        {
            var workingHours = new WorkingHours
            {
                ManFullDay = 8,
                ManNotFullDay = 8,
                WomanFullDay = 7.5,
                WomanNotFullDay = 7.2
            };
            var timeSheetRecordLList = new List<TimeSheetRecord>();
            for (var i = _timeSheet.DateBegin.Day - 1; i < _timeSheet.DateBegin.Day; i++)
            {
                TimeSheetRecord timeSheetRecord;
                var date = _timeSheet.DateBegin.AddDays(i);
                if ((employee.DateEnd != null && employee.DateEnd < date) || employee.DateStart > date)
                {
                    timeSheetRecord = new TimeSheetRecord(date, dayStatusX, 0);
                    timeSheetRecordLList.Add(timeSheetRecord);
                    continue;
                }
                switch (date.DayOfWeek)
                {
                    case DayOfWeek.Friday:
                        {
                            var fridayWorkingHours = workingHours;
                            fridayWorkingHours.WomanFullDay = 6;
                            timeSheetRecord = GenerageTimeSheetRecord(employee, date, dayStatusYavka, fridayWorkingHours);
                            break;
                        }
                    case DayOfWeek.Saturday:
                    case DayOfWeek.Sunday:
                        {
                            timeSheetRecord = GenerageTimeSheetRecord(employee, date, dayStatusHolidays, new WorkingHours());
                            break;
                        }
                    default:
                        {
                            timeSheetRecord = GenerageTimeSheetRecord(employee, date, dayStatusYavka, workingHours);
                            break;
                        }
                }
                timeSheetRecordLList.Add(timeSheetRecord);
            }
            return timeSheetRecordLList;
        }

        //private List<TimeSheetRecord> PpsTimeSheetGenerate(Employee employee)
        //{
        //    const double fullPps = 6.25;
        //    const double notFullPps = 6;
        //    const double saturdayPps = 4.75;

        //    var timeSheetRecordLList = new List<TimeSheetRecord>();
        //    for (int i = _timeSheet.DateBeginPeriod.Day - 1; i < _timeSheet.DateEndPeriod.Day; i++)
        //    {
        //        TimeSheetRecord timeSheetRecord;
        //        var date = _timeSheet.DateBeginPeriod.AddDays(i);
        //        if ((employee.DateEnd != null && employee.DateEnd < date) || employee.DateBegin > date)
        //        {
        //            timeSheetRecord = NewTimeSheetRecord(date, employee.idFactStaffHistory, IdX, _timeSheet.id, 0);
        //            timeSheetRecordLList.Add(timeSheetRecord);
        //            continue;
        //        }
        //        switch (date.DayOfWeek)
        //        {
        //            case DayOfWeek.Saturday:
        //                {
        //                    timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdYavka, saturdayPps, notFullPps,
        //                        saturdayPps, notFullPps);
        //                    break;
        //                }
        //            case DayOfWeek.Sunday:
        //                {
        //                    timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdVihodnoy);
        //                    break;
        //                }
        //            default:
        //                {
        //                    timeSheetRecord = GenerageTimeSheetRecord(employee, date, IdYavka, fullPps, notFullPps,
        //                        fullPps, notFullPps);
        //                    break;
        //                }
        //        }
        //        timeSheetRecordLList.Add(timeSheetRecord);
        //    }
        //    return timeSheetRecordLList;
        //}

        //private TimeSheetRecord GenerageTimeSheetRecord(Employee employee, DateTime date, DayStatus dayStatus,
        //    double manFullDay = 0, double manNotFullDay = 0, double womanFullDay = 0, double womanNotFullDay = 0)
        //{
        //    return employee.Rate == 1
        //        ? new TimeSheetRecord(date, dayStatus, employee.SexBit ? manNotFullDay : womanNotFullDay)
        //        : new TimeSheetRecord(date, dayStatus,
        //            (double) employee.Rate*(employee.SexBit ? manNotFullDay : womanNotFullDay));
        //}
        private TimeSheetRecord GenerageTimeSheetRecord(Employee employee, DateTime date, DayStatus dayStatus, WorkingHours hours)
        {
            return employee.Rate == 1
                ? new TimeSheetRecord(date, dayStatus, employee.SexBit ? hours.ManFullDay : hours.WomanFullDay)
                : new TimeSheetRecord(date, dayStatus,
                    (double)employee.Rate * (employee.SexBit ? hours.ManNotFullDay : hours.WomanNotFullDay));
        }


        public void Save()
        {
            //_timeSheet.IsFake = saveAsFake;
            //return _provider.SaveTimeSheet(_timeSheet) ? _timeSheet : null;
        }
    }
}
