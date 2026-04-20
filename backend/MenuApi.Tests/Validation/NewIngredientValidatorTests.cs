using FluentValidation.TestHelper;
using MenuApi.Validation;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Xunit;

namespace MenuApi.Tests.Validation;

public class NewIngredientValidatorTests
{
    private readonly NewIngredientValidator validator = new();

    [Fact]
    public void ValidIngredient_Passes()
    {
        var ingredient = new NewIngredient
        {
            Name = IngredientName.From("Flour"),
            UnitIds = [1, 2]
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UninitializedName_Fails()
    {
#pragma warning disable VOG009
        var ingredient = new NewIngredient
        {
            Name = default,
            UnitIds = [1, 2]
        };
#pragma warning restore VOG009

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor("Name");
    }

    [Fact]
    public void NameTooLong_Fails()
    {
        var ingredient = new NewIngredient
        {
            Name = IngredientName.From(new string('a', 51)),
            UnitIds = [1]
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor("Name");
    }

    [Fact]
    public void EmptyUnitIds_Fails()
    {
        var ingredient = new NewIngredient
        {
            Name = IngredientName.From("Flour"),
            UnitIds = []
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor(x => x.UnitIds);
    }

    [Fact]
    public void ZeroUnitId_Fails()
    {
        var ingredient = new NewIngredient
        {
            Name = IngredientName.From("Flour"),
            UnitIds = [0]
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor("UnitIds[0]");
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("  \t  ")]
    public void WhitespaceName_Fails(string name)
    {
        var ingredient = new NewIngredient
        {
            Name = IngredientName.From(name),
            UnitIds = [1]
        };

        var result = validator.TestValidate(ingredient);

        result.ShouldHaveValidationErrorFor("Name");
    }
}
