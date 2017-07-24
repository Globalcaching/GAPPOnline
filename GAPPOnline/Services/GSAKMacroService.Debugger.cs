using GAPPOnline.Hubs;
using GAPPOnline.Models.Settings;
using GAPPOnline.Services.Database;
using GAPPOnline.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPOnline.Services
{
    public partial class GSAKMacroService
    {
        public class DebugScreenInfo
        {
            public bool Connected { get; set; }
            public bool StepMode { get; set; }
            public User User { get; set; }
            public Macro ActiveMacro { get; set; }
            public string DebuggerConnectionId { get; set; }
            public List<int> BreakPoints { get; set; }

            public bool ContinueRun { get; set; }
        }

        public List<DebugScreenInfo> DebugScreens = new List<DebugScreenInfo>();

        public void DebuggerMacroStarted(Macro macro)
        {
            lock (DebugScreens)
            {
                var dbg = (from a in DebugScreens where a.User.Id == macro.User.Id && a.ActiveMacro == null select a).FirstOrDefault();
                if (dbg != null)
                {
                    dbg.ActiveMacro = macro;
                    dbg.StepMode = true;
                    dbg.ContinueRun = false;
                    dbg.BreakPoints = new List<int>();
                    GSAKMacroDebugHub.MacroIsStarted(dbg.DebuggerConnectionId, macro.FileName, (from a in macro.Lines select a.LineText).ToList());
                }
            }
        }

        public void DebuggerMacroFinished(Macro macro)
        {
            lock (DebugScreens)
            {
                var dbg = (from a in DebugScreens where a.User.Id == macro.User.Id && a.ActiveMacro == macro select a).FirstOrDefault();
                if (dbg != null)
                {
                    dbg.ActiveMacro = null;
                    dbg.BreakPoints = new List<int>();
                    GSAKMacroDebugHub.MacroIsFinished(dbg.DebuggerConnectionId);
                }
            }
        }

        public void DebuggerExecuteLine(Line line)
        {
            if (line.Macro._stopping)
            {
                throw new Exception("Macro stopped.");
            }
            if (!(line.Statement is StatementEmpty))
            {
                DebugScreenInfo dbg = null;
                lock (DebugScreens)
                {
                    dbg = (from a in DebugScreens where a.ActiveMacro == line.Macro select a).FirstOrDefault();
                }
                if (dbg != null)
                {
                    bool shouldBreak;
                    lock (DebugScreens)
                    {
                        shouldBreak = dbg.StepMode || dbg.BreakPoints.Contains(line.LineNumber);
                        if (shouldBreak)
                        {
                            dbg.ContinueRun = false;
                        }
                    }
                    if (shouldBreak)
                    {
                        //if break, we need to send the current state to the client
                        GSAKMacroDebugHub.MacroIsPaused(dbg.DebuggerConnectionId, line.LineNumber);

                        //wait for continue
                        while (shouldBreak)
                        {
                            if (line.Macro._stopping)
                            {
                                throw new Exception("Macro stopped.");
                            }

                            System.Threading.Thread.Sleep(100);
                            lock (DebugScreens)
                            {
                                dbg = (from a in DebugScreens where a.ActiveMacro == line.Macro select a).FirstOrDefault();
                                if (dbg != null)
                                {
                                    shouldBreak = !dbg.ContinueRun;
                                }
                                else
                                {
                                    shouldBreak = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void AttachDebugScreen(string pconnectionId, string puserGuid)
        {
            var usr = AccountService.Instance.GetUserByUserGuid(puserGuid);
            if (usr != null)
            {
                lock (DebugScreens)
                {
                    var dbg = new DebugScreenInfo()
                    {
                        Connected = true,
                        StepMode = true,
                        ActiveMacro = null,
                        ContinueRun = false,
                        BreakPoints = new List<int>(),
                        DebuggerConnectionId = pconnectionId,
                        User = usr
                    };
                    DebugScreens.Add(dbg);
                }
            }
        }

        public void DetachDebugScreen(string pconnectionId)
        {
            lock (DebugScreens)
            {
                var dbg = (from a in DebugScreens where a.DebuggerConnectionId == pconnectionId select a).FirstOrDefault();
                if (dbg != null)
                {
                    dbg.Connected = false;
                    DebugScreens.Remove(dbg);
                }
            }
        }

        public void DebugRun(string connectionId)
        {
            lock (DebugScreens)
            {
                var dbg = (from a in DebugScreens where a.DebuggerConnectionId == connectionId select a).FirstOrDefault();
                if (dbg != null)
                {
                    dbg.StepMode = false;
                    dbg.ContinueRun = true;
                }
            }
        }

        public void DebugStep(string connectionId)
        {
            lock (DebugScreens)
            {
                var dbg = (from a in DebugScreens where a.DebuggerConnectionId == connectionId select a).FirstOrDefault();
                if (dbg != null)
                {
                    dbg.StepMode = true;
                    dbg.ContinueRun = true;
                }
            }
        }

        public void DebugAddBreakPoint(string connectionId, int lineNumber)
        {
            lock (DebugScreens)
            {
                var dbg = (from a in DebugScreens where a.DebuggerConnectionId == connectionId select a).FirstOrDefault();
                if (dbg != null)
                {
                    if (!dbg.BreakPoints.Contains(lineNumber))
                    {
                        dbg.BreakPoints.Add(lineNumber);
                    }
                }
            }
        }

        public void DebugRemoveBreakPoint(string connectionId, int lineNumber)
        {
            lock (DebugScreens)
            {
                var dbg = (from a in DebugScreens where a.DebuggerConnectionId == connectionId select a).FirstOrDefault();
                if (dbg != null)
                {
                    if (dbg.BreakPoints.Contains(lineNumber))
                    {
                        dbg.BreakPoints.Remove(lineNumber);
                    }
                }
            }
        }

        public void DebugStop(string connectionId)
        {
            lock (DebugScreens)
            {
                var dbg = (from a in DebugScreens where a.DebuggerConnectionId == connectionId select a).FirstOrDefault();
                if (dbg != null && !dbg.ActiveMacro._stopped && !dbg.ActiveMacro._stopping)
                {
                    dbg.ActiveMacro._stopping = true;
                }
                //if (dbg != null)
                //{
                //    dbg.ActiveMacro.Stop();
                //}
            }
        }

    }
}
