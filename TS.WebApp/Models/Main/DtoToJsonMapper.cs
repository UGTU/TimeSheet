using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeSheetMvc4WebApplication.ClassesDTO;

namespace TimeSheetMvc4WebApplication.Models.Main
{
    public static class DtoToJsonMapper
    {
        public static JsTimeSheetViewModel TimeSheet(DtoTimeSheet timeSheet, DtoDayStatus[] dayStatusList)
        {
            var cult = System.Globalization.CultureInfo.GetCultureInfo("ru-Ru");
            var ts = new JsTimeSheetViewModel
            {
                DayStatusList = dayStatusList,
                TimeSheet = new JsTimeSheetModel
                {
                    Department = timeSheet.Department.DepartmentFullName,
                    IdTimeSheet = timeSheet.IdTimeSheet,
                    TimeSheetApproveStep = timeSheet.ApproveStep
                }
            };

            var employees = new List<JsEmployeeModel>();
            foreach (var empl in timeSheet.Employees)
            {
                var records = new List<JsTimeSheetRecordModel>();
                var recordsArr = empl.Records.OrderBy(o => o.Date).ToArray();
                for (int i = 0; i < recordsArr.Length; i++)
                {
                    var rec = recordsArr[i];
                    records.Add(new JsTimeSheetRecordModel
                    {
                        IdTimeSheetRecord = rec.IdTimeSheetRecord,
                        DayAweek = rec.Date.ToString("dddd", cult),
                        IdDayStatus = rec.DayStays.IdDayStatus,
                        JobTimeCount = rec.JobTimeCount,
                        NightTimeCount = (rec.NightTimeCount != "") ? Convert.ToDouble(rec.NightTimeCount) : 0,
                        Date = rec.Date.ToString("dd MMMM"),
                        Order = i
                    });
                }
                employees.Add(new JsEmployeeModel
                {
                    Surname = empl.FactStaffEmployee.Surname,
                    Name = empl.FactStaffEmployee.Name,
                    Patronymic = empl.FactStaffEmployee.Patronymic,
                    Post = empl.FactStaffEmployee.Post,
                    WorkShedule = empl.FactStaffEmployee.WorkShedule,
                    StaffRate = empl.FactStaffEmployee.StaffRate,
                    //FinSrc = empl.FactStaffEmployee.FinSrc,
                    Records = records.ToArray()
                });
            }
            ts.TimeSheet.Employees = employees.ToArray();
            return ts;
        }
    }
}