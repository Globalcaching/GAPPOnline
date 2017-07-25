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
        public class Function
        {
            public Line Line { get; private set; }
            public Statement Statement { get; private set; }
            public string FunctionText { get; private set; }
            public List<string> Parameters { get; private set; }

            public Function(Line line, Statement statement, string function)
            {
                Line = line;
                Statement = statement;
                FunctionText = function;
                Parameters = new List<string>();
                GetParameters();
            }

            public virtual object Value
            {
                get { return null; }
            }

            private void GetParameters()
            {
                var fn = FunctionText.Trim();
                while (!string.IsNullOrEmpty(fn))
                {
                    var parValue = "";
                    var pos = 0;
                    var inString = false;
                    while (pos < fn.Length && (inString || fn[pos] != ','))
                    {
                        parValue += fn[pos];
                        if (fn[pos] == '"')
                        {
                            inString = !inString;
                        }
                        pos++;
                    }
                    pos++;
                    if (pos < fn.Length)
                    {
                        fn = fn.Substring(pos);
                    }
                    else
                    {
                        fn = null;
                    }
                    Parameters.Add(parValue.Trim());
                }
            }
        }
    }
}
