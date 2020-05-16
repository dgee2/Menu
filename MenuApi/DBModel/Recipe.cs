using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MenuApi.DBModel
{
    public class Recipe
    {
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "ingredients")]
        public List<RecipeIngredient> Ingredients { get; set; }

        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }
    }
}
