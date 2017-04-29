namespace GAPPOnline.ViewModels
{
    public class FormGroupDateTimeItem : FormGroupViewModelItem
    {
        public string SelectMethod { get; set; }
        public string InputId { get; set; }

        public FormGroupDateTimeItem(string display, string fieldName, string inputId, string selectMethod)
        {
            this.FormGroupType = "FormGroupDateTime";
            this.Display = display;
            this.FieldName = fieldName;
            this.InputId = inputId;
            this.SelectMethod = selectMethod;
        }

        public FormGroupDateTimeItem() { }
    }
}