using MenuApi.ValueObjects;

namespace MenuApi.DomainEvents;

public sealed record RecipeCreatedEvent(RecipeId RecipeId);
