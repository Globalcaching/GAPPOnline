using GAPPOnline.Services.Database;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GAPPOnline.Services
{
    public class AccountService
    {
        private static AccountService _uniqueInstance = null;
        private static object _lockObject = new object();

        private AccountService()
        {
        }

        public static AccountService Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new AccountService();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        public Models.Settings.User GetUserByUserGuid(string userGuid)
        {
            Models.Settings.User result = null;
            SettingsDatabaseService.Instance.Execute((db) =>
            {
                result = db.FirstOrDefault<Models.Settings.User>("where UserGuid = @0 COLLATE NOCASE", userGuid);
                if (result != null)
                {
                    result.SessionInfo = db.FirstOrDefault<Models.Settings.UserSessionInfo>("where UserId = @0", result.Id);
                    if (result.SessionInfo != null)
                    {
                        result.SessionInfo.UserGuid = result.UserGuid;
                    }
                }
            });
            return result;
        }

        public Models.Settings.User GetUser(string name)
        {
            Models.Settings.User result = null;
            SettingsDatabaseService.Instance.Execute((db) =>
            {
                result = db.FirstOrDefault<Models.Settings.User>("where Name = @0 COLLATE NOCASE", name);
                if (result!=null)
                {
                    result.SessionInfo = db.FirstOrDefault<Models.Settings.UserSessionInfo>("where UserId = @0", result.Id);
                }
                else if (result == null && name.ToLower() == "admin")
                {
                    result = new Models.Settings.User();
                    result.Name = "admin";
                    result.Password = StringToHash("admin");
                    result.IsAdmin = true;
                    result.MemberTypeId = 1;
                    result.PreferredLanguage = "en-US";
                    result.UserGuid = Guid.NewGuid().ToString("N");
                    db.Save(result);
                }
                if (result != null && result.SessionInfo == null)
                {
                    result.SessionInfo = new Models.Settings.UserSessionInfo();
                    result.SessionInfo.UserId = result.Id;
                    db.Save(result.SessionInfo);
                }
                if (result?.SessionInfo != null)
                {
                    result.SessionInfo.UserGuid = result.UserGuid;
                }
            });
            return result;
        }

        private string StringToHash(string s)
        {
            return s;
        }

        public async Task<bool> SignIn(HttpContext httpContext, string name, string password, bool rememberMe)
        {
            const string Issuer = "GAPPOnline";

            bool result = false;
            var usr = GetUser(name);
            if (usr != null)
            {
                var pwd = StringToHash(password);
                if (pwd == usr.Password)
                {
                    var claims = new List<Claim> {
                        new Claim(ClaimTypes.Name, usr.Name, ClaimValueTypes.String, Issuer),
                        new Claim(ClaimTypes.NameIdentifier, usr.Id.ToString(), ClaimValueTypes.Integer32, Issuer),
                        new Claim(ClaimTypes.Expiration, DateTime.UtcNow.AddMinutes(20).ToString("s"), ClaimValueTypes.Integer32, Issuer)
                    };
                    var userIdentity = new ClaimsIdentity(claims, "Passport");
                    var userPrincipal = new ClaimsPrincipal(userIdentity);

                    await httpContext.Authentication.SignInAsync("Cookie", userPrincipal,
                        new AuthenticationProperties
                        {
                            ExpiresUtc = DateTime.UtcNow.AddDays(20),
                            IsPersistent = rememberMe,
                            AllowRefresh = true
                        });
                    result = true;
                }
            }
            return result;
        }

        public async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            // Look for the last changed claim.
            string expTime;
            expTime = (from c in context.Principal.Claims
                           where c.Type == ClaimTypes.Expiration
                           select c.Value).FirstOrDefault();

            if (string.IsNullOrEmpty(expTime))
            {
                context.RejectPrincipal();
                await context.HttpContext.Authentication.SignOutAsync("Cookie");
            }
            else if (DateTime.UtcNow > DateTime.Parse(expTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime())
            {
                //todo: check
                //context.RejectPrincipal();
                //await context.HttpContext.Authentication.SignOutAsync("Cookie");
            }
        }

        public async Task SignOut(HttpContext httpContext)
        {
            await httpContext.Authentication.SignOutAsync("Cookie");
        }
    }
}
