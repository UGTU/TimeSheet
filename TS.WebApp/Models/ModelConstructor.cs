using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Configuration;
using CommonBase;
using Microsoft.Ajax.Utilities;
using TimeSheetMvc4WebApplication.ClassesDTO;
using TimeSheetMvc4WebApplication.Source;

namespace TimeSheetMvc4WebApplication.Models
{
    public enum DayStatus
    {
        [Description("Я")]
        Я=0,
        [Description("Н")]
        Н=1,
        [Description("РВ")]
        РВ = 2,
        [Description("С")]
        С = 3,
        [Description("К")]
        К = 4,
        [Description("ПК")]
        ПК = 5,
        [Description("О")]
        О = 6,
        [Description("У")]
        У = 7,
        [Description("УД")]
        УД = 8,
        [Description("Р")]
        Р = 9,
        [Description("ОЖ")]
        ОЖ = 10,
        [Description("ДО")]
        ДО = 11,
        [Description("ОЗ")]
        ОЗ = 12,
        [Description("Б")]
        Б = 13,
        [Description("Т")]
        Т = 14,
        [Description("ЛЧ")]
        ЛЧ = 15,
        [Description("ПР")]
        ПР = 16,
        [Description("В")]
        В = 17,
        [Description("НН")]
        НН = 18,
        [Description("НБ")]
        НБ = 19,
        [Description("ОВ")]
        ОВ = 20,
        [Description("Х")]
        Х = 21,
        [Description("ПП")]
        ПП = 22,
        [Description("ОКД")]
        ОКД = 23,
        [Description("Г")]
        Г = 25,
        [Description("НВ")]
        НВ = 26
        //[Description("Empty")]
        //Empty = 22
    }

    public static class ModelConstructor
    {
        // Статусы дней попадающие в поле табеля Неявки по причинам
        private static readonly DayStatus[] NonWorked =
        {
            DayStatus.В,
            DayStatus.Б,
            DayStatus.О,
            DayStatus.ОЖ,
            DayStatus.ОЗ,
            DayStatus.ДО,
            DayStatus.У,
            DayStatus.К,
            DayStatus.НН,
            DayStatus.Р,
            DayStatus.ПР,
            DayStatus.УД,
            DayStatus.ОВ,
            DayStatus.Х, 
            DayStatus.ОКД,
            DayStatus.Г,
            DayStatus.НВ,
            DayStatus.ПК
        };

