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
        public class StatementDeclare : Statement
        {
            private Calculation _name = null;
            private Calculation _type = null;

            public StatementDeclare(Line line, string statement): 
                base(line, statement)
            {
                var parameters = GetParameters(statement, enums: new string[] { "Type" });
                if (!parameters.TryGetValue("Var", out _name))
                {
                    line.SyntaxError("Missing parameter Var");
                }
                if (!parameters.TryGetValue("Type", out _type))
                {
                    line.SyntaxError("Missing parameter Type");
                }
            }

            public static string Syntax { get { return "DECLARE"; } }

            protected override int PreExecuteStatement()
            {
                Variable v;
                if (!Line.Macro.Variables.TryGetValue(_name.VariableName, out v))
                {
                    v = new Variable(Line.Macro, _name.VariableName);
                    switch(_type.StringValue.ToLower())
                    {
                        case "string":
                            v.Type = typeof(string);
                            break;
                        case "numeric":
                            v.Type = typeof(double);
                            break;
                        case "boolean":
                            v.Type = typeof(bool);
                            break;
                        case "date":
                            v.Type = typeof(DateTime);
                            break;
                    }
                }
                else
                {
                    Line.SyntaxError($"Variable {_name.Value} already exists");
                }
                Line.Macro.Variables.Add(v.Name, v);
                return base.PreExecuteStatement();
            }

        }
    }
}
