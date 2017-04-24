using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class UrlHelperExtension
{
    public static string MakeActiveClass(this IUrlHelper urlHelper, string controller)
    {
        var result = "active";
        var controllerName = urlHelper.ActionContext.RouteData.Values["controller"].ToString();
        var actionName = urlHelper.ActionContext.RouteData.Values["action"].ToString();

        if (!controllerName.Equals(controller, StringComparison.OrdinalIgnoreCase))
        {
            result = null;
        }

        if (result == null)
        {
            if (actionName != null && actionName.Equals(controller, StringComparison.OrdinalIgnoreCase))
            {
                result = "submenu-active";
            }
        }
        return result;
    }

    public static string MakeActiveClass(this IUrlHelper urlHelper, string[] controllers)
    {
        var result = "active";

        foreach (string controller in controllers)
        {
            result = urlHelper.MakeActiveClass(controller);

            if (result == "active" || result == "submenu-active")
                break;
        }

        return result;
    }
}

