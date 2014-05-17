using System;
using System.Collections.Generic;
using System.Linq;
using TS.AppDomine.Abstract;
using TS.AppDomine.DomineModel;

namespace TS.AppDomine.Implementation
{
    public class TimeSheetGenerator : ITimeSheetGenerator
    {
        private readonly IDataProvider _provider;
        private readonly TimeSheet _timeSheet;
        public BaseTimeSheet TimeSheet { get { return _timeSheet; } }
        public TimeSheetGenerator(IDataProvider provider, Department department, DateTime dateBeginPeriod, DateTime dateEndPeriod, bool isCorrection = false, bool isFake = false)
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
                ApproveStep = 0,
            };
        }

        public void GenerateTimeSheet(IEnumerable<Employee> employees = null)
        {
            if (_timeSheet.IsFake) return;
            if (employees == null)
                employees = _provider.GetEmployeesForTimeSheet(_timeSheet.Department.IdDepartment, _timeSheet.DateBegin, _timeSheet.DateEnd);
            _timeSheet.Employees = employees;
            var exeptionDays = _provider.GetExeptionsDays(_timeSheet.DateBegin, _timeSheet.DateEnd).ToArray();
            foreach (var employee in _timeSheet.Employees)
            {
                employee.Records = GenerateTimeSheetForEmployee(employee,
                    exeptionDays.Where(w => w.WorkShedule == employee.WorkShedule));
            }
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

        private IEnumerable<TimeSheetRecord> GenerateTimeSheetForEmployee(Employee employee, IEnumerable<ExceptionDay> exceptionDays)
        {
            var timeSheetRecordLList = new List<TimeSheetRecord>();
            for (var i = _timeSheet.DateBegin.Day - 1; i < _timeSheet.DateBegin.Day; i++)
            {
                var date = _timeSheet.DateBegin.AddDays(i);
                if ((employee.DateEnd != null && employee.DateEnd < date) || employee.DateStart > date)
                {
                    timeSheetRecordLList.Add(new TimeSheetRecord(date, dayStatusX, 0));
                    continue;
                }
                if (employee.IsPps)
                {
                    timeSheetRecordLList.Add(PpsTimeSheetGenerate1(employee, date));
                    continue;
                }
                if (employee.WorkShedule.Id == (int) WorkScheduleType.SixDay)
                {
                    timeSheetRecordLList.Add(SixDayesTimeSheetGenerate1(employee, date));
                    continue;
                }
                if (employee.WorkShedule.Id == (int)WorkScheduleType.FiveDay)
                {
                    timeSheetRecordLList.Add(FiveDayesTimeSheetGenerate1(employee, date));
                    continue;
                }
            }
            return timeSheetRecordLList;
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

        private TimeSheetRecord SixDayesTimeSheetGenerate1(Employee employee, DateTime date)
        {
            var workingHours = new WorkingHours
            {
                ManFullDay = 7,
                ManNotFullDay = 6.6,
                WomanFullDay = 6.25,
                WomanNotFullDay = 6
            };
            TimeSheetRecord timeSheetRecord;
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                    {
                        var saturdayWorkingHours = workingHours;
                        saturdayWorkingHours.WomanFullDay = 4.75;
                        saturdayWorkingHours.ManFullDay = 5;
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, dayStatusYavka, saturdayWorkingHours);
                        break;
                    }
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

        private TimeSheetRecord PpsTimeSheetGenerate1(Employee employee, DateTime date)
        {
            var workingHours = new WorkingHours
            {
                ManFullDay = 6.25,
                ManNotFullDay = 6,
                WomanFullDay = 6.25,
                WomanNotFullDay = 6
            };
            TimeSheetRecord timeSheetRecord;
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                    {
                        var saturdayWorkingHours = workingHours;
                        saturdayWorkingHours.WomanFullDay = 4.75;
                        saturdayWorkingHours.ManFullDay = 4.75;
                        timeSheetRecord = GenerageTimeSheetRecord(employee, date, dayStatusYavka, saturdayWorkingHours);
                        break;
                    }
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

        private TimeSheetRecord GenerageTimeSheetRecord(Employee employee, DateTime date, DayStatus dayStatus, WorkingHours hours)
        {
            return employee.Rate == 1
                ? new TimeSheetRecord(date, dayStatus, employee.SexBit ? hours.ManFullDay : hours.WomanFullDay)
                : new TimeSheetRecord(date, dayStatus,
                    (double)employee.Rate * (employee.SexBit ? hours.ManNotFullDay : hours.WomanNotFullDay));
        }

        public void Save()
        {
            _provider.SaveTimeSheet(_timeSheet);
        }
    }
}
