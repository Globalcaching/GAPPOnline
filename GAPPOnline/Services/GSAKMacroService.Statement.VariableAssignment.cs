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
        public class StatementVariableAssignment : Statement
        {
            private Calculation calc = null;
            private string VariableName;
            private Variable Variable;

            public StatementVariableAssignment(Line line, string variableName, string statement): 
                base(line, statement)
            {
                VariableName = variableName;
                statement = statement.Trim();
                if (statement.Length == 0)
                {
                    Line.SyntaxError("Expected '='");
                }
                statement = statement.Substring(1).Trim();
                if (statement.Length == 0)
                {
                    Line.SyntaxError("Expected a statement after '='");
                }
                calc = new Calculation(line.Macro, this, statement);
            }

            protected override int ExecuteStatement()
            {
                if (Variable == null)
                {
                    Variable v;
                    if (!Line.Macro.Variables.TryGetValue(VariableName, out v))
                    {
                        Variable = new Variable(Line.Macro, VariableName);
                        Line.Macro.Variables.Add(VariableName, Variable);
                    }
                    else
                    {
                        Variable = v;
                    }
                }
                Variable.Value = calc.Value;
                return Line.LineNumber + 1;
            }

        }
    }
}
