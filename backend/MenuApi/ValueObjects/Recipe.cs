using Vogen;

namespace MenuApi.ValueObjects;

[ValueObject<Guid>]
public readonly partial struct RecipeId { }

[ValueObject<string>]
public readonly partial struct RecipeTitle { }
