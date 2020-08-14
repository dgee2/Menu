using System.Collections.Generic;

namespace MenuApi.ViewModel
{
    public class Ingredient
    {
        public Ingredient(int id, string name, IEnumerable<(string Name, string Abbreviation)> units)
        {
            Id = id;
            Name = name;
            Units = units;
        }

        public int Id { get; }

        public string Name { get; }

        public IEnumerable<(string Name, string Abbreviation)> Units { get; }
    }
}
