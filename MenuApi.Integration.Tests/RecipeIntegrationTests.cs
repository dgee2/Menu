using AutoFixture.Xunit2;
using FluentAssertions;
using MenuApi.Integration.Tests.Factory;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public class RecipeIntegrationTests : IClassFixture<ApiTestFixture>
{
    readonly JsonSerializerOptions jsonOptions;
    private readonly ApiTestFixture fixture;

    public RecipeIntegrationTests(ApiTestFixture fixture)
    {
        jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        this.fixture = fixture;
    }

    [Fact]
    public async Task Get_ReturnsAListOfCategories_CategoriesController()
    {
        using var client = fixture.GetHttpClient();
        using var response = await client.GetAsync("/api/recipe");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await response.Content.ReadAsStringAsync();

        var deserializedData = JsonSerializer.Deserialize<HashSet<Recipe>>(data, jsonOptions);
        deserializedData.Should().NotBeNull();
    }

    [Theory, AutoData]
    public async Task Create_Recipe(NewRecipe recipe)
    {
        using var client = fixture.GetHttpClient();

        var (_, name) = await PostRecipeAsync(client, recipe);

        name.Should().Be(recipe.Name);
    }

    [Theory, AutoData]
    public async Task Create_and_Update_Recipe(NewRecipe recipe, NewRecipe newRecipe)
    {
        using var client = fixture.GetHttpClient();

        var (id, _) = await PostRecipeAsync(client, recipe);

        var (_, name) = await PutRecipeAsync(client, id, newRecipe);

        name.Should().Be(newRecipe.Name);
    }

    [Theory, AutoData]
    public async Task Create_And_Get_Recipe(NewRecipe recipe)
    {
        using var client = fixture.GetHttpClient();

        var (id, _) = await PostRecipeAsync(client, recipe);
        var (getId, name) = await GetRecipeAsync(client, id);

        getId.Should().Be(id);
        name.Should().Be(recipe.Name);
    }

    private static async Task<(int Id, string Name)> PostRecipeAsync(HttpClient client, NewRecipe recipe)
    {
        var requestContent = new StringContent(JsonSerializer.Serialize(recipe), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/recipe", requestContent);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var streamResponse = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(streamResponse);

        return GetRecipeFromJson(jsonDoc);
    }

    private static async Task<(int Id, string Name)> PutRecipeAsync(HttpClient client, int id, NewRecipe recipe)
    {
        var requestContent = new StringContent(JsonSerializer.Serialize(recipe), Encoding.UTF8, "application/json");
        using var response = await client.PutAsync($"/api/recipe/{id}", requestContent);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var streamResponse = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(streamResponse);

        return GetRecipeFromJson(jsonDoc);
    }

    private static async Task<(int Id, string Name)> GetRecipeAsync(HttpClient client, int id)
    {
        using var response = await client.GetAsync($"/api/recipe/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var streamResponse = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(streamResponse);

        return GetRecipeFromJson(jsonDoc);
    }



    private static (int Id, string Name) GetRecipeFromJson(JsonDocument doc)
    {
        var rootElement = doc.RootElement;
        return (
            rootElement.GetProperty("id").GetInt32(),
            rootElement.GetProperty("name").GetString()
        );
    }

    private class Recipe
    {
#pragma warning disable S1144 // Unused private types or members should be removed
        public int Id { get; set; }

        public string Name { get; set; }
#pragma warning restore S1144 // Unused private types or members should be removed
    }

    public class NewRecipe
    {
        public List<RecipeIngredient> Ingredients { get; set; }

        public string Name { get; set; }
    }

    public class RecipeIngredient
    {
        public string Name { get; set; }

        public string Unit { get; set; }

        public decimal Amount { get; set; }
    }
}