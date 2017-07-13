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
            private Calculation _calc = null;
            private Line _lineElse = null;
            private Line _lineEndIf = null;

            public StatementIf(Line line, string statement): 
                base(line, statement)
            {
                statement = statement.Trim();
                if (statement.Length > 0)
                {
                    _calc = new Calculation(line.Macro, this, statement);
                }
                else
                {
                    line.SyntaxError("Expected a condition");
                }
            }

            public static string Syntax { get { return "IF"; } }

            protected override int PreExecuteStatement()
            {
                //find matching elseand/or endif
                int depth = 0;
                int index = Line.LineNumber+1;
                while (index < Line.Macro.Lines.Count && _lineEndIf==null)
                {
                    if (Line.Macro.Lines[index].Statement is StatementElse)
                    {
                        if (depth == 0)
                        {
                            _lineElse = Line.Macro.Lines[index];
                        }
                    }
                    else if (Line.Macro.Lines[index].Statement is StatementEndIf)
                    {
                        if (depth == 0)
                        {
                            _lineEndIf = Line.Macro.Lines[index];
                        }
                        else
                        {
                            depth--;
                        }
                    }
                    else if (Line.Macro.Lines[index].Statement is StatementIf)
                    {
                        depth++;
                    }
                    index++;
                }

                if (_lineEndIf == null)
                {
                    Line.SyntaxError("No matching ENDIF found");
                }

                //default: next statement
                return Line.LineNumber + 1;
            }

            protected override int ExecuteStatement()
            {
                if ((bool)_calc.Value)
                {
                    return Line.LineNumber + 1;
                }
                else if (_lineElse != null)
                {
                    return _lineElse.LineNumber + 1;
                }
                else
                {
                    return _lineEndIf.LineNumber + 1;
                }
            }

        }
    }
}
