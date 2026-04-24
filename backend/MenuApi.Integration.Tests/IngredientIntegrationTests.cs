using AwesomeAssertions;
using MenuApi.Integration.Tests.Factory;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public class IngredientIntegrationTests : IClassFixture<ApiTestFixture>
{
    private readonly JsonSerializerOptions jsonOptions;
    private readonly ApiTestFixture fixture;

    public IngredientIntegrationTests(ApiTestFixture fixture)
    {
        jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        this.fixture = fixture;
    }

    [Fact]
    public async Task Get_Ingredients_Returns_Ok()
    {
        using var client = await fixture.GetHttpClient();
        using var response = await client.GetAsync("/api/ingredient");

        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        var data = await response.Content.ReadAsStringAsync();
        var ingredients = JsonSerializer.Deserialize<List<Ingredient>>(data, jsonOptions);
        ingredients.Should().NotBeNull();
    }

    [Fact]
    public async Task Get_Ingredient_Units_Returns_Seeded_Units()
    {
        using var client = await fixture.GetHttpClient();
        using var response = await client.GetAsync("/api/ingredient/unit");

        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        var data = await response.Content.ReadAsStringAsync();
        var units = JsonSerializer.Deserialize<List<IngredientUnit>>(data, jsonOptions);
        units.Should().NotBeNull();
        units.Should().HaveCount(5);

        units.Should().ContainSingle(u => u.Name == "Millilitres" && u.Abbreviation == "ml" && u.Type == "Volume");
        units.Should().ContainSingle(u => u.Name == "Litres" && u.Abbreviation == "l" && u.Type == "Volume");
        units.Should().ContainSingle(u => u.Name == "Quantity" && u.Abbreviation == null && u.Type == "Quantity");
        units.Should().ContainSingle(u => u.Name == "Grams" && u.Abbreviation == "g" && u.Type == "Weight");
        units.Should().ContainSingle(u => u.Name == "Kilograms" && u.Abbreviation == "kg" && u.Type == "Weight");
    }

    [Theory, ShortStringAutoData]
    public async Task Create_Ingredient(string ingredientName)
    {
        using var client = await fixture.GetHttpClient();

        var (id, name, units) = await PostIngredientAsync(client, ingredientName, [1, 4]);

        id.Should().BeGreaterThan(0);
        name.Should().Be(ingredientName);
        units.Should().HaveCount(2);
        units.Should().ContainSingle(u => u.Name == "Millilitres");
        units.Should().ContainSingle(u => u.Name == "Grams");
    }

    [Theory, ShortStringAutoData]
    public async Task Create_Ingredient_Then_Get_Ingredients_Contains_Created(string ingredientName)
    {
        using var client = await fixture.GetHttpClient();

        var (createdId, _, _) = await PostIngredientAsync(client, ingredientName, [3]);

        using var response = await client.GetAsync("/api/ingredient");
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        var data = await response.Content.ReadAsStringAsync();
        var ingredients = JsonSerializer.Deserialize<List<Ingredient>>(data, jsonOptions);
        ingredients.Should().NotBeNull();
        ingredients.Should().Contain(i => i.Id == createdId && i.Name == ingredientName);
    }

    [Theory, ShortStringAutoData]
    public async Task Create_Ingredient_Same_Name_Same_Units_Returns_Existing(string ingredientName)
    {
        using var client = await fixture.GetHttpClient();

        var (firstId, _, _) = await PostIngredientAsync(client, ingredientName, [1, 4]);
        var (secondId, _, _) = await PostIngredientAsync(client, ingredientName, [1, 4]);

        firstId.Should().Be(secondId);
    }

    [Theory, ShortStringAutoData]
    public async Task Create_Ingredient_Same_Name_Different_Units_Returns_Conflict(string ingredientName)
    {
        using var client = await fixture.GetHttpClient();

        await PostIngredientAsync(client, ingredientName, [1]);

        using var response = await PostIngredientRawAsync(client, ingredientName, [4]);

        await response.ShouldHaveStatusCode(HttpStatusCode.Conflict);
    }

    [Theory, ShortStringAutoData]
    public async Task Create_Ingredient_Duplicate_UnitIds_Are_Deduplicated(string ingredientName)
    {
        using var client = await fixture.GetHttpClient();

        var (id, name, units) = await PostIngredientAsync(client, ingredientName, [1, 1, 4]);

        id.Should().BeGreaterThan(0);
        name.Should().Be(ingredientName);
        units.Should().HaveCount(2);
        units.Should().ContainSingle(u => u.Name == "Millilitres");
        units.Should().ContainSingle(u => u.Name == "Grams");
    }

    [Theory, ShortStringAutoData]
    public async Task Create_Ingredient_Duplicate_UnitIds_Then_Create_Again_Reuses(string ingredientName)
    {
        using var client = await fixture.GetHttpClient();

        // First request has a duplicate unit ID - should be collapsed to {1, 4}
        var (firstId, _, _) = await PostIngredientAsync(client, ingredientName, [1, 1, 4]);

        // Second request with the canonical set {1, 4} - should reuse
        var (secondId, _, _) = await PostIngredientAsync(client, ingredientName, [1, 4]);

        firstId.Should().Be(secondId);
    }

    private async Task<HttpResponseMessage> PostIngredientRawAsync(
        HttpClient client, string name, List<int> unitIds)
    {
        var body = new NewIngredient { Name = name, UnitIds = unitIds };
        var requestContent = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, "application/json");
        return await client.PostAsync("/api/ingredient", requestContent);
    }

    internal async Task<(int Id, string Name, List<IngredientUnit> Units)> PostIngredientAsync(
        HttpClient client, string name, List<int> unitIds)
    {
        var body = new NewIngredient { Name = name, UnitIds = unitIds };
        var requestContent = new StringContent(JsonSerializer.Serialize(body, jsonOptions), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/ingredient", requestContent);

        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var stream = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(stream);

        var root = jsonDoc.RootElement;
        var id = root.GetProperty("id").GetInt32();
        var ingredientName = root.GetProperty("name").GetString()!;
        var units = JsonSerializer.Deserialize<List<IngredientUnit>>(
            root.GetProperty("units").GetRawText(), jsonOptions)!;

        return (id, ingredientName, units);
    }

    private class Ingredient
    {
#pragma warning disable S1144 // Unused private types or members should be removed
        public int Id { get; set; }
        public string Name { get; set; }
        public List<IngredientUnit> Units { get; set; }
#pragma warning restore S1144 // Unused private types or members should be removed
    }

    public class IngredientUnit
    {
#pragma warning disable S1144 // Unused private types or members should be removed
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public string Type { get; set; }
#pragma warning restore S1144 // Unused private types or members should be removed
    }

    public class NewIngredient
    {
        public string Name { get; set; }
        public List<int> UnitIds { get; set; }
    }
}






