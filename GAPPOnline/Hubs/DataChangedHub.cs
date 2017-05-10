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
    [HubName("DataChangedHub")]
    public class DataChangedHub : Hub
    {
        private static IHubContext _context;

        public DataChangedHub()
        {
        }

        private static IHubContext HubContext
        {
            get
            {
                if (_context == null)
                {
                    _context = Startup.ConnectionManager.GetHubContext<DataChangedHub>();
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

        [HubMethodName("RegisterForChanges")]
        public void RegisterForChanges(string userGuid)
        {
            this.Groups.Add(this.Context.ConnectionId, userGuid);
        }

        [HubMethodName("ReportChanges")]
        public void ReportChanges(string userGuid, string[] changes, string data)
        {
            this.Clients.Group(userGuid).ReportChanges(null, changes, data);
        }

        //server can call this method
        public static void ReportChangeToClients(Models.Settings.User user, string change)
        {
            HubContext.Clients.Group(user.UserGuid).ReportChanges(user.SessionInfo, new string[] { change });
        }

        public static void ReportChangesToClients(Models.Settings.User user, string[] changes)
        {
            HubContext.Clients.Group(user.UserGuid).ReportChanges(user.SessionInfo, changes);
        }

    }
}
