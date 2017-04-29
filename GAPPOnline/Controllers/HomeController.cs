using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GAPPOnline.Helpers;
using GAPPOnline.Services;

namespace GAPPOnline.Controllers
{
    public class HomeController : BaseController
    {
        [Authorize]
        public IActionResult Index()
        {
            return View(GetSessionInfo());
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        [Authorize]
        public ActionResult GetGSAKDatabases(int page, int pageSize, List<string> filterColumns, List<string> filterValues, string sortOn, bool? sortAsc)
        {
            var filterName = ControllerHelper.GetFilterValue(filterColumns, filterValues, "Name");
            var filterDescription = ControllerHelper.GetFilterValue(filterColumns, filterValues, "Description");
            var filterUserName = ControllerHelper.GetFilterValue(filterColumns, filterValues, "User");
            sortOn = ControllerHelper.GetSortField(new Dictionary<string, string>()
            {
                { "name", "Name" }
                , { "user", "UserName" }
                , { "description", "Description" }
                , { "created", "CreatedAt" }
            }, sortOn);
            long? userId = null;
            if (!CurrentUser.IsAdmin)
            {
                userId = CurrentUser.Id;
            }
            return Json(GSAKDatabaseService.Instance.GetGSAKDatabases(CurrentUser, page, Math.Min(100, pageSize), userId: userId, filterUserName: filterUserName, filterName: filterName, sortOn: sortOn, sortAsc: sortAsc ?? true));
        }

        [HttpPost]
        [Authorize]
        public ActionResult SaveGSAKDatabase(Models.Settings.GSAKDatabase item)
        {
            GSAKDatabaseService.Instance.SaveGSAKDatabase(CurrentUser, item);
            return Json(null);
        }

        [HttpPost]
        [Authorize]
        public ActionResult CheckGSAKDatabaseName(Models.Settings.GSAKDatabase item)
        {
            return Json(GSAKDatabaseService.Instance.CheckGSAKDatabaseName(CurrentUser, item));
        }

        [HttpPost]
        [Authorize]
        public ActionResult SelectGSAKDatabase(Models.Settings.GSAKDatabase item)
        {
            GSAKDatabaseService.Instance.SelectGSAKDatabase(CurrentUser, item);
            return Json(null);
        }

        [HttpPost]
        [Authorize]
        public ActionResult DeleteGSAKDatabase(Models.Settings.GSAKDatabase item)
        {
            return Json(null);
        }

        [HttpPost]
        [Authorize]
        public ActionResult CloneGSAKDatabase(Models.Settings.GSAKDatabase item)
        {
            return Json(null);
        }

    }
}
