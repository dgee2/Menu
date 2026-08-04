using AwesomeAssertions;
using MenuApi.Integration.Tests.Factory;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public class RecipeCreateUpdateIntegrationTests
{
    private readonly JsonSerializerOptions jsonOptions;
    private readonly ApiTestFixture fixture;

    public RecipeCreateUpdateIntegrationTests(ApiTestFixture fixture)
    {
        jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        this.fixture = fixture;
    }

    [Theory]
    [InlineData("Recipe With Steps")]
    public async Task Create_Recipe_With_Ingredients_And_Steps_Returns_Both(string recipeTitle)
    {
        using var client = await fixture.GetHttpClient();

        var body = new
        {
            Title = recipeTitle,
            AccessScope = "Private",
            Ingredients = new[] { new { SortOrder = 0, IngredientText = "Flour", MeasureText = "200g", IsOptional = false } },
            Steps = new[] { new { SortOrder = 0, InstructionText = "Mix well." } },
        };

        using var content = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/recipe", content);
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        root.GetProperty("ingredients").GetArrayLength().Should().Be(1);
        root.GetProperty("steps").GetArrayLength().Should().Be(1);
        root.GetProperty("steps")[0].GetProperty("instructionText").GetString().Should().Be("Mix well.");
    }

    [Theory]
    [InlineData("Recipe To Update With Steps")]
    public async Task Update_Recipe_Replaces_Ingredients_And_Steps(string recipeTitle)
    {
        using var client = await fixture.GetHttpClient();

        var createBody = new
        {
            Title = recipeTitle,
            AccessScope = "Private",
            Ingredients = new[] { new { SortOrder = 0, IngredientText = "Flour", MeasureText = "200g", IsOptional = false } },
            Steps = new[] { new { SortOrder = 0, InstructionText = "Original step." } },
        };
        using var createContent = new StringContent(JsonSerializer.Serialize(createBody, jsonOptions), Encoding.UTF8, "application/json");
        using var createResponse = await client.PostAsync("/api/recipe", createContent);
        await createResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var createStream = await createResponse.Content.ReadAsStreamAsync();
        using var createDoc = await JsonDocument.ParseAsync(createStream);
        var recipeId = createDoc.RootElement.GetProperty("id").GetInt32();

        var updateBody = new
        {
            Title = recipeTitle,
            AccessScope = "Private",
            Ingredients = new[] { new { SortOrder = 0, IngredientText = "Sugar", MeasureText = "1 cup", IsOptional = false } },
            Steps = new[]
            {
                new { SortOrder = 0, InstructionText = "Updated step one." },
                new { SortOrder = 1, InstructionText = "Updated step two." },
            },
        };
        using var updateContent = new StringContent(JsonSerializer.Serialize(updateBody, jsonOptions), Encoding.UTF8, "application/json");
        using var updateResponse = await client.PutAsync($"/api/recipe/{recipeId}", updateContent);
        await updateResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var updateStream = await updateResponse.Content.ReadAsStreamAsync();
        using var updateDoc = await JsonDocument.ParseAsync(updateStream);
        var root = updateDoc.RootElement;

        root.GetProperty("ingredients").GetArrayLength().Should().Be(1);
        root.GetProperty("ingredients")[0].GetProperty("ingredientText").GetString().Should().Be("Sugar");
        root.GetProperty("steps").GetArrayLength().Should().Be(2);
    }

    [Theory]
    [InlineData("Minimal Recipe")]
    public async Task Create_Recipe_With_Zero_Ingredients_And_Steps_Succeeds(string recipeTitle)
    {
        using var client = await fixture.GetHttpClient();

        var body = new
        {
            Title = recipeTitle,
            AccessScope = "Private",
            Ingredients = Array.Empty<object>(),
            Steps = Array.Empty<object>(),
        };

        using var content = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/recipe", content);
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        root.GetProperty("ingredients").GetArrayLength().Should().Be(0);
        root.GetProperty("steps").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task Update_Recipe_NotOwner_Returns403()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = await fixture.GetHttpClient();

        var otherOwnerId = await TestDatabaseSeeder.AddMenuUserAsync(fixture, $"other-user-{Guid.NewGuid()}", cancellationToken);
        var recipeId = await TestDatabaseSeeder.AddRecipeAsync(
            fixture, $"Someone Else's Recipe {Guid.NewGuid()}", otherOwnerId, "Private", cancellationToken);

        var updateBody = new
        {
            Title = "Attempted Takeover",
            AccessScope = "Private",
            Ingredients = Array.Empty<object>(),
            Steps = Array.Empty<object>(),
        };
        using var content = new StringContent(JsonSerializer.Serialize(updateBody, jsonOptions), Encoding.UTF8, "application/json");
        using var response = await client.PutAsync($"/api/recipe/{recipeId}", content);

        await response.ShouldHaveStatusCode(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_Recipe_NotFound_Returns404()
    {
        using var client = await fixture.GetHttpClient();

        var updateBody = new
        {
            Title = "Does Not Exist",
            AccessScope = "Private",
            Ingredients = Array.Empty<object>(),
            Steps = Array.Empty<object>(),
        };
        using var content = new StringContent(JsonSerializer.Serialize(updateBody, jsonOptions), Encoding.UTF8, "application/json");
        using var response = await client.PutAsync("/api/recipe/999999999", content);

        await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }
}
