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
        public class FunctionFileExists : Function
        {
            Calculation calc = null;

            public FunctionFileExists(Line line, Statement statement, string function): 
                base(line, statement, function)
            {
                if (Parameters.Count == 0)
                {
                    Line.SyntaxError("Expected file name");
                }
                calc = new Calculation(line.Macro, statement, Parameters[0]);
            }

            public static string Syntax { get { return "FileExists"; } }

            public override object Value
            {
                get
                {
                    var fn = calc?.Value;
                    if (fn==null || !(fn is string) || (string)fn=="")
                    {
                        Line.SyntaxError("Expected file name.");
                    }
                    return System.IO.File.Exists((string)fn);
                }
            }

        }
    }
}
