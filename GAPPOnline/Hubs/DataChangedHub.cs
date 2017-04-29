using GAPPOnline.Models.Settings;
using GAPPOnline.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GAPPOnline.Hubs
{
    public class DataChangedHub: Hub
    {
        private static IHubContext _context;

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

        public async Task RegisterUser(string userGuid)
        {
            var u = AccountService.Instance.GetUserByUserGuid(userGuid);
            if (u != null)
            {
                await Groups.Add(Context.ConnectionId, userGuid);
                Clients.Group(u.UserGuid).sessionInfoUpdated(u.SessionInfo);
            }
        }

        //server can call this method
        public static void SendSessionInfoToClient(User user)
        {
            HubContext.Clients.Group(user.UserGuid).sessionInfoUpdated(user.SessionInfo);
        }

        public static void SendDataChangedToClient(User user, string data)
        {
            HubContext.Clients.Group(user.UserGuid).dataChanged(data);
        }

    }
}
