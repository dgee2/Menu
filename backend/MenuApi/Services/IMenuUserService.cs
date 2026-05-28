using MenuApi.ValueObjects;

namespace MenuApi.Services;

public interface IMenuUserService
{
    Task<MenuUserId> ProvisionAsync(string authSubject, string displayName, string? email, string? avatarUrl);
}
