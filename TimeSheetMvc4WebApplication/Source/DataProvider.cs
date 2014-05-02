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
                    TimeSheets = query.OrderByDescending(o => o.DateBeginPeriod).Skip(skip).Take(take)
                        .Select(s => DtoClassConstructor.DtoTimeSheet(s)).ToArray()
                };
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