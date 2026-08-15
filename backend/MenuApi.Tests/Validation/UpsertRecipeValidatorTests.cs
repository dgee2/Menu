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
        RecipeAccessScope? accessScope = null,
        List<RecipeIngredientItem>? ingredients = null,
        List<RecipeStepItem>? steps = null,
        string? summary = null,
        string? yieldText = null,
        int? servings = null,
        int? prepTimeMinutes = null,
        int? cookTimeMinutes = null,
        int? totalTimeMinutes = null) => new()
    {
        Title = title ?? RecipeTitle.From("Test Recipe"),
        AccessScope = accessScope ?? RecipeAccessScope.Private,
        Ingredients = ingredients ?? [CreateValidIngredient()],
        Steps = steps ?? [],
        Summary = summary,
        YieldText = yieldText,
        Servings = servings,
        PrepTimeMinutes = prepTimeMinutes,
        CookTimeMinutes = cookTimeMinutes,
        TotalTimeMinutes = totalTimeMinutes,
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

    [Theory]
    // 0 is default(RecipeAccessScope) - an omitted or unrecognised value, not a real scope.
    [InlineData(0)]
    [InlineData(99)]
    public void UnknownAccessScope_Fails(byte accessScope)
    {
        var recipe = CreateValidRecipe(accessScope: (RecipeAccessScope)accessScope);

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor(x => x.AccessScope);
    }

    [Theory]
    [InlineData(RecipeAccessScope.Private)]
    [InlineData(RecipeAccessScope.AuthenticatedUsers)]
    public void KnownAccessScope_Passes(RecipeAccessScope accessScope)
    {
        var recipe = CreateValidRecipe(accessScope: accessScope);

        var result = validator.TestValidate(recipe);

        result.ShouldNotHaveValidationErrorFor(x => x.AccessScope);
    }

    [Fact]
    public void YieldTextAtColumnWidth_Passes()
    {
        var recipe = CreateValidRecipe(yieldText: new string('a', 100));

        var result = validator.TestValidate(recipe);

        result.ShouldNotHaveValidationErrorFor(x => x.YieldText);
    }

    [Fact]
    public void YieldTextOverColumnWidth_Fails()
    {
        // Without this rule the nvarchar(100) column reports it, and it reaches the caller as a 500.
        var recipe = CreateValidRecipe(yieldText: new string('a', 101));

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor(x => x.YieldText);
    }

    [Fact]
    public void SummaryTooLong_Fails()
    {
        var recipe = CreateValidRecipe(summary: new string('a', 4001));

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor(x => x.Summary);
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(1001)]
    public void ServingsOutOfRange_Fails(int servings)
    {
        var recipe = CreateValidRecipe(servings: servings);

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor(x => x.Servings);
    }

    [Theory]
    // Zero is accepted so the server does not reject a value the editor validates as good.
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1000)]
    public void ServingsInRange_Passes(int servings)
    {
        var recipe = CreateValidRecipe(servings: servings);

        var result = validator.TestValidate(recipe);

        result.ShouldNotHaveValidationErrorFor(x => x.Servings);
    }

    [Fact]
    public void NullOptionalNumbers_Pass()
    {
        var result = validator.TestValidate(CreateValidRecipe());

        result.ShouldNotHaveValidationErrorFor(x => x.Servings);
        result.ShouldNotHaveValidationErrorFor(x => x.PrepTimeMinutes);
        result.ShouldNotHaveValidationErrorFor(x => x.CookTimeMinutes);
        result.ShouldNotHaveValidationErrorFor(x => x.TotalTimeMinutes);
    }

    [Fact]
    public void NegativePrepTime_Fails()
    {
        var recipe = CreateValidRecipe(prepTimeMinutes: -1);

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor(x => x.PrepTimeMinutes);
    }

    [Fact]
    public void NegativeCookTime_Fails()
    {
        var recipe = CreateValidRecipe(cookTimeMinutes: -1);

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor(x => x.CookTimeMinutes);
    }

    [Fact]
    public void ImplausiblyLargeTotalTime_Fails()
    {
        var recipe = CreateValidRecipe(totalTimeMinutes: (60 * 24 * 14) + 1);

        var result = validator.TestValidate(recipe);

        result.ShouldHaveValidationErrorFor(x => x.TotalTimeMinutes);
    }

    [Fact]
    public void ZeroDurations_Pass()
    {
        var recipe = CreateValidRecipe(prepTimeMinutes: 0, cookTimeMinutes: 0, totalTimeMinutes: 0);

        var result = validator.TestValidate(recipe);

        result.ShouldNotHaveAnyValidationErrors();
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
