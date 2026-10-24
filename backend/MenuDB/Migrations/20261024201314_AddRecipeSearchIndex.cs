using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuDB.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeSearchIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecipeSearchIndex",
                columns: table => new
                {
                    RecipeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    SummaryText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IngredientsText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StepsText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessScope = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    IsPublishedExternally = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeSearchIndex", x => x.RecipeId);
                    table.ForeignKey(
                        name: "FK_RecipeSearchIndex_ToMenuUser",
                        column: x => x.OwnerUserId,
                        principalSchema: "identity",
                        principalTable: "MenuUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecipeSearchIndex_ToRecipe",
                        column: x => x.RecipeId,
                        principalTable: "Recipe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeSearchIndex_OwnerUserId",
                table: "RecipeSearchIndex",
                column: "OwnerUserId");

            migrationBuilder.Sql(
                "CREATE FULLTEXT CATALOG RecipeCatalog AS DEFAULT;" +
                "CREATE FULLTEXT INDEX ON dbo.RecipeSearchIndex (Title, SummaryText, IngredientsText, StepsText) " +
                "KEY INDEX PK_RecipeSearchIndex ON RecipeCatalog;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP FULLTEXT INDEX ON dbo.RecipeSearchIndex;" +
                "DROP FULLTEXT CATALOG RecipeCatalog;");

            migrationBuilder.DropTable(
                name: "RecipeSearchIndex");
        }
    }
}
