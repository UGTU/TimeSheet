using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CommonBase;
using TimeSheetMvc4WebApplication.ClassesDTO;

namespace TimeSheetMvc4WebApplication.Models
{
    public enum DayStatus
    {
        [Description("Я")]
        Я=0,
        [Description("Н")]
        Н=1,
        [Description("РВ")]
        Рв = 2,
        [Description("С")]
        С = 3,
        [Description("К")]
        К = 4,
        [Description("ПК")]
        Пк = 5,
        [Description("О")]
        О = 6,
        [Description("У")]
        У = 7,
        [Description("УД")]
        Уд = 8,
        [Description("Р")]
        Р = 9,
        [Description("ОЖ")]
        Ож = 10,
        [Description("ДО")]
        До = 11,
        [Description("ОЗ")]
        Оз = 12,
        [Description("Б")]
        Б = 13,
        [Description("Т")]
        Т = 14,
        [Description("ЛЧ")]
        Лч = 15,
        [Description("ПР")]
        Пр = 16,
        [Description("В")]
        В = 17,
        [Description("НН")]
        Нн = 18,
        [Description("НБ")]
        Нб = 19,
        [Description("ОВ")]
        Ов = 20,
        [Description("Х")]
        Х = 21,
        [Description("ПП")]
        Пп = 22
    }

    public static class ModelConstructor
    {
        // Статусы дней попадающие в поле табеля Неявки по причинам
        private static readonly DayStatus[] NonWorked =
        {
            DayStatus.В,
            DayStatus.Б,
            DayStatus.О,
            DayStatus.Ож,
            DayStatus.Оз,
            DayStatus.До,
            DayStatus.У,
            DayStatus.К,
            DayStatus.Нн,
            DayStatus.Р,
            DayStatus.Пр,
            DayStatus.Уд,
            DayStatus.Ов
        };