        public static TimeSheetModel[] TimeSheetForDepartment(DtoTimeSheet timeSheet, int firsPaperPaperRecorddsColl, int lastPaperRecordsColl, int paperRecordColl, bool isForPrint)
        {
            var timeSheets = new List<TimeSheetModel>();
            if (timeSheet.Employees.Any(a => a.FactStaffEmployee.Post.Category.IsPPS))
            {
                timeSheets.Add(TimeSheetModelConstructor(timeSheet.IdTimeSheet, timeSheet.DateComposition,
                                                         timeSheet.DateBegin, timeSheet.DateEnd,
                                                         timeSheet.Employees.Where(
                                                             w => w.FactStaffEmployee.Post.Category.IsPPS).OrderByDescending(o => o.FactStaffEmployee.Post.IsMenager).ThenBy(
                                                                  o => o.FactStaffEmployee.Post.Category.OrderBy).ThenBy(t => t.FactStaffEmployee.Surname).ToArray(),
                                                         timeSheet.Holidays,
                                                         timeSheet.Department.DepartmentFullName,
                                                         firsPaperPaperRecorddsColl,
                                                         lastPaperRecordsColl, paperRecordColl, isForPrint,
                                                         timeSheet.Approvers,false
                                   ));
            }
            if (timeSheet.Employees.Any(a => !a.FactStaffEmployee.Post.Category.IsPPS)) //не ППС
            {
                if (timeSheet.Employees.Any(a => !a.FactStaffEmployee.WorkShedule.AllowNight)) //5-ти и 6-ти дневный рабочий график
                {
                    timeSheets.Add(TimeSheetModelConstructor(timeSheet.IdTimeSheet, timeSheet.DateComposition,
                        timeSheet.DateBegin, timeSheet.DateEnd,
                        timeSheet.Employees.Where(
                            w => !w.FactStaffEmployee.Post.Category.IsPPS && !w.FactStaffEmployee.WorkShedule.AllowNight)
                            .OrderByDescending(o => o.FactStaffEmployee.Post.IsMenager)
                            .ThenBy(
                                o => o.FactStaffEmployee.Post.Category.OrderBy)
                            .ThenBy(t => t.FactStaffEmployee.Surname)
                            .ToArray(),
                        timeSheet.Holidays,
                        timeSheet.Department.DepartmentFullName,
                        firsPaperPaperRecorddsColl,
                        lastPaperRecordsColl, paperRecordColl, isForPrint,
                        timeSheet.Approvers, false
                        ));
                }

                if (timeSheet.Employees.Any(a => a.FactStaffEmployee.WorkShedule.AllowNight)) //гибкий рабочий график
                {
                    timeSheets.Add(TimeSheetModelConstructor(timeSheet.IdTimeSheet, timeSheet.DateComposition,
                        timeSheet.DateBegin, timeSheet.DateEnd,
                        timeSheet.Employees.Where(
                            w => !w.FactStaffEmployee.Post.Category.IsPPS && w.FactStaffEmployee.WorkShedule.AllowNight)
                            .OrderByDescending(o => o.FactStaffEmployee.Post.IsMenager)
                            .ThenBy(
                                o => o.FactStaffEmployee.Post.Category.OrderBy)
                            .ThenBy(t => t.FactStaffEmployee.Surname)
                            .ToArray(),
                        timeSheet.Holidays,
                        timeSheet.Department.DepartmentFullName,
                        firsPaperPaperRecorddsColl,
                        lastPaperRecordsColl, paperRecordColl, isForPrint,
                        timeSheet.Approvers, true
                        ));
                }
            }
            var idsheet = 0;
            foreach (var sheet in timeSheets)
            {
                foreach (var paper in sheet.Papers)
                {
                    paper.id = idsheet;
                    idsheet++;
                }
            }
            return timeSheets.ToArray();
        }

