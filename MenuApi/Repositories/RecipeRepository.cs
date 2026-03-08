using System.Diagnostics.CodeAnalysis;
using MenuDB;
using MenuDB.Data;
using MenuApi.DBModel;
using MenuApi.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MenuApi.Repositories;

[ExcludeFromCodeCoverage]
public class RecipeRepository(MenuDbContext db) : IRecipeRepository
{
    public async Task<IEnumerable<DBModel.Recipe>> GetRecipesAsync()
    {
        return await db.Recipes
            .Select(r => new DBModel.Recipe { Id = RecipeId.From(r.Id), Name = RecipeName.From(r.Name) })
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<DBModel.Recipe?> GetRecipeAsync(RecipeId recipeId)
    {
        return await db.Recipes
            .Where(r => r.Id == recipeId.Value)
            .Select(r => new DBModel.Recipe { Id = RecipeId.From(r.Id), Name = RecipeName.From(r.Name) })
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<GetRecipeIngredient>> GetRecipeIngredientsAsync(RecipeId recipeId)
    {
        return await db.RecipeIngredients
            .Where(ri => ri.RecipeId == recipeId.Value)
            .Select(ri => new GetRecipeIngredient
            {
                IngredientName = IngredientName.From(ri.Ingredient.Name),
                Amount = IngredientAmount.From(ri.Amount),
                UnitName = IngredientUnitName.From(ri.Unit.Name),
                UnitAbbreviation = IngredientUnitAbbreviation.From(ri.Unit.Abbreviation ?? string.Empty),
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<RecipeId> CreateRecipeAsync(RecipeName name)
    {
        var entity = new RecipeEntity { Name = name.Value };
        db.Recipes.Add(entity);
        await db.SaveChangesAsync().ConfigureAwait(false);
        return RecipeId.From(entity.Id);
    }

    public async Task UpsertRecipeIngredientsAsync(RecipeId recipeId, IEnumerable<DBModel.RecipeIngredient> recipeIngredients)
    {
        ArgumentNullException.ThrowIfNull(recipeIngredients);

        var incoming = recipeIngredients.ToList();
        var incomingKeys = incoming
            .Select(i => new { IngredientName = i.IngredientName.Value, UnitName = i.UnitName.Value })
            .ToHashSet();

        var existing = await db.RecipeIngredients
            .Where(ri => ri.RecipeId == recipeId.Value)
            .Include(ri => ri.Ingredient)
            .Include(ri => ri.Unit)
            .ToListAsync()
            .ConfigureAwait(false);

        var toDelete = existing
            .Where(e => !incomingKeys.Contains(new { IngredientName = e.Ingredient.Name, UnitName = e.Unit.Name }))
            .ToList();
        db.RecipeIngredients.RemoveRange(toDelete);

        var ingredientNames = incoming.Select(i => i.IngredientName.Value).Distinct().ToList();
        var unitNames = incoming.Select(i => i.UnitName.Value).Distinct().ToList();

        var ingredientLookup = await db.Ingredients
            .Where(i => ingredientNames.Contains(i.Name))
            .ToDictionaryAsync(i => i.Name, i => i.Id)
            .ConfigureAwait(false);

        var unitLookup = await db.Units
            .Where(u => unitNames.Contains(u.Name))
            .ToDictionaryAsync(u => u.Name, u => u.Id)
            .ConfigureAwait(false);

        foreach (var item in incoming)
        {
            if (!ingredientLookup.TryGetValue(item.IngredientName.Value, out var ingredientId))
                throw new InvalidOperationException($"Ingredient '{item.IngredientName.Value}' does not exist.");
            if (!unitLookup.TryGetValue(item.UnitName.Value, out var unitId))
                throw new InvalidOperationException($"Unit '{item.UnitName.Value}' does not exist.");

            var existingRow = existing.FirstOrDefault(e => e.IngredientId == ingredientId && e.UnitId == unitId);
            if (existingRow is not null)
            {
                existingRow.Amount = item.Amount.Value;
            }
            else
            {
                db.RecipeIngredients.Add(new RecipeIngredientEntity
                {
                    RecipeId = recipeId.Value,
                    IngredientId = ingredientId,
                    UnitId = unitId,
                    Amount = item.Amount.Value,
                });
            }
        }

        await db.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task UpdateRecipeAsync(RecipeId recipeId, RecipeName name)
    {
        await db.Recipes
            .Where(r => r.Id == recipeId.Value)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Name, name.Value))
            .ConfigureAwait(false);
    }
}
