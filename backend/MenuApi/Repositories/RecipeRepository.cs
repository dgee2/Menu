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
    public async Task<IEnumerable<DBModel.Recipe>> GetRecipesAsync()
    {
        return await db.Recipes
            .Select(r => new DBModel.Recipe { Id = RecipeId.From(r.Id), Title = RecipeTitle.From(r.Title) })
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<DBModel.Recipe?> GetRecipeAsync(RecipeId recipeId)
    {
        return await db.Recipes
            .Where(r => r.Id == recipeId.Value)
            .Select(r => new DBModel.Recipe { Id = RecipeId.From(r.Id), Title = RecipeTitle.From(r.Title) })
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

    public async Task<RecipeId> CreateRecipeAsync(RecipeTitle title)
    {
        var now = DateTime.UtcNow;
        var entity = new RecipeEntity
        {
            Title = title.Value,
            AccessScope = "Private",
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
            throw new BusinessValidationException($"A recipe titled '{title.Value}' already exists.");
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

    public async Task UpdateRecipeAsync(RecipeId recipeId, RecipeTitle title)
    {
        var now = DateTime.UtcNow;

        try
        {
            await db.Recipes
                .Where(r => r.Id == recipeId.Value)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.Title, title.Value)
                    .SetProperty(r => r.UpdatedAtUtc, now))
                .ConfigureAwait(false);
        }
        catch (SqlException ex) when (ex.IsUniqueConstraintViolation())
        {
            throw new BusinessValidationException($"A recipe titled '{title.Value}' already exists.");
        }
    }
}
