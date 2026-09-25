using System.Security.Claims;
using AwesomeAssertions;
using FakeItEasy;
using MenuApi.Middleware;
using MenuApi.Services;
using MenuDB.Data;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace MenuApi.Tests.Middleware;

public class UserProvisioningMiddlewareTests
{
    [Fact]
    public async Task Reserved_Legacy_Owner_Is_Not_Provisioned_From_An_Authenticated_Claim()
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, LegacyRecipeOwner.AuthSubject)], "test")),
        };
        var menuUserService = A.Fake<IMenuUserService>();
        var nextCalled = false;
        var sut = new UserProvisioningMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await sut.InvokeAsync(context, menuUserService);

        nextCalled.Should().BeTrue();
        context.Items.Should().NotContainKey(MenuUserHttpContextKeys.MenuUserId);
        A.CallTo(() => menuUserService.ProvisionAsync(A<string>._, A<string?>._, A<string?>._, A<string?>._))
            .MustNotHaveHappened();
    }
}
