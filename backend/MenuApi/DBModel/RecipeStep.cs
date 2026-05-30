namespace MenuApi.DBModel;

public record RecipeStep(int SortOrder, string InstructionText, string? Title, int? DurationMinutes);