        ////private static string _я = "Я";
        ////private static string _н = "Н";
        ////private static string _рв = "РВ";
        ////private static string _с = "С";
        //private const string К = "К";
        ////private static string _пк = "ПК";
        //private const string О = "О";
        //private const string У = "У";
        //private const string Уд = "УД";
        //private const string Р = "Р";
        //private const string Ож = "ОЖ";
        //private const string До = "ДО";
        //private const string Оз = "ОЗ";
        //private const string Б = "Б";
        ////private static string _т = "Т";
        ////private static string _лч = "ЛЧ";
        //private const string Пр = "ПР";
        //private const string В = "В";
        //private const string Нн = "НН";
        ////private static string _нб = "НБ";
        ////private static string _ов = "ОВ";
        //private const string Х = "X";
        //private const string Ов = "ОВ";

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
                                                         timeSheet.Department.DepartmentFullName,
                                                         firsPaperPaperRecorddsColl,
                                                         lastPaperRecordsColl, paperRecordColl, isForPrint,
                                                         timeSheet.Approvers
                                   ));
            }
            if (timeSheet.Employees.Any(a => !a.FactStaffEmployee.Post.Category.IsPPS))
            {
                timeSheets.Add(TimeSheetModelConstructor(timeSheet.IdTimeSheet, timeSheet.DateComposition,
                                                         timeSheet.DateBegin, timeSheet.DateEnd,
                                                         timeSheet.Employees.Where(
                                                             w => !w.FactStaffEmployee.Post.Category.IsPPS).OrderByDescending(o => o.FactStaffEmployee.Post.IsMenager).ThenBy(
                                                                 o => o.FactStaffEmployee.Post.Category.OrderBy).ThenBy(t => t.FactStaffEmployee.Surname).ToArray(),
                                                         timeSheet.Department.DepartmentFullName,
                                                         firsPaperPaperRecorddsColl,
                                                         lastPaperRecordsColl, paperRecordColl, isForPrint,
                                                         timeSheet.Approvers
                                   ));
            }
            return timeSheets.ToArray();
        }

        public static HeaderStyle[] GetHeaderStyle(DateTime dateBegin, DateTime dateEnd ,bool isForPrint)
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
                    DayText = "X"
                });
            }
            return headerStyle.ToArray();
        }

        public static TimeSheetModel TimeSheetModelConstructor(int documentNumber, DateTime dateComposition, DateTime dateBeginPeriod,
                                                        DateTime dateEndPeriod, DtoTimeSheetEmployee[] timeSheetEmployees, string departmentName,
                                                        int firsPaperPaperRecorddsColl, int lastPaperRecordsColl, int paperRecordColl, bool isForPrint,
                                                        DtoTimeSheetApprover[] approvers)
        {
            var employees = new List<EmployeeModel>();
            for (int i = 0; i < timeSheetEmployees.Count(); i++)
            {
                employees.Add(EmployeeModelConstructor(timeSheetEmployees[i], i + 1, isForPrint));
            }
            var papers = new List<PaperModel>();
            var headerStyle = GetHeaderStyle(dateBeginPeriod, dateEndPeriod,isForPrint);
            var distributedEmployees = 0;
            while (distributedEmployees < employees.Count())
            {
                if (distributedEmployees == 0)
                {
                    papers.Add(PaperModelConstructor(false, false, employees.Take(firsPaperPaperRecorddsColl).ToArray(),headerStyle));
                    distributedEmployees += firsPaperPaperRecorddsColl < employees.Count
                        ? firsPaperPaperRecorddsColl
                        : employees.Count;
                }
                else
                {
                    papers.Add(PaperModelConstructor(false, false, employees.Skip(distributedEmployees).Take(paperRecordColl).ToArray(), headerStyle));
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
                temp = LastPaperCorrecter(papers.Last(), firsPaperPaperRecorddsColl-1,
                    headerStyle, takingEmployeys);
            }
            else
            {
                temp = LastPaperCorrecter(papers.Last(), lastPaperRecordsColl,
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

        public static PaperModel[] LastPaperCorrecter(PaperModel lastPaper, int lastPaperEmplCall, HeaderStyle[] headerStyle, int transferEmpl)
        {
            var papers = new List<PaperModel> {lastPaper};
            if (papers.Last().Employees.Count() >= lastPaperEmplCall)
            {
                var employeys = papers.Last().Employees;
                papers.Last().Employees = employeys.Take(employeys.Count()-transferEmpl).ToArray();
                var lastPageEnpl = employeys.Skip(employeys.Count() - transferEmpl).ToArray();
                papers.Add(PaperModelConstructor(false, false, lastPageEnpl, headerStyle));
            }
            return papers.ToArray();
        }

        public static PaperModel PaperModelConstructor(bool isFirst, bool isLast, EmployeeModel[] employees, HeaderStyle[] headerStyle)
        {
            return new PaperModel
            {
                HeaderStyle = headerStyle,
                Employees = employees,
                IsFirst = isFirst,
                IsLast = isLast
            };
        }

        public static EmployeeModel EmployeeModelConstructor(DtoTimeSheetEmployee employee, int employeeNumber, bool isForPrint)
        {
            var records = new List<EmployeeRecordModel>();
            for (int i = 1; i <= 32; i++)
            {
                var currentRecord = employee.Records.FirstOrDefault(f => f.Date.Day == i);
                records.Add(currentRecord != null
                                ? EmployeeRecordModelConstructor(i, isForPrint, currentRecord)
                                : EmployeeRecordModelConstructor(i, isForPrint));
            }
            var mounthDay = employee.Records.Count(c => c.DayStays.IdDayStatus != (int)DayStatus.Х);
            return new EmployeeModel
            {
                Surname = employee.FactStaffEmployee.Surname,
                Name = employee.FactStaffEmployee.Name,
                Patronymic = employee.FactStaffEmployee.Patronymic,
                Post = employee.FactStaffEmployee.Post.PostSmallName,
                StaffRate = employee.FactStaffEmployee.StaffRate,
                EmployeeNumber = employeeNumber,
                Records = records.ToArray(),
                FirstHalfMonthDays = employee.Records.Count(w => w.Date.Day < 16 & w.JobTimeCount > 0),
                FirstHalfMonthHours =
                    employee.Records.Where(w => w.Date.Day < 16 & w.JobTimeCount > 0).Sum(s => s.JobTimeCount),
                SecondHalfMonthDays = employee.Records.Count(w => w.Date.Day >= 16 & w.JobTimeCount > 0),
                SecondHalfMonthHours =
                    employee.Records.Where(w => w.Date.Day >= 16 & w.JobTimeCount > 0).Sum(s => s.JobTimeCount),
                Days = employee.Records.Count(w => w.JobTimeCount > 0),
                MounthDays = mounthDay,
                Hours = employee.Records.Where(w => w.JobTimeCount > 0).Sum(s => s.JobTimeCount),
                NonWorkedDays =NonWorked.Select(
                        s => new {DayStatus = s, Count = employee.Records.Count(w => w.DayStays.IdDayStatus == (int) s)})
                        .OrderByDescending(o => o.Count)
                        .ThenBy(o => o.DayStatus.Description())
                        .Select(s => EmployeeRecordModelConstructor(s.Count, s.DayStatus.Description(), isForPrint))
                        .ToArray()
            };
        }

        public static EmployeeRecordModel EmployeeRecordModelConstructor(int day, bool isForPrint, DtoTimeSheetRecord record = null)
        {
            if (record != null)
            {
                if(record.DayStays.SmallDayStatusName=="X")
                    return new EmployeeRecordModel
                    {
                        Day = day,
                        DayStatus = " ",
                        Value = " ",
                        CSS = GetCSS("X", isForPrint)
                    };
                return new EmployeeRecordModel
                    {
                        Day = day,
                        DayStatus = record.DayStays.SmallDayStatusName,
                        Value = record.JobTimeCount.ToString(CultureInfo.InvariantCulture),
                        CSS =  GetCSS(record.DayStays.SmallDayStatusName, isForPrint)
                    };
            }
            return new EmployeeRecordModel
            {
                Day = day,
                DayStatus = "X",
                Value = "X",
                CSS =  GetCSS("X", isForPrint)
            };
        }

        public static EmployeeRecordModel EmployeeRecordModelConstructor(int days, string dayStatus, bool isForPrint)
        {
            return new EmployeeRecordModel
            {
                Day = -1,
                DayStatus = dayStatus,
                Value = days.ToString(CultureInfo.InvariantCulture),
                CSS = days == 0? GetCSS("Empty",isForPrint) : GetCSS(dayStatus, isForPrint)
            };
        }

        public static ApproverModel ApproverModelConstructor (DtoTimeSheetApprover approver)
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

        private static string GetCSS(string dayStaus,bool isForPrint)
        {
            if (isForPrint)
            {
                switch (dayStaus)
                {
                    case "В":
                        return "PrintVCSS";
                    case "X":
                        return "PrintXCSS";
                    default:
                        return "EmptyCSS";
                }
            }
            switch (dayStaus)
            {
                case "Я":
                    return "ICSS";
                case "Н":
                    return "NCSS";
                case "РВ":
                    return "RvCSS";
                case "С":
                    return "SCSS";
                case "К":
                    return "KCSS";
                case "ПК":
                    return "PkCSS";
                case "О":
                    return "OCSS";
                case "У":
                    return "YCSS";
                case "УД":
                    return "YdCSS";
                case "Р":
                    return "PCSS";
                case "ОЖ":
                    return "OzhCSS";
                case "ДО":
                    return "DoCSS";
                case "ОЗ":
                    return "OzCSS";
                case "Б":
                    return "BCSS";
                case "Т":
                    return "TCSS";
                case "ЛЧ":
                    return "LchCSS";
                case "ПР":
                    return "PrCSS";
                case "В":
                    return "VCSS";
                case "НН":
                    return "NnCSS";
                case "НБ":
                    return "NbCSS";
                case "ОВ":
                    return "OvCSS";
                case "X":
                    return "XCSS";
                case "Empty":
                    return "EmptyCSS";
                default:
                    return "defCSS";
            }
        }
    }
}