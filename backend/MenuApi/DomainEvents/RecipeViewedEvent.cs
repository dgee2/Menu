using MenuApi.ValueObjects;

namespace MenuApi.DomainEvents;

public sealed record RecipeViewedEvent(
    RecipeId RecipeId,
    MenuUserId ViewerUserId,
    bool IsAuthenticated,
    DateTime Timestamp);
