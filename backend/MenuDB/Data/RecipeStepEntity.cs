namespace MenuDB.Data;

public class RecipeStepEntity
{
    public Guid Id { get; set; }

    public Guid RecipeId { get; set; }

    public RecipeEntity Recipe { get; set; } = null!;

    public int SortOrder { get; set; }

    public string? Title { get; set; }

    public required string InstructionText { get; set; }

    public int? DurationMinutes { get; set; }
}
