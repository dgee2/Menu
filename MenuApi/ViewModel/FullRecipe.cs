using System.Collections.Generic;

namespace MenuApi.ViewModel
{
    public class FullRecipe : Recipe
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public IEnumerable<RecipeIngredient> Ingredients { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}
