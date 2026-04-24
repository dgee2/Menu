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

    [Fact]
    public void UninitializedName_Fails()
    {
#pragma warning disable VOG009
        var recipe = new NewRecipe
        {
            Name = default,
            Ingredients = [CreateValidIngredient()]
        };
#pragma warning restore VOG009

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor("Name");
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

    [Fact]
    public void ExactDuplicateIngredients_SameNameUnitAmount_Passes()
    {
        var ingredient = CreateValidIngredient();
        var recipe = new NewRecipe
        {
            Name = RecipeName.From("Test Recipe"),
            Ingredients = [ingredient, ingredient]
        };

        var result = validator.TestValidate(recipe);

        result.ShouldNotHaveValidationErrorFor(x => x.Ingredients);
    }

    [Fact]
    public void ConflictingDuplicateIngredients_SameNameAndUnit_DifferentAmount_Fails()
    {
        var recipe = new NewRecipe
        {
            Name = RecipeName.From("Test Recipe"),
            Ingredients =
            [
                new RecipeIngredient { Name = IngredientName.From("Flour"), Unit = IngredientUnitName.From("Grams"), Amount = IngredientAmount.From(100m) },
                new RecipeIngredient { Name = IngredientName.From("Flour"), Unit = IngredientUnitName.From("Grams"), Amount = IngredientAmount.From(200m) }
            ]
        };

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor(x => x.Ingredients);
    }

    [Fact]
    public void DifferentUnitsSameName_Passes()
    {
        var recipe = new NewRecipe
        {
            Name = RecipeName.From("Test Recipe"),
            Ingredients =
            [
                new RecipeIngredient { Name = IngredientName.From("Flour"), Unit = IngredientUnitName.From("Grams"), Amount = IngredientAmount.From(100m) },
                new RecipeIngredient { Name = IngredientName.From("Flour"), Unit = IngredientUnitName.From("Kilograms"), Amount = IngredientAmount.From(1m) }
            ]
        };

        var result = validator.TestValidate(recipe);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
