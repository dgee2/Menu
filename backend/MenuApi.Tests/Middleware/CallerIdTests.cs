#nullable enable

using AwesomeAssertions;
using MenuApi.Middleware;
using MenuApi.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;

namespace MenuApi.Tests.Middleware;

public class CallerIdTests
{
    [Theory, CustomAutoData]
    public async Task BindAsync_WithProvisionedUser_BindsTheId(MenuUserId menuUserId)
    {
        var httpContext = CreateHttpContext(menuUserId);

        var caller = await CallerId.BindAsync(httpContext);

        caller.Value.Should().Be(menuUserId);
    }

    [Fact]
    public async Task BindAsync_WithoutProvisionedUser_BindsEmpty()
    {
        // Binding runs before endpoint filters, so it must succeed even with no caller - otherwise
        // the framework would answer 400 and RequireCallerFilter would never get to say 401.
        var caller = await CallerId.BindAsync(CreateHttpContext(null));

        caller.MenuUserId.Should().BeNull();
    }

    [Fact]
    public void Value_WithoutProvisionedUser_Throws()
    {
        var caller = new CallerId(null);

        var act = () => caller.Value;

        act.Should().Throw<InvalidOperationException>().WithMessage("*RequireCallerFilter*");
    }

    [Theory, CustomAutoData]
    public async Task RequireCallerFilter_WithProvisionedUser_CallsTheEndpoint(MenuUserId menuUserId)
    {
        var filter = new RequireCallerFilter();
        var called = false;

        var result = await filter.InvokeAsync(
            CreateFilterContext(menuUserId),
            _ => { called = true; return ValueTask.FromResult<object?>(Results.NoContent()); });

        called.Should().BeTrue();
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task RequireCallerFilter_WithoutProvisionedUser_Returns401()
    {
        var filter = new RequireCallerFilter();
        var called = false;

        var result = await filter.InvokeAsync(
            CreateFilterContext(null),
            _ => { called = true; return ValueTask.FromResult<object?>(Results.NoContent()); });

        called.Should().BeFalse();
        result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    private static HttpContext CreateHttpContext(MenuUserId? menuUserId)
    {
        var httpContext = new DefaultHttpContext();
        if (menuUserId is not null)
        {
            httpContext.Items[MenuUserHttpContextKeys.MenuUserId] = menuUserId.Value;
        }

        return httpContext;
    }

    private static EndpointFilterInvocationContext CreateFilterContext(MenuUserId? menuUserId) =>
        EndpointFilterInvocationContext.Create(CreateHttpContext(menuUserId));
}
