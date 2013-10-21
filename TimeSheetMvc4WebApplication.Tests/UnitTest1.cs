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
    public class TimeSheetCreateTest
    {
        //[TestMethod]
        //public void TimeSheetCreate()
        //{
        //    const int idDep = 39;
        //    var dateStart = new DateTime(2024, 2, 1);
        //    var dateEnd = dateStart.AddMonths(1).AddDays(-1);
        //    var service = new TimeSheetService();
        //    var employeeLogin = "atipunin@ugtu.net";
        //    var fs = new FileStream("e:\\logout.txt", FileMode.Create);
        //    var sr = new StreamWriter(fs);
        //    var db = new KadrDataContext("Data Source=ugtudb.ugtu.net;Initial Catalog=Kadr;Persist Security Info=True;User ID=atipunin;Password=090990");
        //    var loadOptions = new DataLoadOptions();
        //    loadOptions.LoadWith((FactStaffWithHistory fswh) => fswh.PlanStaff);
        //    loadOptions.LoadWith((PlanStaff ps) => ps.Post);
        //    loadOptions.LoadWith((Post p) => p.Category);
        //    loadOptions.LoadWith((PlanStaff ps) => ps.WorkShedule);
        //    loadOptions.LoadWith((FactStaffWithHistory fswh) => fswh.Employee);
        //    loadOptions.LoadWith((OK_Otpusk oko) => oko.OK_Otpuskvid);
        //    db.LoadOptions = loadOptions;
        //    db.Log = sr;
        //    if (employeeLogin.ToLower().StartsWith(@"ugtu\".ToLower()))
        //    {
        //        employeeLogin =
        //            string.Format("{0}@{1}.NET", employeeLogin.Substring(5, employeeLogin.Length - 5),
        //                employeeLogin.Substring(0, 4)).ToLower();
        //    }
        //    var idEmployee = db.Employee.Where(w => w.EmployeeLogin.ToLower() == employeeLogin.ToLower()).Select(s => s.id).FirstOrDefault();
        //    var r = DtoClassConstructor.DtoApprover(db, idEmployee).DtoApproverDepartments.FirstOrDefault(w => w.IdDepartment == idDep);
        //    var timeSheet = new TimeSheetCrater(idDep, dateStart, dateEnd, r, db);  
        //    //var gt = timeSheet.GenerateTimeSheet();
        //    sr.Flush();
        //    sr.Close();
        //    sr.Dispose();
        //    db.Dispose();
        //    Assert.IsNotNull(timeSheet.IdTimeSheet);
        //    //Assert.IsTrue(gt.Result);
        //}

        //[TestMethod]
        //public void TimeSheetDelete()
        //{
        //    const int idDep = 39;
        //    var dateStart = new DateTime(2024, 1, 1);
        //    var db = new KadrDataContext("Data Source=ugtudb.ugtu.net;Initial Catalog=Kadr;Persist Security Info=True;User ID=atipunin;Password=090990");
        //    var idTimeSheet = db.TimeSheet.Where(w => w.DateBeginPeriod == dateStart).Select(s => s.id).First();
        //    db.TimeSheetRecord.DeleteAllOnSubmit(db.TimeSheetRecord.Where(w => w.idTimeSheet == idTimeSheet));
        //    db.TimeSheet.DeleteAllOnSubmit(db.TimeSheet.Where(w => w.id == idTimeSheet));
        //    db.SubmitChanges();
        //}

        //[TestMethod]
        //public void TimeSheetCreateAndDelete()
        //{
        //    const int idDep = 39;
        //    var dateStart = new DateTime(2024, 1, 1);
        //    var dateEnd = dateStart.AddMonths(1).AddDays(-1);
        //    var service = new TimeSheetService();
        //    var employeeLogin = "atipunin@ugtu.net";
        //    var fs = new FileStream("e:\\logout.txt", FileMode.Create);
        //    var sr = new StreamWriter(fs);
        //    var db = new KadrDataContext("Data Source=ugtudb.ugtu.net;Initial Catalog=Kadr;Persist Security Info=True;User ID=atipunin;Password=090990");
        //    var loadOptions = new DataLoadOptions();
        //    loadOptions.LoadWith((FactStaffWithHistory fswh) => fswh.PlanStaff);
        //    loadOptions.LoadWith((PlanStaff ps) => ps.Post);
        //    loadOptions.LoadWith((Post p) => p.Category);
        //    loadOptions.LoadWith((PlanStaff ps) => ps.WorkShedule);
        //    loadOptions.LoadWith((FactStaffWithHistory fswh) => fswh.Employee);
        //    loadOptions.LoadWith((OK_Otpusk oko) => oko.OK_Otpuskvid);
        //    db.LoadOptions = loadOptions;
        //    db.Log = sr;
        //    if (employeeLogin.ToLower().StartsWith(@"ugtu\".ToLower()))
        //    {
        //        employeeLogin =
        //            string.Format("{0}@{1}.NET", employeeLogin.Substring(5, employeeLogin.Length - 5),
        //                employeeLogin.Substring(0, 4)).ToLower();
        //    }
        //    var idEmployee = db.Employee.Where(w => w.EmployeeLogin.ToLower() == employeeLogin.ToLower()).Select(s => s.id).FirstOrDefault();
        //    var r = DtoClassConstructor.DtoApprover(db, idEmployee).DtoApproverDepartments.FirstOrDefault(w => w.IdDepartment == idDep);
        //    var timeSheet = new TimeSheetCrater(idDep, dateStart, dateEnd, r, db);
        //    Assert.IsNotNull(timeSheet.IdTimeSheet);
        //    //var gt = timeSheet.GenerateTimeSheet();
        //    //Assert.IsTrue(gt.Result);
        //    db.TimeSheetRecord.DeleteAllOnSubmit(db.TimeSheetRecord.Where(w=>w.idTimeSheet==timeSheet.IdTimeSheet));
        //    db.TimeSheet.DeleteAllOnSubmit(db.TimeSheet.Where(w => w.id == timeSheet.IdTimeSheet));
        //    db.SubmitChanges();
        //    sr.Flush();
        //    sr.Close();
        //    sr.Dispose();
        //    db.Dispose();
        //}

        [TestMethod]
        public void NewTimeSheetCreateAndDelete()
        {
            const int idDep = 39;
            var dateStart = new DateTime(2024, 1, 1);
            var dateEnd = dateStart.AddMonths(1).AddDays(-1);
            var service = new TimeSheetService();
            var employeeLogin = "atipunin@ugtu.net";
            var fs = new FileStream("e:\\logout.txt", FileMode.Create);
            var sr = new StreamWriter(fs);
            var db = new KadrDataContext("Data Source=ugtudb.ugtu.net;Initial Catalog=Kadr;Persist Security Info=True;User ID=atipunin;Password=090990");
            var loadOptions = new DataLoadOptions();
            loadOptions.LoadWith((FactStaffWithHistory fswh) => fswh.PlanStaff);
            loadOptions.LoadWith((PlanStaff ps) => ps.Post);
            loadOptions.LoadWith((Post p) => p.Category);
            loadOptions.LoadWith((PlanStaff ps) => ps.WorkShedule);
            loadOptions.LoadWith((FactStaffWithHistory fswh) => fswh.Employee);
            loadOptions.LoadWith((OK_Otpusk oko) => oko.OK_Otpuskvid);
            db.LoadOptions = loadOptions;
            db.Log = sr;
            if (employeeLogin.ToLower().StartsWith(@"ugtu\".ToLower()))
            {
                employeeLogin =
                    string.Format("{0}@{1}.NET", employeeLogin.Substring(5, employeeLogin.Length - 5),
                        employeeLogin.Substring(0, 4)).ToLower();
            }
            var idEmployee = db.Employee.Where(w => w.EmployeeLogin.ToLower() == employeeLogin.ToLower()).Select(s => s.id).FirstOrDefault();
            var r = DtoClassConstructor.DtoApprover(db, idEmployee).DtoApproverDepartments.FirstOrDefault(w => w.IdDepartment == idDep);
            var timeSheet = new TimeSheetCraterNew(idDep, dateStart, dateEnd, r, db);
            timeSheet.GenerateTimeSheet();
            Assert.IsTrue(timeSheet.SubmitTimeSheet());
            Assert.IsNotNull(timeSheet.IdTimeSheet);
            db.TimeSheetRecord.DeleteAllOnSubmit(db.TimeSheetRecord.Where(w => w.idTimeSheet == timeSheet.IdTimeSheet));
            db.TimeSheet.DeleteAllOnSubmit(db.TimeSheet.Where(w => w.id == timeSheet.IdTimeSheet));
            db.SubmitChanges();
            sr.Flush();
            sr.Close();
            sr.Dispose();
            db.Dispose();
        }


    }
}
