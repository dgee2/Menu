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
        [StringLength(50, MinimumLength = 1)] string ingredientName,
        [StringLength(500, MinimumLength = 1)] string recipeName)
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

    [Theory, AutoData]
    public async Task Create_Recipe_With_Duplicate_Equivalent_Ingredients_Returns_Single_Link(
        [StringLength(50, MinimumLength = 1)] string ingredientName,
        [StringLength(500, MinimumLength = 1)] string recipeName)
    {
        using var client = await fixture.GetHttpClient();

        await PostIngredientAsync(client, ingredientName, [4]);

        var newRecipe = new NewRecipe
        {
            Name = recipeName,
            Ingredients =
            [
                new RecipeIngredient { Name = ingredientName, Unit = "Grams", Amount = 250.5m },
                new RecipeIngredient { Name = ingredientName, Unit = "Grams", Amount = 250.5m },
            ]
        };

        var (_, returnedName, returnedIngredients) = await PostRecipeAsync(client, newRecipe);

        returnedName.Should().Be(recipeName);
        returnedIngredients.Should().HaveCount(1);
        returnedIngredients[0].Name.Should().Be(ingredientName);
        returnedIngredients[0].Unit.Should().Be("Grams");
        returnedIngredients[0].Amount.Should().Be(250.5m);
    }

    [Theory, AutoData]
    public async Task Create_Recipe_With_Ingredients_Then_Get_Recipe_Returns_Ingredients(
        [StringLength(50, MinimumLength = 1)] string ingredientName,
        [StringLength(500, MinimumLength = 1)] string recipeName)
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

    [Theory, AutoData]
    public async Task Create_Recipe_With_Ingredients_Then_Get_Recipe_Ingredients_Endpoint(
        [StringLength(50, MinimumLength = 1)] string ingredientName,
        [StringLength(500, MinimumLength = 1)] string recipeName)
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

    [Theory, AutoData]
    public async Task Update_Recipe_With_Different_Ingredients(
        [StringLength(50, MinimumLength = 1)] string ingredientName1,
        [StringLength(50, MinimumLength = 1)] string ingredientName2,
        [StringLength(500, MinimumLength = 1)] string recipeName,
        [StringLength(500, MinimumLength = 1)] string updatedRecipeName)
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

        var (_, returnedName, returnedIngredients) = await PutRecipeAsync(client, recipeId, updatedRecipe);

        returnedName.Should().Be(updatedRecipeName);
        returnedIngredients.Should().HaveCount(1);
        returnedIngredients[0].Name.Should().Be(ingredientName2);
        returnedIngredients[0].Unit.Should().Be("Millilitres");
        returnedIngredients[0].Amount.Should().Be(200m);
    }

    [Theory, AutoData]
    public async Task Update_Recipe_With_Duplicate_Equivalent_Existing_Ingredient_Remains_Single_Link(
        [StringLength(50, MinimumLength = 1)] string ingredientName,
        [StringLength(500, MinimumLength = 1)] string recipeName,
        [StringLength(500, MinimumLength = 1)] string updatedRecipeName)
    {
        using var client = await fixture.GetHttpClient();

        await PostIngredientAsync(client, ingredientName, [4]);

        var newRecipe = new NewRecipe
        {
            Name = recipeName,
            Ingredients =
            [
                new RecipeIngredient { Name = ingredientName, Unit = "Grams", Amount = 100m }
            ]
        };

        var (recipeId, _, _) = await PostRecipeAsync(client, newRecipe);

        var updatedRecipe = new NewRecipe
        {
            Name = updatedRecipeName,
            Ingredients =
            [
                new RecipeIngredient { Name = ingredientName, Unit = "Grams", Amount = 125m },
                new RecipeIngredient { Name = ingredientName, Unit = "Grams", Amount = 125m },
            ]
        };

        var (_, returnedName, returnedIngredients) = await PutRecipeAsync(client, recipeId, updatedRecipe);

        returnedName.Should().Be(updatedRecipeName);
        returnedIngredients.Should().HaveCount(1);
        returnedIngredients[0].Name.Should().Be(ingredientName);
        returnedIngredients[0].Unit.Should().Be("Grams");
        returnedIngredients[0].Amount.Should().Be(125m);
    }

    [Theory, AutoData]
    public async Task Create_Recipe_With_Conflicting_Ingredient_Amounts_Returns_UnprocessableEntity(
        [StringLength(50, MinimumLength = 1)] string ingredientName,
        [StringLength(500, MinimumLength = 1)] string recipeName)
    {
        using var client = await fixture.GetHttpClient();

        await PostIngredientAsync(client, ingredientName, [4]);

        var newRecipe = new NewRecipe
        {
            Name = recipeName,
            Ingredients =
            [
                new RecipeIngredient { Name = ingredientName, Unit = "Grams", Amount = 100m },
                new RecipeIngredient { Name = ingredientName, Unit = "Grams", Amount = 200m },
            ]
        };

        using var content = new StringContent(JsonSerializer.Serialize(newRecipe, jsonOptions), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/recipe", content);

        await response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        var responseBody = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseBody);
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(422);
    }

    [Theory, AutoData]
    public async Task Update_Recipe_With_Conflicting_Ingredient_Amounts_Returns_UnprocessableEntity(
        [StringLength(50, MinimumLength = 1)] string ingredientName,
        [StringLength(500, MinimumLength = 1)] string recipeName,
        [StringLength(500, MinimumLength = 1)] string updatedRecipeName)
    {
        using var client = await fixture.GetHttpClient();

        await PostIngredientAsync(client, ingredientName, [4]);

        var (recipeId, _, _) = await PostRecipeAsync(client, new NewRecipe
        {
            Name = recipeName,
            Ingredients = [new RecipeIngredient { Name = ingredientName, Unit = "Grams", Amount = 100m }]
        });

        var updateBody = new NewRecipe
        {
            Name = updatedRecipeName,
            Ingredients =
            [
                new RecipeIngredient { Name = ingredientName, Unit = "Grams", Amount = 100m },
                new RecipeIngredient { Name = ingredientName, Unit = "Grams", Amount = 200m },
            ]
        };

        using var content = new StringContent(JsonSerializer.Serialize(updateBody, jsonOptions), Encoding.UTF8, "application/json");
        using var response = await client.PutAsync($"/api/recipe/{recipeId}", content);

        await response.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        var responseBody = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseBody);
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(422);
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
        using var content = new StringContent(JsonSerializer.Serialize(recipe, jsonOptions), Encoding.UTF8, "application/json");
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

    private async Task<(int Id, string Name, List<RecipeIngredient> Ingredients)> PutRecipeAsync(
        HttpClient client, int id, NewRecipe recipe)
    {
        using var content = new StringContent(JsonSerializer.Serialize(recipe, jsonOptions), Encoding.UTF8, "application/json");
        using var response = await client.PutAsync($"/api/recipe/{id}", content);

        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var stream = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        var root = jsonDoc.RootElement;

        var updatedId = root.GetProperty("id").GetInt32();
        var name = root.GetProperty("name").GetString()!;
        var ingredients = JsonSerializer.Deserialize<List<RecipeIngredient>>(
            root.GetProperty("ingredients").GetRawText(), jsonOptions) ?? [];

        return (updatedId, name, ingredients);
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






