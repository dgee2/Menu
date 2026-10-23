using Microsoft.AspNetCore.Builder;
using OwaspHeaders.Core.Extensions;
using OwaspHeaders.Core.Models;

namespace Microsoft.Extensions.Hosting;

public static class SecurityHeadersExtensions
{
    public static WebApplication UseMenuSecurityHeaders(this WebApplication app)
    {
        // Swagger UI uses inline scripts, so apply the default CSP only to API responses.
        var browserUiHeaders = SecureHeadersMiddlewareBuilder
            .CreateBuilder()
            .UseHsts()
            .UseXFrameOptions()
            .UseContentTypeOptions()
            .UsePermittedCrossDomainPolicies()
            .UseReferrerPolicy()
            .UseCacheControl()
            .UseXssProtection()
            .UseCrossOriginResourcePolicy()
            .UseCrossOriginOpenerPolicy()
            .UseCrossOriginEmbedderPolicy()
            .Build();

        app.UseWhen(
            context => context.Request.Path.StartsWithSegments("/api"),
            apiApp => apiApp.UseSecureHeadersMiddleware());
        app.UseWhen(
            context => !context.Request.Path.StartsWithSegments("/api"),
            uiApp => uiApp.UseSecureHeadersMiddleware(browserUiHeaders));

        return app;
    }
}
