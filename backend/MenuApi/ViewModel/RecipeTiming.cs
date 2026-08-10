namespace MenuApi.ViewModel;

/// <summary>
/// Total time is derived once, here, so the read DTOs cannot drift from each other - and so the
/// frontend never has to reimplement it across the detail page and the list page.
/// </summary>
public static class RecipeTiming
{
    /// <summary>
    /// Resolves the total time a recipe takes. An explicit <paramref name="totalTimeMinutes"/>
    /// always wins - total is not simply prep + cook, because resting, marinating, proving and
    /// chilling are real recipe time that is neither. Clearing it reverts to the derived value.
    /// </summary>
    /// <returns>
    /// <paramref name="totalTimeMinutes"/> when set; otherwise the sum of prep and cook, treating a
    /// missing one as zero; otherwise <see langword="null"/> when neither prep nor cook is known.
    /// </returns>
    public static int? Effective(int? totalTimeMinutes, int? prepTimeMinutes, int? cookTimeMinutes)
    {
        if (totalTimeMinutes.HasValue)
        {
            return totalTimeMinutes;
        }

        if (!prepTimeMinutes.HasValue && !cookTimeMinutes.HasValue)
        {
            return null;
        }

        return (prepTimeMinutes ?? 0) + (cookTimeMinutes ?? 0);
    }
}
