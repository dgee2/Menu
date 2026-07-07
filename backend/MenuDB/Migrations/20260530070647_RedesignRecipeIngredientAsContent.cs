using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuDB.Migrations
{
    /// <inheritdoc />
    public partial class RedesignRecipeIngredientAsContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TABLE [RecipeIngredient_Staging] (
                    [RecipeId] int NOT NULL,
                    [SortOrder] int NOT NULL,
                    [IngredientText] nvarchar(200) NOT NULL,
                    [MeasureText] nvarchar(100) NOT NULL,
                    [SectionTitle] nvarchar(100) NULL,
                    [Amount] decimal(10,4) NULL,
                    [UnitText] nvarchar(50) NULL,
                    [PreparationText] nvarchar(100) NULL,
                    [IsOptional] bit NOT NULL,
                    [CanonicalIngredientId] int NULL,
                    [CanonicalUnitId] int NULL
                );
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [RecipeIngredient_Staging] (
                    [RecipeId],
                    [SortOrder],
                    [IngredientText],
                    [MeasureText],
                    [SectionTitle],
                    [Amount],
                    [UnitText],
                    [PreparationText],
                    [IsOptional],
                    [CanonicalIngredientId],
                    [CanonicalUnitId])
                SELECT
                    [ri].[RecipeId],
                    ROW_NUMBER() OVER (PARTITION BY [ri].[RecipeId] ORDER BY [ri].[IngredientId], [ri].[UnitId]) - 1,
                    COALESCE([i].[Name], N''),
                    LEFT(LTRIM(RTRIM(CONCAT(CONVERT(nvarchar(32), [ri].[Amount]), N' ', COALESCE(NULLIF([u].[Abbreviation], N''), [u].[Name], N'')))), 100),
                    NULL,
                    [ri].[Amount],
                    LEFT(COALESCE(NULLIF([u].[Abbreviation], N''), [u].[Name]), 50),
                    NULL,
                    CAST(0 AS bit),
                    [ri].[IngredientId],
                    [ri].[UnitId]
                FROM [RecipeIngredient] AS [ri]
                LEFT JOIN [Ingredient] AS [i] ON [i].[Id] = [ri].[IngredientId]
                LEFT JOIN [Unit] AS [u] ON [u].[Id] = [ri].[UnitId];
                """);

            migrationBuilder.DropTable(
                name: "RecipeIngredient");

            migrationBuilder.CreateTable(
                name: "RecipeIngredient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    table.ForeignKey(
                        name: "FK_RecipeIngredient_Ingredient_CanonicalIngredientId",
                        column: x => x.CanonicalIngredientId,
                        principalTable: "Ingredient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RecipeIngredient_ToRecipe",
                        column: x => x.RecipeId,
                        principalTable: "Recipe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeIngredient_Unit_CanonicalUnitId",
                        column: x => x.CanonicalUnitId,
                        principalTable: "Unit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.Sql(
                """
                SET IDENTITY_INSERT [RecipeIngredient] ON;

                INSERT INTO [RecipeIngredient] (
                    [Id],
                    [RecipeId],
                    [SortOrder],
                    [IngredientText],
                    [MeasureText],
                    [SectionTitle],
                    [Amount],
                    [UnitText],
                    [PreparationText],
                    [IsOptional],
                    [CanonicalIngredientId],
                    [CanonicalUnitId])
                SELECT
                    ROW_NUMBER() OVER (ORDER BY [RecipeId], [SortOrder], [CanonicalIngredientId], [CanonicalUnitId]),
                    [RecipeId],
                    [SortOrder],
                    [IngredientText],
                    [MeasureText],
                    [SectionTitle],
                    [Amount],
                    [UnitText],
                    [PreparationText],
                    [IsOptional],
                    [CanonicalIngredientId],
                    [CanonicalUnitId]
                FROM [RecipeIngredient_Staging];

                SET IDENTITY_INSERT [RecipeIngredient] OFF;
                """);

            migrationBuilder.DropTable(
                name: "RecipeIngredient_Staging");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredient_CanonicalIngredientId",
                table: "RecipeIngredient",
                column: "CanonicalIngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredient_CanonicalUnitId",
                table: "RecipeIngredient",
                column: "CanonicalUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredient_RecipeId",
                table: "RecipeIngredient",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM [RecipeIngredient]
                    WHERE [CanonicalIngredientId] IS NULL
                       OR [CanonicalUnitId] IS NULL
                )
                BEGIN
                    THROW 51001, 'Migration rollback aborted: RecipeIngredient contains rows that cannot be represented in the legacy schema. Resolve or remove non-canonical rows before rolling back migration 20260530070647.', 1;
                END
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM [RecipeIngredient]
                    WHERE [CanonicalIngredientId] IS NOT NULL
                      AND [CanonicalUnitId] IS NOT NULL
                      AND [Amount] IS NULL
                )
                BEGIN
                    THROW 51003, 'Migration rollback aborted: canonical RecipeIngredient rows contain NULL Amount values that cannot be represented in the legacy schema. Populate Amount before rolling back migration 20260530070647.', 1;
                END
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM [RecipeIngredient]
                    WHERE [CanonicalIngredientId] IS NOT NULL
                      AND [CanonicalUnitId] IS NOT NULL
                    GROUP BY [RecipeId], [CanonicalIngredientId], [CanonicalUnitId]
                    HAVING COUNT(*) > 1
                )
                BEGIN
                    THROW 51004, 'Migration rollback aborted: multiple RecipeIngredient rows map to the same legacy canonical ingredient and unit pair. Deduplicate canonical rows before rolling back migration 20260530070647.', 1;
                END
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE [RecipeIngredient_Staging] (
                    [RecipeId] int NOT NULL,
                    [IngredientId] int NOT NULL,
                    [UnitId] int NOT NULL,
                    [Amount] decimal(10,4) NOT NULL
                );
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO [RecipeIngredient_Staging] (
                    [RecipeId],
                    [IngredientId],
                    [UnitId],
                    [Amount])
                SELECT
                    [RecipeId],
                    [CanonicalIngredientId],
                    [CanonicalUnitId],
                    [Amount]
                FROM [RecipeIngredient]
                WHERE [CanonicalIngredientId] IS NOT NULL
                  AND [CanonicalUnitId] IS NOT NULL;
                """);

            migrationBuilder.DropTable(
                name: "RecipeIngredient");

            migrationBuilder.CreateTable(
                name: "RecipeIngredient",
                columns: table => new
                {
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    IngredientId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeIngredient", x => new { x.RecipeId, x.IngredientId, x.UnitId });
                    table.ForeignKey(
                        name: "FK_RecipeIngredient_Ingredient_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeIngredient_ToRecipe",
                        column: x => x.RecipeId,
                        principalTable: "Recipe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeIngredient_Unit_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Unit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO [RecipeIngredient] (
                    [RecipeId],
                    [IngredientId],
                    [UnitId],
                    [Amount])
                SELECT
                    [RecipeId],
                    [IngredientId],
                    [UnitId],
                    [Amount]
                FROM [RecipeIngredient_Staging];
                """);

            migrationBuilder.DropTable(
                name: "RecipeIngredient_Staging");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredient_IngredientId",
                table: "RecipeIngredient",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredient_UnitId",
                table: "RecipeIngredient",
                column: "UnitId");
        }
    }
}
