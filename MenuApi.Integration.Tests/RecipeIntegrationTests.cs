using AutoFixture.NUnit3;
using FluentAssertions;
using MenuApi.Integration.Tests.Factory;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MenuApi.Integration.Tests
{
    public class RecipeIntegrationTests : IntegrationBaseClass
    {
        readonly JsonSerializerOptions jsonOptions;

        public RecipeIntegrationTests()
        {
            jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        [Test]
        public async Task Get_ReturnsAListOfCategories_CategoriesController()
        {
            using var client = Factory.CreateClient();
            using var response = await client.GetAsync("/api/recipe").ConfigureAwait(false);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var deserializedData = JsonSerializer.Deserialize<HashSet<Recipe>>(data, jsonOptions);
            deserializedData.Should().NotBeNull();
        }

        [Test, AutoData]
        public async Task Create_Recipe(NewRecipe recipe)
        {
            using var client = Factory.CreateClient();

            var (_, name) = await PostRecipeAsync(client, recipe);

            name.Should().Be(recipe.Name);
        }

        [Test, AutoData]
        public async Task Create_and_Update_Recipe(NewRecipe recipe, NewRecipe newRecipe)
        {
            using var client = Factory.CreateClient();

            var (id, _) = await PostRecipeAsync(client, recipe);

            var (_, name) = await PutRecipeAsync(client, id, newRecipe);

            name.Should().Be(newRecipe.Name);
        }

        [Test, AutoData]
        public async Task Create_And_Get_Recipe(NewRecipe recipe)
        {
            using var client = Factory.CreateClient();

            var (id, _) = await PostRecipeAsync(client, recipe);
            var (getId, name) = await GetRecipeAsync(client, id);

            getId.Should().Be(id);
            name.Should().Be(recipe.Name);
        }

        private static async Task<(int Id, string Name)> PostRecipeAsync(HttpClient client, NewRecipe recipe)
        {
            var requestContent = new StringContent(JsonSerializer.Serialize(recipe), Encoding.UTF8, "application/json");
            using var response = await client.PostAsync("/api/recipe", requestContent).ConfigureAwait(false);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var streamResponse = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var jsonDoc = await JsonDocument.ParseAsync(streamResponse).ConfigureAwait(false);

            return GetRecipeFromJson(jsonDoc);
        }

        private static async Task<(int Id, string Name)> PutRecipeAsync(HttpClient client, int id, NewRecipe recipe)
        {
            var requestContent = new StringContent(JsonSerializer.Serialize(recipe), Encoding.UTF8, "application/json");
            using var response = await client.PutAsync($"/api/recipe/{id}", requestContent).ConfigureAwait(false);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var streamResponse = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var jsonDoc = await JsonDocument.ParseAsync(streamResponse).ConfigureAwait(false);

            return GetRecipeFromJson(jsonDoc);
        }

        private static async Task<(int Id, string Name)> GetRecipeAsync(HttpClient client, int id)
        {
            using var response = await client.GetAsync($"/api/recipe/{id}").ConfigureAwait(false);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var streamResponse = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var jsonDoc = await JsonDocument.ParseAsync(streamResponse).ConfigureAwait(false);

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed")]
        private class Recipe
        {
            public int Id { get; set; }

            public string Name { get; set; }
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
}