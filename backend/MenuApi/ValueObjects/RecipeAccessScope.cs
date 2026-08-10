using System.Text.Json.Serialization;

namespace MenuApi.ValueObjects;

/// <summary>
/// Who can read a recipe. A closed set, so this is a plain C# enum rather than a Vogen value
/// object: Vogen would emit a bare <c>string</c> schema, forcing the frontend to keep a
/// hand-written mirror of the allowed values. As an enum, <c>openapi-typescript</c> generates
/// the union directly.
/// </summary>
/// <remarks>
/// Numeric values are persisted as the <c>RecipeAccessScope</c> lookup table's primary key, so
/// they are part of the database contract - never renumber a member. The name, not the number,
/// crosses the API boundary; <see cref="JsonStringEnumConverter{TEnum}"/> enforces that.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<RecipeAccessScope>))]
public enum RecipeAccessScope : byte
{
    /// <summary>Only the owner can read the recipe.</summary>
    Private = 1,

    /// <summary>Any authenticated Menu user can read the recipe.</summary>
    AuthenticatedUsers = 2,
}
