using MenuDB;
using MenuDB.Data;
using MenuApi.Exceptions;
using MenuApi.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MenuApi.Repositories;

public class MenuUserRepository(MenuDbContext db) : IMenuUserRepository
{
    public async Task<MenuUserId> UpsertAsync(string authSubject, string displayName, string? email, string? avatarUrl)
    {
        var now = DateTime.UtcNow;

        var updated = await db.MenuUsers
            .Where(u => u.AuthSubject == authSubject)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.DisplayName, displayName)
                .SetProperty(u => u.Email, email)
                .SetProperty(u => u.AvatarUrl, avatarUrl)
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
            DisplayName = displayName,
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
                    .SetProperty(u => u.DisplayName, displayName)
                    .SetProperty(u => u.Email, email)
                    .SetProperty(u => u.AvatarUrl, avatarUrl)
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
}
