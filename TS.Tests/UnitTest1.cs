using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeSheetMvc4WebApplication.ClassesDTO;
using TimeSheetMvc4WebApplication.Source;

namespace TimeSheetMvc4WebApplication.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsNull(SessionHelper.Approver);
            SessionHelper.Approver = new DtoApprover();
            Assert.IsNotNull(SessionHelper.Approver);
        }
    }
}
