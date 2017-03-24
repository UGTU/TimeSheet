using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Web;
using TimeSheetMvc4WebApplication.ClassesDTO;
using TimeSheetMvc4WebApplication.Models.Main;

namespace TimeSheetMvc4WebApplication.Source
{
    public class DataProvider
    {
        public TimeSheetsAndCount GetTimeSheetList(int idDepartment, TimeSheetFilter filter, int skip, int take)
        {
            using (var db = new KadrDataContext())
            //using (var dbloger = new DataContextLoger("GetTimeSheetList.txt", FileMode.OpenOrCreate, db))
            {
                var approveSteps = TimeSheetFilterAdapter(filter);
                var query = db.TimeSheetView.Where(
                    w => w.idDepartment == idDepartment && approveSteps.Contains(w.ApproveStep)
                    );
                return new TimeSheetsAndCount
                {
                    Count = query.Count(),
                    TimeSheets = query.OrderByDescending(o => o.DateBeginPeriod)
                        .ThenByDescending(t=>t.DateComposition)
                        .Skip(skip)
                        .Take(take)
                        .Select(s => DtoClassConstructor.DtoTimeSheet(s)).ToArray()
                };
            }
        }

        public DtoTimeSheet GetTimeSheet(int idTimeSheet, bool isEmpty = false)
        {
            using (var db = new KadrDataContext())
            using (var dbloger = new DataContextLoger("GetTimeSheetLog.sql", FileMode.OpenOrCreate, db))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith((TimeSheet ts) => ts.TimeSheetRecords);
                loadOptions.LoadWith((TimeSheet ts) => ts.Dep);
                loadOptions.LoadWith((Dep d) => d.Department);
                loadOptions.LoadWith((TimeSheetRecord tsr) => tsr.FactStaffHistory);
                loadOptions.LoadWith((FactStaffWithHistory f) => f.FactStaff);
                loadOptions.LoadWith((FactStaff f) => f.Employee);
                loadOptions.LoadWith((FactStaffWithHistory f) => f.FactStaff.PlanStaff);
                loadOptions.LoadWith((PlanStaff p) => p.Post);
                loadOptions.LoadWith((Post p) => p.Category);
                db.LoadOptions = loadOptions;
                //return DtoClassConstructor.DtoTimeSheet(db, idTimeSheet, isEmpty);
                return db.TimeSheetView.Where(f =>f.id==idTimeSheet).Select(s=> DtoClassConstructor.DtoTimeSheet(s, isEmpty)).FirstOrDefault();
            }
        }

        //===================================================================================
        private IEnumerable<int> TimeSheetFilterAdapter(TimeSheetFilter filter)
        {
            switch (filter)
            {
                case TimeSheetFilter.All:
                    return new[] { 0, 1, 2, 3 };
                case TimeSheetFilter.Edit:
                    return new[] { 0 };
                case TimeSheetFilter.Approve:
                    return new[] { 1, 2 };
                case TimeSheetFilter.Approved:
                    return new[] { 3 };
                default:
                    throw new System.Exception("Заданое условие фильтрации недоступно");
            }
        }
    }
}