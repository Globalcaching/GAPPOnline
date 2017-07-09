using GAPPOnline.Hubs;
using GAPPOnline.Models.Settings;
using GAPPOnline.Services.Database;
using GAPPOnline.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

            private string VariableName = null;
            private string StringValue = null;
            private string Operator = null;
            private Calculation SubAction = null;
            private Calculation NextAction = null;
            private Function Function = null;

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
                var startOfRemainder = -1;
                if (line.Length == 0)
                {
                    Statement.Line.SyntaxError("Expected a variable, string or function.");
                }
                if (line[0] == '$')
                {
                    //a variable
                    startOfRemainder = line.IndexOfAny("-+* \t\\<>=".ToCharArray());
                    if (startOfRemainder < 0)
                    {
                        VariableName = line;
                    }
                    else
                    {
                        VariableName = line.Substring(0, startOfRemainder);
                    }
                }
                else if (line[0] == '"')
                {
                    //a string
                    //gsak does not support quote escapes
                    startOfRemainder = line.IndexOf('"', 1);
                    if (startOfRemainder < 0)
                    {
                        statement.Line.SyntaxError("Unexpected end of string. Missing '\"'");
                    }
                    startOfRemainder++;
                    if (startOfRemainder == 2)
                    {
                        StringValue = "";
                    }
                    else
                    {
                        StringValue = line.Substring(1, startOfRemainder - 2);
                    }
                }
                else if (line[0] == '(')
                {
                    //sub
                }
                else
                {
                    // a function?
                    startOfRemainder = line.IndexOf('(', 1);
                    if (startOfRemainder < 0)
                    {
                        statement.Line.SyntaxError("Unexpected end of line. Expecting '('");
                    }
                    var funcName = line.Substring(0, startOfRemainder).Trim();
                    var funcArguments = "";
                    startOfRemainder++;
                    var startOfFuncArguments = startOfRemainder;
                    var open = 1;
                    var close = 0;
                    var inString = false;
                    while (startOfRemainder < line.Length)
                    {
                        switch (line[startOfRemainder])
                        {
                            case ')':
                                if (!inString) close++;
                                break;
                            case '(':
                                if (!inString) open++;
                                break;
                            case '"':
                                inString = !inString;
                                break;
                        }
                        if (open == close) break;
                        startOfRemainder++;
                    }
                    if (open != close)
                    {
                        Statement.Line.SyntaxError("Unexpected end of line. Expected ')'.");
                    }
                    if ((startOfRemainder - startOfFuncArguments - 1) > 0)
                    {
                        funcArguments = line.Substring(startOfFuncArguments, startOfRemainder - startOfFuncArguments - 1);
                    }
                    startOfRemainder++;

                    Type func;
                    if (!Macro.Functions.TryGetValue(funcName, out func))
                    {
                        Statement.Line.SyntaxError($"Unknown function {funcName}");
                    }
                    var constr = func.GetConstructor(new Type[] { typeof(Line), typeof(Statement), typeof(string) });
                    Function = (Function)constr.Invoke(new object[] { Statement.Line, Statement, funcArguments });
                }

                if (startOfRemainder > 0 && startOfRemainder < line.Length)
                {
                    //expect operator =<>+-
                }
            }

            public object Value
            {
                get
                {
                    object result = null;

                    if (StringValue != null)
                    {
                        //todo perform inline replacements
                        result = StringValue;
                    }
                    else if (VariableName!=null)
                    {
                        result = Owner.GetVariable(VariableName);
                    }
                    else if (Function != null)
                    {
                        result = Function.Value;
                    }
                    else if (SubAction != null)
                    {
                        result = SubAction.Value;
                    }

                    if (!string.IsNullOrEmpty(Operator))
                    {
                        //todo
                    }

                    return result;
                }
            }
        }
    }
}
