using System;
using System.Collections.Generic;

namespace MenuApi.ViewModel
{
    public class RecipeIngredient
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public HashSet<string> Units { get; set; }
        public string Unit { get; set; }
        public decimal Amount { get; set; }
    }
}
