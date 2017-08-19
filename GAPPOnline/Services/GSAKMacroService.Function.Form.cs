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
        public class FunctionForm : Function
        {
            Calculation _formText = null;
            private volatile bool _waitForResult = false;

            public FunctionForm(Line line, Statement statement, string function): 
                base(line, statement, function)
            {
                if (Parameters.Count == 0)
                {
                    Line.SyntaxError("Expected form data");
                }
                _formText = new Calculation(line.Macro, statement, Parameters[0]);
            }

            public static string Syntax { get { return "Form"; } }

            public override object Value
            {
                get
                {
                    string result = "";

                    var formtext = _formText.Value;
                    var macroForm = MacroForm.Parse(Line, formtext as string);

                    _waitForResult = true;
                    Line.Macro.OnShowForm = (values) =>
                    {
                        //set result and values
                        _waitForResult = false;
                    };
                    GSAKMacroHub.ShowForm(Line.Macro.ConnectionId, macroForm);
                    while (!Line.Macro._stopped && !Line.Macro._stopping && _waitForResult)
                    {
                        System.Threading.Thread.Sleep(100);
                    }

                    return result;
                }
            }

        }
    }
}
