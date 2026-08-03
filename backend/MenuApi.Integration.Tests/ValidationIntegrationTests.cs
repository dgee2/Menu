using AwesomeAssertions;
using MenuApi.Integration.Tests.Factory;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public class ValidationIntegrationTests
{
    private const string ApplicationJson = "application/json";
    private const string ApiRecipeRoute = "/api/recipe";

    private readonly JsonSerializerOptions jsonOptions;
    private readonly ApiTestFixture fixture;

    public ValidationIntegrationTests(ApiTestFixture fixture)
    {
        jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        this.fixture = fixture;
    }

    [Fact]
    public async Task CreateRecipe_MissingProperties_Returns400()
    {
        using var client = await fixture.GetHttpClient();
        using var content = new StringContent("{}", Encoding.UTF8, ApplicationJson);
        using var response = await client.PostAsync(ApiRecipeRoute, content);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRecipe_EmptyTitle_Returns400WithProblemDetails()
    {
        using var client = await fixture.GetHttpClient();
        var body = new UpsertRecipe { Title = "", Ingredients = [] };
        using var content = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, ApplicationJson);
        using var response = await client.PostAsync(ApiRecipeRoute, content);

        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        var responseBody = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        root.GetProperty("status").GetInt32().Should().Be(400);
        root.GetProperty("title").GetString().Should().Be("One or more validation errors occurred.");
        root.TryGetProperty("errors", out _).Should().BeTrue();
    }

    [Fact]
    public async Task CreateRecipe_EmptyIngredientText_Returns400()
    {
        using var client = await fixture.GetHttpClient();
        var body = new UpsertRecipe
        {
            Title = "Test Recipe",
            Ingredients = [new RecipeIngredientItem { SortOrder = 0, IngredientText = "", MeasureText = "1", IsOptional = false }]
        };
        using var content = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, ApplicationJson);
        using var response = await client.PostAsync(ApiRecipeRoute, content);

        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRecipe_EmptyMeasureText_Returns400()
    {
        using var client = await fixture.GetHttpClient();
        var body = new UpsertRecipe
        {
            Title = "Test Recipe",
            Ingredients = [new RecipeIngredientItem { SortOrder = 0, IngredientText = "Flour", MeasureText = "", IsOptional = false }]
        };
        using var content = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, ApplicationJson);
        using var response = await client.PostAsync(ApiRecipeRoute, content);

        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetRecipe_NonExistentId_Returns404()
    {
        using var client = await fixture.GetHttpClient();
        using var response = await client.GetAsync("/api/recipe/99999");

        await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        var responseBody = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseBody);
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(404);
    }

    [Fact]
    public async Task CreateIngredient_EmptyName_Returns400()
    {
        using var client = await fixture.GetHttpClient();
        var body = new NewIngredient { Name = "", UnitIds = [1] };
        using var content = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, ApplicationJson);
        using var response = await client.PostAsync("/api/ingredient", content);

        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateIngredient_EmptyUnitIds_Returns400()
    {
        using var client = await fixture.GetHttpClient();
        var body = new NewIngredient { Name = "SomeIngredient", UnitIds = [] };
        using var content = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, ApplicationJson);
        using var response = await client.PostAsync("/api/ingredient", content);

        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateRecipe_EmptyTitle_Returns400()
    {
        using var client = await fixture.GetHttpClient();
        var recipeId = await CreateRecipeAsync(client, $"Original Recipe {Guid.NewGuid()}");

        var updateBody = new UpsertRecipe { Title = "", Ingredients = [new RecipeIngredientItem { SortOrder = 0, IngredientText = "Flour", MeasureText = "1", IsOptional = false }] };
        using var updateContent = new StringContent(JsonSerializer.Serialize(updateBody, jsonOptions), Encoding.UTF8, ApplicationJson);
        using var updateResponse = await client.PutAsync($"/api/recipe/{recipeId}", updateContent);

        await updateResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateRecipe_EmptyIngredientText_Returns400()
    {
        using var client = await fixture.GetHttpClient();
        var recipeId = await CreateRecipeAsync(client, $"Original Recipe {Guid.NewGuid()}");

        var updateBody = new UpsertRecipe
        {
            Title = "Updated Recipe",
            Ingredients = [new RecipeIngredientItem { SortOrder = 0, IngredientText = "", MeasureText = "1", IsOptional = false }]
        };
        using var updateContent = new StringContent(JsonSerializer.Serialize(updateBody, jsonOptions), Encoding.UTF8, ApplicationJson);
        using var updateResponse = await client.PutAsync($"/api/recipe/{recipeId}", updateContent);

        await updateResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    private async Task<int> CreateRecipeAsync(HttpClient client, string recipeTitle)
    {
        var createBody = new UpsertRecipe
        {
            Title = recipeTitle,
            Ingredients = [new RecipeIngredientItem { SortOrder = 0, IngredientText = "Sugar", MeasureText = "1 cup", IsOptional = false }]
        };
        using var createContent = new StringContent(JsonSerializer.Serialize(createBody, jsonOptions), Encoding.UTF8, ApplicationJson);
        using var createResponse = await client.PostAsync(ApiRecipeRoute, createContent);
        await createResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var createStream = await createResponse.Content.ReadAsStreamAsync();
        using var createDoc = await JsonDocument.ParseAsync(createStream);
        return createDoc.RootElement.GetProperty("id").GetInt32();
    }

    public class UpsertRecipe
    {
        public List<RecipeIngredientItem> Ingredients { get; set; } = [];
        public List<RecipeStepItem> Steps { get; set; } = [];
        public string Title { get; set; } = null!;
        public string AccessScope { get; set; } = "Private";
    }

    public class RecipeIngredientItem
    {
        public int SortOrder { get; set; }
        public string IngredientText { get; set; } = null!;
        public string MeasureText { get; set; } = null!;
        public bool IsOptional { get; set; }
    }

    public class RecipeStepItem
    {
        public int SortOrder { get; set; }
        public string InstructionText { get; set; } = null!;
    }

    public class NewIngredient
    {
        public string Name { get; set; } = null!;
        public List<int> UnitIds { get; set; } = [];
    }
}
