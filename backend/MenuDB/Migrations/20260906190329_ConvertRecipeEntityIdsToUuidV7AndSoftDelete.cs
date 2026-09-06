using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuDB.Migrations;

/// <summary>
/// Rebuilds the still-empty entity tables so their keys can move from identity integers to
/// application-generated UUIDs in one schema change. A populated environment must be migrated
/// deliberately rather than having this migration discard user-owned rows.
/// </summary>
public partial class ConvertRecipeEntityIdsToUuidV7AndSoftDelete : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            IF EXISTS (SELECT 1 FROM [identity].[MenuUser])
                THROW 51000, 'ConvertRecipeEntityIdsToUuidV7AndSoftDelete requires an empty identity.MenuUser table.', 1;
            IF EXISTS (SELECT 1 FROM [Recipe])
                THROW 51000, 'ConvertRecipeEntityIdsToUuidV7AndSoftDelete requires an empty Recipe table.', 1;
            IF EXISTS (SELECT 1 FROM [RecipeIngredient])
                THROW 51000, 'ConvertRecipeEntityIdsToUuidV7AndSoftDelete requires an empty RecipeIngredient table.', 1;
            IF EXISTS (SELECT 1 FROM [RecipeStep])
                THROW 51000, 'ConvertRecipeEntityIdsToUuidV7AndSoftDelete requires an empty RecipeStep table.', 1;");

        migrationBuilder.DropTable(name: "RecipeIngredient");
        migrationBuilder.DropTable(name: "RecipeStep");
        migrationBuilder.DropTable(name: "Recipe");
        migrationBuilder.DropTable(name: "MenuUser", schema: "identity");

        migrationBuilder.CreateTable(
            name: "MenuUser",
            schema: "identity",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AuthSubject = table.Column<string>(type: "nvarchar(256)", nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                Email = table.Column<string>(type: "nvarchar(256)", nullable: true),
                AvatarUrl = table.Column<string>(type: "nvarchar(512)", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastSeenAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_MenuUser", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Recipe",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", nullable: false),
                OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                AccessScopeId = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Servings = table.Column<int>(type: "int", nullable: true),
                YieldText = table.Column<string>(type: "nvarchar(100)", nullable: true),
                PrepTimeMinutes = table.Column<int>(type: "int", nullable: true),
                CookTimeMinutes = table.Column<int>(type: "int", nullable: true),
                TotalTimeMinutes = table.Column<int>(type: "int", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Recipe", x => x.Id);
                table.ForeignKey("FK_Recipe_ToMenuUser", x => x.OwnerUserId, "MenuUser", "Id", principalSchema: "identity", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_Recipe_ToRecipeAccessScope", x => x.AccessScopeId, "RecipeAccessScope", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "RecipeIngredient",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RecipeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false),
                IngredientText = table.Column<string>(type: "nvarchar(200)", nullable: false),
                MeasureText = table.Column<string>(type: "nvarchar(100)", nullable: false),
                SectionTitle = table.Column<string>(type: "nvarchar(100)", nullable: true),
                Amount = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                UnitText = table.Column<string>(type: "nvarchar(50)", nullable: true),
                PreparationText = table.Column<string>(type: "nvarchar(100)", nullable: true),
                IsOptional = table.Column<bool>(type: "bit", nullable: false),
                CanonicalIngredientId = table.Column<int>(type: "int", nullable: true),
                CanonicalUnitId = table.Column<int>(type: "int", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RecipeIngredient", x => x.Id);
                table.ForeignKey("FK_RecipeIngredient_Ingredient_CanonicalIngredientId", x => x.CanonicalIngredientId, "Ingredient", "Id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_RecipeIngredient_ToRecipe", x => x.RecipeId, "Recipe", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_RecipeIngredient_Unit_CanonicalUnitId", x => x.CanonicalUnitId, "Unit", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "RecipeStep",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RecipeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", nullable: true),
                InstructionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                DurationMinutes = table.Column<int>(type: "int", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RecipeStep", x => x.Id);
                table.ForeignKey("FK_RecipeStep_ToRecipe", x => x.RecipeId, "Recipe", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("UX_MenuUser_AuthSubject", "MenuUser", "AuthSubject", unique: true, schema: "identity");
        migrationBuilder.CreateIndex("IX_Recipe_AccessScopeId", "Recipe", "AccessScopeId");
        migrationBuilder.CreateIndex("UX_Recipe_OwnerUserId_Title", "Recipe", new[] { "OwnerUserId", "Title" }, unique: true, filter: "[DeletedAtUtc] IS NULL");
        migrationBuilder.CreateIndex("IX_RecipeIngredient_CanonicalIngredientId", "RecipeIngredient", "CanonicalIngredientId");
        migrationBuilder.CreateIndex("IX_RecipeIngredient_CanonicalUnitId", "RecipeIngredient", "CanonicalUnitId");
        migrationBuilder.CreateIndex("IX_RecipeIngredient_RecipeId", "RecipeIngredient", "RecipeId");
        migrationBuilder.CreateIndex("IX_RecipeStep_RecipeId", "RecipeStep", "RecipeId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            IF EXISTS (SELECT 1 FROM [identity].[MenuUser])
                THROW 51000, 'Rolling back ConvertRecipeEntityIdsToUuidV7AndSoftDelete requires an empty identity.MenuUser table.', 1;
            IF EXISTS (SELECT 1 FROM [Recipe])
                THROW 51000, 'Rolling back ConvertRecipeEntityIdsToUuidV7AndSoftDelete requires an empty Recipe table.', 1;
            IF EXISTS (SELECT 1 FROM [RecipeIngredient])
                THROW 51000, 'Rolling back ConvertRecipeEntityIdsToUuidV7AndSoftDelete requires an empty RecipeIngredient table.', 1;
            IF EXISTS (SELECT 1 FROM [RecipeStep])
                THROW 51000, 'Rolling back ConvertRecipeEntityIdsToUuidV7AndSoftDelete requires an empty RecipeStep table.', 1;");

        migrationBuilder.DropTable(name: "RecipeIngredient");
        migrationBuilder.DropTable(name: "RecipeStep");
        migrationBuilder.DropTable(name: "Recipe");
        migrationBuilder.DropTable(name: "MenuUser", schema: "identity");

        migrationBuilder.CreateTable(
            name: "MenuUser",
            schema: "identity",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                AuthSubject = table.Column<string>(type: "nvarchar(256)", nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                Email = table.Column<string>(type: "nvarchar(256)", nullable: true),
                AvatarUrl = table.Column<string>(type: "nvarchar(512)", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastSeenAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_MenuUser", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Recipe",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                Title = table.Column<string>(type: "nvarchar(200)", nullable: false),
                OwnerUserId = table.Column<int>(type: "int", nullable: true),
                AccessScopeId = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Servings = table.Column<int>(type: "int", nullable: true),
                YieldText = table.Column<string>(type: "nvarchar(100)", nullable: true),
                PrepTimeMinutes = table.Column<int>(type: "int", nullable: true),
                CookTimeMinutes = table.Column<int>(type: "int", nullable: true),
                TotalTimeMinutes = table.Column<int>(type: "int", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Recipe", x => x.Id);
                table.ForeignKey("FK_Recipe_ToMenuUser", x => x.OwnerUserId, "MenuUser", "Id", principalSchema: "identity", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_Recipe_ToRecipeAccessScope", x => x.AccessScopeId, "RecipeAccessScope", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "RecipeIngredient",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                RecipeId = table.Column<int>(type: "int", nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false),
                IngredientText = table.Column<string>(type: "nvarchar(200)", nullable: false),
                MeasureText = table.Column<string>(type: "nvarchar(100)", nullable: false),
                SectionTitle = table.Column<string>(type: "nvarchar(100)", nullable: true),
                Amount = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                UnitText = table.Column<string>(type: "nvarchar(50)", nullable: true),
                PreparationText = table.Column<string>(type: "nvarchar(100)", nullable: true),
                IsOptional = table.Column<bool>(type: "bit", nullable: false),
                CanonicalIngredientId = table.Column<int>(type: "int", nullable: true),
                CanonicalUnitId = table.Column<int>(type: "int", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RecipeIngredient", x => x.Id);
                table.ForeignKey("FK_RecipeIngredient_Ingredient_CanonicalIngredientId", x => x.CanonicalIngredientId, "Ingredient", "Id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_RecipeIngredient_ToRecipe", x => x.RecipeId, "Recipe", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_RecipeIngredient_Unit_CanonicalUnitId", x => x.CanonicalUnitId, "Unit", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "RecipeStep",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                RecipeId = table.Column<int>(type: "int", nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", nullable: true),
                InstructionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                DurationMinutes = table.Column<int>(type: "int", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RecipeStep", x => x.Id);
                table.ForeignKey("FK_RecipeStep_ToRecipe", x => x.RecipeId, "Recipe", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("UX_MenuUser_AuthSubject", "MenuUser", "AuthSubject", unique: true, schema: "identity");
        migrationBuilder.CreateIndex("IX_Recipe_AccessScopeId", "Recipe", "AccessScopeId");
        migrationBuilder.CreateIndex("UX_Recipe_OwnerUserId_Title", "Recipe", new[] { "OwnerUserId", "Title" }, unique: true);
        migrationBuilder.CreateIndex("IX_RecipeIngredient_CanonicalIngredientId", "RecipeIngredient", "CanonicalIngredientId");
        migrationBuilder.CreateIndex("IX_RecipeIngredient_CanonicalUnitId", "RecipeIngredient", "CanonicalUnitId");
        migrationBuilder.CreateIndex("IX_RecipeIngredient_RecipeId", "RecipeIngredient", "RecipeId");
        migrationBuilder.CreateIndex("IX_RecipeStep_RecipeId", "RecipeStep", "RecipeId");
    }
}
