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
        public class MacroFormElement
        {
            public string ControlType { get; set; }
            public string Name { get; set; }
            public string Container { get; set; }
            public int Height { get; set; }
            public int Width { get; set; }
            public int Top { get; set; }
            public int Left { get; set; }
            public int Taborder { get; set; }
        }
    }
}
