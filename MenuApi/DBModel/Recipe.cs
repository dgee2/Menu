﻿using MenuApi.ValueObjects;

namespace MenuApi.DBModel;

public sealed record Recipe
{
    public RecipeId Id { get; init; }
    public required string Name { get; init; }
}
