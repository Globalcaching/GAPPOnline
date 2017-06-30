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
        public class Variable : IDisposable
        {
            public Macro Owner { get; private set; }
            public string Name { get; private set; }

            public Type Type { get; set; } //todo check value against type
            public object Value { get; set; } //todo check value against type

            public Variable(Macro owner, string name)
            {
                Owner = owner;
                Name = name;
            }

            public void Dispose()
            {
            }
        }
    }
}
