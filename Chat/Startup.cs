using Microsoft.ApplicationInsights.Extensibility.Web.ContextInitializers;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Practices.Unity;
using Ninject;
using Owin;

[assembly: OwinStartup(typeof(Chat.Startup))]

namespace Chat
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalHost.DependencyResolver.Register(
                typeof(ChatHub), () => new ChatHub(new ChatMessageRepository()));

            const string connectionString = "Endpoint=sb://srchatservicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=DFsAlPhqBtiBujb1Szh1VL1BkRm05UeigA4OHfiuRuc=";
            
            GlobalHost.DependencyResolver.UseServiceBus(connectionString, "Chat");
            app.MapSignalR();
        }
    }
}
