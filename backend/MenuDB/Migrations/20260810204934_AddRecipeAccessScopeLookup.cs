using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MenuDB.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeAccessScopeLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Scaffolded operations reordered by hand, and the two Sql() calls added: EF put the
            // DropColumn first, which would have discarded every recipe's existing visibility.
            // The lookup table and the new column both have to exist before the data can move.
            migrationBuilder.CreateTable(
                name: "RecipeAccessScope",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeAccessScope", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "RecipeAccessScope",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { (byte)1, "Private" },
                    { (byte)2, "AuthenticatedUsers" }
                });

            migrationBuilder.CreateIndex(
                name: "UX_RecipeAccessScope_Name",
                table: "RecipeAccessScope",
                column: "Name",
                unique: true);

            migrationBuilder.AddColumn<byte>(
                name: "AccessScopeId",
                table: "Recipe",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);

            // Anything that does not match a known name keeps the default of 1 (Private) - the safe
            // direction to fail, since it can only ever narrow who could already see the recipe.
            migrationBuilder.Sql(@"
                UPDATE r
                SET r.AccessScopeId = s.Id
                FROM [Recipe] r
                INNER JOIN [RecipeAccessScope] s ON s.Name = r.AccessScope;");

            migrationBuilder.DropColumn(
                name: "AccessScope",
                table: "Recipe");

            migrationBuilder.CreateIndex(
                name: "IX_Recipe_AccessScopeId",
                table: "Recipe",
                column: "AccessScopeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipe_ToRecipeAccessScope",
                table: "Recipe",
                column: "AccessScopeId",
                principalTable: "RecipeAccessScope",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessScope",
                table: "Recipe",
                type: "nvarchar(30)",
                nullable: false,
                defaultValue: "Private");

            migrationBuilder.Sql(@"
                UPDATE r
                SET r.AccessScope = s.Name
                FROM [Recipe] r
                INNER JOIN [RecipeAccessScope] s ON s.Id = r.AccessScopeId;");

            migrationBuilder.DropForeignKey(
                name: "FK_Recipe_ToRecipeAccessScope",
                table: "Recipe");

            migrationBuilder.DropIndex(
                name: "IX_Recipe_AccessScopeId",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "AccessScopeId",
                table: "Recipe");

            migrationBuilder.DropTable(
                name: "RecipeAccessScope");
        }
    }
}
