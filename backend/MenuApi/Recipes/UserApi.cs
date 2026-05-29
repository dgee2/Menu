using MenuApi.Middleware;
using MenuApi.Services;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;

namespace MenuApi.Recipes;

public static class UserApi
{
    public static RouteGroupBuilder MapUsers(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/user");

        group.WithTags("Users");

        group.MapGet("/me", GetCurrentUserAsync)
            .Produces<UserProfile>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> GetCurrentUserAsync(HttpContext httpContext, IMenuUserService menuUserService)
    {
        if (httpContext.Items[MenuUserHttpContextKeys.MenuUserId] is not MenuUserId menuUserId)
        {
            return Results.Unauthorized();
        }

        var profile = await menuUserService.GetCurrentUserAsync(menuUserId).ConfigureAwait(false);

        return profile is not null ? Results.Ok(profile) : Results.Unauthorized();
    }
}
