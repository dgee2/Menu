using FluentValidation;
using MenuApi.ViewModel;

namespace MenuApi.Validation;

public class NewIngredientValidator : AbstractValidator<NewIngredient>
{
    public NewIngredientValidator()
    {
        RuleFor(x => x.Name)
            .Must(name => name.IsInitialized())
            .OverridePropertyName("Name")
            .WithMessage("'Name' must not be empty.");

        RuleFor(x => x.Name.Value)
            .NotEmpty()
            .OverridePropertyName("Name")
            .WithMessage("'Name' must not be empty.")
            .When(x => x.Name.IsInitialized());

        RuleFor(x => x.Name.Value)
            .MaximumLength(50)
            .OverridePropertyName("Name")
            .WithMessage("'Name' must be 50 characters or fewer.")
            .When(x => x.Name.IsInitialized());

        RuleFor(x => x.UnitIds)
            .NotNull()
            .WithMessage("'Unit Ids' must not be empty.");

        RuleFor(x => x.UnitIds)
            .Must(u => u is { Count: > 0 })
            .When(x => x.UnitIds is not null)
            .WithMessage("'Unit Ids' must not be empty.");

        RuleForEach(x => x.UnitIds)
            .GreaterThan(0)
            .WithMessage("Each unit ID must be greater than 0.");
    }
}
