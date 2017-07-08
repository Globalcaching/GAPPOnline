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
        public class Line : IDisposable
        {
            public Macro Macro { get; private set; }
            public int LineNumber { get; private set; }
            public string LineText { get; private set; }
            public string LineWithoutComment { get; private set; }
            public Statement Statement { get; private set; }

            public Line(Macro macro, int number, string line)
            {
                Macro = macro;
                LineNumber = number;
                LineText = line;
            }

            public bool Prepare(bool withinDataBlock)
            {
                var result = true;
                if (withinDataBlock)
                {
                    LineWithoutComment = LineText;
                    if (LineWithoutComment.Trim().StartsWith(StatementEndData.Syntax, StringComparison.OrdinalIgnoreCase))
                    {
                        Statement = new StatementEndData(this, LineWithoutComment);
                    }
                }
                else
                {
                    LineWithoutComment = StripComments(LineText);
                    if (LineWithoutComment.Length > 0)
                    {
                        var words = LineWithoutComment.Split(new char[] { ' ' }, 2);
                        if (words[0][0] == '$')
                        {
                            Statement = new StatementVariableAssignment(this, words[0], words.Length > 1 ? words[1] : "");
                        }
                        else
                        {
                            Type t = null;
                            if (Macro.Statements.TryGetValue(words[0], out t))
                            {
                                var constructor = t.GetConstructor(new Type[] { typeof(Line), typeof(string) });
                                Statement = (Statement)constructor.Invoke(new object[] { this, words.Length>1 ? words[1] : "" });
                            }
                        }
                        if (Statement == null)
                        {
                            SyntaxError($"Unknown statement: {words[0]}");
                        }
                    }
                }
                return result;
            }

            public int PreExecute()
            {
                return Statement?.PreExecute() ?? (LineNumber + 1);
            }

            public int Execute()
            {
                return Statement?.Execute() ?? (LineNumber + 1);
            }

            public void Dispose()
            {
                if (Statement != null)
                {
                    Statement.Dispose();
                    Statement = null;
                }
            }

            public void SyntaxError(string message)
            {
                throw new Exception($"Syntax error in line {LineNumber}: {message}");
            }

            private string StripComments(string line)
            {
                int length = 0;
                int quoteOpening = 0;
                int quoteClosing = 0;
                foreach (var c in line.ToArray())
                {
                    if (c == '#' && quoteOpening == quoteClosing)
                    {
                        break;
                    }
                    length++;
                    if (c == '"')
                    {
                        if (quoteOpening == quoteClosing)
                        {
                            quoteOpening++;
                        }
                        else
                        {
                            quoteClosing++;
                        }
                    }
                }
                if (length > 0)
                {
                    return line.Substring(0, length).Trim();
                }
                else
                {
                    return "";
                }
            }
        }
    }
}
