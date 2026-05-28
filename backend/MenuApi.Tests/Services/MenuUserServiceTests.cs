using AwesomeAssertions;
using FakeItEasy;
using MenuApi.Repositories;
using MenuApi.Services;
using MenuApi.ValueObjects;
using Xunit;

namespace MenuApi.Tests.Services;

public class MenuUserServiceTests
{
    private readonly IMenuUserRepository repository;
    private readonly MenuUserService sut;

    public MenuUserServiceTests()
    {
        repository = A.Fake<IMenuUserRepository>();
        sut = new MenuUserService(repository);
    }

    [Fact]
    public async Task ProvisionAsync_CallsRepository_WithCorrectArguments()
    {
        var expected = MenuUserId.From(42);
        A.CallTo(() => repository.UpsertAsync("auth0|123", "Alice", "alice@example.com", "https://example.com/pic.png"))
            .Returns(expected);

        var result = await sut.ProvisionAsync("auth0|123", "Alice", "alice@example.com", "https://example.com/pic.png");

        result.Should().Be(expected);
        A.CallTo(() => repository.UpsertAsync("auth0|123", "Alice", "alice@example.com", "https://example.com/pic.png"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ProvisionAsync_CallsRepository_WithNullDisplayNameAndOptionalFields()
    {
        var expected = MenuUserId.From(7);
        A.CallTo(() => repository.UpsertAsync("auth0|456", null, null, null))
            .Returns(expected);

        var result = await sut.ProvisionAsync("auth0|456", null, null, null);

        result.Should().Be(expected);
    }
}
