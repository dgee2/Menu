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
        Name = IngredientName.From("Flour"),
        Unit = IngredientUnitName.From("Grams"),
        Amount = IngredientAmount.From(100m)
    };

    [Fact]
    public void ValidRecipe_Passes()
    {
        var recipe = new NewRecipe
        {
            Name = RecipeName.From("Test Recipe"),
            Ingredients = [CreateValidIngredient()]
        };

        var result = validator.TestValidate(recipe);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void NameTooLong_Fails()
    {
        var recipe = new NewRecipe
        {
            Name = RecipeName.From(new string('a', 501)),
            Ingredients = [CreateValidIngredient()]
        };

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor("Name");
    }

    [Fact]
    public void EmptyIngredients_Fails()
    {
        var recipe = new NewRecipe
        {
            Name = RecipeName.From("Valid Recipe"),
            Ingredients = []
        };

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor(x => x.Ingredients);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("  \t  ")]
    public void WhitespaceName_Fails(string name)
    {
        var recipe = new NewRecipe
        {
            Name = RecipeName.From(name),
            Ingredients = [CreateValidIngredient()]
        };

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor("Name");
    }
}