        private static HeaderStyle[] GetHeaderStyle(DateTime dateBegin, DateTime dateEnd ,bool isForPrint)
        {
            var holiday = "VCSS";
            var dayX = "XCSS";
            if(isForPrint)
            {
                holiday = "PrintVCSS";
                dayX = "PrintXCSS";
            }
            var headerStyle = new List<HeaderStyle>();
            for (int i = dateBegin.Day; i <= dateEnd.Day; i++)
            {
                var date = dateBegin.AddDays(i-1);
                if (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
                    headerStyle.Add(new HeaderStyle
                                        {
                                            Day = i,
                                            CSS = holiday,
                                            DayText = i.ToString(CultureInfo.InvariantCulture)
                                        });
                else
                {
                    headerStyle.Add(new HeaderStyle
                    {
                        Day = i,
                        CSS = "Empty",
                        DayText = i.ToString(CultureInfo.InvariantCulture)
                    });
                }
            }
            for (int i = dateEnd.Day+1; i <= 31; i++)
            {
                headerStyle.Add(new HeaderStyle
                {
                    Day = i,
                    CSS = dayX,
                    DayText = DayStatus.Х.Description()
                });
            }
            return headerStyle.ToArray();
        }

        private static TimeSheetModel TimeSheetModelConstructor(int documentNumber, DateTime dateComposition, DateTime dateBeginPeriod,
                                                        DateTime dateEndPeriod, DtoTimeSheetEmployee[] timeSheetEmployees, DtoExceptionDay[] holiDays, string departmentName,
                                                        int firsPaperPaperRecorddsColl, int lastPaperRecordsColl, int paperRecordColl, bool isForPrint,
                                                        IEnumerable<DtoTimeSheetApprover> approvers, bool withHolHours)
        {
            var employees = new List<EmployeeModel>();
            for (int i = 0; i < timeSheetEmployees.Count(); i++)
            {
                employees.Add(EmployeeModelConstructor(timeSheetEmployees[i], holiDays, i + 1, isForPrint));
            }

            var papers = new List<PaperModel>();
            var headerStyle = GetHeaderStyle(dateBeginPeriod, dateEndPeriod,isForPrint);
            var distributedEmployees = 0;
            while (distributedEmployees < employees.Count())
            {
                if (distributedEmployees == 0)
                {
                    papers.Add(PaperModelConstructor(false, false, employees.Take(firsPaperPaperRecorddsColl).ToArray(), withHolHours, headerStyle));
                    distributedEmployees += firsPaperPaperRecorddsColl < employees.Count
                        ? firsPaperPaperRecorddsColl
                        : employees.Count;
                }
                else
                {
                    papers.Add(PaperModelConstructor(false, false, employees.Skip(distributedEmployees).Take(paperRecordColl).ToArray(), withHolHours, headerStyle));
                    distributedEmployees += paperRecordColl;

                }
            }
            papers.First().IsFirst = true;
            //==========================================================
            const int takingEmployeys = 1;
            // Если первый лист являеться последним
            PaperModel[] temp;
            if (papers.Last().IsFirst)
            {
                temp = LastPaperCorrecter(papers.Last(), firsPaperPaperRecorddsColl-1, withHolHours,
                    headerStyle, takingEmployeys);
            }
            else
            {
                temp = LastPaperCorrecter(papers.Last(), lastPaperRecordsColl, withHolHours,
                    headerStyle, takingEmployeys);
            }
            papers.Remove(papers.Last());
            papers.AddRange(temp);
            papers.Last().IsLast = true;
            papers.Last().Approvers = approvers.Select(ApproverModelConstructor).ToArray();
            //==========================================================
            for (int i = 0; i < papers.Count; i++)
            {
                papers[i].PaperNum = i + 1;
                papers[i].PaperOf = papers.Count;
                papers[i].TimeSheetNum = documentNumber;
            }
            //==========================================================
            return new TimeSheetModel
            {
                DepartmentName = departmentName,
                DateBeginPeriod = dateBeginPeriod,
                DateComposition = dateComposition,
                DateEndPeriod = dateEndPeriod,
                DocumentNumber = documentNumber,
                Papers = papers.ToArray()
            };
        }

        private static PaperModel[] LastPaperCorrecter(PaperModel lastPaper, int lastPaperEmplCall, bool withHoldays, HeaderStyle[] headerStyle, int transferEmpl)
        {
            var papers = new List<PaperModel> {lastPaper};
            if (papers.Last().Employees.Count() >= lastPaperEmplCall)
            {
                var employeys = papers.Last().Employees;
                papers.Last().Employees = employeys.Take(employeys.Count()-transferEmpl).ToArray();
                var lastPageEnpl = employeys.Skip(employeys.Count() - transferEmpl).ToArray();
                papers.Add(PaperModelConstructor(false, false, lastPageEnpl, withHoldays, headerStyle));
            }
            return papers.ToArray();
        }

        private static PaperModel PaperModelConstructor(bool isFirst, bool isLast, EmployeeModel[] employees, bool withHoldays, HeaderStyle[] headerStyle)
        {
            return new PaperModel
            {
                HeaderStyle = headerStyle,
                Employees = employees,
                IsFirst = isFirst,
                IsLast = isLast,
                DisplayWithHours = (withHoldays ? "visible" : "hidden")
            };
        }

        private static EmployeeModel EmployeeModelConstructor(DtoTimeSheetEmployee employee, DtoExceptionDay[] holiDays, int employeeNumber, bool isForPrint)
        {
            var records = new List<EmployeeRecordModel>();
            for (var i = 1; i <= 32; i++)
            {
                var currentRecord = employee.Records.FirstOrDefault(f => f.Date.Day == i);
                records.Add(EmployeeRecordModelConstructor(i, isForPrint, currentRecord));
            }
            var mounthDay = employee.Records.Count(c => c.DayStays.IdDayStatus != (int)DayStatus.Х);
            var em = new EmployeeModel
            {
                Surname = employee.FactStaffEmployee.Surname,
                Name = employee.FactStaffEmployee.Name,
                Patronymic = employee.FactStaffEmployee.Patronymic,
                Post = employee.FactStaffEmployee.Post.PostSmallName,
                StaffRate = employee.FactStaffEmployee.StaffRate,
                EmployeeNumber = employeeNumber,
                Records = records.ToArray(),

                FirstHalfMonthDays = employee.Records.Count(w => w.Date.Day < 16 && !NonWorked.Any(a=>(int)a == w.DayStays.IdDayStatus)),
                FirstHalfMonthNights = employee.Records.Count(w => w.Date.Day < 16 && w.NightTimeCount != ""),
                FirstHalfMonthHours = employee.Records.Where(w => w.Date.Day < 16 && !NonWorked.Any(a => (int)a == w.DayStays.IdDayStatus)).Sum(s => s.JobTimeCount),
                FirstHalfMonthNightHours = employee.Records.Where(w => w.Date.Day < 16 && w.NightTimeCount != "").Sum(s => Convert.ToDouble(s.NightTimeCount)),

                SecondHalfMonthDays = employee.Records.Count(w => w.Date.Day >= 16 && !NonWorked.Any(a => (int)a == w.DayStays.IdDayStatus)),
                SecondHalfMonthNights = employee.Records.Count(w => w.Date.Day >= 16 && w.NightTimeCount != ""),
                SecondHalfMonthHours = employee.Records.Where(w => w.Date.Day >= 16 && !NonWorked.Any(a => (int)a == w.DayStays.IdDayStatus)).Sum(s => s.JobTimeCount),
                SecondHalfMonthNightHours = employee.Records.Where(w => w.Date.Day >= 16 && w.NightTimeCount != "").Sum(s => Convert.ToDouble(s.NightTimeCount)),
                HolidayHours = employee.Records.Where(w => holiDays.Select(h=>h.Date).Contains(w.Date)).Sum(s => s.JobTimeCount + Convert.ToDouble((s.NightTimeCount!="") ? s.NightTimeCount : "0")),                              

                //DayStatus.Х - не должен попадать в "неявки по причинам"
                NonWorkedDays = NonWorked.Where(w=>w != DayStatus.Х).Select(
                        s => new {DayStatus = s, Count = employee.Records.Count(w => w.DayStays.IdDayStatus == (int) s)})
                        .OrderByDescending(o => o.Count)
                        .ThenBy(o => o.DayStatus.Description())
                        .Select(s => EmployeeRecordModelConstructor(s.Count, s.DayStatus.Description(), isForPrint))
                        .ToArray()
            };
            return em;
        }

        private static EmployeeRecordModel EmployeeRecordModelConstructor(int day, bool isForPrint,DtoTimeSheetRecord record = null)
        {
            EmployeeRecordModel model;
            if (record == null)
                model = new EmployeeRecordModel
                {
                    Day = day,
                    DayStatus = DayStatus.Х.Description(),
                    Value = DayStatus.Х.Description(),
                    Night = DayStatus.Х.Description(),
                    NightStatus = DayStatus.Х.Description()
                };
            else
            {
                if (record.DayStays.IdDayStatus == (int)DayStatus.Х)
                    model = new EmployeeRecordModel
                    {
                        Day = day,
                        DayStatus = string.Empty,
                        Value = string.Empty,
                        Night = string.Empty,
                        NightStatus = string.Empty,
                    };
                else
                    model = new EmployeeRecordModel
                    {
                        Day = day,
                        DayStatus = record.DayStays.SmallDayStatusName,
                        Value = record.JobTimeCount.ToString(CultureInfo.InvariantCulture),
                        Night = record.NightTimeCount.ToString(CultureInfo.InvariantCulture),
                        NightStatus = record.NightTimeCount != "" ? "Н" : ""
                    };
            }
            return EmployeeRecordModelCssDecorator(model, isForPrint);
        }

        private static EmployeeRecordModel EmployeeRecordModelConstructor(int days, string dayStatus, bool isForPrint, int nights = 0)
        {
            return EmployeeRecordModelCssDecorator(new EmployeeRecordModel
            {
                Day = -1,
                DayStatus = dayStatus,
                Value = days.ToString(CultureInfo.InvariantCulture),
                Night = nights.ToString(CultureInfo.InvariantCulture),
                NightStatus = nights > 0 ?  "Н" : "",
                Count = days
            }, isForPrint);
        }

        private static ApproverModel ApproverModelConstructor (DtoTimeSheetApprover approver)
        {
            if(approver!=null)
            return new ApproverModel
                       {
                           Surname = approver.Surname,
                           Name = approver.Name,
                           Patronymic = approver.Patronymic,
                           Post = approver.Post.PostSmallName,
                           ApproveStep = approver.AppoverNumber,
                           ApproveTime = approver.ApproverDate,
                           Login = approver.EmployeeLogin
                       };
            return new ApproverModel();
        }

        private static EmployeeRecordModel EmployeeRecordModelCssDecorator(EmployeeRecordModel model, bool isForPrint)
        {
            string css = string.Empty;
            DayStatus status;
            if (Enum.TryParse(model.DayStatus, out status))
            {
                if (isForPrint)
                {
                    if ((model.Day == -1 && model.Value != "0") || (model.Day != -1 && status != DayStatus.Я))
                    {
                        switch (status)
                        {
                            case DayStatus.В:
                                css = "PrintVCSS";
                                break;
                            case DayStatus.Х:
                                css = "PrintXCSS";
                                break;
                            case DayStatus.Я:
                                css = "EmptyCSS";
                                break;
                            default:
                                css = "PrintDefCSS";
                                break;
                        }
                    }
                }
                else if ((model.Day == -1 && model.Value != "0") || model.Day != -1)
                {
                    switch (status)
                    {
                        case DayStatus.Я:
                            css = "ICSS";
                            break;
                        case DayStatus.Н:
                            css = "NCSS";
                            break;
                        case DayStatus.РВ:
                            css = "RvCSS";
                            break;
                        case DayStatus.С:
                            css = "SCSS";
                            break;
                        case DayStatus.К:
                            css = "KCSS";
                            break;
                        case DayStatus.ПК:
                            css = "PkCSS";
                            break;
                        case DayStatus.О:
                            css = "OCSS";
                            break;
                        case DayStatus.У:
                            css = "YCSS";
                            break;
                        case DayStatus.УД:
                            css = "YdCSS";
                            break;
                        case DayStatus.Р:
                            css = "PCSS";
                            break;
                        case DayStatus.ОЖ:
                            css = "OzhCSS";
                            break;
                        case DayStatus.ДО:
                            css = "DoCSS";
                            break;
                        case DayStatus.ОЗ:
                            css = "OzCSS";
                            break;
                        case DayStatus.Б:
                            css = "BCSS";
                            break;
                        case DayStatus.Т:
                            css = "TCSS";
                            break;
                        case DayStatus.ЛЧ:
                            css = "LchCSS";
                            break;
                        case DayStatus.ПР:
                            css = "PrCSS";
                            break;
                        case DayStatus.В:
                            css = "VCSS";
                            break;
                        case DayStatus.НН:
                            css = "NnCSS";
                            break;
                        case DayStatus.НБ:
                            css = "NbCSS";
                            break;
                        case DayStatus.ОВ:
                            css = "OvCSS";
                            break;
                        case DayStatus.Х:
                            css = "XCSS";
                            break;
                        default:
                            css = "defCSS";
                            break;
                    }
                }
            }
            model.CSS = css;
            return model;
        }
    }
}