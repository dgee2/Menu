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
    private static readonly (string DisplayName, string QualifiedName)[] AffectedTables =
    [
        ("identity.MenuUser", "[identity].[MenuUser]"),
        ("Recipe", "[Recipe]"),
        ("RecipeIngredient", "[RecipeIngredient]"),
        ("RecipeStep", "[RecipeStep]"),
    ];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        EnsureAffectedTablesAreEmpty(migrationBuilder);
        DropAffectedTables(migrationBuilder);
        CreateSchema(migrationBuilder, useUuidKeys: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        EnsureAffectedTablesAreEmpty(migrationBuilder);
        DropAffectedTables(migrationBuilder);
        CreateSchema(migrationBuilder, useUuidKeys: false);
    }

    private static void EnsureAffectedTablesAreEmpty(MigrationBuilder migrationBuilder)
    {
        foreach (var (displayName, qualifiedName) in AffectedTables)
        {
            migrationBuilder.Sql($"IF EXISTS (SELECT 1 FROM {qualifiedName}) THROW 51000, 'ConvertRecipeEntityIdsToUuidV7AndSoftDelete requires an empty {displayName} table.', 1;");
        }
    }

    private static void DropAffectedTables(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "RecipeIngredient");
        migrationBuilder.DropTable(name: "RecipeStep");
        migrationBuilder.DropTable(name: "Recipe");
        migrationBuilder.DropTable(name: "MenuUser", schema: "identity");
    }

    private static void CreateSchema(MigrationBuilder migrationBuilder, bool useUuidKeys)
    {
        var menuUserId = useUuidKeys ? "uniqueidentifier NOT NULL" : "int IDENTITY(1, 1) NOT NULL";
        var recipeId = useUuidKeys ? "uniqueidentifier NOT NULL" : "int IDENTITY(1, 1) NOT NULL";
        var ownerUserId = useUuidKeys ? "uniqueidentifier NULL" : "int NULL";
        var childId = useUuidKeys ? "uniqueidentifier NOT NULL" : "int IDENTITY(1, 1) NOT NULL";
        var childRecipeId = useUuidKeys ? "uniqueidentifier NOT NULL" : "int NOT NULL";
        var deletedAtColumn = useUuidKeys ? "    [DeletedAtUtc] datetime2 NULL,\n" : string.Empty;
        var titleIndex = useUuidKeys
            ? "CREATE UNIQUE INDEX [UX_Recipe_OwnerUserId_Title] ON [Recipe] ([OwnerUserId], [Title]) WHERE [DeletedAtUtc] IS NULL;"
            : "CREATE UNIQUE INDEX [UX_Recipe_OwnerUserId_Title] ON [Recipe] ([OwnerUserId], [Title]);";

        migrationBuilder.Sql($"""
            CREATE TABLE [identity].[MenuUser]
            (
                [Id] {menuUserId},
                [AuthSubject] nvarchar(256) NOT NULL,
                [DisplayName] nvarchar(100) NOT NULL,
                [Email] nvarchar(256) NULL,
                [AvatarUrl] nvarchar(512) NULL,
                [CreatedAtUtc] datetime2 NOT NULL,
                [LastSeenAtUtc] datetime2 NOT NULL,
                CONSTRAINT [PK_MenuUser] PRIMARY KEY ([Id])
            );

            CREATE TABLE [Recipe]
            (
                [Id] {recipeId},
                [Title] nvarchar(200) NOT NULL,
                [OwnerUserId] {ownerUserId},
                [AccessScopeId] tinyint NOT NULL CONSTRAINT [DF_Recipe_AccessScopeId] DEFAULT (1),
                [Summary] nvarchar(max) NULL,
                [Servings] int NULL,
                [YieldText] nvarchar(100) NULL,
                [PrepTimeMinutes] int NULL,
                [CookTimeMinutes] int NULL,
                [TotalTimeMinutes] int NULL,
                [CreatedAtUtc] datetime2 NOT NULL CONSTRAINT [DF_Recipe_CreatedAtUtc] DEFAULT (GETUTCDATE()),
                [UpdatedAtUtc] datetime2 NOT NULL CONSTRAINT [DF_Recipe_UpdatedAtUtc] DEFAULT (GETUTCDATE()),
            {deletedAtColumn}        CONSTRAINT [PK_Recipe] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_Recipe_ToMenuUser] FOREIGN KEY ([OwnerUserId]) REFERENCES [identity].[MenuUser] ([Id]) ON DELETE SET NULL,
                CONSTRAINT [FK_Recipe_ToRecipeAccessScope] FOREIGN KEY ([AccessScopeId]) REFERENCES [RecipeAccessScope] ([Id]) ON DELETE NO ACTION
            );

            CREATE TABLE [RecipeIngredient]
            (
                [Id] {childId},
                [RecipeId] {childRecipeId},
                [SortOrder] int NOT NULL,
                [IngredientText] nvarchar(200) NOT NULL,
                [MeasureText] nvarchar(100) NOT NULL,
                [SectionTitle] nvarchar(100) NULL,
                [Amount] decimal(10,4) NULL,
                [UnitText] nvarchar(50) NULL,
                [PreparationText] nvarchar(100) NULL,
                [IsOptional] bit NOT NULL,
                [CanonicalIngredientId] int NULL,
                [CanonicalUnitId] int NULL,
                CONSTRAINT [PK_RecipeIngredient] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_RecipeIngredient_Ingredient_CanonicalIngredientId] FOREIGN KEY ([CanonicalIngredientId]) REFERENCES [Ingredient] ([Id]) ON DELETE SET NULL,
                CONSTRAINT [FK_RecipeIngredient_ToRecipe] FOREIGN KEY ([RecipeId]) REFERENCES [Recipe] ([Id]) ON DELETE CASCADE,
                CONSTRAINT [FK_RecipeIngredient_Unit_CanonicalUnitId] FOREIGN KEY ([CanonicalUnitId]) REFERENCES [Unit] ([Id]) ON DELETE SET NULL
            );

            CREATE TABLE [RecipeStep]
            (
                [Id] {childId},
                [RecipeId] {childRecipeId},
                [SortOrder] int NOT NULL,
                [Title] nvarchar(200) NULL,
                [InstructionText] nvarchar(max) NOT NULL,
                [DurationMinutes] int NULL,
                CONSTRAINT [PK_RecipeStep] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_RecipeStep_ToRecipe] FOREIGN KEY ([RecipeId]) REFERENCES [Recipe] ([Id]) ON DELETE CASCADE
            );

            CREATE UNIQUE INDEX [UX_MenuUser_AuthSubject] ON [identity].[MenuUser] ([AuthSubject]);
            CREATE INDEX [IX_Recipe_AccessScopeId] ON [Recipe] ([AccessScopeId]);
            {titleIndex}
            CREATE INDEX [IX_RecipeIngredient_CanonicalIngredientId] ON [RecipeIngredient] ([CanonicalIngredientId]);
            CREATE INDEX [IX_RecipeIngredient_CanonicalUnitId] ON [RecipeIngredient] ([CanonicalUnitId]);
            CREATE INDEX [IX_RecipeIngredient_RecipeId] ON [RecipeIngredient] ([RecipeId]);
            CREATE INDEX [IX_RecipeStep_RecipeId] ON [RecipeStep] ([RecipeId]);
            """);
    }
}
