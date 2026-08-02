namespace MenuApi.ViewModel;

public class RecipeStepItem
{
    public int SortOrder { get; init; }

#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public string InstructionText { get; init; }
#pragma warning restore CS8618

    public string? Title { get; init; }

    public int? DurationMinutes { get; init; }
}
