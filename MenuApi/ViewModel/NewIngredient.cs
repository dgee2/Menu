using System.Collections.Generic;

namespace MenuApi.ViewModel
{
    public class NewIngredient
    {
        public string Name { get; set; }
        public HashSet<string> Units { get; set; }
    }
}
