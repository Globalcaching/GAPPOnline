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
        public class Statement : IDisposable
        {
            public Line Line { get; private set; }
            public string StatementText { get; private set; }

            public Statement(Line line, string statement)
            {
                Line = line;
                StatementText = statement;
            }

            public int PreExecute()
            {
                return PreExecuteStatement();
            }

            public int Execute()
            {
                return ExecuteStatement();
            }

            protected virtual int PreExecuteStatement()
            {
                //default: next statement
                return Line.LineNumber + 1;
            }

            protected virtual int ExecuteStatement()
            {
                //default: next statement
                return Line.LineNumber + 1;
            }

            public virtual void Dispose()
            {
            }

            public Dictionary<string, Calculation> GetParameters(string txt, string[] enums = null)
            {
                var result = new Dictionary<string, Calculation>(StringComparer.OrdinalIgnoreCase);
                var res = txt;
                while (!string.IsNullOrEmpty(res))
                {
                    var parts = res.Split(new char[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        var parName = parts[0].Trim();
                        var pos = parts[0].Length + 1;
                        var inString = false;
                        while (pos < res.Length && (inString || res[pos] != ' '))
                        {
                            if (res[pos] == '"')
                            {
                                inString = !inString;
                            }
                            pos++;
                        }
                        var parValue = res.Substring(parts[0].Length + 1, pos - parts[0].Length - 1).Trim();
                        result.Add(parName, new Calculation(Line.Macro, this, parValue, isEnum: enums==null?true: ((from a in enums where string.Compare(a, parName, true) == 0 select a).Any())));

                        if (pos < res.Length)
                        {
                            res = res.Substring(pos);
                        }
                        else
                        {
                            res = "";
                        }
                    }
                    else
                    {
                        Line.SyntaxError("Cannot read parameter");
                    }
                }
                return result;
            }
        }
    }
}
