using GAPPOnline.Models.Settings;
using GAPPOnline.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GAPPOnline.Hubs
{
    [HubName("GSAKMacroHub")]
    public class GSAKMacroHub : Hub
    {
        private static IHubContext _context;

        public GSAKMacroHub()
        {
        }

        private static IHubContext HubContext
        {
            get
            {
                if (_context == null)
                {
                    _context = Startup.ConnectionManager.GetHubContext<GSAKMacroHub>();
                }
                return _context;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        [HubMethodName("RunMacro")]
        public void RunMacro(string userGuid, string filename)
        {
            GSAKMacroService.Instance.RunMacro(this.Context.ConnectionId, userGuid, filename);
        }

        public static void MsgOK(string userGuid, string msg)
        {
            HubContext.Clients.Client(userGuid).MsgOK(msg);
        }

    }
}
