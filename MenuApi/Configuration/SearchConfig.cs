using System.ComponentModel.DataAnnotations;

namespace MenuApi.Configuration
{
    public class SearchConfig : IValidatable
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        [Required]
        public string ServiceName { get; set; }

        [Required]
        public string AdminApiKey { get; set; }

        [Required]
        public string IngredientIndex { get; set; }

        [Required]
        public string QueryApiKey { get; set; }

        [Required]
        public string RecipeIndex { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}
