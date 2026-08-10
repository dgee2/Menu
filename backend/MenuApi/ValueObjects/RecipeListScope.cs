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
    /// Parses the <c>scope</c> query value. Names only - a numeric string is rejected, so the enum's
    /// ordinals never become an undocumented second spelling of the API contract.
    /// </summary>
    public static bool TryParse(string? scope, out RecipeListScope recipeListScope)
    {
        if (!string.IsNullOrEmpty(scope)
            && !char.IsDigit(scope[0])
            && scope[0] != '-'
            && Enum.TryParse(scope, ignoreCase: true, out recipeListScope)
            && Enum.IsDefined(recipeListScope))
        {
            return true;
        }

        recipeListScope = default;
        return false;
    }
}
