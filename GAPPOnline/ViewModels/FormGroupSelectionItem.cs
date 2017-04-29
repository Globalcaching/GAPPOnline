using System.Collections.Generic;

namespace GAPPOnline.ViewModels
{
    public class FormGroupSelectionItem : FormGroupViewModelItem
    {
        public List<FormGroupListItem> Selection { get; set; }

        public FormGroupSelectionItem(string display, string fieldName)
        {
            this.FormGroupType = "FormGroupSelection";
            this.Display = display;
            this.FieldName = fieldName;
        }

        public FormGroupSelectionItem() { }
    }
}