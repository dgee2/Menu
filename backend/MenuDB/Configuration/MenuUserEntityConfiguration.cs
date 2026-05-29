using MenuDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuDB.Configuration;

public class MenuUserEntityConfiguration : IEntityTypeConfiguration<MenuUserEntity>
{
    public void Configure(EntityTypeBuilder<MenuUserEntity> builder)
    {
        builder.ToTable("MenuUser", "identity");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.AuthSubject).HasColumnType("nvarchar(256)").IsRequired();
        builder.Property(x => x.DisplayName).HasColumnType("nvarchar(100)").IsRequired();
        builder.Property(x => x.Email).HasColumnType("nvarchar(256)");
        builder.Property(x => x.AvatarUrl).HasColumnType("nvarchar(512)");
        builder.Property(x => x.CreatedAtUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.LastSeenAtUtc).HasColumnType("datetime2").IsRequired();
        builder.HasIndex(x => x.AuthSubject).IsUnique().HasDatabaseName("UX_MenuUser_AuthSubject");
    }
}
