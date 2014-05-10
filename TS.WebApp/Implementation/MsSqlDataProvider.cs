using System.Collections.Generic;
using System.Linq;
using TimeSheetMvc4WebApplication.Abstract;
using TimeSheetMvc4WebApplication.AppDomine;
using TimeSheetMvc4WebApplication.Models.Main;

namespace TimeSheetMvc4WebApplication.Implementation
{
    public class MsSqlDataProvider:IDataProvider
    {
        readonly ObjectMapper _mapper = new ObjectMapper();
        public IEnumerable<BaseTimeSheet> GetTimeSheetList(int idDepartment, TimeSheetFilter filter, int skip, int take, bool isEmpty)
        {
            using (var db = new KadrDataContext())
            {
                var query = MakeQueryForTimeSheetList(db, idDepartment, filter);
                return query.Skip(skip).Take(take).Select(s => isEmpty
                    ? _mapper.BaseTimeSheet(s)
                    : _mapper.TimeSheet(s));
            }
        }

        public int GetTimeSheetListCount(int idDepartment, TimeSheetFilter filter)
        {
            using (var db = new KadrDataContext())
            {
                var query = MakeQueryForTimeSheetList(db, idDepartment, filter);
                return query.Count();
            }
        }

        public BaseTimeSheet GetTimeSheet(int idTimeSheet, bool isEmpty)
        {
            using (var db = new KadrDataContext())
            {
                var ts = db.TimeSheetView.SingleOrDefault(s => s.id == idTimeSheet);
                return isEmpty
                    ? _mapper.BaseTimeSheet(ts)
                    : _mapper.TimeSheet(ts);
            }
        }

        //============================================================================================
        private IQueryable<TimeSheetView> MakeQueryForTimeSheetList(KadrDataContext db, int idDepartment, TimeSheetFilter filter)
        {
            var approveSteps = TimeSheetFilterAdapter(filter);
            return db.TimeSheetView.Where(
                w => w.idDepartment == idDepartment && approveSteps.Contains(w.ApproveStep)
                );
        }

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