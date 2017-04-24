using GAPPOnline.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GAPPOnline.ViewModels
{
    public class BaseViewModel
    {
        public NotificationService.Messages NotificationMessages { get; set; }
    }
}
