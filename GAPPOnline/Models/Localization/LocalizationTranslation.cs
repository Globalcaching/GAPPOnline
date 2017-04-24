using System;
using System.Collections.Generic;
using System.Linq;

namespace GAPPOnline.Models.Localization
{
    [NPoco.TableName("LocalizationTranslation")]
    public class LocalizationTranslation: AutoIdModel
    {
        public long LocalizationCultureId { get; set; }
        public long LocalizationOriginalTextId { get; set; }
        public string TranslatedText { get; set; }
    }
}