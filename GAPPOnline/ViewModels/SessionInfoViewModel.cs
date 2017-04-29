using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GAPPOnline.ViewModels
{
    public class SessionInfoViewModel
    {
        public string UserGuid { get; set; }
        public string UserName { get; set; }
        public long? SelectedGSAKDatabase { get; set; }
        public string ActiveGCCode { get; set; }
    }
}
