namespace MenuApi.ViewModel
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "This file is for json")]
    public class RecipeIngredient
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public int Id { get; set; }

        public string Name { get; set; }

        public string Unit { get; set; }

        public decimal Amount { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}
