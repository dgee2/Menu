using MenuApi.ValueObjects;

namespace MenuApi.Middleware;

/// <summary>
/// The provisioned <see cref="MenuUserId"/> for the current request, bound straight into endpoint
/// handlers so they do not each repeat the "is it there? no? 401" dance.
/// </summary>
/// <remarks>
/// Binding runs before endpoint filters, so this always binds and <see cref="RequireCallerFilter"/>
/// is what turns a missing caller into a 401. Endpoints taking a <see cref="CallerId"/> must add
/// that filter; <see cref="Value"/> throws if they forget, rather than silently acting as nobody.
/// </remarks>
public readonly record struct CallerId(MenuUserId? MenuUserId)
{
    /// <summary>The caller's id.</summary>
    /// <exception cref="InvalidOperationException">The endpoint is missing <see cref="RequireCallerFilter"/>.</exception>
    public MenuUserId Value => MenuUserId
        ?? throw new InvalidOperationException(
            $"No caller id on the request. Add {nameof(RequireCallerFilter)} to any endpoint taking a {nameof(CallerId)}.");

    public static ValueTask<CallerId> BindAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var menuUserId = context.Items[MenuUserHttpContextKeys.MenuUserId] as MenuUserId?;
        return ValueTask.FromResult(new CallerId(menuUserId));
    }
}

/// <summary>Rejects requests that reached an endpoint without a provisioned Menu user.</summary>
public sealed class RequireCallerFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (context.HttpContext.Items[MenuUserHttpContextKeys.MenuUserId] is not MenuUserId)
        {
            return Results.Unauthorized();
        }

        return await next(context).ConfigureAwait(false);
    }
}
