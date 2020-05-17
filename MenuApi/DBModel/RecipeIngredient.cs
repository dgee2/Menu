using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MenuApi.DBModel
{
    public class RecipeIngredient : Ingredient
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        [JsonProperty(PropertyName = "unit", Required = Required.Always)]
        public string Unit { get; set; }

        [JsonProperty(PropertyName = "amount", Required = Required.Always)]
        public decimal Amount { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}
