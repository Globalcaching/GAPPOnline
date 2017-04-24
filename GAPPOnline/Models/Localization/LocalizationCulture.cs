using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GAPPOnline.Models.Localization
{
    [NPoco.TableName("LocalizationCulture")]
    public class LocalizationCulture: AutoIdModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}