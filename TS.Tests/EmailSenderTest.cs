using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeSheetMvc4WebApplication.Source;

namespace TimeSheetMvc4WebApplication.Tests
{
    [TestClass]
    public class EmailSenderTest
    {
        [TestMethod]
        public async void emailSent()
        {
            var appDominUrl = ".";//Url.Action("Index", null, null, Request.Url.Scheme);
            var TES = new TimeEmailSender("mail.ugtu.net",
                        appDominUrl);
            Assert.IsTrue(await TES.SendMail("tabel-no-reply@ugtu.net", "iulyashev@ugtu.net", "test", "body", true));
        }
    }
}
