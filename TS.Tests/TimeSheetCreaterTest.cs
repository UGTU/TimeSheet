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
        const int IdDepartmentSixDays = 59;
        readonly DateTime _dateStart = new DateTime(2024, 1, 1);

        private static readonly DateTime CurrentBegin = DateTime.Today.AddDays(-DateTime.Today.Day + 1);
        private static readonly DateTime CurrentEnd = CurrentBegin.AddMonths(1).AddDays(-1);
        //Работа
        readonly KadrDataContext _db = new KadrDataContext("Data Source=ugtudb.ugtu.net;Initial Catalog=KadrTest;Persist Security Info=True;User ID=testkadr;Password=^%$#@!Qq");
        //Дом
        //readonly KadrDataContext _db = new KadrDataContext("Data Source=ALEXEY-PC\\SQLEXPRESS; Initial Catalog=Kadr; Integrated Security=True;");

        [TestMethod]
        public void TimeSheetCreate()
        {
            var timeSheetCreater = new TimeSheetManaget(IdDepartment, _dateStart, GetDateEnd(), "ochernova@ugtu.net", _db);
            timeSheetCreater.GenerateTimeSheet();
            Assert.IsTrue(timeSheetCreater.DateBegin != null);
            timeSheetCreater.RemoveTimeSheet();
            Assert.IsNull(timeSheetCreater.DateBegin);
        }
        DateTime GetDateEnd()
        {
            return _dateStart.AddMonths(1).AddDays(-1);
        }

        [TestMethod]
        public void TestCurrentDates()
        {
            Console.WriteLine(CurrentBegin.ToString());
            Console.WriteLine(CurrentEnd.ToString());
            Console.Read();
        }

        [TestMethod]
        public void TestSixDayesTimeSheetGenerate()
        {
            var timeSheetCreater = new TimeSheetManaget(IdDepartmentSixDays, CurrentBegin, CurrentEnd,
                "ochernova@ugtu.net", _db);
           timeSheetCreater.GenerateTimeSheet();
           Assert.IsTrue(timeSheetCreater.DateBegin != null);
           timeSheetCreater.RemoveTimeSheet();
        }

        DtoApproverDepartment GetDtoApprover(string employeeLogin)
        {
            var idEmployee = _db.Employees.Where(w => w.EmployeeLogin.ToLower() == employeeLogin.ToLower()).Select(s => s.id).FirstOrDefault();
            return DtoClassConstructor.DtoApprover(_db, idEmployee).DtoApproverDepartments.FirstOrDefault(w => w.IdDepartment == IdDepartment);
        }


    }
}
