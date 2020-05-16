using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MenuApi.DBModel
{
    public class Ingredient
    {

        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "units")]
        public HashSet<string> Units { get; set; }
    }
}
