using System.Linq.Expressions;
using FluentValidation;

namespace MenuApi.Validation;

public static class VogenValidationRules
{
    public static InlineValidator<T> StringRules<T, TVogen>(
        Expression<Func<T, TVogen>> vogenSelector,
        Expression<Func<T, string>> valueSelector,
        Func<T, bool> isInitialized,
        string propertyName,
        int maxLength) where TVogen : struct
    {
        var rules = new InlineValidator<T>();

        rules.RuleFor(vogenSelector)
            .Must((parent, _) => isInitialized(parent))
            .OverridePropertyName(propertyName)
            .WithMessage($"'{propertyName}' must not be empty.");

        rules.RuleFor(valueSelector)
            .NotEmpty()
            .OverridePropertyName(propertyName)
            .WithMessage($"'{propertyName}' must not be empty.")
            .When(x => isInitialized(x));

        rules.RuleFor(valueSelector)
            .MaximumLength(maxLength)
            .OverridePropertyName(propertyName)
            .WithMessage($"'{propertyName}' must be {maxLength} characters or fewer.")
            .When(x => isInitialized(x));

        return rules;
    }
}
