using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuDB.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueNameConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_UnitType_Name",
                table: "UnitType",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Unit_Abbreviation",
                table: "Unit",
                column: "Abbreviation",
                unique: true,
                filter: "[Abbreviation] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Unit_Name",
                table: "Unit",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Recipe_Name",
                table: "Recipe",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Ingredient_Name",
                table: "Ingredient",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_UnitType_Name",
                table: "UnitType");

            migrationBuilder.DropIndex(
                name: "UX_Unit_Abbreviation",
                table: "Unit");

            migrationBuilder.DropIndex(
                name: "UX_Unit_Name",
                table: "Unit");

            migrationBuilder.DropIndex(
                name: "UX_Recipe_Name",
                table: "Recipe");

            migrationBuilder.DropIndex(
                name: "UX_Ingredient_Name",
                table: "Ingredient");
        }
    }
}
