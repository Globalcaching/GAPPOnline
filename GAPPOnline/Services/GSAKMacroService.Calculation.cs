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
        public class Calculation
        {
            public Macro Owner { get; private set; }
            public Statement Statement { get; private set; }

            public Calculation(Macro owner, Statement statement, string line)
            {
                Owner = owner;
                Statement = statement;

                //inspect line
                //$x + $y....
                //$x
                //$x < $y+$z
                //$x+$y < $a+$b
                //FileExists(...
                //"..."
                //$a + "..."
                //(

                line = line.Trim();
                if (line.Length == 0)
                {
                    Statement.Line.SyntaxError("Expected a variable, string or function.");
                }
                if (line[0] == '$')
                {
                    //a variable
                }
                else if (line[0] == '"')
                {
                    //a string
                }
                else if (line[0] == '(')
                {
                    //sub
                }
                else
                {
                    // a function?
                }
            }

            public object Value
            {
                get
                {
                    object result = null;
                    //todo
                    return result;
                }
            }
        }
    }
}
