using AwesomeAssertions;
using MenuApi.Integration.Tests.Factory;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public class ValidationIntegrationTests : IClassFixture<ApiTestFixture>
{
    private const string ApplicationJson = "application/json";
    private const string ApiRecipeRoute = "/api/recipe";
    private const string Grams = "Grams";

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
        var content = new StringContent("{}", Encoding.UTF8, ApplicationJson);
        using var response = await client.PostAsync(ApiRecipeRoute, content);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRecipe_EmptyName_Returns400WithProblemDetails()
    {
        using var client = await fixture.GetHttpClient();
        var body = new NewRecipe { Name = "", Ingredients = [] };
        var content = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, ApplicationJson);
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
    public async Task CreateRecipe_NonExistentIngredient_Returns422()
    {
        using var client = await fixture.GetHttpClient();
        var body = new NewRecipe
        {
            Name = "Test Recipe",
            Ingredients = [new RecipeIngredient { Name = "NonExistentIngredient", Unit = Grams, Amount = 100 }]
        };
        var content = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, ApplicationJson);
        using var response = await client.PostAsync(ApiRecipeRoute, content);

        await response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        var responseBody = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseBody);
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(422);
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
        var content = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, ApplicationJson);
        using var response = await client.PostAsync("/api/ingredient", content);

        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateIngredient_EmptyUnitIds_Returns400()
    {
        using var client = await fixture.GetHttpClient();
        var body = new NewIngredient { Name = "SomeIngredient", UnitIds = [] };
        var content = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, ApplicationJson);
        using var response = await client.PostAsync("/api/ingredient", content);

        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Theory, ShortStringAutoData]
    public async Task UpdateRecipe_EmptyName_Returns400(string recipeName, string ingredientName)
    {
        using var client = await fixture.GetHttpClient();
        var recipeId = await CreateRecipeAsync(client, recipeName, ingredientName);

        var updateBody = new NewRecipe { Name = "", Ingredients = [new RecipeIngredient { Name = ingredientName, Unit = Grams, Amount = 100 }] };
        var updateContent = new StringContent(JsonSerializer.Serialize(updateBody, jsonOptions), Encoding.UTF8, ApplicationJson);
        using var updateResponse = await client.PutAsync($"/api/recipe/{recipeId}", updateContent);

        await updateResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Theory, ShortStringAutoData]
    public async Task UpdateRecipe_NonExistentIngredient_Returns422(string recipeName, string ingredientName)
    {
        using var client = await fixture.GetHttpClient();
        var recipeId = await CreateRecipeAsync(client, recipeName, ingredientName);

        var updateBody = new NewRecipe
        {
            Name = recipeName,
            Ingredients = [new RecipeIngredient { Name = "NonExistentIngredient", Unit = Grams, Amount = 100 }]
        };
        var updateContent = new StringContent(JsonSerializer.Serialize(updateBody, jsonOptions), Encoding.UTF8, ApplicationJson);
        using var updateResponse = await client.PutAsync($"/api/recipe/{recipeId}", updateContent);

        await updateResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);
    }

    private async Task<int> CreateRecipeAsync(HttpClient client, string recipeName, string ingredientName)
    {
        await PostIngredientAsync(client, ingredientName);

        var createBody = new NewRecipe { Name = recipeName, Ingredients = [new RecipeIngredient { Name = ingredientName, Unit = Grams, Amount = 100 }] };
        var createContent = new StringContent(JsonSerializer.Serialize(createBody, jsonOptions), Encoding.UTF8, ApplicationJson);
        using var createResponse = await client.PostAsync(ApiRecipeRoute, createContent);
        await createResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var createStream = await createResponse.Content.ReadAsStreamAsync();
        using var createDoc = await JsonDocument.ParseAsync(createStream);
        return createDoc.RootElement.GetProperty("id").GetInt32();
    }

    private async Task PostIngredientAsync(HttpClient client, string name)
    {
        var body = new NewIngredient { Name = name, UnitIds = [4] }; // Grams
        var content = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, ApplicationJson);
        using var response = await client.PostAsync("/api/ingredient", content);
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    public class NewRecipe
    {
        public List<RecipeIngredient> Ingredients { get; set; } = [];
        public string Name { get; set; }
    }

    public class RecipeIngredient
    {
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal Amount { get; set; }
    }

    public class NewIngredient
    {
        public string Name { get; set; }
        public List<int> UnitIds { get; set; } = [];
    }
}
