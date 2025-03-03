using Vogen;

namespace MenuApi.ValueObjects;

[ValueObject<int>]
public readonly partial struct IngredientId { }

[ValueObject<string>]
public readonly partial struct IngredientName { }

[ValueObject<decimal>]
public readonly partial struct IngredientAmount { }