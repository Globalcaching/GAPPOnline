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
        public class SystemVariableVersion : Variable
        {
            public SystemVariableVersion(Macro owner)
                :base(owner, "$_Version")
            {
                Value = new Version(8,2,1,0);
                Type = typeof(Version);
            }
        }
    }
}
