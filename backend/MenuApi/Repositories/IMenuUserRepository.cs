using MenuApi.ValueObjects;
using MenuApi.ViewModel;

namespace MenuApi.Repositories;

public interface IMenuUserRepository
{
    Task<MenuUserId> UpsertAsync(string authSubject, string? displayName, string? email, string? avatarUrl);

    Task<UserProfile?> GetAsync(MenuUserId menuUserId);
}
