using MenuApi.Repositories;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;

namespace MenuApi.Services;

public class MenuUserService(IMenuUserRepository menuUserRepository) : IMenuUserService
{
    public async Task<MenuUserId> ProvisionAsync(string authSubject, string? displayName, string? email, string? avatarUrl)
    {
        return await menuUserRepository.UpsertAsync(authSubject, displayName, email, avatarUrl).ConfigureAwait(false);
    }

    public async Task<UserProfile?> GetCurrentUserAsync(MenuUserId menuUserId)
    {
        return await menuUserRepository.GetAsync(menuUserId).ConfigureAwait(false);
    }
}
