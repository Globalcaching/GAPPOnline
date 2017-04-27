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
            return View();
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

        public ActionResult GetGSAKDatabases(int page, int pageSize, List<string> filterColumns, List<string> filterValues, string sortOn, bool? sortAsc)
        {
            var filterName = ControllerHelper.GetFilterValue(filterColumns, filterValues, "Name");
            var filterDescription = ControllerHelper.GetFilterValue(filterColumns, filterValues, "Description");
            sortOn = ControllerHelper.GetSortField(new Dictionary<string, string>()
            {
                { "name", "Name" }
                , { "description", "Description" }
                , { "created", "CreatedAt" }
            }, sortOn);
            long? userId = null;
            if (!CurrentUser.IsAdmin)
            {
                userId = CurrentUser.Id;
            }
            return Json(GSAKDatabaseService.Instance.GetGSAKDatabases(page, pageSize, userId: userId, filterName: filterName, sortOn: sortOn, sortAsc: sortAsc ?? true));
        }

    }
}
