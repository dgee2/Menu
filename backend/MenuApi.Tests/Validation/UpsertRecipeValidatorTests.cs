using FluentValidation.TestHelper;
using MenuApi.Validation;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Xunit;

namespace MenuApi.Tests.Validation;

public class UpsertRecipeValidatorTests
{
    private readonly UpsertRecipeValidator validator = new();

    private static RecipeIngredientItem CreateValidIngredient() => new()
    {
        SortOrder = 0,
        IngredientText = "Flour",
        MeasureText = "200g",
        IsOptional = false,
    };

    private static UpsertRecipe CreateValidRecipe(
        RecipeTitle? title = null,
        string? accessScope = null,
        List<RecipeIngredientItem>? ingredients = null,
        List<RecipeStepItem>? steps = null) => new()
    {
        Title = title ?? RecipeTitle.From("Test Recipe"),
        AccessScope = accessScope ?? RecipeAccessScope.Private,
        Ingredients = ingredients ?? [CreateValidIngredient()],
        Steps = steps ?? [],
    };

    [Fact]
    public void ValidRecipe_Passes()
    {
        var result = validator.TestValidate(CreateValidRecipe());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void TitleTooLong_Fails()
    {
        var recipe = CreateValidRecipe(title: RecipeTitle.From(new string('a', 201)));

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor("Title");
    }

    [Fact]
    public void EmptyIngredients_Passes()
    {
        var recipe = CreateValidRecipe(ingredients: []);

        var result = validator.TestValidate(recipe);

        result.ShouldNotHaveValidationErrorFor(x => x.Ingredients);
    }

    [Fact]
    public void NullIngredients_Fails()
    {
        var recipe = new UpsertRecipe
        {
            Title = RecipeTitle.From("Test Recipe"),
            AccessScope = RecipeAccessScope.Private,
            Ingredients = null!,
            Steps = [],
        };

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor(x => x.Ingredients);
    }

    [Fact]
    public void UninitializedTitle_Fails()
    {
#pragma warning disable VOG009
        var recipe = new UpsertRecipe
        {
            Title = default,
            AccessScope = RecipeAccessScope.Private,
            Ingredients = [CreateValidIngredient()],
            Steps = [],
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
        var recipe = CreateValidRecipe(title: RecipeTitle.From(title));

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor("Title");
    }

    [Fact]
    public void NullAccessScope_Fails()
    {
        var recipe = new UpsertRecipe
        {
            Title = RecipeTitle.From("Test Recipe"),
            AccessScope = null!,
            Ingredients = [CreateValidIngredient()],
            Steps = [],
        };

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor(x => x.AccessScope);
    }

    [Theory]
    [InlineData("private")]
    [InlineData("Public")]
    [InlineData("")]
    public void UnknownAccessScope_Fails(string accessScope)
    {
        var recipe = CreateValidRecipe(accessScope: accessScope);

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor(x => x.AccessScope);
    }

    [Theory]
    [InlineData(RecipeAccessScope.Private)]
    [InlineData(RecipeAccessScope.AuthenticatedUsers)]
    public void KnownAccessScope_Passes(string accessScope)
    {
        var recipe = CreateValidRecipe(accessScope: accessScope);

        var result = validator.TestValidate(recipe);

        result.ShouldNotHaveValidationErrorFor(x => x.AccessScope);
    }

    [Fact]
    public void NullSteps_Fails()
    {
        var recipe = new UpsertRecipe
        {
            Title = RecipeTitle.From("Test Recipe"),
            AccessScope = RecipeAccessScope.Private,
            Ingredients = [CreateValidIngredient()],
            Steps = null!,
        };

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor(x => x.Steps);
    }

    [Fact]
    public void EmptySteps_Passes()
    {
        var recipe = CreateValidRecipe(steps: []);

        var result = validator.TestValidate(recipe);

        result.ShouldNotHaveValidationErrorFor(x => x.Steps);
    }

    [Fact]
    public void InvalidStepItem_Fails()
    {
        var recipe = CreateValidRecipe(steps: [new RecipeStepItem { SortOrder = 0, InstructionText = string.Empty }]);

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor("Steps[0].InstructionText");
    }
}
