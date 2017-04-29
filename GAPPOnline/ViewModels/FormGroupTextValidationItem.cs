namespace GAPPOnline.ViewModels
{
    public class FormGroupTextValidationItem : FormGroupViewModelItem
    {
        public string CheckMethod { get; set; }
        public string BusyField { get; set; }
        public string InvalidField { get; set; }

        public FormGroupTextValidationItem(string display, string fieldName)
        {
            this.FormGroupType = "FormGroupTextValidation";
            this.Display = display;
            this.FieldName = fieldName;
        }

        public FormGroupTextValidationItem() { }
    }
}