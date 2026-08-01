using FluentValidation.TestHelper;
using MenuApi.Validation;
using MenuApi.ViewModel;
using Xunit;

namespace MenuApi.Tests.Validation;

public class RecipeStepItemValidatorTests
{
    private readonly RecipeStepItemValidator validator = new();

    [Fact]
    public void ValidStep_Passes()
    {
        var step = new RecipeStepItem
        {
            SortOrder = 0,
            InstructionText = "Preheat the oven to 200C.",
        };

        var result = validator.TestValidate(step);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NullOrEmptyInstructionText_Fails(string? text)
    {
        var step = new RecipeStepItem
        {
            SortOrder = 0,
            InstructionText = text!,
        };

        var result = validator.TestValidate(step);

        result.ShouldHaveValidationErrorFor("InstructionText");
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("  \t  ")]
    public void WhitespaceInstructionText_Fails(string text)
    {
        var step = new RecipeStepItem
        {
            SortOrder = 0,
            InstructionText = text,
        };

        var result = validator.TestValidate(step);

        result.ShouldHaveValidationErrorFor("InstructionText");
    }

    [Fact]
    public void NegativeSortOrder_Fails()
    {
        var step = new RecipeStepItem
        {
            SortOrder = -1,
            InstructionText = "Mix well.",
        };

        var result = validator.TestValidate(step);

        result.ShouldHaveValidationErrorFor("SortOrder");
    }

    [Fact]
    public void TitleTooLong_Fails()
    {
        var step = new RecipeStepItem
        {
            SortOrder = 0,
            InstructionText = "Mix well.",
            Title = new string('a', 201),
        };

        var result = validator.TestValidate(step);

        result.ShouldHaveValidationErrorFor("Title");
    }

    [Fact]
    public void NullTitle_Passes()
    {
        var step = new RecipeStepItem
        {
            SortOrder = 0,
            InstructionText = "Mix well.",
            Title = null,
        };

        var result = validator.TestValidate(step);

        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ZeroOrNegativeDurationMinutes_Fails(int duration)
    {
        var step = new RecipeStepItem
        {
            SortOrder = 0,
            InstructionText = "Mix well.",
            DurationMinutes = duration,
        };

        var result = validator.TestValidate(step);

        result.ShouldHaveValidationErrorFor("DurationMinutes");
    }

    [Fact]
    public void NullDurationMinutes_Passes()
    {
        var step = new RecipeStepItem
        {
            SortOrder = 0,
            InstructionText = "Mix well.",
            DurationMinutes = null,
        };

        var result = validator.TestValidate(step);

        result.ShouldNotHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Fact]
    public void PositiveDurationMinutes_Passes()
    {
        var step = new RecipeStepItem
        {
            SortOrder = 0,
            InstructionText = "Mix well.",
            DurationMinutes = 5,
        };

        var result = validator.TestValidate(step);

        result.ShouldNotHaveValidationErrorFor(x => x.DurationMinutes);
    }
}
