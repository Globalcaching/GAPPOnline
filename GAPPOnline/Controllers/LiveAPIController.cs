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
    public class LiveAPIController : BaseController
    {
        [Authorize]
        public IActionResult Index()
        {
            return View(GetSessionInfo());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetYourUserProfile(string token)
        {
            var result = await LiveAPIService.Instance.GetYourUserProfile(CurrentUser, token);
            return Json(result);
        }
    }
}
