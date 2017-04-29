namespace GAPPOnline.ViewModels
{
    public class FormGroupSelectItem : FormGroupViewModelItem
    {
        public string SelectMethod { get; set; }
        public string ClearMethod { get; set; }
        public string SelectButtonId { get; set; }
        public string ClearButtonId { get; set; }

        public FormGroupSelectItem(string display, string fieldName)
        {
            this.FormGroupType = "FormGroupSelect";
            this.Display = display;
            this.FieldName = fieldName;
        }

        public FormGroupSelectItem() { }
    }
}