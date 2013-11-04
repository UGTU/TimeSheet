using System;
using System.Data.Linq;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeSheetMvc4WebApplication.ClassesDTO;
using TimeSheetMvc4WebApplication.Source;


namespace TimeSheetMvc4WebApplication.Tests
{
    [TestClass]
    public class TimeSheetCreaterTest
    {
        const int IdDepartment = 39;
        readonly DateTime _dateStart = new DateTime(2024, 1, 1);
        //Работа
        //readonly KadrDataContext _db = new KadrDataContext("Data Source=ugtudb.ugtu.net;Initial Catalog=Kadr;Persist Security Info=True;User ID=atipunin;Password=090990");
        //Дом
        readonly KadrDataContext _db = new KadrDataContext("Data Source=ALEXEY-PC\\SQLEXPRESS; Initial Catalog=Kadr; Integrated Security=True;");

        [TestMethod]
        public void TimeSheetCreate()
        {
            var timeSheetCreater = new TimeSheetCrater(IdDepartment, _dateStart, GetDateEnd(), GetDtoApprover("atipunin@ugtu.net"), _db);
            timeSheetCreater.GenerateTimeSheet();
            timeSheetCreater.SubmitTimeSheet();
        }
        DateTime GetDateEnd()
        {
            return _dateStart.AddMonths(1).AddDays(-1);
        }

        DtoApproverDepartment GetDtoApprover(string employeeLogin)
        {
            var idEmployee = _db.Employee.Where(w => w.EmployeeLogin.ToLower() == employeeLogin.ToLower()).Select(s => s.id).FirstOrDefault();
            return DtoClassConstructor.DtoApprover(_db, idEmployee).DtoApproverDepartments.FirstOrDefault(w => w.IdDepartment == IdDepartment);
        }
    }
}
