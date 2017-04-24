
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace System.Web
{
    /// <summary>
    /// Making System.Web.HttpContext.Current available again, for easy migration from .net 4.6 to .net core.
    /// This is not a advised design pattern. So it should be better to use HttpContext.Current as less as possible, and gradually remove it from the project.
    /// A better approach should be dependency injection.
    /// </summary>
    public static class HttpContext
    {
        private static IHttpContextAccessor _contextAccessor;

        public static Microsoft.AspNetCore.Http.HttpContext Current => _contextAccessor.HttpContext;

        internal static void Configure(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
    }
}

public static class HttpContextExtension
{
    public static void AddHttpContextAccessor(this IServiceCollection services)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    }

    public static IApplicationBuilder UseStaticHttpContext(this IApplicationBuilder app)
    {
        var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
        System.Web.HttpContext.Configure(httpContextAccessor);
        return app;
    }

}



