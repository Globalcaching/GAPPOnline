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
    public class GSAKCacheListController : BaseController
    {
        [Authorize]
        public IActionResult Index()
        {
            return View(GetSessionInfo());
        }

        [Authorize]
        public ActionResult GetGeocaches(int page, int pageSize, List<string> filterColumns, List<string> filterValues, string sortOn, bool? sortAsc)
        {
            var filterCode = ControllerHelper.GetFilterValue(filterColumns, filterValues, "Code");
            var filterName = ControllerHelper.GetFilterValue(filterColumns, filterValues, "Name");
            sortOn = ControllerHelper.GetSortField(new Dictionary<string, string>()
            {
                { "name", "Name" }
                , { "code", "Code" }
            }, sortOn);
            return Json(GSAKDatabaseService.Instance.GetGSAKGeocaches(CurrentUser, page, Math.Min(100, pageSize)
                , filterCode: filterCode
                , filterName: filterName
                , sortOn: sortOn, sortAsc: sortAsc ?? true));
        }

        [HttpPost]
        [Authorize]
        public IActionResult SetUserFlag(string gcCode, bool value)
        {
            GSAKDatabaseService.Instance.SetUserFlag(CurrentUser, gcCode, value);
            return Json(null);
        }

        [HttpPost]
        [Authorize]
        public IActionResult SetActiveGeocache(string gcCode)
        {
            GSAKDatabaseService.Instance.SetActiveGeocache(CurrentUser, gcCode);
            return Json(null);
        }

    }
}
