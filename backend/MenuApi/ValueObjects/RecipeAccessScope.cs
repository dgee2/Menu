namespace MenuApi.ValueObjects;

public static class RecipeAccessScope
{
    public const string Private = "Private";

    public const string AuthenticatedUsers = "AuthenticatedUsers";

    public static readonly IReadOnlyCollection<string> AllValues = [Private, AuthenticatedUsers];
}
