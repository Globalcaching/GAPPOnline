using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using GAPPOnline.Services;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.AspNetCore.SignalR.Infrastructure;

namespace GAPPOnline
{
    public class Startup
    {
        public static IConnectionManager ConnectionManager;
        public static IHostingEnvironment HostingEnvironment { private set; get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("hosting.json", optional: true)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public static IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                    options.LoginPath = new PathString("/Account/SignIn");
                    options.AccessDeniedPath = new PathString("/Account/SignIn");
                    options.Events = new CookieAuthenticationEvents
                    {
                        // Set other options
                        OnValidatePrincipal = AccountService.Instance.ValidateAsync
                    };
            });
            //services.AddAuthentication(options =>
            //{
            //    options.SignInScheme = "Cookie";
            //});

            services.AddAuthorization();

            // Add framework services.
            services.AddMvc().AddJsonOptions(opt =>
            {
                var resolver = opt.SerializerSettings.ContractResolver;
                if (resolver != null)
                {
                    var res = resolver as DefaultContractResolver;
                    res.NamingStrategy = null;  // <<!-- this removes the camelcasing
                }
                opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });


            services.AddSignalR(options => options.Hubs.EnableDetailedErrors = true);

            //services.AddDistributedMemoryCache();
            services.AddSession();

            services.Configure<IISOptions>(options => {
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            HostingEnvironment = env;

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseSession();
            app.UseAuthentication();
            //app.UseCookieAuthentication(new CookieAuthenticationOptions()
            //{
            //    AuthenticationScheme = "Cookie",
            //    LoginPath = new PathString("/Account/SignIn"),
            //    AccessDeniedPath = new PathString("/Account/SignIn"),
            //    AutomaticAuthenticate = true,
            //    AutomaticChallenge = true,
            //    Events = new CookieAuthenticationEvents
            //    {
            //        // Set other options
            //        OnValidatePrincipal = AccountService.Instance.ValidateAsync
            //    }
            //});
            app.UseStaticHttpContext();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseWebSockets();
            app.UseSignalR();

            AccountService.Instance.GetUser("admin");
            ConnectionManager = serviceProvider.GetService<IConnectionManager>();
        }
    }
}
