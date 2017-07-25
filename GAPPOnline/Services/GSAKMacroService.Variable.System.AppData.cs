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
        public class SystemVariableAppData : SystemVariable
        {
            public SystemVariableAppData(Macro owner)
                :base(owner, "$_AppData")
            {
                Value = Path.Combine(Startup.HostingEnvironment.ContentRootPath, "App_Data", $"User_{owner.User.Id}");
                Type = typeof(string);
            }
        }
    }
}
