using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GAPPOnline.Services
{
    public class BaseService
    {
        protected string _T(string key, params object[] args)
        {
            return string.Format(LocalizationService.Instance[key], args);
        }
    }
}
