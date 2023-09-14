using System.ComponentModel.DataAnnotations;

namespace MenuApi.ViewModel;

public class RecipeIngredient
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    [Required]
    public string Name { get; init; }

    [Required]
    public string Unit { get; init; }

    [Required]
    public decimal Amount { get; init; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}
