using AwesomeAssertions;
using MenuApi.Integration.Tests.Factory;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public class RecipeIntegrationTests
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
    public async Task Get_ReturnsAListOfRecipes()
    {
        using var client = await fixture.GetHttpClient();
        using var response = await client.GetAsync("/api/recipe?scope=mine");

        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        var data = await response.Content.ReadAsStringAsync();

        var deserializedData = JsonSerializer.Deserialize<HashSet<RecipeListItem>>(data, jsonOptions);
        deserializedData.Should().NotBeNull();
    }

    [Fact]
    public async Task Get_UnknownScope_ReturnsBadRequest()
    {
        using var client = await fixture.GetHttpClient();
        using var response = await client.GetAsync("/api/recipe?scope=everyone");

        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("Simple Recipe")]
    public async Task Create_Recipe_With_Free_Text_Ingredients(string recipeTitle)
    {
        using var client = await fixture.GetHttpClient();
        var recipe = new UpsertRecipe
        {
            Title = recipeTitle,
            Ingredients =
            [
                new RecipeIngredientItem { SortOrder = 0, IngredientText = "Flour", MeasureText = "2 cups", IsOptional = false }
            ]
        };
        var (_, title) = await PostRecipeAsync(client, recipe);

        title.Should().Be(recipeTitle);
    }

    [Theory]
    [InlineData("Recipe 1", "Recipe 2")]
    public async Task Create_and_Update_Recipe(string recipeTitle1, string recipeTitle2)
    {
        using var client = await fixture.GetHttpClient();
        var recipe = new UpsertRecipe
        {
            Title = recipeTitle1,
            Ingredients =
            [
                new RecipeIngredientItem { SortOrder = 0, IngredientText = "Sugar", MeasureText = "1 cup", IsOptional = false }
            ]
        };

        var (id, _) = await PostRecipeAsync(client, recipe);

        var updatedRecipe = new UpsertRecipe
        {
            Title = recipeTitle2,
            Ingredients =
            [
                new RecipeIngredientItem { SortOrder = 0, IngredientText = "Butter", MeasureText = "1/2 cup", IsOptional = false }
            ]
        };

        var (_, title) = await PutRecipeAsync(client, id, updatedRecipe);

        title.Should().Be(recipeTitle2);
    }

    [Theory]
    [InlineData("Test Recipe")]
    public async Task Create_And_Get_Recipe(string recipeTitle)
    {
        using var client = await fixture.GetHttpClient();
        var recipe = new UpsertRecipe
        {
            Title = recipeTitle,
            Ingredients =
            [
                new RecipeIngredientItem { SortOrder = 0, IngredientText = "Eggs", MeasureText = "3", IsOptional = false }
            ]
        };

        var (id, _) = await PostRecipeAsync(client, recipe);
        var (getId, title) = await GetRecipeAsync(client, id);

        getId.Should().Be(id);
        title.Should().Be(recipeTitle);
    }

    [Theory]
    [InlineData("Recipe with Ingredients")]
    public async Task Create_Recipe_And_Get_Ingredients(string recipeTitle)
    {
        using var client = await fixture.GetHttpClient();
        var recipe = new UpsertRecipe
        {
            Title = recipeTitle,
            Ingredients =
            [
                new RecipeIngredientItem { SortOrder = 0, IngredientText = "Salt", MeasureText = "1 tbsp", IsOptional = false }
            ]
        };

        var (id, _) = await PostRecipeAsync(client, recipe);

        using var response = await client.GetAsync($"/api/recipe/{id}/ingredient");
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        var data = await response.Content.ReadAsStringAsync();
        var ingredients = JsonSerializer.Deserialize<List<RecipeIngredientItem>>(data, jsonOptions);
        ingredients.Should().NotBeNull();
        ingredients.Should().HaveCount(1);
        ingredients![0].IngredientText.Should().Be("Salt");
    }

    [Theory]
    [InlineData("Unique Title")]
    public async Task Create_Recipe_With_Duplicate_Title_Returns_Conflict(string recipeTitle)
    {
        using var client = await fixture.GetHttpClient();
        var recipe = new UpsertRecipe
        {
            Title = recipeTitle,
            Ingredients =
            [
                new RecipeIngredientItem { SortOrder = 0, IngredientText = "Ingredient", MeasureText = "1", IsOptional = false }
            ]
        };

        await PostRecipeAsync(client, recipe);

        using var requestContent = new StringContent(JsonSerializer.Serialize(recipe), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/recipe", requestContent);

        await response.ShouldHaveStatusCode(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("Recipe A", "Recipe B")]
    public async Task Update_Recipe_To_Duplicate_Title_Returns_Conflict(string recipeTitle1, string recipeTitle2)
    {
        using var client = await fixture.GetHttpClient();
        var recipe1 = new UpsertRecipe
        {
            Title = recipeTitle1,
            Ingredients =
            [
                new RecipeIngredientItem { SortOrder = 0, IngredientText = "Ingredient1", MeasureText = "1", IsOptional = false }
            ]
        };

        var recipe2 = new UpsertRecipe
        {
            Title = recipeTitle2,
            Ingredients =
            [
                new RecipeIngredientItem { SortOrder = 0, IngredientText = "Ingredient2", MeasureText = "2", IsOptional = false }
            ]
        };

        await PostRecipeAsync(client, recipe1);
        var (id2, _) = await PostRecipeAsync(client, recipe2);

        // Try to rename recipe2 to recipe1's title - should be 409
        recipe2.Title = recipeTitle1;
        using var requestContent = new StringContent(JsonSerializer.Serialize(recipe2), Encoding.UTF8, "application/json");
        using var response = await client.PutAsync($"/api/recipe/{id2}", requestContent);

        await response.ShouldHaveStatusCode(HttpStatusCode.Conflict);
    }

    private static async Task<(int Id, string Title)> PostRecipeAsync(HttpClient client, UpsertRecipe recipe)
    {
        using var requestContent = new StringContent(JsonSerializer.Serialize(recipe), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/recipe", requestContent);

        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var streamResponse = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(streamResponse);

        return GetRecipeFromJson(jsonDoc);
    }

    private static async Task<(int Id, string Title)> PutRecipeAsync(HttpClient client, int id, UpsertRecipe recipe)
    {
        using var requestContent = new StringContent(JsonSerializer.Serialize(recipe), Encoding.UTF8, "application/json");
        using var response = await client.PutAsync($"/api/recipe/{id}", requestContent);

        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var streamResponse = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(streamResponse);

        return GetRecipeFromJson(jsonDoc);
    }

    private static async Task<(int Id, string Title)> GetRecipeAsync(HttpClient client, int id)
    {
        using var response = await client.GetAsync($"/api/recipe/{id}");

        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var streamResponse = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(streamResponse);

        return GetRecipeFromJson(jsonDoc);
    }

    private static (int Id, string Title) GetRecipeFromJson(JsonDocument doc)
    {
        var rootElement = doc.RootElement;
        return (
            rootElement.GetProperty("id").GetInt32(),
            rootElement.GetProperty("title").GetString()!
        );
    }

    private class RecipeListItem
    {
#pragma warning disable S1144 // Unused private types or members should be removed
        public int Id { get; set; }

        public string Title { get; set; } = null!;
#pragma warning restore S1144 // Unused private types or members should be removed
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
#pragma warning disable S1144 // Unused private types or members should be removed
        public int SortOrder { get; set; }
        public string IngredientText { get; set; } = null!;
        public string MeasureText { get; set; } = null!;
        public bool IsOptional { get; set; }
#pragma warning restore S1144 // Unused private types or members should be removed
    }

    public class RecipeStepItem
    {
#pragma warning disable S1144 // Unused private types or members should be removed
        public int SortOrder { get; set; }
        public string InstructionText { get; set; } = null!;
#pragma warning restore S1144 // Unused private types or members should be removed
    }
}
