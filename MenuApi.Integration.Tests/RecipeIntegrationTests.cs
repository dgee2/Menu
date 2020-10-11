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

            var requestContent = new StringContent(JsonSerializer.Serialize(recipe), Encoding.UTF8, "application/json");
            using var postResponse = await client.PostAsync("/api/recipe", requestContent).ConfigureAwait(false);

            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            using var postStreamResponse = await postResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var postJsonDoc = await JsonDocument.ParseAsync(postStreamResponse).ConfigureAwait(false);

            var (_, name) = GetRecipeFromJson(postJsonDoc);

            name.Should().Be(recipe.Name);
        }

        private (int Id, string Name) GetRecipeFromJson(JsonDocument doc)
        {
            var rootElement = doc.RootElement;
            return (
                rootElement.GetProperty("id").GetInt32(),
                rootElement.GetProperty("name").GetString()
            );
        }

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