using System.Collections.Generic;

namespace GAPPOnline.ViewModels
{
    public class GSAKGeocacheViewModelItem : Models.GSAK.GSAKCaches
    {
        public bool CanDelete { get; set; }
        public bool CanEdit { get; set; }
        public bool CanClone { get; set; }
        public bool CanSelect { get; set; }
    }

    public class GSAKGeocacheViewModel : BaseViewModel
    {
        public List<GSAKGeocacheViewModelItem> Items { get; set; }
        public long CurrentPage { get; set; }
        public long PageCount { get; set; }
        public long TotalCount { get; set; }
    }
}