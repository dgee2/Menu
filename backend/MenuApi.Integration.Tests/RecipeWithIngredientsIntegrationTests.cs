using AutoFixture.Xunit3;
using AwesomeAssertions;
using MenuApi.Integration.Tests.Factory;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public class RecipeWithIngredientsIntegrationTests
{
    private const string ApplicationJson = "application/json";
    private const string ApiRecipeRoute = "/api/recipe";

    private readonly JsonSerializerOptions jsonOptions;
    private readonly ApiTestFixture fixture;

    public RecipeWithIngredientsIntegrationTests(ApiTestFixture fixture)
    {
        jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        this.fixture = fixture;
    }

    [Theory, AutoData]
    public async Task Create_Recipe_With_Ingredients(
        [StringLength(50, MinimumLength = 1)] string ingredientText,
        [StringLength(50, MinimumLength = 1)] string measureText,
        [StringLength(200, MinimumLength = 1)] string recipeTitle)
    {
        using var client = await fixture.GetHttpClient();

        var newRecipe = CreateRecipe(recipeTitle, ingredientText, measureText);

        var (recipeId, returnedTitle, returnedIngredients) = await PostRecipeAsync(client, newRecipe);

        recipeId.Should().BeGreaterThan(0);
        returnedTitle.Should().Be(recipeTitle);
        returnedIngredients.Should().HaveCount(1);
        returnedIngredients[0].IngredientText.Should().Be(ingredientText);
        returnedIngredients[0].MeasureText.Should().Be(measureText);
    }

    [Theory, AutoData]
    public async Task Create_Recipe_With_Ingredients_Then_Get_Recipe_Returns_Ingredients(
        [StringLength(50, MinimumLength = 1)] string ingredientText,
        [StringLength(50, MinimumLength = 1)] string measureText,
        [StringLength(200, MinimumLength = 1)] string recipeTitle)
    {
        using var client = await fixture.GetHttpClient();

        var newRecipe = CreateRecipe(recipeTitle, ingredientText, measureText);

        var (recipeId, _, _) = await PostRecipeAsync(client, newRecipe);

        using var getResponse = await client.GetAsync($"/api/recipe/{recipeId}");
        await getResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var getStream = await getResponse.Content.ReadAsStreamAsync();
        using var getDoc = await JsonDocument.ParseAsync(getStream);
        var root = getDoc.RootElement;

        root.GetProperty("title").GetString().Should().Be(recipeTitle);
        var ingredients = JsonSerializer.Deserialize<List<RecipeIngredient>>(
            root.GetProperty("ingredients").GetRawText(), jsonOptions)!;
        ingredients.Should().HaveCount(1);
        ingredients[0].IngredientText.Should().Be(ingredientText);
        ingredients[0].MeasureText.Should().Be(measureText);
    }

    [Theory, AutoData]
    public async Task Create_Recipe_With_Ingredients_Then_Get_Recipe_Ingredients_Endpoint(
        [StringLength(50, MinimumLength = 1)] string ingredientText,
        [StringLength(50, MinimumLength = 1)] string measureText,
        [StringLength(200, MinimumLength = 1)] string recipeTitle)
    {
        using var client = await fixture.GetHttpClient();

        var newRecipe = CreateRecipe(recipeTitle, ingredientText, measureText);

        var (recipeId, _, _) = await PostRecipeAsync(client, newRecipe);

        using var response = await client.GetAsync($"/api/recipe/{recipeId}/ingredient");
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        var data = await response.Content.ReadAsStringAsync();
        var ingredients = JsonSerializer.Deserialize<List<RecipeIngredient>>(data, jsonOptions)!;
        ingredients.Should().HaveCount(1);
        ingredients[0].IngredientText.Should().Be(ingredientText);
        ingredients[0].MeasureText.Should().Be(measureText);
    }

    [Theory, AutoData]
    public async Task Update_Recipe_With_Different_Ingredients(
        [StringLength(50, MinimumLength = 1)] string ingredientText1,
        [StringLength(50, MinimumLength = 1)] string ingredientText2,
        [StringLength(50, MinimumLength = 1)] string measureText,
        [StringLength(200, MinimumLength = 1)] string recipeTitle,
        [StringLength(200, MinimumLength = 1)] string updatedRecipeTitle)
    {
        using var client = await fixture.GetHttpClient();

        var newRecipe = CreateRecipe(recipeTitle, ingredientText1, measureText);

        var (recipeId, _, _) = await PostRecipeAsync(client, newRecipe);

        var updatedRecipe = CreateRecipe(updatedRecipeTitle, ingredientText2, measureText);

        var (_, returnedTitle, returnedIngredients) = await PutRecipeAsync(client, recipeId, updatedRecipe);

        returnedTitle.Should().Be(updatedRecipeTitle);
        returnedIngredients.Should().HaveCount(1);
        returnedIngredients[0].IngredientText.Should().Be(ingredientText2);
    }

    private async Task<(int Id, string Title, List<RecipeIngredient> Ingredients)> PostRecipeAsync(
        HttpClient client, NewRecipe recipe)
    {
        using var response = await SendRecipeAsync(client, HttpMethod.Post, ApiRecipeRoute, recipe);
        return await DeserializeRecipeResponseAsync(response);
    }

    private async Task<(int Id, string Title, List<RecipeIngredient> Ingredients)> PutRecipeAsync(
        HttpClient client, int id, NewRecipe recipe)
    {
        using var response = await SendRecipeAsync(client, HttpMethod.Put, $"{ApiRecipeRoute}/{id}", recipe);
        return await DeserializeRecipeResponseAsync(response);
    }

    private static NewRecipe CreateRecipe(string title, string ingredientText, string measureText)
    {
        return new NewRecipe
        {
            Title = title,
            Ingredients = [new RecipeIngredient { SortOrder = 0, IngredientText = ingredientText, MeasureText = measureText, IsOptional = false }]
        };
    }

    private async Task<HttpResponseMessage> SendRecipeAsync(HttpClient client, HttpMethod method, string url, NewRecipe recipe)
    {
        using var request = new HttpRequestMessage(method, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(recipe, jsonOptions), Encoding.UTF8, ApplicationJson)
        };
        var response = await client.SendAsync(request);
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);
        return response;
    }

    private async Task<(int Id, string Title, List<RecipeIngredient> Ingredients)> DeserializeRecipeResponseAsync(
        HttpResponseMessage response)
    {
        using var stream = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        var root = jsonDoc.RootElement;

        return (
            root.GetProperty("id").GetInt32(),
            root.GetProperty("title").GetString()!,
            JsonSerializer.Deserialize<List<RecipeIngredient>>(root.GetProperty("ingredients").GetRawText(), jsonOptions) ?? []);
    }

    public class NewRecipe
    {
#pragma warning disable S1144 // Unused private types or members should be removed
        public string Title { get; set; } = null!;
        public List<RecipeIngredient> Ingredients { get; set; } = [];
#pragma warning restore S1144 // Unused private types or members should be removed
    }

    public class RecipeIngredient
    {
#pragma warning disable S1144 // Unused private types or members should be removed
        public int SortOrder { get; set; }
        public string IngredientText { get; set; } = null!;
        public string MeasureText { get; set; } = null!;
        public bool IsOptional { get; set; }
#pragma warning restore S1144 // Unused private types or members should be removed
    }
}





