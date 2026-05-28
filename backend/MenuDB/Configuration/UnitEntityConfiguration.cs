using MenuDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuDB.Configuration;

public class UnitEntityConfiguration : IEntityTypeConfiguration<UnitEntity>
{
    public void Configure(EntityTypeBuilder<UnitEntity> builder)
    {
        builder.ToTable("Unit");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).HasColumnType("varchar(50)").IsRequired();
        builder.Property(x => x.Abbreviation).HasColumnType("varchar(5)");
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UX_Unit_Name");
        builder.HasIndex(x => x.Abbreviation).IsUnique().HasDatabaseName("UX_Unit_Abbreviation").HasFilter("[Abbreviation] IS NOT NULL");
        builder.HasOne(x => x.UnitType)
            .WithMany(x => x.Units)
            .HasForeignKey(x => x.UnitTypeId)
            .HasConstraintName("FK_Unit_ToUnitType");
        builder.HasData(
            new UnitEntity { Id = 1, Name = "Millilitres", Abbreviation = "ml", UnitTypeId = 1 },
            new UnitEntity { Id = 2, Name = "Litres", Abbreviation = "l", UnitTypeId = 1 },
            new UnitEntity { Id = 3, Name = "Quantity", Abbreviation = null, UnitTypeId = 2 },
            new UnitEntity { Id = 4, Name = "Grams", Abbreviation = "g", UnitTypeId = 3 },
            new UnitEntity { Id = 5, Name = "Kilograms", Abbreviation = "kg", UnitTypeId = 3 });
    }
}
