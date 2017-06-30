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
            public StatementVariableAssignment(Line line, string variableName, string statement): 
                base(line, statement)
            {
            }

            protected override int PreExecuteStatement()
            {

                //default: next statement
                return Line.LineNumber + 1;
            }

        }
    }
}
