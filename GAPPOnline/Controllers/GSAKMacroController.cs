using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GAPPOnline.Helpers;
using GAPPOnline.Services;
using System.ServiceModel.Channels;
using System.Xml;
using System.ServiceModel;

namespace GAPPOnline.Controllers
{
    public class GSAKMacroController : BaseController
    {
        [Authorize]
        public IActionResult Index()
        {
            return View(GetSessionInfo());
        }

        [Authorize]
        public IActionResult Debug()
        {
            return View(GetSessionInfo());
        }

        [HttpPost]
        [Authorize]
        public ActionResult GetGSAKMacros(int page, int pageSize, List<string> filterColumns, List<string> filterValues, string sortOn, bool? sortAsc)
        {
            sortOn = ControllerHelper.GetSortField(new Dictionary<string, string>()
            {
                { "name", "FileName" }
                , { "last run", "LastRun" }
            }, sortOn);
            return Json(GSAKMacroService.Instance.GetGSAKMacros(CurrentUser, page, Math.Min(100, pageSize), sortOn: sortOn, sortAsc: sortAsc ?? true));
        }

        [HttpPost]
        public ActionResult UploadGSAKMacro(string filename)
        {
            try
            {
                using (var tmpFile = new TemporaryFile(true))
                {
                    FileHandlingService.Instance.UploadFiles(HttpContext.Request, System.IO.Path.GetDirectoryName(tmpFile.Path), System.IO.Path.GetFileName(tmpFile.Path), maxFileSize: 200*1024);
                    GSAKMacroService.Instance.InstallMacro(CurrentUser, tmpFile.Path, filename);
                }
            }
            catch
            {
            }
            return Json(null);
        }

    }
}
