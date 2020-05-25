namespace MenuApi.Configuration
{
    public class CosmosConfig
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string ConnectionString { get; set; }

        public string DatabaseId { get; set; }

        public string IngredientContainerId { get; set; }

        public string RecipeContainerId { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}
