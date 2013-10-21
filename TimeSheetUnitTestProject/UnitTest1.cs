using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeSheetUnitTestProject.TimeSheetServiceReference;

namespace TimeSheetUnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        public static TimeSheetServiceReference.TimeSheetServiceClient Client = new TimeSheetServiceClient();

        //[TestMethod]
        //public void TestMethod1()
        //{
        //}

        //[TestMethod]
        //public void GetApproverForTimeSheet()
        //{
        //    var approver = Client.GetApproverForTimeSheet(47, 1);
        //    var approver1 = Client.GetApproverForTimeSheet(47, 2);
        //    var approver2 = Client.GetApproverForTimeSheet(47, 3);
        //}

        //[TestMethod]
        //public void SendMail()
        //{
        //    var idTimeSheet = 47;
        //    //var comments = "Не добавлен человек";
        //    //Client.TimeSheetApproval(idTimeSheet, 6, true, "Не добавлен человек");
        //    //Client.TimeSheetApproval(idTimeSheet, 7, false, "Не добавлен человек");
        //    //Client.TimeSheetApproval(idTimeSheet, 6, true, "Не добавлен человек");
        //    //Client.TimeSheetApproval(idTimeSheet, 7, true, "Не добавлен человек");
        //    //Client.TimeSheetApproval(idTimeSheet, 8, false, "Не добавлен человек");
        //    //Client.TimeSheetApproval(idTimeSheet, 6, true, "Не добавлен человек");
        //    //Client.TimeSheetApproval(idTimeSheet, 7, true, "Не добавлен человек");
        //    //Client.TimeSheetApproval(idTimeSheet, 8, true, "Не добавлен человек");
        //}

        //[TestMethod]
        //public void GetWindowsUsername()
        //{
        //    var st = Client.GetWindowsUsername();
        //    var e = st;
        //}

        [TestMethod]
        public void CreateTimeSheet1()
        {
            Client.CreateTimeSheet1(39, new DateTime(2012, 10, 1), new DateTime(2012, 10, 31));
        }
    }
}
