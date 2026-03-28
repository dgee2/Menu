using AwesomeAssertions;
using FluentValidation.TestHelper;
using MenuApi.Validation;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Xunit;

namespace MenuApi.Tests.Validation;

public class RecipeIngredientValidatorTests
{
    private readonly RecipeIngredientValidator validator = new();

    [Fact]
    public void ValidIngredient_Passes()
    {
        var ingredient = new RecipeIngredient
        {
            Name = IngredientName.From("Flour"),
            Unit = IngredientUnitName.From("Grams"),
            Amount = IngredientAmount.From(100m)
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void NameTooLong_Fails()
    {
        var ingredient = new RecipeIngredient
        {
            Name = IngredientName.From(new string('a', 51)),
            Unit = IngredientUnitName.From("Grams"),
            Amount = IngredientAmount.From(100m)
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor(x => x.Name.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ZeroOrNegativeAmount_Fails(int amount)
    {
        var ingredient = new RecipeIngredient
        {
            Name = IngredientName.From("Flour"),
            Unit = IngredientUnitName.From("Grams"),
            Amount = IngredientAmount.From(amount)
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor(x => x.Amount.Value);
    }

    [Fact]
    public void TooManyDecimalPlaces_Fails()
    {
        var ingredient = new RecipeIngredient
        {
            Name = IngredientName.From("Flour"),
            Unit = IngredientUnitName.From("Grams"),
            Amount = IngredientAmount.From(1.12345m)
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor(x => x.Amount.Value);
    }

    [Fact]
    public void TooManyTotalDigits_Fails()
    {
        var ingredient = new RecipeIngredient
        {
            Name = IngredientName.From("Flour"),
            Unit = IngredientUnitName.From("Grams"),
            Amount = IngredientAmount.From(1234567m)
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor(x => x.Amount.Value);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("  \t  ")]
    public void WhitespaceName_Fails(string name)
    {
        var ingredient = new RecipeIngredient
        {
            Name = IngredientName.From(name),
            Unit = IngredientUnitName.From("Grams"),
            Amount = IngredientAmount.From(100m)
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor(x => x.Name.Value);
    }

    [Fact]
    public void UnitTooLong_Fails()
    {
        var ingredient = new RecipeIngredient
        {
            Name = IngredientName.From("Flour"),
            Unit = IngredientUnitName.From(new string('a', 51)),
            Amount = IngredientAmount.From(100m)
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor(x => x.Unit.Value);
    }
}
