using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using TimeSheetMvc4WebApplication.ClassesDTO;
using TimeSheetMvc4WebApplication.Hubs;

namespace TimeSheetMvc4WebApplication.Source
{
    public class TimeEmailSender
    {
        private readonly string _host;
        private readonly string _appDominUrl;

        public TimeEmailSender(string smtpClientHost, string appDominUrl)
        {
            _host = smtpClientHost;
            _appDominUrl = appDominUrl;
        }

        public async Task<bool> SendMail(string from, string to, string subject, string body, bool isBodyHtml)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var mailMessage = new MailMessage(from, to, subject, body) { IsBodyHtml = isBodyHtml };
                    var client = new SmtpClient(_host);
                    client.Send(mailMessage);
                    return true;
                }
                catch (System.Exception)
                {
                    return false;
                }
            });
        }

        private string GenerateMailBody(DtoApprover approver, int idTimeSheet, bool approveResult, string comment,
            string departmentName, bool isApproveFinished = false)
        {
            var timeSheetAppLink = new TagBuilder("a");
            timeSheetAppLink.Attributes.Add("href", _appDominUrl);
            timeSheetAppLink.InnerHtml = "ИС \"Табель\"";

            var timeSheetShowLink = new TagBuilder("a");
            timeSheetShowLink.Attributes.Add("href", _appDominUrl + string.Format("TabelShow/{0}", idTimeSheet));
            timeSheetShowLink.InnerHtml = "просмотр табеля";

            var timeSheetPrintLink = new TagBuilder("a");
            timeSheetPrintLink.Attributes.Add("href", _appDominUrl + string.Format("TabelPdf/{0}", idTimeSheet));
            timeSheetPrintLink.InnerHtml = "печать табеля";

            var timeSheetApprovalLink = new TagBuilder("a");
            timeSheetApprovalLink.Attributes.Add("href", _appDominUrl + string.Format("Main/TimeSheetApprovalNew?idtimesheet={0}", idTimeSheet));
            timeSheetApprovalLink.InnerHtml = "согласование табеля";

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Здравствуйте, {0} {1}.", approver.Name, approver.Patronymic);
            stringBuilder.AppendLine("</br>");

            if (isApproveFinished)
            {
                stringBuilder.AppendFormat("Табель для структурного подразделения {0} успешно согласован.", departmentName);
            }
            else
            {
                if (approveResult)
                {
                    stringBuilder.AppendFormat(
                        "Вам на согласование был направлен табель рабочего времени структурного подразделения {0}.",
                        departmentName);
                    stringBuilder.AppendFormat(
                        "Для того, что бы приступить к согласованию тебеля перейдите по ссылке: {0}. ",
                        timeSheetApprovalLink);
                }
                else
                {
                    stringBuilder.AppendFormat("Согласование табеля для структурного подоразделения {0} было отклонено по причине: {1}", departmentName, comment);
                }
            }
            stringBuilder.AppendLine("</br>");
            stringBuilder.AppendFormat("Вы пожете просмотреть табель перейдя по ссылке {0}, ", timeSheetShowLink);
            stringBuilder.AppendFormat("вывести его на печать: {0}. ", timeSheetPrintLink);
            stringBuilder.AppendFormat("или просмотреть историю согласования: {0}. ", timeSheetApprovalLink);
            stringBuilder.AppendFormat("Так же вы можете посетить {0}. ", timeSheetAppLink);
            return stringBuilder.ToString();
        }

        public async Task<bool> TimeSheetApproveEmailSending(string userName, IEnumerable<DtoApprover> approversToSend, int idTimeSheet, bool result, string comments, int approvalStep, string departmentName, bool isApproveFinished)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    var noticeHub = NoticeHub.Instance;
                    var sendingResulLIst = new List<bool>();
                    foreach (var approver in approversToSend)
                    {
                        var mailBody = GenerateMailBody(approver, idTimeSheet, result, comments, departmentName, isApproveFinished);
                        sendingResulLIst.Add(await SendMail("tabel-no-reply@ugtu.net", approver.EmployeeLogin,
                            "ИС Табель рабочего времени для " + departmentName, mailBody, true));
                    }
                    if (sendingResulLIst.Any(a => a == false))
                    {
                        //Отправка сообщения пользователю о том, что во впемя отправи писенм произошла ошибка
                        await
                            noticeHub.Notice(MessageType.Danger, "Согласование",
                                "Во время отправки электронной почты возникли прпоблемы, возможно почта не была отправлена. Убедитесь что адресат получил уведомление",
                                userName);
                    }
                    else
                    {
                        //Отправка оповещения всем пользователям
                        foreach (var approver in approversToSend)
                        {
                            var mailBody = GenerateMailBody(approver, idTimeSheet, result, comments, departmentName, isApproveFinished);
                            await noticeHub.Notice(MessageType.Info, "Согласование", mailBody, approver.EmployeeLogin);
                        }
                    }
                    return true;
                }
                catch (System.Exception)
                {
                    return false;
                }
            });
        }






        //public async Task<bool> TimeSheetApproveEmailSending(string userName, IEnumerable<DtoApprover> approversToSend, int idTimeSheet, bool result, string comments, int approvalStep, string departmentName, bool isApproveFinished)
        //{
        //    //await Task.Run(() =>
        //    //{
        //    try
        //    {



        //        var sandedMail = new List<Task<bool>>();
        //        foreach (var approver in approversToSend)
        //        {
        //            var mailBody = GenerateMailBody(approver, idTimeSheet, result, comments, departmentName, isApproveFinished);
        //            sandedMail.Add(SendMail("tabel-no-reply@ugtu.net", approver.EmployeeLogin,
        //                "ИС Табель рабочего времени для " + departmentName, mailBody, true));
        //        }
        //        await Task.WhenAll(sandedMail).ContinueWith(async task =>
        //        {
        //            var noticeHub = NoticeHub.Instance;
        //            var sandedMailResults = await task;
        //            if (sandedMailResults.Any(a => a == false))
        //            {
        //                //Отправка сообщения пользователю о том, что во впемя отправи писенм произошла ошибка
        //                await noticeHub.Notice(MessageType.Danger, "Согласование", "Во время отправки электронной почты возникли прпоблемы, возможно почта не была отправлена. Убедитесь что адресат получил уведомление", userName);
        //            }
        //            else
        //            {
        //                //Отправка оповещения всем пользователям
        //                foreach (var approver in approversToSend)
        //                {
        //                    var mailBody = GenerateMailBody(approver, idTimeSheet, result, comments, departmentName, isApproveFinished);
        //                    await noticeHub.Notice(MessageType.Info, "Согласование", mailBody, approver.EmployeeLogin);
        //                }
        //            }
        //        });
        //        return true;
        //    }
        //    catch (System.Exception e)
        //    {
        //        return false;
        //    }
        //    //});
        //}
    }
}