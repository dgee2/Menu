using MenuDB;
using MenuDB.Data;
using MenuApi.Exceptions;
using MenuApi.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MenuApi.Repositories;

public class MenuUserRepository(MenuDbContext db) : IMenuUserRepository
{
    public async Task<MenuUserId> UpsertAsync(string authSubject, string? displayName, string? email, string? avatarUrl)
    {
        var now = DateTime.UtcNow;

        var existing = await db.MenuUsers
            .FirstOrDefaultAsync(u => u.AuthSubject == authSubject)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            ApplyProfileUpdates(existing, displayName, email, avatarUrl, now);
            await db.SaveChangesAsync().ConfigureAwait(false);
            return MenuUserId.From(existing.Id);
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

            var concurrent = await db.MenuUsers
                .FirstAsync(u => u.AuthSubject == authSubject)
                .ConfigureAwait(false);

            ApplyProfileUpdates(concurrent, displayName, email, avatarUrl, now);
            await db.SaveChangesAsync().ConfigureAwait(false);
            return MenuUserId.From(concurrent.Id);
        }
    }

    private static void ApplyProfileUpdates(
        MenuUserEntity user,
        string? displayName,
        string? email,
        string? avatarUrl,
        DateTime now)
    {
        user.LastSeenAtUtc = now;
        if (displayName is not null)
        {
            user.DisplayName = displayName;
        }

        if (email is not null)
        {
            user.Email = email;
        }

        if (avatarUrl is not null)
        {
            user.AvatarUrl = avatarUrl;
        }
    }
}
