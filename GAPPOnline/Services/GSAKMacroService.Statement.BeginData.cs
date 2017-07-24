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
        public class StatementBeginData : Statement
        {
            private Calculation _name = null;

            public StatementBeginData(Line line, string statement): 
                base(line, statement)
            {
                var parameters = GetParameters(statement);
                if (!parameters.TryGetValue("VarName", out _name))
                {
                    line.SyntaxError("Missing parameter VarName");
                }
            }

            public static string Syntax { get { return "<data>"; } }

            protected override int PreExecuteStatement()
            {
                Variable v;
                if (!Line.Macro.Variables.TryGetValue(_name.StringValue, out v))
                {
                    v = new Variable(Line.Macro, _name.StringValue);
                    Line.Macro.Variables.Add(v.Name, v);
                }
                var value = new StringBuilder();
                var index = Line.LineNumber + 1;
                while (index < Line.Macro.Lines.Count && !(Line.Macro.Lines[index].Statement is StatementEndData))
                {
                    value.AppendLine(Line.Macro.Lines[index].LineText);
                    index++;
                }
                return index;
            }

        }
    }
}
