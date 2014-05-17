using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeSheetMvc4WebApplication.AppDomine;
using UOW;

namespace TimeSheetMvc4WebApplication.UOWRepositories
{
    public static class TsRepositoryExtensions
    {
        public static BaseTimeSheet GetBaseTimeSheet(this IRepository<TimeSheet> source, int id)
        {
            var ts = source.AsQueryable().SingleOrDefault(x => x.id == id);
            return new BaseTimeSheet() {IdTimeSheet = ts.id};
        }
    }
}