using GAPPOnline.Hubs;
using GAPPOnline.Models.Settings;
using GAPPOnline.Services.Database;
using GAPPOnline.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GAPPOnline.Services
{
    public partial class GSAKMacroService
    {
        public class Calculation
        {
            public Macro Owner { get; private set; }
            public Statement Statement { get; private set; }

            public class CalculationStep
            {
                public Calculation Owner { get; private set; }

                public string Operator { get; set; } //+, -, *, < etc
                public bool Negative { get; set; } = false;
                public Calculation Calculation { get; set; }
                public double? DirectValueDouble { get; set; }
                public long? DirectValueInt { get; set; }
                public string VariableName { get; set; }
                public string StringValue { get; set; }
                public Function Function { get; set; }

                public object CalculatedValue { get; set; }

                private Regex _regex = null;

                public CalculationStep(Calculation owner)
                {
                    Owner = owner;
                }

                public object Value
                {
                    get
                    {
                        object result = null;
                        if (Calculation != null)
                        {
                            result = Calculation.Value;
                        }
                        else if (DirectValueDouble != null)
                        {
                            result = DirectValueDouble.Value;
                        }
                        else if (DirectValueInt != null)
                        {
                            result = DirectValueInt.Value;
                        }
                        else if (!string.IsNullOrEmpty(VariableName))
                        {
                            result = Owner.Owner.GetVariable(VariableName);
                        }
                        else if (!string.IsNullOrEmpty(StringValue))
                        {
                            if (_regex == null)
                            {
                                _regex = new Regex(@"\$.+?\b");
                            }
                            result = _regex.Replace(StringValue, (m) => ((Owner.Owner.GetVariable(m.Value)??"")).ToString());
                        }
                        else if (Function != null)
                        {
                            result = Function.Value;
                        }
                        if (result == null)
                        {
                            Owner.Statement.Line.SyntaxError("Error");
                        }
                        if (Negative)
                        {
                            if (result.GetType() == typeof(double))
                            {
                                result = -1 * (double)result;
                            }
                            else if (result.GetType() == typeof(long))
                            {
                                result = -1 * (long)result;
                            }
                        }
                        return result;
                    }
                }
            }

            public List<CalculationStep> _calculationSteps;

            public Calculation(Macro owner, Statement statement, string line)
            {
                Owner = owner;
                Statement = statement;
                _calculationSteps = new List<CalculationStep>();

                //inspect line
                //-2....
                //2....
                //$x + $y....
                //$x
                //$x < $y+$z
                //$x+$y < $a+$b
                //FileExists(...
                //"..."
                //$a + "..."
                //(

                var step = new CalculationStep(this);
                step.Operator = "+";
                line = line.Trim();
                while (line.Length > 0)
                {
                    var startOfRemainder = -1;
                    if (line[0] == '-')
                    {
                        step.Negative = true;
                    }

                    if ("1234567890".Contains(line[0]))
                    {
                        //direct value
                        startOfRemainder = line.IndexOfAny("-+* \t/<>=".ToCharArray());
                        string v;
                        if (startOfRemainder < 0)
                        {
                            v = line;
                        }
                        else
                        {
                            v = line.Substring(0, startOfRemainder);
                        }
                        long val;
                        double dval;
                        if (long.TryParse(v, out val))
                        {
                            step.DirectValueInt = val;
                        }
                        else if (double.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out dval))
                        {
                            step.DirectValueDouble = dval;
                        }
                    }

                    if (line[0] == '$')
                    {
                        //a variable
                        startOfRemainder = line.IndexOfAny("-+* \t/<>=^".ToCharArray());
                        if (startOfRemainder < 0)
                        {
                            step.VariableName = line;
                        }
                        else
                        {
                            step.VariableName = line.Substring(0, startOfRemainder);
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
                            step.StringValue = "";
                        }
                        else
                        {
                            step.StringValue = line.Substring(1, startOfRemainder - 2);
                        }
                    }
                    else if (line[0] == '(')
                    {
                        //sub
                        startOfRemainder++;
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
                        step.Calculation = new Calculation(Owner, Statement, line.Substring(1, startOfRemainder - 2));
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
                        step.Function = (Function)constr.Invoke(new object[] { Statement.Line, Statement, funcArguments });
                    }

                    _calculationSteps.Add(step);
                    if (startOfRemainder < 0 || startOfRemainder >= line.Length)
                    {
                        break;
                    }

                    step = new CalculationStep(this);
                    //get operator
                    startOfRemainder = line.IndexOfAny("-+*\\<>=^".ToCharArray());
                    if (startOfRemainder < 0)
                    {
                        Statement.Line.SyntaxError("Unexpected end of line. Expected operator");
                    }
                    line = line.Substring(startOfRemainder);
                    if (line[1] == '=' || line[1] == '>')
                    {
                        step.Operator = line.Substring(0, 2);
                        line = line.Substring(2);
                    }
                    else
                    {
                        step.Operator = line.Substring(0, 1);
                        line = line.Substring(1);
                    }
                    line = line.Trim();
                }
                if (_calculationSteps.Count == 0)
                {
                    Statement.Line.SyntaxError("Expected value, variable or function");
                }
            }

            private int GetOperatorLevel(string op)
            {
                switch (op)
                {
                    case "+":
                    case "-":
                        return 1;
                    case "*":
                    case "/":
                        return 2;
                    case "^":
                        return 3;
                    default:
                        return 0;
                }
            }

            private object Calc(object val1, object val2, object op)
            {
                object result = null;
                switch (op)
                {
                    case "+":
                        if (val1.GetType() == typeof(string) && val2.GetType() == typeof(string))
                        {
                            result = string.Concat((string)val1, (string)val2);
                        }
                        else if (val1.GetType() == typeof(double) && val2.GetType() == typeof(double))
                        {
                            result = (double)val1 + (double)val2;
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(long))
                        {
                            result = (long)val1 + (long)val2;
                        }
                        else if (val1.GetType() == typeof(double) && val2.GetType() == typeof(long))
                        {
                            result = (double)val1 + (long)val2;
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(double))
                        {
                            result = (long)val1 + (double)val2;
                        }
                        break;
                    case "-":
                        if (val1.GetType() == typeof(double) && val2.GetType() == typeof(double))
                        {
                            result = (double)val1 - (double)val2;
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(long))
                        {
                            result = (long)val1 - (long)val2;
                        }
                        else if (val1.GetType() == typeof(double) && val2.GetType() == typeof(long))
                        {
                            result = (double)val1 - (long)val2;
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(double))
                        {
                            result = (long)val1 - (double)val2;
                        }
                        break;
                    case "*":
                        if (val1.GetType() == typeof(double) && val2.GetType() == typeof(double))
                        {
                            result = (double)val1 * (double)val2;
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(long))
                        {
                            result = (long)val1 * (long)val2;
                        }
                        else if (val1.GetType() == typeof(double) && val2.GetType() == typeof(long))
                        {
                            result = (double)val1 * (long)val2;
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(double))
                        {
                            result = (long)val1 * (double)val2;
                        }
                        break;
                    case "/":
                        if (val1.GetType() == typeof(double) && val2.GetType() == typeof(double))
                        {
                            result = (double)val1 / (double)val2;
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(long))
                        {
                            result = (long)val1 / (long)val2;
                        }
                        else if (val1.GetType() == typeof(double) && val2.GetType() == typeof(long))
                        {
                            result = (double)val1 / (long)val2;
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(double))
                        {
                            result = (long)val1 / (double)val2;
                        }
                        break;
                    case "^":
                        if (val1.GetType() == typeof(double) && val2.GetType() == typeof(double))
                        {
                            result = Math.Pow((double)val1,(double)val2);
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(long))
                        {
                            result = Math.Pow((long)val1, (long)val2);
                        }
                        else if (val1.GetType() == typeof(double) && val2.GetType() == typeof(long))
                        {
                            result = Math.Pow((double)val1, (long)val2);
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(double))
                        {
                            result = Math.Pow((long)val1, (double)val2);
                        }
                        break;
                    case "<":
                        if (val1.GetType() == typeof(string) && val2.GetType() == typeof(string))
                        {
                            result = string.Compare((string)val1, (string)val2) < 0;
                        }
                        else if (val1.GetType() == typeof(double) && val2.GetType() == typeof(double))
                        {
                            result = (double)val1 < (double)val2;
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(long))
                        {
                            result = (long)val1 < (long)val2;
                        }
                        else if (val1.GetType() == typeof(double) && val2.GetType() == typeof(long))
                        {
                            result = (double)val1 < (long)val2;
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(double))
                        {
                            result = (long)val1 < (double)val2;
                        }
                        break;
                    case "<=":
                        if (val1.GetType() == typeof(string) && val2.GetType() == typeof(string))
                        {
                            result = string.Compare((string)val1, (string)val2) <= 0;
                        }
                        else if (val1.GetType() == typeof(double) && val2.GetType() == typeof(double))
                        {
                            result = (double)val1 <= (double)val2;
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(long))
                        {
                            result = (long)val1 <= (long)val2;
                        }
                        else if (val1.GetType() == typeof(double) && val2.GetType() == typeof(long))
                        {
                            result = (double)val1 <= (long)val2;
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(double))
                        {
                            result = (long)val1 <= (double)val2;
                        }
                        break;
                    case ">=":
                        if (val1.GetType() == typeof(string) && val2.GetType() == typeof(string))
                        {
                            result = string.Compare((string)val1, (string)val2) >= 0;
                        }
                        else if (val1.GetType() == typeof(double) && val2.GetType() == typeof(double))
                        {
                            result = (double)val1 >= (double)val2;
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(long))
                        {
                            result = (long)val1 >= (long)val2;
                        }
                        else if (val1.GetType() == typeof(double) && val2.GetType() == typeof(long))
                        {
                            result = (double)val1 >= (long)val2;
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(double))
                        {
                            result = (long)val1 >= (double)val2;
                        }
                        break;
                    case ">":
                        if (val1.GetType() == typeof(string) && val2.GetType() == typeof(string))
                        {
                            result = string.Compare((string)val1, (string)val2) > 0;
                        }
                        else if (val1.GetType() == typeof(double) && val2.GetType() == typeof(double))
                        {
                            result = (double)val1 > (double)val2;
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(long))
                        {
                            result = (long)val1 > (long)val2;
                        }
                        else if (val1.GetType() == typeof(double) && val2.GetType() == typeof(long))
                        {
                            result = (double)val1 > (long)val2;
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(double))
                        {
                            result = (long)val1 > (double)val2;
                        }
                        break;
                    case "<>":
                        if (val1.GetType() == typeof(string) && val2.GetType() == typeof(string))
                        {
                            result = string.Compare((string)val1, (string)val2) != 0;
                        }
                        else if (val1.GetType() == typeof(double) && val2.GetType() == typeof(double))
                        {
                            result = (double)val1 != (double)val2;
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(long))
                        {
                            result = (long)val1 != (long)val2;
                        }
                        else if (val1.GetType() == typeof(double) && val2.GetType() == typeof(long))
                        {
                            result = (double)val1 != (long)val2;
                        }
                        else if (val1.GetType() == typeof(long) && val2.GetType() == typeof(double))
                        {
                            result = (long)val1 != (double)val2;
                        }
                        break;
                    default:
                        break;
                }
                return result;
            }

            public object Value
            {
                get
                {
                    //3 + 2 * 4 ^ 2
                    var cs = _calculationSteps.ToList();
                    foreach (var c in cs)
                    {
                        c.CalculatedValue = c.Value;
                    }
                    while (cs.Count>1)
                    {
                        int index = 1;
                        var curLevel = GetOperatorLevel(cs[0].Operator);
                        var nextLevel = GetOperatorLevel(cs[1].Operator);
                        while (index < cs.Count - 1 && nextLevel>curLevel)
                        {
                            curLevel = nextLevel;
                            nextLevel = GetOperatorLevel(cs[index].Operator);
                            index++;
                        }
                        cs[index - 1].CalculatedValue = Calc(cs[index - 1].CalculatedValue, cs[index].CalculatedValue, cs[index].Operator);
                        cs.RemoveAt(index);
                    }
                    return cs[0].CalculatedValue;
                }
            }
        }
    }
}
