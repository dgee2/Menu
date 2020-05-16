using System;
using System.Collections.Generic;

namespace MenuApi.ViewModel
{
    public class Recipe
    {

        public Guid Id { get; set; }
        public List<RecipeIngredient> Ingredients { get; set; }

        public string Name { get; set; }
    }
}
