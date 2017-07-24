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
    [HubName("GSAKMacroDebugHub")]
    public class GSAKMacroDebugHub : Hub
    {
        private static IHubContext _context;

        public GSAKMacroDebugHub()
        {
        }

        private static IHubContext HubContext
        {
            get
            {
                if (_context == null)
                {
                    _context = Startup.ConnectionManager.GetHubContext<GSAKMacroDebugHub>();
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
            Clients.Client(this.Context.ConnectionId).DebuggerConnected();
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            GSAKMacroService.Instance.DetachDebugScreen(this.Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        [HubMethodName("AttachDebugScreen")]
        public void AttachDebugScreen(string userGuid)
        {
            GSAKMacroService.Instance.AttachDebugScreen(this.Context.ConnectionId, userGuid);
        }

        [HubMethodName("DebugRun")]
        public void DebugRun()
        {
            GSAKMacroService.Instance.DebugRun(this.Context.ConnectionId);
        }

        [HubMethodName("DebugStep")]
        public void DebugStep()
        {
            GSAKMacroService.Instance.DebugStep(this.Context.ConnectionId);
        }

        [HubMethodName("DebugStop")]
        public void DebugStop()
        {
            GSAKMacroService.Instance.DebugStop(this.Context.ConnectionId);
        }

        [HubMethodName("DebugAddBreakPoint")]
        public void DebugAddBreakPoint(int lineNumber)
        {
            GSAKMacroService.Instance.DebugAddBreakPoint(this.Context.ConnectionId, lineNumber);
        }

        [HubMethodName("DebugRemoveBreakPoint")]
        public void DebugRemoveBreakPoint(int lineNumber)
        {
            GSAKMacroService.Instance.DebugRemoveBreakPoint(this.Context.ConnectionId, lineNumber);
        }

        public static void MacroIsStarted(string connectionId, string filename, List<string> lines)
        {
            HubContext.Clients.Client(connectionId).MacroIsStarted(filename, lines);
        }

        public static void MacroIsPaused(string connectionId, int lineNumber)
        {
            HubContext.Clients.Client(connectionId).MacroIsPaused(lineNumber);
        }

        public static void MacroIsFinished(string connectionId)
        {
            HubContext.Clients.Client(connectionId).MacroIsFinished();
        }

    }
}
