using AwesomeAssertions;
using MenuApi.Integration.Tests.Factory;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public class RecipeScopeIntegrationTests
{
    private readonly JsonSerializerOptions jsonOptions;
    private readonly ApiTestFixture fixture;

    public RecipeScopeIntegrationTests(ApiTestFixture fixture)
    {
        jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        this.fixture = fixture;
    }

    [Fact]
    public async Task ScopeMine_ExcludesRecipesOwnedByOtherUsers()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = await fixture.GetHttpClient();

        var myTitle = $"My Recipe {Guid.NewGuid()}";
        await PostRecipeAsync(client, myTitle, "Private");

        var otherOwnerId = await TestDatabaseSeeder.AddMenuUserAsync(fixture, $"other-user-{Guid.NewGuid()}", cancellationToken);
        var otherTitle = $"Someone Else's Recipe {Guid.NewGuid()}";
        await TestDatabaseSeeder.AddRecipeAsync(fixture, otherTitle, otherOwnerId, "Private", cancellationToken);

        var titles = await GetRecipeTitlesAsync(client, "mine");

        titles.Should().Contain(myTitle);
        titles.Should().NotContain(otherTitle);
    }

    [Fact]
    public async Task ScopeAuthenticated_ReturnsAuthenticatedScopedRecipesRegardlessOfOwner()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = await fixture.GetHttpClient();

        var myPrivateTitle = $"My Private Recipe {Guid.NewGuid()}";
        await PostRecipeAsync(client, myPrivateTitle, "Private");

        var mySharedTitle = $"My Shared Recipe {Guid.NewGuid()}";
        await PostRecipeAsync(client, mySharedTitle, "AuthenticatedUsers");

        var otherOwnerId = await TestDatabaseSeeder.AddMenuUserAsync(fixture, $"other-user-{Guid.NewGuid()}", cancellationToken);
        var otherSharedTitle = $"Someone Else's Shared Recipe {Guid.NewGuid()}";
        await TestDatabaseSeeder.AddRecipeAsync(fixture, otherSharedTitle, otherOwnerId, "AuthenticatedUsers", cancellationToken);

        var titles = await GetRecipeTitlesAsync(client, "authenticated");

        titles.Should().Contain(mySharedTitle);
        titles.Should().Contain(otherSharedTitle);
        titles.Should().NotContain(myPrivateTitle);
    }

    private async Task<HashSet<string>> GetRecipeTitlesAsync(HttpClient client, string scope)
    {
        using var response = await client.GetAsync($"/api/recipe?scope={scope}&take=200");
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        var data = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(data);

        return doc.RootElement.EnumerateArray()
            .Select(e => e.GetProperty("title").GetString()!)
            .ToHashSet();
    }

    private async Task PostRecipeAsync(HttpClient client, string title, string accessScope)
    {
        var body = new
        {
            Title = title,
            AccessScope = accessScope,
            Ingredients = new[] { new { SortOrder = 0, IngredientText = "Water", MeasureText = "1 cup", IsOptional = false } },
            Steps = Array.Empty<object>(),
        };

        using var content = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/recipe", content);
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }
}
