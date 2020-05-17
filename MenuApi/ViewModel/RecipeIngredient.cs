using System;
using System.Collections.Generic;

namespace MenuApi.ViewModel
{
    public class RecipeIngredient
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public Guid Id { get; set; }
        public string Name { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
        public HashSet<string> Units { get; set; }
        public string Unit { get; set; }
        public decimal Amount { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}
