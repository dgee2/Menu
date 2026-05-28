using System.Security.Claims;
using MenuApi.Services;
using MenuApi.ValueObjects;

namespace MenuApi.Middleware;

public static class MenuUserHttpContextKeys
{
    public const string MenuUserId = "MenuUserId";
}

public class UserProvisioningMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IMenuUserService menuUserService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var authSubject = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (authSubject is not null)
            {
                var displayName = context.User.FindFirstValue("name") ?? authSubject;
                var email = context.User.FindFirstValue(ClaimTypes.Email)
                    ?? context.User.FindFirstValue("email");
                var avatarUrl = context.User.FindFirstValue("picture");

                var menuUserId = await menuUserService
                    .ProvisionAsync(authSubject, displayName, email, avatarUrl)
                    .ConfigureAwait(false);

                context.Items[MenuUserHttpContextKeys.MenuUserId] = menuUserId;
            }
        }

        await next(context).ConfigureAwait(false);
    }
}
