using AwesomeAssertions;
using MenuApi.Integration.Tests.Factory;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public sealed class RecipeSearchProjectionIntegrationTests(ApiTestFixture fixture)
{
    [Fact]
    public async Task CreateRecipe_ProjectsCurrentRecipeContentAndAcknowledgesEvent()
    {
        using var client = await fixture.GetHttpClient();
        var title = $"Search projection {Guid.NewGuid():N}";
        var body = new
        {
            Title = title,
            Summary = "A bright lemon cake.",
            AccessScope = "Private",
            Ingredients = new[]
            {
                new { SortOrder = 1, IngredientText = "lemon zest", MeasureText = "1 tbsp", IsOptional = false },
                new { SortOrder = 0, IngredientText = "plain flour", MeasureText = "200 g", IsOptional = false },
            },
            Steps = new[]
            {
                new { SortOrder = 1, InstructionText = "Bake until golden." },
                new { SortOrder = 0, InstructionText = "Whisk the batter." },
            },
        };

        using var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/api/recipe", content);
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var recipeId = doc.RootElement.GetProperty("id").GetGuid();

        var projection = await WaitForProjectionAsync(recipeId, x =>
            x.Title == title &&
            x.SummaryText == "A bright lemon cake." &&
            x.IngredientsText == "plain flour lemon zest" &&
            x.StepsText == "Whisk the batter. Bake until golden.");

        var projectedRecipe = projection
            ?? throw new InvalidOperationException("Recipe search projection was not created.");
        projectedRecipe.AccessScope.Should().Be("Private");
        projectedRecipe.IsPublishedExternally.Should().BeFalse();

        var updatedBody = new
        {
            Title = title,
            Summary = "Updated lemon cake summary.",
            AccessScope = "AuthenticatedUsers",
            Ingredients = new[] { new { SortOrder = 0, IngredientText = "candied lemon", MeasureText = "2 tbsp", IsOptional = false } },
            Steps = new[] { new { SortOrder = 0, InstructionText = "Cool before serving." } },
        };
        using var updateContent = new StringContent(JsonSerializer.Serialize(updatedBody), Encoding.UTF8, "application/json");
        using var updateResponse = await client.PutAsync($"/api/recipe/{recipeId}", updateContent);
        await updateResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        var updatedProjection = await WaitForProjectionAsync(recipeId, x =>
            x.SummaryText == "Updated lemon cake summary." &&
            x.AccessScope == "AuthenticatedUsers" &&
            x.IngredientsText == "candied lemon" &&
            x.StepsText == "Cool before serving.");
        updatedProjection.Should().NotBeNull();

        var events = await TestDatabaseSeeder.GetOutboxEventsForRecipeAsync(
            fixture, recipeId, TestContext.Current.CancellationToken);
        events.Should().HaveCount(2);
        events.Should().OnlyContain(e => e.ProcessedAtUtc != null);
    }

    private async Task<MenuDB.Data.RecipeSearchIndexEntity?> WaitForProjectionAsync(
        Guid recipeId,
        Func<MenuDB.Data.RecipeSearchIndexEntity, bool> matches)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var timeout = DateTime.UtcNow.AddSeconds(15);

        while (DateTime.UtcNow < timeout)
        {
            var projection = await TestDatabaseSeeder.GetRecipeSearchIndexAsync(fixture, recipeId, cancellationToken);
            if (projection is not null && matches(projection))
            {
                return projection;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
        }

        return await TestDatabaseSeeder.GetRecipeSearchIndexAsync(fixture, recipeId, cancellationToken);
    }
}
