namespace MenuApi.ViewModel;

public class NewRecipe
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public List<RecipeIngredient> Ingredients { get; set; }

    public string Name { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}
