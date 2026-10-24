using AwesomeAssertions;
using MenuApi.Integration.Tests.Factory;
using System.Net;
using System.Text.Json;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public sealed class RecipeSearchIntegrationTests(ApiTestFixture fixture)
{
    private const string Saffron = "saffron";
    private const string AuthenticatedAccessScope = "AuthenticatedUsers";

    [Fact]
    public async Task Search_ReturnsTitleMatchesAheadOfIngredientMatches()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = await fixture.GetHttpClient();
        var term = $"{Saffron} cake";
        var titleOwner = await TestDatabaseSeeder.AddMenuUserAsync(fixture, $"search-title-{Guid.NewGuid():N}", cancellationToken);
        var ingredientOwner = await TestDatabaseSeeder.AddMenuUserAsync(fixture, $"search-ingredient-{Guid.NewGuid():N}", cancellationToken);
        var title = $"Saffron recipe {Guid.NewGuid():N}";
        var ingredientOnlyTitle = $"Other recipe {Guid.NewGuid():N}";
        var titleRecipeId = await TestDatabaseSeeder.AddRecipeAsync(fixture, title, titleOwner, AuthenticatedAccessScope, cancellationToken);
        var ingredientRecipeId = await TestDatabaseSeeder.AddRecipeAsync(fixture, ingredientOnlyTitle, ingredientOwner, AuthenticatedAccessScope, cancellationToken);
        await TestDatabaseSeeder.AddRecipeSearchIndexAsync(
            fixture,
            new RecipeSearchIndexSeed(titleRecipeId, titleOwner, title, "rice", null, AuthenticatedAccessScope),
            cancellationToken);
        await TestDatabaseSeeder.AddRecipeSearchIndexAsync(
            fixture,
            new RecipeSearchIndexSeed(ingredientRecipeId, ingredientOwner, ingredientOnlyTitle, $"{term} threads", null, AuthenticatedAccessScope),
            cancellationToken);
        await TestDatabaseSeeder.WaitForFullTextIndexingAsync(fixture, titleRecipeId, Saffron, cancellationToken);
        await TestDatabaseSeeder.WaitForFullTextIndexingAsync(fixture, ingredientRecipeId, Saffron, cancellationToken);

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
        var privateTitle = $"Secret {Saffron} {Guid.NewGuid():N}";
        var sharedTitle = $"Shared {Saffron} {Guid.NewGuid():N}";
        var privateRecipe = await TestDatabaseSeeder.AddRecipeAsync(fixture, privateTitle, owner, "Private", cancellationToken);
        var sharedRecipe = await TestDatabaseSeeder.AddRecipeAsync(fixture, sharedTitle, owner, AuthenticatedAccessScope, cancellationToken);
        await TestDatabaseSeeder.AddRecipeSearchIndexAsync(
            fixture, new RecipeSearchIndexSeed(privateRecipe, owner, privateTitle, Saffron, null, "Private"), cancellationToken);
        await TestDatabaseSeeder.AddRecipeSearchIndexAsync(
            fixture, new RecipeSearchIndexSeed(sharedRecipe, owner, sharedTitle, Saffron, null, AuthenticatedAccessScope), cancellationToken);
        await TestDatabaseSeeder.WaitForFullTextIndexingAsync(fixture, privateRecipe, Saffron, cancellationToken);
        await TestDatabaseSeeder.WaitForFullTextIndexingAsync(fixture, sharedRecipe, Saffron, cancellationToken);

        using var response = await client.GetAsync(
            $"/api/recipe?scope=authenticated&query={Uri.EscapeDataString($"{Saffron} OR (*)")}&take=20",
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
