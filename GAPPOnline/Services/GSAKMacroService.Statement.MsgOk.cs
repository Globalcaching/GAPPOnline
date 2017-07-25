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
        public class StatementMsgOk : Statement
        {
            private Calculation _msg = null;
            private Calculation _caption = null;
            private volatile bool _waitForResult = false;

            public StatementMsgOk(Line line, string statement): 
                base(line, statement)
            {
                var parameters = GetParameters(statement);
                string s;
                if (parameters.TryGetValue("msg", out s))
                {
                    _msg = new Calculation(Line.Macro, this, s);
                }
                else
                {
                    line.SyntaxError("Missing parameter msg");
                }
                if (parameters.TryGetValue("msg", out s))
                {
                    _caption = new Calculation(Line.Macro, this, s);
                }
            }

            public static string Syntax { get { return "MSGOK"; } }

            protected override int ExecuteStatement()
            {
                _waitForResult = true;
                Line.Macro.OnMessageOK = () =>
                {
                    _waitForResult = false;
                };
                GSAKMacroHub.MsgOK(Line.Macro.ConnectionId, _msg.Value?.ToString(), _caption?.Value?.ToString() ?? "Message");
                while (!Line.Macro._stopped && !Line.Macro._stopping && _waitForResult)
                {
                    System.Threading.Thread.Sleep(100);
                }
                return base.ExecuteStatement();
            }

        }
    }
}
