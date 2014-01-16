using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        public static readonly NoticeHub Instance = new NoticeHub();
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private new static readonly IHubContext Context;
        private static volatile Dictionary<string,string> _userNameAndConnectionId = new Dictionary<string, string>();
        static NoticeHub()
        {            
            Context = GlobalHost.ConnectionManager.GetHubContext<NoticeHub>();
        }
        public async Task Notice(MessageType messageType, string title, string message, string username = null)
        {
            await Task.Run(() =>
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
                    Context.Clients.Group(username).addNoticeToPage(dyn);
                }
                else
                {
                    //Multicast notice
                    Logger.Info("multicast notice send");
                    Context.Clients.All.addNoticeToPage(dyn);
                }
            });
        }
        public async Task Notice(string title, string message, string username = null)
        {
            await Notice(MessageType.Info, title, message, username);
        }
        public async Task Notice(string message, string username = null)
        {
            await Notice(string.Empty, message, username);
        }
        public Task Test(string userName = null)
        {
            return Task.Run(() =>
            {
                var message = "temp mesage" + userName;
                var arr = new[] {MessageType.Info, MessageType.Warning, MessageType.Danger};
                var i = 0;
                foreach (var item in arr)
                {
                    message = message + i;
                    Thread.Sleep(3000);
                    Notice(item, "Title", message, userName);
                    i++;
                }
            });
        }
        public override Task OnConnected()
        {
            try
            {
                var name = UserNameAdapter.Adapt(base.Context.User.Identity.Name);
                var connectionId = base.Context.ConnectionId;
                _userNameAndConnectionId.Add(connectionId,name);
                Groups.Add(base.Context.ConnectionId, name);
                Logger.Info("connect as " + name);
            }
            catch (System.Exception ex)
            {
                Logger.Error(ex);
            }
            return base.OnConnected();
        }
        public override Task OnReconnected()
        {
            try
            {
                var connectionId = base.Context.ConnectionId;
                var name = _userNameAndConnectionId.FirstOrDefault(f => f.Key == connectionId).Value;
                if (string.IsNullOrWhiteSpace(name))
                {
                    Groups.Remove(base.Context.ConnectionId, name);
                    Logger.Info("disconnect as " + name);
                    name = UserNameAdapter.Adapt(base.Context.User.Identity.Name);
                    _userNameAndConnectionId.Add(connectionId, name);
                    Groups.Add(base.Context.ConnectionId, name);
                }
                Logger.Info("reconnect as " + name);
            }
            catch (System.Exception ex)
            {
                Logger.Error(ex);
            }
            return base.OnReconnected();
        }

        public override Task OnDisconnected()
        {
            //try
            //{
            //    var name = _userNameAndConnectionId.FirstOrDefault(f => f.Key == base.Context.ConnectionId).Value;
            //    if (!string.IsNullOrWhiteSpace(name))
            //    {
            //        Groups.Remove(base.Context.ConnectionId, name);
            //        Logger.Info("disconnect as " + name);
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    Logger.Error(ex);
            //}
            //return base.OnDisconnected();

            try
            {
                Logger.Error("=====> OnDisconnected");
                var connectionId = base.Context.ConnectionId;
                Logger.Error(connectionId);
                var name = _userNameAndConnectionId.FirstOrDefault(f => f.Key == connectionId).Value;
                Logger.Error(name);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    Groups.Remove(base.Context.ConnectionId, name);
                    Logger.Info("disconnect as " + name);
                    Logger.Error("=====> OnDisconnected success");
                }
                Logger.Error("=====================");
            }
            catch (System.Exception ex)
            {
                Logger.Error(ex);
            }
            return base.OnDisconnected();
        }
    }
}