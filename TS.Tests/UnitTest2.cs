using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Mail;

namespace TimeSheetMvc4WebApplication.Tests
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void emailSent()
        {
            MailAddress from = new MailAddress("tabel-no-reply@ugtu.net");
            MailAddress to = new MailAddress("ngrigoriev@ugtu.net");
            MailMessage m = new MailMessage(from, to);
            m.Subject = "Тест";
            m.Body = "<h2>Здорова колян</h2>";
            m.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("mail.ugtu.net");
            smtp.Send(m);

            //var appDominUrl = ".";//Url.Action("Index", null, null, Request.Url.Scheme);
            //var TES = new TimeEmailSender("mail.ugtu.net",
            //            appDominUrl);
            //TES.SendMail("iulyashev@ugtu.net", "ngrigoriev@ugtu.net", "test", "body", true);
        }
    }
}
