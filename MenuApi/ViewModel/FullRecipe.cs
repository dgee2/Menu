using System.ComponentModel.DataAnnotations;

namespace MenuApi.ViewModel;

public class FullRecipe : Recipe
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    [Required]
    public IEnumerable<RecipeIngredient> Ingredients { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}
