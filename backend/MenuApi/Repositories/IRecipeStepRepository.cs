using MenuApi.ValueObjects;

namespace MenuApi.Repositories;

public interface IRecipeStepRepository
{
    Task<IEnumerable<DBModel.RecipeStep>> GetStepsByRecipeIdAsync(RecipeId recipeId);

    Task UpsertStepCollectionAsync(RecipeId recipeId, IEnumerable<DBModel.RecipeStep> steps);

    Task DeleteStepsByRecipeIdAsync(RecipeId recipeId);
}
