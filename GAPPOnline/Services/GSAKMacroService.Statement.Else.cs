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
        public class StatementElse : Statement
        {
            private Line _lineEndIf = null;

            public StatementElse(Line line, string statement): 
                base(line, statement)
            {
            }

            public static string Syntax { get { return "ELSE"; } }

            protected override int PreExecuteStatement()
            {
                //find matching endif
                int depth = 0;
                int index = Line.LineNumber + 1;
                while (index < Line.Macro.Lines.Count && _lineEndIf == null)
                {
                    if (Line.Macro.Lines[index].Statement is StatementEndIf)
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
                return _lineEndIf.LineNumber + 1;
            }

        }
    }
}
