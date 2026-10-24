using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using MenuDB;
using MenuDB.Data;
using MenuApi.Authorization;
using MenuApi.DBModel;
using MenuApi.Exceptions;
using MenuApi.ValueObjects;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MenuApi.Repositories;

[ExcludeFromCodeCoverage]
public class RecipeRepository(MenuDbContext db) : IRecipeRepository
{
    // Kept as an Expression (not a method) so EF Core can translate it inside .Select - a compiled
    // method call there isn't translatable and forces client-side evaluation of a tracked entity.
    private static readonly Expression<Func<RecipeEntity, DBModel.Recipe>> ToDbModel = r => new DBModel.Recipe
    {
        Id = RecipeId.From(r.Id),
        Title = RecipeTitle.From(r.Title),
        AccessScope = (RecipeAccessScope)r.AccessScopeId,
        OwnerUserId = MenuUserId.From(r.OwnerUserId),
        Summary = r.Summary,
        Servings = r.Servings,
        YieldText = r.YieldText,
        PrepTimeMinutes = r.PrepTimeMinutes,
        CookTimeMinutes = r.CookTimeMinutes,
        TotalTimeMinutes = r.TotalTimeMinutes,
        CreatedAtUtc = r.CreatedAtUtc,
        UpdatedAtUtc = r.UpdatedAtUtc,
    };

    public async Task<IEnumerable<DBModel.Recipe>> GetRecipesAsync(RecipeListScope scope, MenuUserId callerId, int take)
    {
        var query = scope switch
        {
            RecipeListScope.Mine => db.Recipes.Where(r => r.OwnerUserId == callerId.Value),
            RecipeListScope.Authenticated => db.Recipes.Where(r => r.AccessScopeId == (byte)RecipeAccessScope.AuthenticatedUsers),
            _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unsupported recipe list scope."),
        };

        return await query
            .OrderByDescending(r => r.UpdatedAtUtc)
            .Take(take)
            .Select(ToDbModel)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<DBModel.Recipe>> SearchRecipesAsync(RecipeListScope scope, MenuUserId callerId, int take, string query)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var searchCondition = BuildContainsSearchCondition(query);
        if (searchCondition.Length == 0)
        {
            return [];
        }

        var mine = scope switch
        {
            RecipeListScope.Mine => true,
            RecipeListScope.Authenticated => false,
            _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unsupported recipe list scope."),
        };
        var rows = await db.Database.SqlQuery<RecipeSearchRow>($"""
            SELECT TOP ({take})
                recipe.Id,
                recipe.Title,
                recipe.AccessScopeId,
                recipe.OwnerUserId,
                recipe.Summary,
                recipe.Servings,
                recipe.YieldText,
                recipe.PrepTimeMinutes,
                recipe.CookTimeMinutes,
                recipe.TotalTimeMinutes,
                recipe.CreatedAtUtc,
                recipe.UpdatedAtUtc,
                matches.RankScore
            FROM dbo.Recipe AS recipe
            INNER JOIN dbo.RecipeSearchIndex AS searchIndex ON searchIndex.RecipeId = recipe.Id
            INNER JOIN (
                SELECT matched.RecipeId, SUM(matched.Score) AS RankScore
                FROM (
                    SELECT [KEY] AS RecipeId, CAST([RANK] AS int) * 3 AS Score
                    FROM CONTAINSTABLE(dbo.RecipeSearchIndex, Title, {searchCondition})
                    UNION ALL
                    SELECT [KEY] AS RecipeId, CAST([RANK] AS int) AS Score
                    FROM CONTAINSTABLE(dbo.RecipeSearchIndex, (SummaryText, IngredientsText, StepsText), {searchCondition})
                ) AS matched
                GROUP BY matched.RecipeId
            ) AS matches ON matches.RecipeId = searchIndex.RecipeId
            WHERE recipe.DeletedAtUtc IS NULL
                AND (({mine} = 1 AND searchIndex.OwnerUserId = {callerId.Value})
                    OR ({!mine} = 1 AND searchIndex.AccessScope = 'AuthenticatedUsers'))
            ORDER BY matches.RankScore DESC, recipe.UpdatedAtUtc DESC, recipe.Id
            """)
            .ToListAsync()
            .ConfigureAwait(false);

        return rows.Select(row => new DBModel.Recipe
        {
            Id = RecipeId.From(row.Id),
            Title = RecipeTitle.From(row.Title),
            AccessScope = row.AccessScopeId switch
            {
                (byte)RecipeAccessScope.Private => RecipeAccessScope.Private,
                (byte)RecipeAccessScope.AuthenticatedUsers => RecipeAccessScope.AuthenticatedUsers,
                _ => throw new InvalidOperationException($"Recipe {row.Id} has an unknown access scope."),
            },
            OwnerUserId = MenuUserId.From(row.OwnerUserId),
            Summary = row.Summary,
            Servings = row.Servings,
            YieldText = row.YieldText,
            PrepTimeMinutes = row.PrepTimeMinutes,
            CookTimeMinutes = row.CookTimeMinutes,
            TotalTimeMinutes = row.TotalTimeMinutes,
            CreatedAtUtc = row.CreatedAtUtc,
            UpdatedAtUtc = row.UpdatedAtUtc,
        }).ToList();
    }

