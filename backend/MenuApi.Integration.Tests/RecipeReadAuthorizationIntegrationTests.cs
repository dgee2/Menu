using AwesomeAssertions;
using MenuApi.Integration.Tests.Factory;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public class RecipeReadAuthorizationIntegrationTests
{
    private readonly JsonSerializerOptions jsonOptions;
    private readonly ApiTestFixture fixture;

    public RecipeReadAuthorizationIntegrationTests(ApiTestFixture fixture)
    {
        jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        this.fixture = fixture;
    }

    [Fact]
    public async Task Get_SomeoneElsesPrivateRecipe_Returns404()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = await fixture.GetHttpClient();

        var otherOwnerId = await TestDatabaseSeeder.AddMenuUserAsync(fixture, $"other-user-{Guid.NewGuid()}", cancellationToken);
        var recipeId = await TestDatabaseSeeder.AddRecipeAsync(
            fixture, $"Someone Else's Private Recipe {Guid.NewGuid()}", otherOwnerId, "Private", cancellationToken);

        using var response = await client.GetAsync($"/api/recipe/{recipeId}");

        // 404 rather than 403: with sequential integer ids, a 403 would let anyone enumerate which
        // ids exist and confirm the presence of other people's private recipes.
        await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetIngredients_SomeoneElsesPrivateRecipe_ReturnsNothing()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = await fixture.GetHttpClient();

        var otherOwnerId = await TestDatabaseSeeder.AddMenuUserAsync(fixture, $"other-user-{Guid.NewGuid()}", cancellationToken);
        var recipeId = await TestDatabaseSeeder.AddRecipeAsync(
            fixture, $"Someone Else's Private Recipe {Guid.NewGuid()}", otherOwnerId, "Private", cancellationToken);

        using var response = await client.GetAsync($"/api/recipe/{recipeId}/ingredient");

        await response.ShouldHaveStatusCode(HttpStatusCode.OK);
        var data = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(data);
        doc.RootElement.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task Get_SomeoneElsesSharedRecipe_IsReadableButNotEditable()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = await fixture.GetHttpClient();

        var otherOwnerId = await TestDatabaseSeeder.AddMenuUserAsync(fixture, $"other-user-{Guid.NewGuid()}", cancellationToken);
        var recipeId = await TestDatabaseSeeder.AddRecipeAsync(
            fixture, $"Someone Else's Shared Recipe {Guid.NewGuid()}", otherOwnerId, "AuthenticatedUsers", cancellationToken);

        using var response = await client.GetAsync($"/api/recipe/{recipeId}");

        await response.ShouldHaveStatusCode(HttpStatusCode.OK);
        var data = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(data);
        doc.RootElement.GetProperty("canEdit").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("canDelete").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Get_OwnRecipe_IsEditableAndSerialisesAccessScopeByName()
    {
        using var client = await fixture.GetHttpClient();

        var recipeId = await PostRecipeAsync(client, $"My Recipe {Guid.NewGuid()}", "Private");

        using var response = await client.GetAsync($"/api/recipe/{recipeId}");
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);
        var data = await response.Content.ReadAsStringAsync();

        // The lookup table's integer id must never reach the wire - the name is the contract.
        data.Should().Contain("\"accessScope\":\"Private\"");

        using var doc = JsonDocument.Parse(data);
        doc.RootElement.GetProperty("canEdit").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("canDelete").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task Get_UnknownScope_ReturnsValidationProblemKeyedOnScope()
    {
        using var client = await fixture.GetHttpClient();

        using var response = await client.GetAsync("/api/recipe?scope=everyone");

        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
        var data = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(data);
        doc.RootElement.GetProperty("errors").TryGetProperty("scope", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Get_TotalTime_IsDerivedWhenNotSetExplicitly()
    {
        using var client = await fixture.GetHttpClient();

        var body = new
        {
            Title = $"Derived Total {Guid.NewGuid()}",
            AccessScope = "Private",
            PrepTimeMinutes = 20,
            CookTimeMinutes = 30,
            Ingredients = Array.Empty<object>(),
            Steps = Array.Empty<object>(),
        };
        using var content = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, "application/json");
        using var createResponse = await client.PostAsync("/api/recipe", content);
        await createResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        var data = await createResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(data);

        doc.RootElement.GetProperty("totalTimeMinutes").ValueKind.Should().Be(JsonValueKind.Null);
        doc.RootElement.GetProperty("effectiveTotalTimeMinutes").GetInt32().Should().Be(50);
    }

    private async Task<int> PostRecipeAsync(HttpClient client, string title, string accessScope)
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

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        return doc.RootElement.GetProperty("id").GetInt32();
    }
}
