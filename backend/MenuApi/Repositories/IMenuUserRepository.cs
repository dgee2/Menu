using MenuApi.ValueObjects;

namespace MenuApi.Repositories;

public interface IMenuUserRepository
{
    Task<MenuUserId> UpsertAsync(string authSubject, string? displayName, string? email, string? avatarUrl);
}
