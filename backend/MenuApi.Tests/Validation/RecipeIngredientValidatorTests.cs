using AwesomeAssertions;
using FluentValidation.TestHelper;
using MenuApi.Validation;
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
            SortOrder = 0,
            IngredientText = "Flour",
            MeasureText = "200g",
            IsOptional = false,
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NullOrEmptyIngredientText_Fails(string? text)
    {
        var ingredient = new RecipeIngredient
        {
            SortOrder = 0,
            IngredientText = text!,
            MeasureText = "200g",
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor("IngredientText");
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("  \t  ")]
    public void WhitespaceIngredientText_Fails(string text)
    {
        var ingredient = new RecipeIngredient
        {
            SortOrder = 0,
            IngredientText = text,
            MeasureText = "200g",
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor("IngredientText");
    }

    [Fact]
    public void IngredientTextTooLong_Fails()
    {
        var ingredient = new RecipeIngredient
        {
            SortOrder = 0,
            IngredientText = new string('a', 201),
            MeasureText = "200g",
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor("IngredientText");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NullOrEmptyMeasureText_Fails(string? text)
    {
        var ingredient = new RecipeIngredient
        {
            SortOrder = 0,
            IngredientText = "Flour",
            MeasureText = text!,
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor("MeasureText");
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("  \t  ")]
    public void WhitespaceMeasureText_Fails(string text)
    {
        var ingredient = new RecipeIngredient
        {
            SortOrder = 0,
            IngredientText = "Flour",
            MeasureText = text,
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor("MeasureText");
    }

    [Fact]
    public void MeasureTextTooLong_Fails()
    {
        var ingredient = new RecipeIngredient
        {
            SortOrder = 0,
            IngredientText = "Flour",
            MeasureText = new string('a', 101),
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor("MeasureText");
    }

    [Fact]
    public void SectionTitleTooLong_Fails()
    {
        var ingredient = new RecipeIngredient
        {
            SortOrder = 0,
            IngredientText = "Flour",
            MeasureText = "200g",
            SectionTitle = new string('a', 101),
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor("SectionTitle");
    }

    [Fact]
    public void UnitTextTooLong_Fails()
    {
        var ingredient = new RecipeIngredient
        {
            SortOrder = 0,
            IngredientText = "Flour",
            MeasureText = "200g",
            UnitText = new string('a', 51),
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor("UnitText");
    }

    [Fact]
    public void PreparationTextTooLong_Fails()
    {
        var ingredient = new RecipeIngredient
        {
            SortOrder = 0,
            IngredientText = "Flour",
            MeasureText = "200g",
            PreparationText = new string('a', 101),
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor("PreparationText");
    }

    [Fact]
    public void NegativeSortOrder_Fails()
    {
        var ingredient = new RecipeIngredient
        {
            SortOrder = -1,
            IngredientText = "Flour",
            MeasureText = "200g",
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor("SortOrder");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ZeroOrNegativeAmount_Fails(int amount)
    {
        var ingredient = new RecipeIngredient
        {
            SortOrder = 0,
            IngredientText = "Flour",
            MeasureText = "200g",
            Amount = amount,
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor("Amount");
    }

    [Fact]
    public void TooManyDecimalPlaces_Fails()
    {
        var ingredient = new RecipeIngredient
        {
            SortOrder = 0,
            IngredientText = "Flour",
            MeasureText = "200g",
            Amount = 1.12345m,
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor("Amount");
    }

    [Fact]
    public void TooManyTotalDigits_Fails()
    {
        var ingredient = new RecipeIngredient
        {
            SortOrder = 0,
            IngredientText = "Flour",
            MeasureText = "200g",
            Amount = 1234567m,
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor("Amount");
    }

    [Fact]
    public void NullAmount_Passes()
    {
        var ingredient = new RecipeIngredient
        {
            SortOrder = 0,
            IngredientText = "Flour",
            MeasureText = "to taste",
            Amount = null,
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldNotHaveValidationErrorFor("Amount");
    }

    [Fact]
    public void ValidAmountWithDecimals_Passes()
    {
        var ingredient = new RecipeIngredient
        {
            SortOrder = 0,
            IngredientText = "Flour",
            MeasureText = "200g",
            Amount = 1.5m,
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
