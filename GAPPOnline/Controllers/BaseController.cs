using GAPPOnline.Services;
using GAPPOnline.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GAPPOnline.Controllers
{
    public class BaseController : Controller
    {
        public BaseController()
        {
            var userName = System.Web.HttpContext.Current.User.Identity.Name;
            if (!string.IsNullOrEmpty(userName))
            {
                var user = AccountService.Instance.GetUser(userName);
                if (user != null)
                {
                    LocalizationService.Instance.CurrentCulture = LocalizationService.Instance.GetLocalizationCulture(user.PreferredLanguage);
                    LocalizationService.Instance.CurrentCultureInfo = new System.Globalization.CultureInfo(user.PreferredLanguage);
                }
            }
            LocalizationService.Instance.Initialize();
        }


        protected string _T(string key, params object[] args)
        {
            return string.Format(LocalizationService.Instance[key], args);
        }

        public override JsonResult Json(object data) // string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            if (data == null)
            {
                data = new BaseViewModel();
            }
            if (data is BaseViewModel)
            {
                (data as BaseViewModel).NotificationMessages = NotificationService.Instance.GetMessages(HttpContext);
            }

            return base.Json(data);
        }
    }

}
