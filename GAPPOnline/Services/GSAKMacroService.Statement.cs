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
        }
    }
}
