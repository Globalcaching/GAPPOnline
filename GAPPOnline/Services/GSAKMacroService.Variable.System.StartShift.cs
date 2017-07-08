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
        public class SystemVariableStartShift : Variable
        {
            public SystemVariableStartShift(Macro owner)
                :base(owner, "$_StartShift")
            {
                Value = false;
                Type = typeof(bool);
            }
        }
    }
}
