using FluentValidation.TestHelper;
using MenuApi.Validation;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Xunit;

namespace MenuApi.Tests.Validation;

public class NewRecipeValidatorTests
{
    private readonly NewRecipeValidator validator = new();

    private static RecipeIngredient CreateValidIngredient() => new()
    {
        SortOrder = 0,
        IngredientText = "Flour",
        MeasureText = "200g",
        IsOptional = false,
    };

    [Fact]
    public void ValidRecipe_Passes()
    {
        var recipe = new NewRecipe
        {
            Title = RecipeTitle.From("Test Recipe"),
            Ingredients = [CreateValidIngredient()]
        };

        var result = validator.TestValidate(recipe);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void TitleTooLong_Fails()
    {
        var recipe = new NewRecipe
        {
            Title = RecipeTitle.From(new string('a', 201)),
            Ingredients = [CreateValidIngredient()]
        };

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor("Title");
    }

    [Fact]
    public void EmptyIngredients_Fails()
    {
        var recipe = new NewRecipe
        {
            Title = RecipeTitle.From("Valid Recipe"),
            Ingredients = []
        };

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor(x => x.Ingredients);
    }

    [Fact]
    public void UninitializedTitle_Fails()
    {
#pragma warning disable VOG009
        var recipe = new NewRecipe
        {
            Title = default,
            Ingredients = [CreateValidIngredient()]
        };
#pragma warning restore VOG009

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor("Title");
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("  \t  ")]
    public void WhitespaceTitle_Fails(string title)
    {
        var recipe = new NewRecipe
        {
            Title = RecipeTitle.From(title),
            Ingredients = [CreateValidIngredient()]
        };

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor("Title");
    }
}
