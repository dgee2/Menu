using System.Diagnostics.CodeAnalysis;
using MenuDB;
using MenuDB.Data;
using MenuApi.DBModel;
using MenuApi.Exceptions;
using MenuApi.ValueObjects;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MenuApi.Repositories;

[ExcludeFromCodeCoverage]
public class RecipeRepository(MenuDbContext db) : IRecipeRepository
{
    public async Task<IEnumerable<DBModel.Recipe>> GetRecipesAsync(RecipeListScope scope, MenuUserId callerId, int take)
    {
        var query = scope switch
        {
            RecipeListScope.Mine => db.Recipes.Where(r => r.OwnerUserId == callerId.Value),
            RecipeListScope.Authenticated => db.Recipes.Where(r => r.AccessScope == RecipeAccessScope.AuthenticatedUsers),
            _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unsupported recipe list scope."),
        };

        return await query
            .OrderByDescending(r => r.UpdatedAtUtc)
            .Take(take)
            .Select(r => new DBModel.Recipe
            {
                Id = RecipeId.From(r.Id),
                Title = RecipeTitle.From(r.Title),
                AccessScope = r.AccessScope,
                OwnerUserId = r.OwnerUserId == null ? null : MenuUserId.From(r.OwnerUserId.Value),
                Summary = r.Summary,
                Servings = r.Servings,
                YieldText = r.YieldText,
                PrepTimeMinutes = r.PrepTimeMinutes,
                CookTimeMinutes = r.CookTimeMinutes,
                TotalTimeMinutes = r.TotalTimeMinutes,
                CreatedAtUtc = r.CreatedAtUtc,
                UpdatedAtUtc = r.UpdatedAtUtc,
            })
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<DBModel.Recipe?> GetRecipeAsync(RecipeId recipeId)
    {
        return await db.Recipes
            .Where(r => r.Id == recipeId.Value)
            .Select(r => new DBModel.Recipe
            {
                Id = RecipeId.From(r.Id),
                Title = RecipeTitle.From(r.Title),
                AccessScope = r.AccessScope,
                OwnerUserId = r.OwnerUserId == null ? null : MenuUserId.From(r.OwnerUserId.Value),
                Summary = r.Summary,
                Servings = r.Servings,
                YieldText = r.YieldText,
                PrepTimeMinutes = r.PrepTimeMinutes,
                CookTimeMinutes = r.CookTimeMinutes,
                TotalTimeMinutes = r.TotalTimeMinutes,
                CreatedAtUtc = r.CreatedAtUtc,
                UpdatedAtUtc = r.UpdatedAtUtc,
            })
            .AsNoTracking()
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<DBModel.RecipeIngredient>> GetRecipeIngredientsAsync(RecipeId recipeId)
    {
        return await db.RecipeIngredients
            .Where(ri => ri.RecipeId == recipeId.Value)
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

    public async Task<RecipeId> CreateRecipeAsync(DBModel.Recipe recipe)
    {
        var now = DateTime.UtcNow;
        var entity = new RecipeEntity
        {
            Title = recipe.Title.Value,
            AccessScope = recipe.AccessScope,
            OwnerUserId = recipe.OwnerUserId?.Value,
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
        var now = DateTime.UtcNow;

        try
        {
            await db.Recipes
                .Where(r => r.Id == recipeId.Value)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.Title, recipe.Title.Value)
                    .SetProperty(r => r.AccessScope, recipe.AccessScope)
                    .SetProperty(r => r.Summary, recipe.Summary)
                    .SetProperty(r => r.Servings, recipe.Servings)
                    .SetProperty(r => r.YieldText, recipe.YieldText)
                    .SetProperty(r => r.PrepTimeMinutes, recipe.PrepTimeMinutes)
                    .SetProperty(r => r.CookTimeMinutes, recipe.CookTimeMinutes)
                    .SetProperty(r => r.TotalTimeMinutes, recipe.TotalTimeMinutes)
                    .SetProperty(r => r.UpdatedAtUtc, now))
                .ConfigureAwait(false);
        }
        catch (SqlException ex) when (ex.IsUniqueConstraintViolation())
        {
            throw new ConflictException($"A recipe titled '{recipe.Title.Value}' already exists.");
        }
    }
}
