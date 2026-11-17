using System.Text.Json.Serialization;

namespace MenuApi.ValueObjects;

/// <summary>
/// Who can read a recipe. A closed set, so this is a plain C# enum rather than a Vogen value
/// object: Vogen would emit a bare <c>string</c> schema, forcing the frontend to keep a
/// hand-written mirror of the allowed values. The string enum lets the generated frontend
/// client derive the allowed values directly from the API contract.
/// </summary>
/// <remarks>
/// Numeric values are persisted as the <c>RecipeAccessScope</c> lookup table's primary key, so
/// they are part of the database contract. The API emits the string name through
/// <see cref="JsonStringEnumConverter{TEnum}"/>.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<RecipeAccessScope>))]
public enum RecipeAccessScope : byte
{
    /// <summary>Only the owner can read the recipe.</summary>
    Private = 1,

    /// <summary>Any authenticated Menu user can read the recipe.</summary>
    AuthenticatedUsers = 2,
}
