using MenuApi.ValueObjects;

namespace MenuApi.ViewModel;

public sealed record UserProfile
{
    public required MenuUserId Id { get; init; }

    public required string AuthSubject { get; init; }

    public required string DisplayName { get; init; }

    public string? Email { get; init; }

    public string? AvatarUrl { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public required DateTime LastSeenAtUtc { get; init; }
}
