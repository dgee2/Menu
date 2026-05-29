using MenuDB;
using MenuDB.Data;
using MenuApi.Exceptions;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace MenuApi.Repositories;

public class MenuUserRepository(MenuDbContext db) : IMenuUserRepository
{
    public async Task<MenuUserId> UpsertAsync(string authSubject, string? displayName, string? email, string? avatarUrl)
    {
        var now = DateTime.UtcNow;

        var updated = await db.MenuUsers
            .Where(u => u.AuthSubject == authSubject)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.DisplayName, u => displayName ?? u.DisplayName)
                .SetProperty(u => u.Email, u => email ?? u.Email)
                .SetProperty(u => u.AvatarUrl, u => avatarUrl ?? u.AvatarUrl)
                .SetProperty(u => u.LastSeenAtUtc, now))
            .ConfigureAwait(false);

        if (updated > 0)
        {
            var existingId = await db.MenuUsers
                .Where(u => u.AuthSubject == authSubject)
                .Select(u => u.Id)
                .FirstAsync()
                .ConfigureAwait(false);

            return MenuUserId.From(existingId);
        }

        var entity = new MenuUserEntity
        {
            AuthSubject = authSubject,
            DisplayName = displayName ?? authSubject,
            Email = email,
            AvatarUrl = avatarUrl,
            CreatedAtUtc = now,
            LastSeenAtUtc = now,
        };

        db.MenuUsers.Add(entity);

        try
        {
            await db.SaveChangesAsync().ConfigureAwait(false);
            return MenuUserId.From(entity.Id);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            db.Entry(entity).State = EntityState.Detached;

            await db.MenuUsers
                .Where(u => u.AuthSubject == authSubject)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.DisplayName, u => displayName ?? u.DisplayName)
                    .SetProperty(u => u.Email, u => email ?? u.Email)
                    .SetProperty(u => u.AvatarUrl, u => avatarUrl ?? u.AvatarUrl)
                    .SetProperty(u => u.LastSeenAtUtc, now))
                .ConfigureAwait(false);

            var concurrentId = await db.MenuUsers
                .Where(u => u.AuthSubject == authSubject)
                .Select(u => u.Id)
                .FirstAsync()
                .ConfigureAwait(false);

            return MenuUserId.From(concurrentId);
        }
    }

    public async Task<UserProfile?> GetAsync(MenuUserId menuUserId)
    {
        return await db.MenuUsers
            .Where(u => u.Id == menuUserId.Value)
            .Select(u => new UserProfile
            {
                Id = MenuUserId.From(u.Id),
                AuthSubject = u.AuthSubject,
                DisplayName = u.DisplayName,
                Email = u.Email,
                AvatarUrl = u.AvatarUrl,
                CreatedAtUtc = u.CreatedAtUtc,
                LastSeenAtUtc = u.LastSeenAtUtc,
            })
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }
}
