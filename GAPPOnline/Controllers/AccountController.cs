using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Authentication;
using GAPPOnline.ViewModels;
using GAPPOnline.Services;

namespace GAPPOnline.Controllers
{
    public class AccountController : BaseController
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            return RedirectToAction("SignIn", new { returnUrl = nameof(HomeController.Index) });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult SignIn(string returnUrl = null)
        {
            var m = new SignInViewModel();
            m.ReturnUrl = returnUrl;
            return View(m);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SignIn(SignInViewModel m)
        {
            if (ModelState.IsValid)
            {
                if (await AccountService.Instance.SignIn(HttpContext, m.Name, m.Password, m.RememberMe))
                {
                    //return Redirect(m.ReturnUrl);
                    return Redirect("/");
                }
            }
            // If we got this far, something failed, redisplay form
            return View(m);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SignOut()
        {
            await AccountService.Instance.SignOut(HttpContext);
            return RedirectToAction("SignIn", new { returnUrl = nameof(HomeController.Index) });
        }

    }
}