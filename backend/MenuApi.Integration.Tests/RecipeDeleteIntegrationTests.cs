using AwesomeAssertions;
using MenuApi.Integration.Tests.Factory;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public class RecipeDeleteIntegrationTests
{
    private const string JsonMediaType = "application/json";
    private const string PrivateAccessScope = "Private";
    private const string RecipeEndpoint = "/api/recipe";

    private readonly JsonSerializerOptions jsonOptions;
    private readonly ApiTestFixture fixture;

    public RecipeDeleteIntegrationTests(ApiTestFixture fixture)
    {
        jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        this.fixture = fixture;
    }

    [Theory]
    [InlineData("Recipe To Delete")]
    public async Task Delete_Recipe_As_Owner_Hides_Recipe_And_Retains_Content(string recipeTitle)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = await fixture.GetHttpClient();

        var body = new
        {
            Title = recipeTitle,
            AccessScope = PrivateAccessScope,
            Ingredients = new[] { new { SortOrder = 0, IngredientText = "Flour", MeasureText = "200g", IsOptional = false } },
            Steps = new[] { new { SortOrder = 0, InstructionText = "Mix well." } },
        };
        using var createContent = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, JsonMediaType);
        using var createResponse = await client.PostAsync(RecipeEndpoint, createContent);
        await createResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var createStream = await createResponse.Content.ReadAsStreamAsync();
        using var createDoc = await JsonDocument.ParseAsync(createStream);
        var recipeId = createDoc.RootElement.GetProperty("id").GetGuid();

        using var deleteResponse = await client.DeleteAsync($"/api/recipe/{recipeId}");
        await deleteResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        using var getResponse = await client.GetAsync($"/api/recipe/{recipeId}");
        await getResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        using var ingredientResponse = await client.GetAsync($"/api/recipe/{recipeId}/ingredient");
        await ingredientResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        var ingredientData = await ingredientResponse.Content.ReadAsStringAsync();
        using var ingredientDoc = JsonDocument.Parse(ingredientData);
        ingredientDoc.RootElement.GetArrayLength().Should().Be(0);

        var stepCount = await TestDatabaseSeeder.CountStepsForRecipeAsync(fixture, recipeId, cancellationToken);
        stepCount.Should().Be(1);
        var ingredientCount = await TestDatabaseSeeder.CountIngredientsForRecipeAsync(fixture, recipeId, cancellationToken);
        ingredientCount.Should().Be(1);
    }

    [Fact]
    public async Task Delete_Then_Restore_Returns_Recipe_With_Content()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = await fixture.GetHttpClient();
        var title = $"Recipe To Restore {Guid.NewGuid()}";
        var body = new
        {
            Title = title,
            AccessScope = PrivateAccessScope,
            Ingredients = new[] { new { SortOrder = 0, IngredientText = "Flour", MeasureText = "200g", IsOptional = false } },
            Steps = new[] { new { SortOrder = 0, InstructionText = "Mix well." } },
        };

        using var createContent = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, JsonMediaType);
        using var createResponse = await client.PostAsync(RecipeEndpoint, createContent);
        await createResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        using var createDoc = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var recipeId = createDoc.RootElement.GetProperty("id").GetGuid();

        await (await client.DeleteAsync($"/api/recipe/{recipeId}")).ShouldHaveStatusCode(HttpStatusCode.NoContent);

        using var restoreResponse = await client.PostAsync($"/api/recipe/{recipeId}/restore", content: null);
        await restoreResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        using var restoredDoc = JsonDocument.Parse(await restoreResponse.Content.ReadAsStringAsync());
        restoredDoc.RootElement.GetProperty("ingredients").GetArrayLength().Should().Be(1);
        restoredDoc.RootElement.GetProperty("steps").GetArrayLength().Should().Be(1);
        restoredDoc.RootElement.GetProperty("id").GetGuid().Should().Be(recipeId);

        (await TestDatabaseSeeder.CountStepsForRecipeAsync(fixture, recipeId, cancellationToken)).Should().Be(1);
    }

    [Fact]
    public async Task Delete_Then_Create_Same_Title_Succeeds()
    {
        using var client = await fixture.GetHttpClient();
        var title = $"Reusable Recipe {Guid.NewGuid()}";
        var body = new
        {
            Title = title,
            AccessScope = PrivateAccessScope,
            Ingredients = Array.Empty<object>(),
            Steps = Array.Empty<object>(),
        };

        using var firstContent = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, JsonMediaType);
        using var firstResponse = await client.PostAsync(RecipeEndpoint, firstContent);
        await firstResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        using var firstDoc = JsonDocument.Parse(await firstResponse.Content.ReadAsStringAsync());
        var firstId = firstDoc.RootElement.GetProperty("id").GetGuid();
        await (await client.DeleteAsync($"/api/recipe/{firstId}")).ShouldHaveStatusCode(HttpStatusCode.NoContent);

        using var secondContent = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, JsonMediaType);
        using var secondResponse = await client.PostAsync(RecipeEndpoint, secondContent);
        await secondResponse.ShouldHaveStatusCode(HttpStatusCode.OK);
        using var secondDoc = JsonDocument.Parse(await secondResponse.Content.ReadAsStringAsync());
        secondDoc.RootElement.GetProperty("id").GetGuid().Should().NotBe(firstId);

        using var restoreResponse = await client.PostAsync($"/api/recipe/{firstId}/restore", content: null);
        await restoreResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Delete_Recipe_NotOwner_Returns403()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = await fixture.GetHttpClient();

        var otherOwnerId = await TestDatabaseSeeder.AddMenuUserAsync(fixture, $"other-user-{Guid.NewGuid()}", cancellationToken);
        var recipeId = await TestDatabaseSeeder.AddRecipeAsync(
            fixture, $"Someone Else's Recipe {Guid.NewGuid()}", otherOwnerId, PrivateAccessScope, cancellationToken);

        using var response = await client.DeleteAsync($"/api/recipe/{recipeId}");

        await response.ShouldHaveStatusCode(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_Recipe_NotFound_Returns404()
    {
        using var client = await fixture.GetHttpClient();

        using var response = await client.DeleteAsync($"/api/recipe/{Guid.NewGuid()}");

        await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Restore_Recipe_NotOwner_Returns403()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = await fixture.GetHttpClient();
        var otherOwnerId = await TestDatabaseSeeder.AddMenuUserAsync(fixture, $"restore-owner-{Guid.NewGuid()}", cancellationToken);
        var recipeId = await TestDatabaseSeeder.AddRecipeAsync(
            fixture, $"Deleted Other Recipe {Guid.NewGuid()}", otherOwnerId, PrivateAccessScope, cancellationToken);
        await TestDatabaseSeeder.SoftDeleteRecipeAsync(fixture, recipeId, cancellationToken);

        using var response = await client.PostAsync($"/api/recipe/{recipeId}/restore", content: null);

        await response.ShouldHaveStatusCode(HttpStatusCode.Forbidden);
    }
}
