using System.Collections.Generic;

namespace GAPPOnline.ViewModels
{
    public class GSAKDatabaseViewModelItem : Models.Settings.GSAKDatabase
    {
        public string UserName { get; set; }
    }

    public class GSAKDatabaseViewModel : BaseViewModel
    {
        public List<GSAKDatabaseViewModelItem> Items { get; set; }
        public long CurrentPage { get; set; }
        public long PageCount { get; set; }
        public long TotalCount { get; set; }
    }
}