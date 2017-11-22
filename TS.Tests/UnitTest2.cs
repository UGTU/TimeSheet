using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Mail;
using System.Threading.Tasks;

namespace TimeSheetMvc4WebApplication.Tests
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void emailSent()
        {
            try
            {
                var count = 15;
                MailAddress from = new MailAddress("iulyashev@ugtu.net");
                MailAddress to = new MailAddress("ngrigoriev@ugtu.net");
                MailMessage m = new MailMessage(from, to);
                m.Subject = "Тест" + count;
                m.Body = $"<h2>Здорова колян{count}! Я сделаль!!!</h2>";
                m.IsBodyHtml = true;
                using (var smtp = new SmtpClient("mail.ugtu.net"))
                {
                    smtp.Send(m);
                }
                //smtp.SendMailAsync(m).ContinueWith(x => {
                //    Task.Run(() => { Console.WriteLine("zzz"); });
                //}).Wait();
                


                /*
                smtp.SendCompleted += (o,e)=>Console.WriteLine("mail sent");
                smtp.SendAsync(m,null);
                */
            }
            catch
            {
                Assert.Fail();
            }
            //var appDominUrl = ".";//Url.Action("Index", null, null, Request.Url.Scheme);
            //var TES = new TimeEmailSender("mail.ugtu.net",
            //            appDominUrl);
            //TES.SendMail("iulyashev@ugtu.net", "ngrigoriev@ugtu.net", "test", "body", true);
        }
    }
}
