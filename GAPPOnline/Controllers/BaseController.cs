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
        protected Models.Settings.User CurrentUser { get; set; }

        public BaseController()
        {
            var userName = System.Web.HttpContext.Current.User.Identity.Name;
            if (!string.IsNullOrEmpty(userName))
            {
                CurrentUser = AccountService.Instance.GetUser(userName);
                if (CurrentUser != null)
                {
                    LocalizationService.Instance.CurrentCulture = LocalizationService.Instance.GetLocalizationCulture(CurrentUser.PreferredLanguage);
                    LocalizationService.Instance.CurrentCultureInfo = new System.Globalization.CultureInfo(CurrentUser.PreferredLanguage);
                }
            }
            LocalizationService.Instance.Initialize();
        }

        protected SessionInfoViewModel GetSessionInfo()
        {
            var result = new SessionInfoViewModel();
            GetSessionInfo(ref result);
            return result;
        }

        protected void GetSessionInfo(ref SessionInfoViewModel sim)
        {
            sim.UserGuid = CurrentUser.UserGuid;
            sim.UserName = CurrentUser.Name;
            sim.SelectedGSAKDatabase = CurrentUser.SessionInfo.SelectedGSAKDatabaseId;
            sim.ActiveGCCode = CurrentUser.SessionInfo.ActiveGCCode;
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
