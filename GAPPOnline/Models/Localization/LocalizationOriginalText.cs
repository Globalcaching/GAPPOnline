using System;
using System.Collections.Generic;
using System.Linq;

namespace GAPPOnline.Models.Localization
{
    [NPoco.TableName("LocalizationOriginalText")]
    [NPoco.PrimaryKey("Id")]
    public class LocalizationOriginalText
    {
        public long Id { get; set; }
        public string OriginalText { get; set; }
    }
}