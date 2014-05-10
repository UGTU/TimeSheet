using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeSheetMvc4WebApplication.AppDomine;
using TimeSheetMvc4WebApplication.Models.Main;

namespace TimeSheetMvc4WebApplication.Abstract
{
    interface IDataProvider
    {
        IEnumerable<BaseTimeSheet> GetTimeSheetList(int idDepartment, TimeSheetFilter filter, int skip, int take, bool isEmpty);
        int GetTimeSheetListCount(int idDepartment, TimeSheetFilter filter);
        BaseTimeSheet GetTimeSheet(int idTimeSheet, bool isEmpty);
    }
}
