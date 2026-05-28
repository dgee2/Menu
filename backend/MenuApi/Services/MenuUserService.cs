using MenuApi.Repositories;
using MenuApi.ValueObjects;

namespace MenuApi.Services;

public class MenuUserService(IMenuUserRepository menuUserRepository) : IMenuUserService
{
    public async Task<MenuUserId> ProvisionAsync(string authSubject, string displayName, string? email, string? avatarUrl)
    {
        return await menuUserRepository.UpsertAsync(authSubject, displayName, email, avatarUrl).ConfigureAwait(false);
    }
}
