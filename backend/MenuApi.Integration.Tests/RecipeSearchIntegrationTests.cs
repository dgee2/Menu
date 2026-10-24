using AwesomeAssertions;
using MenuApi.Integration.Tests.Factory;
using System.Net;
using System.Text.Json;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public sealed class RecipeSearchIntegrationTests(ApiTestFixture fixture)
{
    [Fact]
    public async Task Search_ReturnsTitleMatchesAheadOfIngredientMatches()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = await fixture.GetHttpClient();
        const string term = "saffron cake";
        var titleOwner = await TestDatabaseSeeder.AddMenuUserAsync(fixture, $"search-title-{Guid.NewGuid():N}", cancellationToken);
        var ingredientOwner = await TestDatabaseSeeder.AddMenuUserAsync(fixture, $"search-ingredient-{Guid.NewGuid():N}", cancellationToken);
        var title = $"Saffron recipe {Guid.NewGuid():N}";
        var ingredientOnlyTitle = $"Other recipe {Guid.NewGuid():N}";
        var titleRecipeId = await TestDatabaseSeeder.AddRecipeAsync(fixture, title, titleOwner, "AuthenticatedUsers", cancellationToken);
        var ingredientRecipeId = await TestDatabaseSeeder.AddRecipeAsync(fixture, ingredientOnlyTitle, ingredientOwner, "AuthenticatedUsers", cancellationToken);
        await TestDatabaseSeeder.AddRecipeSearchIndexAsync(
            fixture, titleRecipeId, titleOwner, title, "rice", null, "AuthenticatedUsers", cancellationToken);
        await TestDatabaseSeeder.AddRecipeSearchIndexAsync(
            fixture, ingredientRecipeId, ingredientOwner, ingredientOnlyTitle, $"{term} threads", null, "AuthenticatedUsers", cancellationToken);
        await TestDatabaseSeeder.WaitForFullTextIndexingAsync(fixture, titleRecipeId, "saffron", cancellationToken);
        await TestDatabaseSeeder.WaitForFullTextIndexingAsync(fixture, ingredientRecipeId, "saffron", cancellationToken);

        using var response = await client.GetAsync(
            $"/api/recipe?scope=authenticated&query={Uri.EscapeDataString(term)}&take=20",
            cancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var titles = document.RootElement.EnumerateArray()
            .Select(recipe => recipe.GetProperty("title").GetString())
            .ToList();

        titles.Should().Contain(title);
        titles.Should().Contain(ingredientOnlyTitle);
        titles.IndexOf(title).Should().BeLessThan(titles.IndexOf(ingredientOnlyTitle));
    }

    [Fact]
    public async Task Search_AuthenticatedScopeDoesNotExposeAnotherUsersPrivateRecipe()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = await fixture.GetHttpClient();
        var owner = await TestDatabaseSeeder.AddMenuUserAsync(fixture, $"search-private-{Guid.NewGuid():N}", cancellationToken);
        var privateTitle = $"Secret saffron {Guid.NewGuid():N}";
        var sharedTitle = $"Shared saffron {Guid.NewGuid():N}";
        var privateRecipe = await TestDatabaseSeeder.AddRecipeAsync(fixture, privateTitle, owner, "Private", cancellationToken);
        var sharedRecipe = await TestDatabaseSeeder.AddRecipeAsync(fixture, sharedTitle, owner, "AuthenticatedUsers", cancellationToken);
        await TestDatabaseSeeder.AddRecipeSearchIndexAsync(
            fixture, privateRecipe, owner, privateTitle, "saffron", null, "Private", cancellationToken);
        await TestDatabaseSeeder.AddRecipeSearchIndexAsync(
            fixture, sharedRecipe, owner, sharedTitle, "saffron", null, "AuthenticatedUsers", cancellationToken);
        await TestDatabaseSeeder.WaitForFullTextIndexingAsync(fixture, privateRecipe, "saffron", cancellationToken);
        await TestDatabaseSeeder.WaitForFullTextIndexingAsync(fixture, sharedRecipe, "saffron", cancellationToken);

        using var response = await client.GetAsync(
            $"/api/recipe?scope=authenticated&query={Uri.EscapeDataString("saffron OR (*)")}&take=20",
            cancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var titles = document.RootElement.EnumerateArray()
            .Select(recipe => recipe.GetProperty("title").GetString())
            .ToHashSet();

        titles.Should().Contain(sharedTitle);
        titles.Should().NotContain(privateTitle);
    }
}
