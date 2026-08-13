namespace MenuApi.ValueObjects;

public enum RecipeListScope
{
    Mine,
    Authenticated,
}

public static class RecipeListScopeParser
{
    /// <summary>The accepted <c>scope</c> query values, in the casing the API documents.</summary>
    public static IReadOnlyCollection<string> AllValues { get; } =
        Enum.GetNames<RecipeListScope>().Select(n => n.ToLowerInvariant()).ToArray();

    /// <summary>
    /// Parses the <c>scope</c> query value. Exactly one member name, matched case-insensitively.
    /// </summary>
    /// <remarks>
    /// Deliberately not <see cref="Enum.TryParse{TEnum}(string, bool, out TEnum)"/>: that trims
    /// whitespace and accepts comma-separated lists, so <c>" 1"</c> and <c>"mine,authenticated"</c>
    /// would both parse and the enum's ordinals would become an undocumented second spelling of the
    /// API contract.
    /// </remarks>
    public static bool TryParse(string? scope, out RecipeListScope recipeListScope)
    {
        foreach (var candidate in Enum.GetValues<RecipeListScope>())
        {
            if (string.Equals(scope, candidate.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                recipeListScope = candidate;
                return true;
            }
        }

        recipeListScope = default;
        return false;
    }
}
