using MenuApi.ValueObjects;
using MenuApi.ViewModel;

namespace MenuApi.Services;

public interface IMenuUserService
{
    Task<MenuUserId> ProvisionAsync(string authSubject, string? displayName, string? email, string? avatarUrl);

    Task<UserProfile?> GetCurrentUserAsync(MenuUserId menuUserId);
}
