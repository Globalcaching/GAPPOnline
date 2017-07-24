using GAPPOnline.Hubs;
using GAPPOnline.Models.Settings;
using GAPPOnline.Services.Database;
using GAPPOnline.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GAPPOnline.Services
{
    public partial class GSAKMacroService
    {
        public class Macro : IDisposable
        {
            public int Depth { get; private set; }
            public string FileName { get; private set; }
            public List<Line> Lines { get; private set; }
            public User User { get; private set; }
            public Macro Caller { get; private set; }
            public string ConnectionId { get; private set; }
            public Dictionary<string, Variable> Variables { get; private set; }
            public static Dictionary<string, Type> Statements { get; private set; }
            public static Dictionary<string, Type> Functions { get; private set; }
            public Dictionary<string, SystemVariable> SystemVariables { get; private set; }
            public volatile bool _stopped = false;
            public volatile bool _stopping = false;

            static Macro()
            {
                Statements = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
                Assembly asm = typeof(GSAKMacroService).GetTypeInfo().Assembly;
                var types = asm.GetTypes().Where(x => x.GetTypeInfo().IsClass && x.GetTypeInfo().BaseType == typeof(Statement));
                foreach (Type t in types)
                {
                    var prop = t.GetProperty("Syntax", BindingFlags.Static | BindingFlags.Public);
                    if (prop != null)
                    {
                        var syntax = prop.GetValue(null, null) as string;
                        if (!string.IsNullOrEmpty(syntax))
                        {
                            Statements.Add(syntax, t);
                        }
                    }
                }

                Functions = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
                types = asm.GetTypes().Where(x => x.GetTypeInfo().IsClass && x.GetTypeInfo().BaseType == typeof(Function));
                foreach (Type t in types)
                {
                    var prop = t.GetProperty("Syntax", BindingFlags.Static | BindingFlags.Public);
                    if (prop != null)
                    {
                        var syntax = prop.GetValue(null, null) as string;
                        if (!string.IsNullOrEmpty(syntax))
                        {
                            Functions.Add(syntax, t);
                        }
                    }
                }
            }

            public Macro(User user, string filename)
            {
                User = user;
                FileName = filename;
                Depth = 0;
                Lines = new List<Line>();

                SystemVariables = new Dictionary<string, SystemVariable>();
                Assembly asm = typeof(GSAKMacroService).GetTypeInfo().Assembly;
                var types = asm.GetTypes().Where(x => x.GetTypeInfo().IsClass && x.GetTypeInfo().BaseType == typeof(SystemVariable));
                foreach (Type t in types)
                {
                    var constr = t.GetConstructor(new Type[]{ typeof(Macro) });
                    if (constr != null)
                    {
                        var sv = constr.Invoke(new object[] { this }) as SystemVariable;
                        SystemVariables.Add(sv.Name, sv);
                    }
                }

                Variables = new Dictionary<string, Variable>(StringComparer.OrdinalIgnoreCase);
            }

            public void Stop()
            {
                if (!_stopped && !_stopping)
                {
                    _stopping = true;
                    var maxWait = DateTime.Now.AddSeconds(10);
                    while (!_stopped && DateTime.Now< maxWait)
                    {
                        System.Threading.Thread.Sleep(500);
                    }
                }
            }

            public void Run(string connectionId, Macro caller, int depth)
            {
                if (_stopped || _stopping) return;

                try
                {
                    Caller = caller;
                    ConnectionId = connectionId;
                    Depth = depth;
                    var tf = Path.Combine(GSAKMacroService.Instance.GetGSAKMacroFolder(User.Id, true), FileName);
                    if (File.Exists(tf))
                    {
                        var allLines = File.ReadAllLines(tf);
                        for (int i = 0; i < allLines.Length; i++)
                        {
                            var l = new Line(this, i, allLines[i]);
                            Lines.Add(l);
                        }
                        bool withinDataBlock = false;
                        foreach (var l in Lines)
                        {
                            l.Prepare(withinDataBlock);
                            if (l.Statement is StatementBeginData)
                            {
                                withinDataBlock = true;
                            }
                            else if (l.Statement is StatementEndData)
                            {
                                withinDataBlock = false;
                            }
                        }
                        GSAKMacroService.Instance.DebuggerMacroStarted(this);
                        int index = 0;
                        while (index >= 0 && index < Lines.Count)
                        {
                            index = Lines[index].PreExecute();
                        }

                        index = 0;
                        while (index >= 0 && index < Lines.Count)
                        {
                            //main loop does not execute subs
                            if (Lines[index].Statement is StatementBeginSub)
                            {
                                index++;
                                while (index < Lines.Count && !(Lines[index].Statement is StatementEndSub))
                                {
                                    index++;
                                }
                                index++;
                            }
                            else if (Lines[index].Statement is StatementBeginData)
                            {
                                index++;
                                while (index < Lines.Count && !(Lines[index].Statement is StatementEndData))
                                {
                                    index++;
                                }
                                index++;
                            }
                            else
                            {
                                index = Lines[index].Execute();
                            }
                        }
                    }
                }
                catch
                {
                    _stopped = true;
                    _stopping = false;
                    throw;
                }
                finally
                {
                    GSAKMacroService.Instance.DebuggerMacroFinished(this);
                }
            }

            public object GetVariable(string name)
            {
                Variable v;
                if (Variables.TryGetValue(name, out v))
                {
                    return v.Value;
                }
                SystemVariable s;
                if (SystemVariables.TryGetValue(name, out s))
                {
                    return s.Value;
                }
                return null;
            }

            public void Dispose()
            {
                if (Lines != null)
                {
                    foreach (var line in Lines)
                    {
                        line.Dispose();
                    }
                    Lines = null;
                }
                if (Variables != null)
                {
                    foreach (var variable in Variables.Values)
                    {
                        if (variable.Owner == this)
                        {
                            variable.Dispose();
                        }
                    }
                    Variables = null;
                }
                if (SystemVariables != null)
                {
                    foreach (var variable in SystemVariables.Values)
                    {
                        variable.Dispose();
                    }
                    SystemVariables = null;
                }
            }
        }
    }
}
