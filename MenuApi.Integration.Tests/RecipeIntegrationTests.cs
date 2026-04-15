using AwesomeAssertions;
using MenuApi.Integration.Tests.Factory;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public class RecipeIntegrationTests : IClassFixture<ApiTestFixture>
{
    private const string Grams = "Grams";

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
        using var client = await fixture.GetHttpClient();
        using var response = await client.GetAsync("/api/recipe");

        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        var data = await response.Content.ReadAsStringAsync();

        var deserializedData = JsonSerializer.Deserialize<HashSet<Recipe>>(data, jsonOptions);
        deserializedData.Should().NotBeNull();
    }

    [Theory, ShortStringAutoData]
    public async Task Create_Recipe(NewRecipe recipe, string ingredientName)
    {
        using var client = await fixture.GetHttpClient();
        await PostIngredientAsync(client, ingredientName);
        recipe.Ingredients = [new RecipeIngredient { Name = ingredientName, Unit = Grams, Amount = 100 }];
        var (_, name) = await PostRecipeAsync(client, recipe);

        name.Should().Be(recipe.Name);
    }

    [Theory, ShortStringAutoData]
    public async Task Create_and_Update_Recipe(NewRecipe recipe, NewRecipe updatedRecipe, string ingredientName, string ingredientName2)
    {
        using var client = await fixture.GetHttpClient();
        await PostIngredientAsync(client, ingredientName);
        await PostIngredientAsync(client, ingredientName2);
        recipe.Ingredients = [new RecipeIngredient { Name = ingredientName, Unit = Grams, Amount = 100 }];
        updatedRecipe.Ingredients = [new RecipeIngredient { Name = ingredientName2, Unit = Grams, Amount = 200 }];
       
        var (id, _) = await PostRecipeAsync(client, recipe);

        var (_, name) = await PutRecipeAsync(client, id, updatedRecipe);

        name.Should().Be(updatedRecipe.Name);
    }

    [Theory, ShortStringAutoData]
    public async Task Create_And_Get_Recipe(NewRecipe recipe, string ingredientName)
    {
        using var client = await fixture.GetHttpClient();
        await PostIngredientAsync(client, ingredientName);
        recipe.Ingredients = [new RecipeIngredient { Name = ingredientName, Unit = Grams, Amount = 100 }];
     
        var (id, _) = await PostRecipeAsync(client, recipe);
        var (getId, name) = await GetRecipeAsync(client, id);

        getId.Should().Be(id);
        name.Should().Be(recipe.Name);
    }

    [Theory, ShortStringAutoData]
    public async Task Create_Recipe_And_Get_Ingredients(NewRecipe recipe, string ingredientName)
    {
        using var client = await fixture.GetHttpClient();
        await PostIngredientAsync(client, ingredientName);
        recipe.Ingredients = [new RecipeIngredient { Name = ingredientName, Unit = Grams, Amount = 100 }];

        var (id, _) = await PostRecipeAsync(client, recipe);

        using var response = await client.GetAsync($"/api/recipe/{id}/ingredient");
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        var data = await response.Content.ReadAsStringAsync();
        var ingredients = JsonSerializer.Deserialize<List<RecipeIngredient>>(data, jsonOptions);
        ingredients.Should().NotBeNull();
        ingredients.Should().HaveCount(1);
        ingredients![0].Name.Should().Be(ingredientName);
    }

    private static async Task PostIngredientAsync(HttpClient client, string name)
    {
        var body = new { name, unitIds = new[] { 4 } }; // Grams
        using var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/ingredient", content);
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    private static async Task<(int Id, string Name)> PostRecipeAsync(HttpClient client, NewRecipe recipe)
    {
        using var requestContent = new StringContent(JsonSerializer.Serialize(recipe), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/recipe", requestContent);

        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var streamResponse = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(streamResponse);

        return GetRecipeFromJson(jsonDoc);
    }

    private static async Task<(int Id, string Name)> PutRecipeAsync(HttpClient client, int id, NewRecipe recipe)
    {
        using var requestContent = new StringContent(JsonSerializer.Serialize(recipe), Encoding.UTF8, "application/json");
        using var response = await client.PutAsync($"/api/recipe/{id}", requestContent);

        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var streamResponse = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(streamResponse);

        return GetRecipeFromJson(jsonDoc);
    }

    private static async Task<(int Id, string Name)> GetRecipeAsync(HttpClient client, int id)
    {
        using var response = await client.GetAsync($"/api/recipe/{id}");

        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

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
        public List<RecipeIngredient> Ingredients { get; set; } = [];

        public string Name { get; set; }
    }

    public class RecipeIngredient
    {
        public string Name { get; set; }

        public string Unit { get; set; }

        public decimal Amount { get; set; }
    }
}

