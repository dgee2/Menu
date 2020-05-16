using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MenuApi.DBModel
{
    public class RecipeIngredient : Ingredient
    {
        [JsonProperty(PropertyName = "unit", Required = Required.Always)]
        public string Unit { get; set; }

        [JsonProperty(PropertyName = "amount", Required = Required.Always)]
        public decimal Amount { get; set; }
    }
}
