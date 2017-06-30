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
        public class StatementEndSub : Statement
        {
            public StatementEndSub(Line line, string statement): 
                base(line, statement)
            {
            }

            public static string Syntax { get { return "ENDSUB"; } }

        }
    }
}
