using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GAPPOnline.ViewModels
{
    public class FormGroupViewModel
    {
        public string NgApp { get; set; }
        public string NgController { get; set; }
        public string NgSaveDisabled { get; set; }
        public string ControllerId { get; set; }
        public string DialogId { get; set; }
        public string Title { get; set; }
        public string SaveFunction { get; set; }
        public List<FormGroupViewModelItem> Items { get; set; }
        public List<FormGroupViewModelButton> Buttons { get; set; }
    }
}