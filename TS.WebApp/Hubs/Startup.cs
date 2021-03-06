﻿using Microsoft.AspNet.SignalR;
using Owin;
using Microsoft.Owin;
using TimeSheetMvc4WebApplication.Hubs;

//[assembly: OwinStartup(typeof(NoticeHub))]
[assembly: OwinStartup(typeof(Startup))]

namespace TimeSheetMvc4WebApplication.Hubs
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HubConfiguration
            {
                EnableJavaScriptProxies = true
            };

            app.MapHubs(config);

            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();
        }
    }
}