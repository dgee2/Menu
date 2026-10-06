using MenuApi.ValueObjects;

namespace MenuApi.DomainEvents;

public sealed record RecipeUpdatedEvent(RecipeId RecipeId);
