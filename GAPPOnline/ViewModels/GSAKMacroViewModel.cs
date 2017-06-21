using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GAPPOnline.ViewModels
{
    public class GSAKMacroViewModelItem : Models.Settings.GSAKMacro
    {
        public string UserName { get; set; }
    }

    public class GSAKMacroViewModel : BaseViewModel
    {
        public List<GSAKMacroViewModelItem> Items { get; set; }
        public long CurrentPage { get; set; }
        public long PageCount { get; set; }
        public long TotalCount { get; set; }
    }
}
