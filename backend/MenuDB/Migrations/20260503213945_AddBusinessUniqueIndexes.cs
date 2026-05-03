using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuDB.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT [Name]
                    FROM [Recipe]
                    GROUP BY [Name]
                    HAVING COUNT(*) > 1
                )
                BEGIN
                    THROW 50000, 'Cannot apply AddBusinessUniqueIndexes while duplicate Recipe.Name rows exist. Remediate duplicate recipe names before retrying the migration.', 1;
                END;
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT [Name]
                    FROM [Ingredient]
                    GROUP BY [Name]
                    HAVING COUNT(*) > 1
                )
                BEGIN
                    THROW 50000, 'Cannot apply AddBusinessUniqueIndexes while duplicate Ingredient.Name rows exist. Remediate duplicate ingredient names before retrying the migration.', 1;
                END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Recipe_Name",
                table: "Recipe",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ingredient_Name",
                table: "Ingredient",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recipe_Name",
                table: "Recipe");

            migrationBuilder.DropIndex(
                name: "IX_Ingredient_Name",
                table: "Ingredient");
        }
    }
}
