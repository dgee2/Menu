using Aspire.Hosting.Testing;
using AwesomeAssertions;
using MenuApi.Exceptions;
using MenuApi.Integration.Tests.Factory;
using MenuApi.Repositories;
using MenuDB;
using MenuDB.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public class LegacyOwnerProvisioningIntegrationTests(ApiTestFixture fixture)
{
    private static readonly Guid LegacyOwnerId = Guid.Empty;

    [Theory]
    [InlineData("MENU:LEGACY-RECIPE-OWNER:1195")]
    [InlineData("menu:legacy-recipe-owner:1195 ")]
    public async Task DatabaseEquivalentLegacySubjectCannotProvisionLegacyOwner(string authSubject)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = await fixture.app.GetConnectionStringAsync("menu", cancellationToken);
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        await using var db = new MenuDbContext(options);
        var now = DateTime.UtcNow;
        var legacyOwner = new MenuUserEntity
        {
            Id = LegacyOwnerId,
            AuthSubject = LegacyRecipeOwner.AuthSubject,
            DisplayName = "Legacy recipe owner",
            CreatedAtUtc = now,
            LastSeenAtUtc = now,
        };
        db.MenuUsers.Add(legacyOwner);
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            var repository = new MenuUserRepository(db);
            Func<Task> provision = () => repository.UpsertAsync(authSubject, "Spoofed owner", null, null);

            await provision.Should().ThrowAsync<ForbiddenAccessException>();

            var storedOwner = await db.MenuUsers.AsNoTracking()
                .SingleAsync(user => user.Id == LegacyOwnerId, cancellationToken);
            storedOwner.DisplayName.Should().Be("Legacy recipe owner");
        }
        finally
        {
            db.MenuUsers.Remove(legacyOwner);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
