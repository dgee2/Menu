using System.Collections.Generic;

namespace MenuApi.ViewModel
{
    public class NewRecipe
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This file is for json")]
        public List<RecipeIngredient> Ingredients { get; set; }

        public string Name { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}
