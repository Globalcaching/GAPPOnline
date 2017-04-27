using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GAPPOnline.Models.Localization
{
    [NPoco.TableName("LocalizationCulture")]
    [NPoco.PrimaryKey("Id")]
    public class LocalizationCulture
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}