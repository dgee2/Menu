using System.ComponentModel.DataAnnotations;

namespace MenuApi.ViewModel;

public class NewRecipe
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    [Required]
    public List<RecipeIngredient> Ingredients { get; init; }

    [Required]
    public string Name { get; init; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}
