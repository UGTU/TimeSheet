using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Providers.Entities;
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
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IHubContext _context;
        private NoticeHub(IHubContext context)
        {
            _context = context;
        }

        public NoticeHub(){}

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
                username = UserNameAdapter.Adapt(username);
                Logger.Info("send notice to " + username);
                _context.Clients.Group(username).addNoticeToPage(dyn);
                //Clients.Group(username).addChatMessage(dyn);
            }
            else
            {
                //Multicast notice
                Logger.Info("multicast notice send");
                _context.Clients.All.addNoticeToPage(dyn);
                //Clients.All.addNoticeToPage(dyn);
            }
        }

        public void Notice(string title, string message, string username = null)
        {
            Notice(MessageType.Info, title, message, username);
        }

        public void Notice(string message, string username = null)
        {
            Notice(string.Empty, message, username);
        }

        public Task Test(string userName = null)
        {
            var message = "temp mesage" + userName;
            Action noticeSending = () =>
            {
                var arr = new[] {MessageType.Info, MessageType.Warning, MessageType.Danger};
                var i = 0;

                foreach (var item in arr)
                {
                    message = message + i;
                    Thread.Sleep(3000);
                    Notice(item, "Title", message, userName);
                    i++;
                }
            };
            return new Task(noticeSending);
        }

        public override Task OnConnected()
        {
            try
            {
                var name = UserNameAdapter.Adapt(Context.User.Identity.Name);
                Groups.Add(Context.ConnectionId, name);
                Logger.Info("connect as " + name);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            try
            {
                var name = UserNameAdapter.Adapt(Context.User.Identity.Name);
                Groups.Remove(Context.ConnectionId, name);
                Groups.Add(Context.ConnectionId, name);
                Logger.Info("reconnect as " + name);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return base.OnReconnected();
        }

        public override Task OnDisconnected()
        {
            try
            {
                var name = UserNameAdapter.Adapt(Context.User.Identity.Name);
                Groups.Remove(Context.ConnectionId, name);
                Logger.Info("disconnect as " + name);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return base.OnDisconnected();
        }
    }
}