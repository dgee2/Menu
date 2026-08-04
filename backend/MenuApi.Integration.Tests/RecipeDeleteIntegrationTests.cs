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
    private readonly JsonSerializerOptions jsonOptions;
    private readonly ApiTestFixture fixture;

    public RecipeDeleteIntegrationTests(ApiTestFixture fixture)
    {
        jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        this.fixture = fixture;
    }

    [Theory]
    [InlineData("Recipe To Delete")]
    public async Task Delete_Recipe_As_Owner_Succeeds_And_Cascades(string recipeTitle)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = await fixture.GetHttpClient();

        var body = new
        {
            Title = recipeTitle,
            AccessScope = "Private",
            Ingredients = new[] { new { SortOrder = 0, IngredientText = "Flour", MeasureText = "200g", IsOptional = false } },
            Steps = new[] { new { SortOrder = 0, InstructionText = "Mix well." } },
        };
        using var createContent = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, "application/json");
        using var createResponse = await client.PostAsync("/api/recipe", createContent);
        await createResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var createStream = await createResponse.Content.ReadAsStreamAsync();
        using var createDoc = await JsonDocument.ParseAsync(createStream);
        var recipeId = createDoc.RootElement.GetProperty("id").GetInt32();

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
        stepCount.Should().Be(0);
    }

    [Fact]
    public async Task Delete_Recipe_NotOwner_Returns403()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = await fixture.GetHttpClient();

        var otherOwnerId = await TestDatabaseSeeder.AddMenuUserAsync(fixture, $"other-user-{Guid.NewGuid()}", cancellationToken);
        var recipeId = await TestDatabaseSeeder.AddRecipeAsync(
            fixture, $"Someone Else's Recipe {Guid.NewGuid()}", otherOwnerId, "Private", cancellationToken);

        using var response = await client.DeleteAsync($"/api/recipe/{recipeId}");

        await response.ShouldHaveStatusCode(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_Recipe_NotFound_Returns404()
    {
        using var client = await fixture.GetHttpClient();

        using var response = await client.DeleteAsync("/api/recipe/999999999");

        await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }
}
