using AwesomeAssertions;
using MenuApi.Integration.Tests.Factory;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public class RecipeWithIngredientsIntegrationTests : IClassFixture<ApiTestFixture>
{
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

    [Theory, ShortStringAutoData]
    public async Task Create_Recipe_With_Ingredients(string ingredientName, string recipeName)
    {
        using var client = await fixture.GetHttpClient();

        // Create an ingredient first (with Grams unit, id=4)
        await PostIngredientAsync(client, ingredientName, [4]);

        // Create a recipe that references the ingredient
        var newRecipe = new NewRecipe
        {
            Name = recipeName,
            Ingredients =
            [
                new RecipeIngredient { Name = ingredientName, Unit = "Grams", Amount = 250.5m }
            ]
        };

        var (recipeId, returnedName, returnedIngredients) = await PostRecipeAsync(client, newRecipe);

        recipeId.Should().BeGreaterThan(0);
        returnedName.Should().Be(recipeName);
        returnedIngredients.Should().HaveCount(1);
        returnedIngredients[0].Name.Should().Be(ingredientName);
        returnedIngredients[0].Unit.Should().Be("Grams");
        returnedIngredients[0].Amount.Should().Be(250.5m);
    }

    [Theory, ShortStringAutoData]
    public async Task Create_Recipe_With_Ingredients_Then_Get_Recipe_Returns_Ingredients(
        string ingredientName, string recipeName)
    {
        using var client = await fixture.GetHttpClient();

        await PostIngredientAsync(client, ingredientName, [1]);

        var newRecipe = new NewRecipe
        {
            Name = recipeName,
            Ingredients =
            [
                new RecipeIngredient { Name = ingredientName, Unit = "Millilitres", Amount = 500m }
            ]
        };

        var (recipeId, _, _) = await PostRecipeAsync(client, newRecipe);

        // GET the recipe by ID and verify ingredients
        using var getResponse = await client.GetAsync($"/api/recipe/{recipeId}");
        await getResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var getStream = await getResponse.Content.ReadAsStreamAsync();
        using var getDoc = await JsonDocument.ParseAsync(getStream);
        var root = getDoc.RootElement;

        root.GetProperty("name").GetString().Should().Be(recipeName);
        var ingredients = JsonSerializer.Deserialize<List<RecipeIngredient>>(
            root.GetProperty("ingredients").GetRawText(), jsonOptions)!;
        ingredients.Should().HaveCount(1);
        ingredients[0].Name.Should().Be(ingredientName);
        ingredients[0].Unit.Should().Be("Millilitres");
        ingredients[0].Amount.Should().Be(500m);
    }

    [Theory, ShortStringAutoData]
    public async Task Create_Recipe_With_Ingredients_Then_Get_Recipe_Ingredients_Endpoint(
        string ingredientName, string recipeName)
    {
        using var client = await fixture.GetHttpClient();

        await PostIngredientAsync(client, ingredientName, [5]);

        var newRecipe = new NewRecipe
        {
            Name = recipeName,
            Ingredients =
            [
                new RecipeIngredient { Name = ingredientName, Unit = "Kilograms", Amount = 2m }
            ]
        };

        var (recipeId, _, _) = await PostRecipeAsync(client, newRecipe);

        // GET the recipe ingredients sub-endpoint
        using var response = await client.GetAsync($"/api/recipe/{recipeId}/ingredient");
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        var data = await response.Content.ReadAsStringAsync();
        var ingredients = JsonSerializer.Deserialize<List<RecipeIngredient>>(data, jsonOptions)!;
        ingredients.Should().HaveCount(1);
        ingredients[0].Name.Should().Be(ingredientName);
        ingredients[0].Unit.Should().Be("Kilograms");
        ingredients[0].Amount.Should().Be(2m);
    }

    [Theory, ShortStringAutoData]
    public async Task Update_Recipe_With_Different_Ingredients(
        string ingredientName1, string ingredientName2, string recipeName, string updatedRecipeName)
    {
        using var client = await fixture.GetHttpClient();

        // Create two ingredients
        await PostIngredientAsync(client, ingredientName1, [4]);
        await PostIngredientAsync(client, ingredientName2, [1]);

        // Create a recipe with the first ingredient
        var newRecipe = new NewRecipe
        {
            Name = recipeName,
            Ingredients =
            [
                new RecipeIngredient { Name = ingredientName1, Unit = "Grams", Amount = 100m }
            ]
        };

        var (recipeId, _, _) = await PostRecipeAsync(client, newRecipe);

        // Update the recipe with a different ingredient
        var updatedRecipe = new NewRecipe
        {
            Name = updatedRecipeName,
            Ingredients =
            [
                new RecipeIngredient { Name = ingredientName2, Unit = "Millilitres", Amount = 200m }
            ]
        };

        var putContent = new StringContent(JsonSerializer.Serialize(updatedRecipe, jsonOptions), Encoding.UTF8, "application/json");
        using var putResponse = await client.PutAsync($"/api/recipe/{recipeId}", putContent);
        await putResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        // GET the recipe and verify the update
        using var getResponse = await client.GetAsync($"/api/recipe/{recipeId}");
        await getResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var getStream = await getResponse.Content.ReadAsStreamAsync();
        using var getDoc = await JsonDocument.ParseAsync(getStream);
        var root = getDoc.RootElement;

        root.GetProperty("name").GetString().Should().Be(updatedRecipeName);
        var ingredients = JsonSerializer.Deserialize<List<RecipeIngredient>>(
            root.GetProperty("ingredients").GetRawText(), jsonOptions)!;
        ingredients.Should().HaveCount(1);
        ingredients[0].Name.Should().Be(ingredientName2);
        ingredients[0].Unit.Should().Be("Millilitres");
        ingredients[0].Amount.Should().Be(200m);
    }

    [Theory, ShortStringAutoData]
    public async Task Create_Recipe_With_Exact_Duplicate_Ingredients_Succeeds(string ingredientName, string recipeName)
    {
        using var client = await fixture.GetHttpClient();

        await PostIngredientAsync(client, ingredientName, [4]);

        var ingredient = new RecipeIngredient { Name = ingredientName, Unit = "Grams", Amount = 100m };
        var newRecipe = new NewRecipe
        {
            Name = recipeName,
            Ingredients = [ingredient, ingredient]
        };

        var (recipeId, returnedName, returnedIngredients) = await PostRecipeAsync(client, newRecipe);

        recipeId.Should().BeGreaterThan(0);
        returnedName.Should().Be(recipeName);
        returnedIngredients.Should().HaveCount(1);
        returnedIngredients[0].Name.Should().Be(ingredientName);
    }

    [Theory, ShortStringAutoData]
    public async Task Create_Recipe_With_Conflicting_Duplicate_Ingredients_Returns_BadRequest(string ingredientName, string recipeName)
    {
        using var client = await fixture.GetHttpClient();

        await PostIngredientAsync(client, ingredientName, [4]);

        var newRecipe = new NewRecipe
        {
            Name = recipeName,
            Ingredients =
            [
                new RecipeIngredient { Name = ingredientName, Unit = "Grams", Amount = 100m },
                new RecipeIngredient { Name = ingredientName, Unit = "Grams", Amount = 200m }
            ]
        };

        var content = new StringContent(JsonSerializer.Serialize(newRecipe, jsonOptions), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/recipe", content);

        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Theory, ShortStringAutoData]
    public async Task Update_Recipe_With_Exact_Duplicate_Ingredients_Succeeds(string ingredientName, string recipeName, string updatedRecipeName)
    {
        using var client = await fixture.GetHttpClient();

        await PostIngredientAsync(client, ingredientName, [4]);

        var (recipeId, _, _) = await PostRecipeAsync(client, new NewRecipe
        {
            Name = recipeName,
            Ingredients = [new RecipeIngredient { Name = ingredientName, Unit = "Grams", Amount = 100m }]
        });

        var ingredient = new RecipeIngredient { Name = ingredientName, Unit = "Grams", Amount = 150m };
        var updatedRecipe = new NewRecipe
        {
            Name = updatedRecipeName,
            Ingredients = [ingredient, ingredient]
        };

        var content = new StringContent(JsonSerializer.Serialize(updatedRecipe, jsonOptions), Encoding.UTF8, "application/json");
        using var putResponse = await client.PutAsync($"/api/recipe/{recipeId}", content);
        await putResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var getResponse = await client.GetAsync($"/api/recipe/{recipeId}");
        await getResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var getStream = await getResponse.Content.ReadAsStreamAsync();
        using var getDoc = await JsonDocument.ParseAsync(getStream);
        var ingredients = JsonSerializer.Deserialize<List<RecipeIngredient>>(
            getDoc.RootElement.GetProperty("ingredients").GetRawText(), jsonOptions)!;
        ingredients.Should().HaveCount(1);
        ingredients[0].Amount.Should().Be(150m);
    }

    [Theory, ShortStringAutoData]
    public async Task Update_Recipe_With_Conflicting_Duplicate_Ingredients_Returns_BadRequest(string ingredientName, string recipeName, string updatedRecipeName)
    {
        using var client = await fixture.GetHttpClient();

        await PostIngredientAsync(client, ingredientName, [4]);

        var (recipeId, _, _) = await PostRecipeAsync(client, new NewRecipe
        {
            Name = recipeName,
            Ingredients = [new RecipeIngredient { Name = ingredientName, Unit = "Grams", Amount = 100m }]
        });

        var updatedRecipe = new NewRecipe
        {
            Name = updatedRecipeName,
            Ingredients =
            [
                new RecipeIngredient { Name = ingredientName, Unit = "Grams", Amount = 100m },
                new RecipeIngredient { Name = ingredientName, Unit = "Grams", Amount = 999m }
            ]
        };

        var content = new StringContent(JsonSerializer.Serialize(updatedRecipe, jsonOptions), Encoding.UTF8, "application/json");
        using var putResponse = await client.PutAsync($"/api/recipe/{recipeId}", content);
        await putResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    private async Task PostIngredientAsync(HttpClient client, string name, List<int> unitIds)
    {
        var body = new NewIngredient { Name = name, UnitIds = unitIds };
        var content = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/ingredient", content);
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    private async Task<(int Id, string Name, List<RecipeIngredient> Ingredients)> PostRecipeAsync(
        HttpClient client, NewRecipe recipe)
    {
        var content = new StringContent(JsonSerializer.Serialize(recipe, jsonOptions), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/recipe", content);

        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var stream = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        var root = jsonDoc.RootElement;

        var id = root.GetProperty("id").GetInt32();
        var name = root.GetProperty("name").GetString()!;
        var ingredients = JsonSerializer.Deserialize<List<RecipeIngredient>>(
            root.GetProperty("ingredients").GetRawText(), jsonOptions) ?? [];

        return (id, name, ingredients);
    }

    private class NewIngredient
    {
#pragma warning disable S1144 // Unused private types or members should be removed
        public string Name { get; set; }
        public List<int> UnitIds { get; set; }
#pragma warning restore S1144 // Unused private types or members should be removed
    }

    public class NewRecipe
    {
#pragma warning disable S1144 // Unused private types or members should be removed
        public string Name { get; set; }
        public List<RecipeIngredient> Ingredients { get; set; } = [];
#pragma warning restore S1144 // Unused private types or members should be removed
    }

    public class RecipeIngredient
    {
#pragma warning disable S1144 // Unused private types or members should be removed
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal Amount { get; set; }
#pragma warning restore S1144 // Unused private types or members should be removed
    }
}