    private static string BuildContainsSearchCondition(string query)
    {
        // User text is not SQL Server full-text syntax. Treat each word as a literal search term
        // and combine the terms ourselves so quotes, parentheses, and operators cannot change the
        // condition or make CONTAINSTABLE reject an ordinary search.
        return string.Join(
            " OR ",
            Regex.Matches(query, @"[\p{L}\p{M}\p{N}]+", RegexOptions.None, TimeSpan.FromSeconds(1))
                .Cast<Match>()
                .Select(match => match.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(term => $"\"{term}\""));
    }

    public async Task<DBModel.Recipe?> GetRecipeAsync(RecipeId recipeId)
    {
        return await db.Recipes
            .Where(r => r.Id == recipeId.Value)
            .Select(ToDbModel)
            .AsNoTracking()
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<DBModel.Recipe?> GetReadableRecipeAsync(RecipeId recipeId, MenuUserId callerId)
    {
        return await db.Recipes
            .Where(r => r.Id == recipeId.Value)
            .Where(RecipeAccessRules.ReadableBy(callerId))
            .Select(ToDbModel)
            .AsNoTracking()
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<DBModel.RecipeIngredient>> GetRecipeIngredientsAsync(RecipeId recipeId, MenuUserId callerId)
    {
        // Filtered through the parent recipe rather than fetched directly, so ingredients cannot be
        // used as a side channel onto a recipe the caller is not allowed to read.
        return await db.Recipes
            .Where(r => r.Id == recipeId.Value)
            .Where(RecipeAccessRules.ReadableBy(callerId))
            .SelectMany(r => r.RecipeIngredients)
            .OrderBy(ri => ri.SortOrder)
            .Select(ri => new DBModel.RecipeIngredient(
                ri.SortOrder,
                ri.IngredientText,
                ri.MeasureText,
                ri.SectionTitle,
                ri.Amount,
                ri.UnitText,
                ri.PreparationText,
                ri.IsOptional,
                ri.CanonicalIngredientId,
                ri.CanonicalUnitId))
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<RecipeId> CreateRecipeAsync(DBModel.Recipe recipe, RecipeId? recipeId = null)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        if (recipe.OwnerUserId is null)
        {
            throw new ArgumentException("A recipe must have an owner.", nameof(recipe));
        }

        var now = DateTime.UtcNow;
        var entity = new RecipeEntity
        {
            Id = recipeId?.Value ?? Guid.CreateVersion7(),
            Title = recipe.Title.Value,
            AccessScopeId = (byte)recipe.AccessScope,
            OwnerUserId = recipe.OwnerUserId.Value.Value,
            Summary = recipe.Summary,
            Servings = recipe.Servings,
            YieldText = recipe.YieldText,
            PrepTimeMinutes = recipe.PrepTimeMinutes,
            CookTimeMinutes = recipe.CookTimeMinutes,
            TotalTimeMinutes = recipe.TotalTimeMinutes,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        db.Recipes.Add(entity);
        try
        {
            await db.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            throw new ConflictException($"A recipe titled '{recipe.Title.Value}' already exists.");
        }

        return RecipeId.From(entity.Id);
    }

    public async Task UpsertRecipeIngredientsAsync(RecipeId recipeId, IEnumerable<DBModel.RecipeIngredient> recipeIngredients)
    {
        ArgumentNullException.ThrowIfNull(recipeIngredients);

        await db.RecipeIngredients
            .Where(ri => ri.RecipeId == recipeId.Value)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);

        var entities = recipeIngredients
            .Select(i => new RecipeIngredientEntity
            {
                Id = Guid.CreateVersion7(),
                RecipeId = recipeId.Value,
                SortOrder = i.SortOrder,
                IngredientText = i.IngredientText,
                MeasureText = i.MeasureText,
                SectionTitle = i.SectionTitle,
                Amount = i.Amount,
                UnitText = i.UnitText,
                PreparationText = i.PreparationText,
                IsOptional = i.IsOptional,
                CanonicalIngredientId = i.CanonicalIngredientId,
                CanonicalUnitId = i.CanonicalUnitId,
            })
            .ToList();

        db.RecipeIngredients.AddRange(entities);
        await db.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task UpdateRecipeAsync(RecipeId recipeId, DBModel.Recipe recipe)
    {
        var entity = await db.Recipes.SingleAsync(r => r.Id == recipeId.Value).ConfigureAwait(false);
        entity.Title = recipe.Title.Value;
        entity.AccessScopeId = (byte)recipe.AccessScope;
        entity.Summary = recipe.Summary;
        entity.Servings = recipe.Servings;
        entity.YieldText = recipe.YieldText;
        entity.PrepTimeMinutes = recipe.PrepTimeMinutes;
        entity.CookTimeMinutes = recipe.CookTimeMinutes;
        entity.TotalTimeMinutes = recipe.TotalTimeMinutes;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        try
        {
            await db.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            throw new ConflictException($"A recipe titled '{recipe.Title.Value}' already exists.");
        }
    }

    public async Task DeleteRecipeAsync(RecipeId recipeId)
    {
        await db.Recipes
            .Where(r => r.Id == recipeId.Value)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.DeletedAtUtc, DateTime.UtcNow))
            .ConfigureAwait(false);
    }

    public async Task RestoreRecipeAsync(RecipeId recipeId)
    {
        try
        {
            await db.Recipes
                .IgnoreQueryFilters()
                .Where(r => r.Id == recipeId.Value)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.DeletedAtUtc, (DateTime?)null))
                .ConfigureAwait(false);
        }
        catch (SqlException ex) when (ex.IsUniqueConstraintViolation())
        {
            throw new ConflictException("A recipe with the same title already exists.");
        }
    }

    public async Task<DBModel.Recipe?> GetRecipeIncludingDeletedAsync(RecipeId recipeId)
    {
        return await db.Recipes
            .IgnoreQueryFilters()
            .Where(r => r.Id == recipeId.Value)
            .Select(ToDbModel)
            .AsNoTracking()
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }
}

/// <summary>Unmapped SQL result row used to preserve the full-text rank ordering.</summary>
internal sealed class RecipeSearchRow
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public byte AccessScopeId { get; set; }
    public Guid OwnerUserId { get; set; }
    public string? Summary { get; set; }
    public int? Servings { get; set; }
    public string? YieldText { get; set; }
    public int? PrepTimeMinutes { get; set; }
    public int? CookTimeMinutes { get; set; }
    public int? TotalTimeMinutes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public int RankScore { get; set; }
}
