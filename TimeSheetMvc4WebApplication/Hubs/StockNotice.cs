using System;
using System.Threading;
using System.Threading.Tasks;
using CommonBase;
using Microsoft.AspNet.SignalR;

namespace TimeSheetMvc4WebApplication.Hubs
{
    public class StockNotice
    {
        private static readonly IHubContext Context = GlobalHost.ConnectionManager.GetHubContext<NoticeHub>();
        public static Task Test(string userName = null)
        {
            Action noticeSending = () =>
            {
                var arr = new[] { MessageType.Info, MessageType.Warning, MessageType.Danger };
                var i = 0;
                foreach (var item in arr)
                {
                    var dyn = new
                    {
                        type = item.Description(),
                        title = i,
                        message = i
                    };
                    Context.Clients.All.addNoticeToPage(dyn);
                    Thread.Sleep(3000);
                    i++;
                }
            };
            return new Task(noticeSending);
        }
    }
}
