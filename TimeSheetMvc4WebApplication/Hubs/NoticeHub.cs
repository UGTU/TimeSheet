using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommonBase;
using Microsoft.AspNet.SignalR;
using TimeSheetMvc4WebApplication.Source;

namespace TimeSheetMvc4WebApplication.Hubs
{
    public enum MessageType
    {
        [Description("info")] Info,
        [Description("warning")] Warning,
        [Description("danger")] Danger
    }

    [Authorize]
    public class NoticeHub : Hub
    {
        public static readonly Lazy<NoticeHub> Instance = new Lazy<NoticeHub>(() => new NoticeHub(GlobalHost.ConnectionManager.GetHubContext<NoticeHub>()));

        private IHubContext _context;
        private NoticeHub(IHubContext context)
        {
            _context = context;
        }
        public void Notice(MessageType messageType, string title, string message, string username = null)
        {
            var dyn = new
            {
                type = messageType.Description(),
                title,
                message
            };
            if (!string.IsNullOrWhiteSpace(username))
            {
                //Singlecast notice
                _context.Clients.Group(UserNameAdapter.Adapt(username)).addChatMessage(dyn);
            }
            //Multicast notice
            _context.Clients.All.addNoticeToPage(dyn);
        }

        public void Notice(string title, string message, string username = null)
        {
            Notice(MessageType.Info, title, message, username);
        }

        public void Notice(string message, string username = null)
        {
            Notice(string.Empty, message, username);
        }

        //public void Test(string userName = null)
        //{
        //    var arr = new[] { MessageType.Info, MessageType.Warning, MessageType.Danger };
        //    const string message = "temp mesage";
        //    foreach (var item in arr)
        //    {
        //        Notice(item, "Title", message, userName);
        //        Thread.Sleep(3000);
        //        Notice(message, userName);
        //        Thread.Sleep(3000);
        //    }
        //}

        //public async void Test(string userName = null)
        //{
        //    Action notifySending = () =>
        //    {
        //        var arr = new[] {MessageType.Info, MessageType.Warning, MessageType.Danger};
        //        const string message = "temp mesage";
        //        foreach (var item in arr)
        //        {
        //            Notice(item, "Title", message, userName);
        //            Thread.Sleep(3000);
        //            Notice(message, userName);
        //            Thread.Sleep(3000);
        //        }
        //    };
        //    await Task.Run(notifySending);
        //}

        public Task Test(string userName = null)
        {
            Action noticeSending = () =>
            {
                var arr = new[] { MessageType.Info, MessageType.Warning, MessageType.Danger };
                var i = 0;
                string message = "temp mesage";
                foreach (var item in arr)
                {
                    message = message + i.ToString();
                    Thread.Sleep(3000);
                    Notice(item, "Title", message, userName);
                    i++;
                }
            };
            return new Task(noticeSending);
        }

        public override Task OnConnected()
        {
            var name = UserNameAdapter.Adapt(Context.User.Identity.Name);
            Groups.Add(Context.ConnectionId, name);
            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            var name = UserNameAdapter.Adapt(Context.User.Identity.Name);
            Groups.Remove(Context.ConnectionId, name);
            return base.OnDisconnected();
        }
    }
}