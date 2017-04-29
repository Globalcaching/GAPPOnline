namespace GAPPOnline.ViewModels
{
    public class FormGroupTextItem : FormGroupViewModelItem
    {
        public string RowCount { get; set; }

        public FormGroupTextItem(string display, string fieldName)
        {
            this.FormGroupType = "FormGroupText";
            this.Display = display;
            this.FieldName = fieldName;
        }

        public FormGroupTextItem() { }
    }
}