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
        public class StatementIf : Statement
        {
            private Calculation calc = null;

            public StatementIf(Line line, string statement): 
                base(line, statement)
            {
                statement = statement.Trim();
                if (statement.Length > 0)
                {
                    calc = new Calculation(line.Macro, this, statement);
                }
                else
                {
                    line.SyntaxError("Expected a condition");
                }
            }

            public static string Syntax { get { return "IF"; } }

        }
    }
}
