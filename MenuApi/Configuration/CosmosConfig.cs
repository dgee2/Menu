using System.ComponentModel.DataAnnotations;

namespace MenuApi.Configuration
{
    public class CosmosConfig : IValidatable
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        [Required]
        public string ConnectionString { get; set; }

        [Required]
        public string DatabaseId { get; set; }

        [Required]
        public string IngredientContainerId { get; set; }

        [Required]
        public string RecipeContainerId { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}
