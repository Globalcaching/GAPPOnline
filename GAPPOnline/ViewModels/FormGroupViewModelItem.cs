using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GAPPOnline.ViewModels
{
    public class FormGroupViewModelItem
    {
        public string FormGroupType { get; set; }
        public string Display { get; set; }
        public string PlaceHolder { get; set; }
        public string FieldName { get; set; }
        public int MaxLength { get; set; } = -1;
        public string FieldAttributes { get; set; }
        public string FormGroupAttributes { get; set; }

        public FormGroupViewModelItem(string formGroupType, string display, string fieldName)
        {
            this.FormGroupType = formGroupType;
            this.Display = display;
            this.FieldName = fieldName;
        }

        public FormGroupViewModelItem() { }
    }
}