using GAPPOnline.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

public static class HtmlHelperExtension
{
    public static string T(this IHtmlHelper html, string key)
    {
        return html.Encode(LocalizationService.Instance[key]);
    }

    public static string T(this HtmlHelper html, string key, params object[] args)
    {
        return html.Encode(string.Format(LocalizationService.Instance[key], args));
    }
}

