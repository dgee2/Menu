using AwesomeAssertions;
using MenuApi.MappingProfiles;
using MenuApi.ViewModel;
using Xunit;

namespace MenuApi.Tests.MappingProfiles;

public class ViewModelMapperTests
{
    [Theory, CustomAutoData]
    public void Map_DBModelRecipe_To_RecipeListItem(DBModel.Recipe recipe)
    {
        var result = ViewModelMapper.Map(recipe);

        result.Id.Should().Be(recipe.Id);
        result.Title.Should().Be(recipe.Title);
        result.AccessScope.Should().Be(recipe.AccessScope);
        result.Summary.Should().Be(recipe.Summary);
        result.Servings.Should().Be(recipe.Servings);
        result.YieldText.Should().Be(recipe.YieldText);
        result.PrepTimeMinutes.Should().Be(recipe.PrepTimeMinutes);
        result.CookTimeMinutes.Should().Be(recipe.CookTimeMinutes);
        result.TotalTimeMinutes.Should().Be(recipe.TotalTimeMinutes);
    }

    [Theory, CustomAutoData]
    public void MapToRecipeDetail_MapsAllScalarFields_AndIgnoresCollections(DBModel.Recipe recipe)
    {
        var result = ViewModelMapper.MapToRecipeDetail(recipe);

        result.Should().NotBeNull();
        result!.Id.Should().Be(recipe.Id);
        result.Title.Should().Be(recipe.Title);
        result.AccessScope.Should().Be(recipe.AccessScope);
        result.OwnerUserId.Should().Be(recipe.OwnerUserId);
        result.Summary.Should().Be(recipe.Summary);
        result.Servings.Should().Be(recipe.Servings);
        result.YieldText.Should().Be(recipe.YieldText);
        result.PrepTimeMinutes.Should().Be(recipe.PrepTimeMinutes);
        result.CookTimeMinutes.Should().Be(recipe.CookTimeMinutes);
        result.TotalTimeMinutes.Should().Be(recipe.TotalTimeMinutes);
        result.CreatedAtUtc.Should().Be(recipe.CreatedAtUtc);
        result.UpdatedAtUtc.Should().Be(recipe.UpdatedAtUtc);
        result.Ingredients.Should().BeNull();
        result.Steps.Should().BeNull();
    }

    [Fact]
    public void MapToRecipeDetail_NullRecipe_ReturnsNull()
    {
        var result = ViewModelMapper.MapToRecipeDetail(null);

        result.Should().BeNull();
    }

    [Theory, CustomAutoData]
    public void Map_UpsertRecipe_To_DBModelRecipe(UpsertRecipe upsertRecipe)
    {
        var result = ViewModelMapper.Map(upsertRecipe);

        result.Title.Should().Be(upsertRecipe.Title);
        result.AccessScope.Should().Be(upsertRecipe.AccessScope);
        result.Summary.Should().Be(upsertRecipe.Summary);
        result.Servings.Should().Be(upsertRecipe.Servings);
        result.YieldText.Should().Be(upsertRecipe.YieldText);
        result.PrepTimeMinutes.Should().Be(upsertRecipe.PrepTimeMinutes);
        result.CookTimeMinutes.Should().Be(upsertRecipe.CookTimeMinutes);
        result.TotalTimeMinutes.Should().Be(upsertRecipe.TotalTimeMinutes);
        result.OwnerUserId.Should().BeNull();
        result.CreatedAtUtc.Should().Be(default);
        result.UpdatedAtUtc.Should().Be(default);
    }

    [Theory, CustomAutoData]
    public void Map_DBModelRecipeIngredient_To_RecipeIngredientItem_RoundTrips(DBModel.RecipeIngredient ingredient)
    {
        var viewModel = ViewModelMapper.Map(ingredient);
        var roundTripped = ViewModelMapper.Map(viewModel);

        viewModel.SortOrder.Should().Be(ingredient.SortOrder);
        viewModel.IngredientText.Should().Be(ingredient.IngredientText);
        viewModel.MeasureText.Should().Be(ingredient.MeasureText);
        viewModel.SectionTitle.Should().Be(ingredient.SectionTitle);
        viewModel.Amount.Should().Be(ingredient.Amount);
        viewModel.UnitText.Should().Be(ingredient.UnitText);
        viewModel.PreparationText.Should().Be(ingredient.PreparationText);
        viewModel.IsOptional.Should().Be(ingredient.IsOptional);
        viewModel.CanonicalIngredientId.Should().Be(ingredient.CanonicalIngredientId);
        viewModel.CanonicalUnitId.Should().Be(ingredient.CanonicalUnitId);

        roundTripped.Should().BeEquivalentTo(ingredient);
    }

    [Theory, CustomAutoData]
    public void Map_DBModelRecipeStep_To_RecipeStepItem_RoundTrips(DBModel.RecipeStep step)
    {
        var viewModel = ViewModelMapper.Map(step);
        var roundTripped = ViewModelMapper.Map(viewModel);

        viewModel.SortOrder.Should().Be(step.SortOrder);
        viewModel.InstructionText.Should().Be(step.InstructionText);
        viewModel.Title.Should().Be(step.Title);
        viewModel.DurationMinutes.Should().Be(step.DurationMinutes);

        roundTripped.Should().BeEquivalentTo(step);
    }
}
