using AwesomeAssertions;
using MenuDB;
using MenuDB.Data;
using MenuApi.Repositories;
using MenuApi.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MenuApi.Tests.Repositories;

// UpdateRecipeAsync uses ExecuteUpdateAsync, which is not supported by the EF Core
// InMemory provider (it requires a relational provider). That code path is covered by
// the RecipeRepository source review rather than a unit test here, to avoid pulling in
// an additional relational test provider (e.g. SQLite) purely for test infrastructure.
public class RecipeRepositoryTests
{
    [Fact]
    public async Task CreateRecipeAsync_Sets_Audit_Timestamps()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = CreateDbContext();
        var sut = new RecipeRepository(db);

        var recipeId = await sut.CreateRecipeAsync(new DBModel.Recipe
        {
            Title = RecipeTitle.From("Created Recipe"),
            AccessScope = RecipeAccessScope.Private,
        });

        var entity = await db.Recipes
            .AsNoTracking()
            .SingleAsync(r => r.Id == recipeId.Value, cancellationToken);

        entity.CreatedAtUtc.Should().NotBe(default);
        entity.UpdatedAtUtc.Should().NotBe(default);
        entity.UpdatedAtUtc.Should().Be(entity.CreatedAtUtc);
    }

    [Fact]
    public async Task GetRecipesAsync_Mine_ReturnsOnlyCallersRecipes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = CreateDbContext();
        var sut = new RecipeRepository(db);

        var owner = MenuUserId.From(1);
        var otherOwner = MenuUserId.From(2);
        AddRecipe(db, "Mine 1", owner, RecipeAccessScope.Private);
        AddRecipe(db, "Someone Else's", otherOwner, RecipeAccessScope.Private);
        await db.SaveChangesAsync(cancellationToken);

        var result = await sut.GetRecipesAsync(RecipeListScope.Mine, owner, 50);

        result.Should().ContainSingle(r => r.Title == RecipeTitle.From("Mine 1"));
    }

    [Fact]
    public async Task GetRecipesAsync_Authenticated_ReturnsAuthenticatedScopedRecipesRegardlessOfOwner()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = CreateDbContext();
        var sut = new RecipeRepository(db);

        var caller = MenuUserId.From(1);
        var otherOwner = MenuUserId.From(2);
        AddRecipe(db, "Private Mine", caller, RecipeAccessScope.Private);
        AddRecipe(db, "Shared By Me", caller, RecipeAccessScope.AuthenticatedUsers);
        AddRecipe(db, "Shared By Someone Else", otherOwner, RecipeAccessScope.AuthenticatedUsers);
        await db.SaveChangesAsync(cancellationToken);

        var result = await sut.GetRecipesAsync(RecipeListScope.Authenticated, caller, 50);

        result.Select(r => r.Title.Value).Should().BeEquivalentTo(["Shared By Me", "Shared By Someone Else"]);
    }

    [Fact]
    public async Task GetRecipesAsync_RespectsTakeLimit()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = CreateDbContext();
        var sut = new RecipeRepository(db);

        var owner = MenuUserId.From(1);
        AddRecipe(db, "Recipe 1", owner, RecipeAccessScope.Private);
        AddRecipe(db, "Recipe 2", owner, RecipeAccessScope.Private);
        AddRecipe(db, "Recipe 3", owner, RecipeAccessScope.Private);
        await db.SaveChangesAsync(cancellationToken);

        var result = await sut.GetRecipesAsync(RecipeListScope.Mine, owner, 2);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetReadableRecipeAsync_OwnPrivateRecipe_IsReturned()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = CreateDbContext();
        var sut = new RecipeRepository(db);

        var caller = MenuUserId.From(1);
        AddRecipe(db, "Private Mine", caller, RecipeAccessScope.Private);
        await db.SaveChangesAsync(cancellationToken);
        var recipeId = RecipeId.From(db.Recipes.Single().Id);

        var result = await sut.GetReadableRecipeAsync(recipeId, caller);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetReadableRecipeAsync_SomeoneElsesPrivateRecipe_IsNotReturned()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = CreateDbContext();
        var sut = new RecipeRepository(db);

        var caller = MenuUserId.From(1);
        var otherOwner = MenuUserId.From(2);
        AddRecipe(db, "Private Theirs", otherOwner, RecipeAccessScope.Private);
        await db.SaveChangesAsync(cancellationToken);
        var recipeId = RecipeId.From(db.Recipes.Single().Id);

        var result = await sut.GetReadableRecipeAsync(recipeId, caller);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetReadableRecipeAsync_SomeoneElsesSharedRecipe_IsReturned()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = CreateDbContext();
        var sut = new RecipeRepository(db);

        var caller = MenuUserId.From(1);
        var otherOwner = MenuUserId.From(2);
        AddRecipe(db, "Shared Theirs", otherOwner, RecipeAccessScope.AuthenticatedUsers);
        await db.SaveChangesAsync(cancellationToken);
        var recipeId = RecipeId.From(db.Recipes.Single().Id);

        var result = await sut.GetReadableRecipeAsync(recipeId, caller);

        result.Should().NotBeNull();
        result!.AccessScope.Should().Be(RecipeAccessScope.AuthenticatedUsers);
    }

    [Fact]
    public async Task GetRecipeIngredientsAsync_SomeoneElsesPrivateRecipe_ReturnsNothing()
    {
        // Ingredients must not become a side channel onto a recipe the caller cannot read.
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = CreateDbContext();
        var sut = new RecipeRepository(db);

        var caller = MenuUserId.From(1);
        var otherOwner = MenuUserId.From(2);
        AddRecipe(db, "Private Theirs", otherOwner, RecipeAccessScope.Private);
        await db.SaveChangesAsync(cancellationToken);
        var recipeId = RecipeId.From(db.Recipes.Single().Id);

        db.RecipeIngredients.Add(new RecipeIngredientEntity
        {
            RecipeId = recipeId.Value,
            SortOrder = 0,
            IngredientText = "Secret Ingredient",
            MeasureText = "1",
        });
        await db.SaveChangesAsync(cancellationToken);

        var result = await sut.GetRecipeIngredientsAsync(recipeId, caller);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecipeIngredientsAsync_OwnRecipe_ReturnsIngredientsInSortOrder()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = CreateDbContext();
        var sut = new RecipeRepository(db);

        var caller = MenuUserId.From(1);
        AddRecipe(db, "Mine", caller, RecipeAccessScope.Private);
        await db.SaveChangesAsync(cancellationToken);
        var recipeId = RecipeId.From(db.Recipes.Single().Id);

        db.RecipeIngredients.AddRange(
            new RecipeIngredientEntity { RecipeId = recipeId.Value, SortOrder = 1, IngredientText = "Second", MeasureText = "1" },
            new RecipeIngredientEntity { RecipeId = recipeId.Value, SortOrder = 0, IngredientText = "First", MeasureText = "1" });
        await db.SaveChangesAsync(cancellationToken);

        var result = await sut.GetRecipeIngredientsAsync(recipeId, caller);

        result.Select(i => i.IngredientText).Should().Equal("First", "Second");
    }

    private static void AddRecipe(MenuDbContext db, string title, MenuUserId ownerId, RecipeAccessScope accessScope)
    {
        db.Recipes.Add(new RecipeEntity
        {
            Title = title,
            OwnerUserId = ownerId.Value,
            AccessScopeId = (byte)accessScope,
        });
    }

    private static MenuDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new MenuDbContext(options);
    }
}
