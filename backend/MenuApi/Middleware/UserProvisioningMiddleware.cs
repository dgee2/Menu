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
            var authSubject = Truncate(context.User.FindFirstValue(ClaimTypes.NameIdentifier), 256);

            if (authSubject is not null)
            {
                // Pass null when the claim is absent so the repository does not
                // overwrite existing profile data with a missing claim value.
                var displayName = Truncate(context.User.FindFirstValue("name"), 100);
                var email = Truncate(
                    context.User.FindFirstValue(ClaimTypes.Email) ?? context.User.FindFirstValue("email"),
                    256);
                var avatarUrl = Truncate(context.User.FindFirstValue("picture"), 512);

                var menuUserId = await menuUserService
                    .ProvisionAsync(authSubject, displayName, email, avatarUrl)
                    .ConfigureAwait(false);

                context.Items[MenuUserHttpContextKeys.MenuUserId] = menuUserId;
            }
        }

        await next(context).ConfigureAwait(false);
    }

    private static string? Truncate(string? value, int maxLength) =>
        value is null || value.Length <= maxLength ? value : value[..maxLength];
}
