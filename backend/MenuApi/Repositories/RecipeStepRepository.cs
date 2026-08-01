using System.Diagnostics.CodeAnalysis;
using MenuDB;
using MenuDB.Data;
using MenuApi.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MenuApi.Repositories;

[ExcludeFromCodeCoverage]
public class RecipeStepRepository(MenuDbContext db) : IRecipeStepRepository
{
    public async Task<IEnumerable<DBModel.RecipeStep>> GetStepsByRecipeIdAsync(RecipeId recipeId)
    {
        return await db.RecipeSteps
            .Where(s => s.RecipeId == recipeId.Value)
            .OrderBy(s => s.SortOrder)
            .Select(s => new DBModel.RecipeStep(s.SortOrder, s.InstructionText, s.Title, s.DurationMinutes))
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task UpsertStepCollectionAsync(RecipeId recipeId, IEnumerable<DBModel.RecipeStep> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);

        await db.RecipeSteps
            .Where(s => s.RecipeId == recipeId.Value)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);

        var entities = steps
            .Select(s => new RecipeStepEntity
            {
                RecipeId = recipeId.Value,
                SortOrder = s.SortOrder,
                InstructionText = s.InstructionText,
                Title = s.Title,
                DurationMinutes = s.DurationMinutes,
            })
            .ToList();

        db.RecipeSteps.AddRange(entities);
        await db.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task DeleteStepsByRecipeIdAsync(RecipeId recipeId)
    {
        await db.RecipeSteps
            .Where(s => s.RecipeId == recipeId.Value)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }
}
