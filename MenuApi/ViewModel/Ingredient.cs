namespace MenuApi.ViewModel;

public class Ingredient
{
    public Ingredient(int id, string name, IEnumerable<IngredientUnit> units)
    {
        Id = id;
        Name = name;
        Units = units;
    }

    public int Id { get; }

    public string Name { get; }

    public IEnumerable<IngredientUnit> Units { get; }
}

public class IngredientUnit
{
    public IngredientUnit(string name, string abbreviation)
    {
        Name = name;
        Abbreviation = abbreviation;
    }

    public string Name { get; }
    public string Abbreviation { get; }
}