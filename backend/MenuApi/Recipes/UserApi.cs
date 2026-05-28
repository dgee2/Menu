using MenuApi.Middleware;
using MenuApi.ValueObjects;

namespace MenuApi.Recipes;

public static class UserApi
{
    public static RouteGroupBuilder MapUsers(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/user");

        group.WithTags("Users");

        group.MapGet("/me", GetCurrentUserAsync)
            .Produces<int>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static IResult GetCurrentUserAsync(HttpContext httpContext)
    {
        if (httpContext.Items[MenuUserHttpContextKeys.MenuUserId] is MenuUserId menuUserId)
        {
            return Results.Ok(menuUserId.Value);
        }

        return Results.Unauthorized();
    }
}
