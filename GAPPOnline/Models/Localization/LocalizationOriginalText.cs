using System;
using System.Collections.Generic;
using System.Linq;

namespace GAPPOnline.Models.Localization
{
    [NPoco.TableName("LocalizationOriginalText")]
    public class LocalizationOriginalText: AutoIdModel
    {
        public string OriginalText { get; set; }
    }
}