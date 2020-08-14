namespace MenuApi.DBModel
{
    public class Ingredient
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public int Id { get; set; }

        public string Name { get; set; }

        public string Unit { get; set; }

        public string Abbreviation { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}
