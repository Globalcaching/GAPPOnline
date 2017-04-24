using System.Collections.Generic;

namespace GAPPOnline.ViewModels
{
    public class LocalizationCultureViewModelItem: Models.Localization.LocalizationCulture
    {
        public bool CanDelete { get; set; }
        public bool CanEdit { get; set; }
        public bool CanClone { get; set; }
        public bool CanSelect { get; set; }
    }

    public class LocalizationCultureViewModel : BaseViewModel
    {
        public List<LocalizationCultureViewModelItem> Items { get; set; }
        public long CurrentPage { get; set; }
        public long PageCount { get; set; }
        public long TotalCount { get; set; }
    }
}