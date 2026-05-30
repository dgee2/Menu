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
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeIngredient_Ingredient_IngredientId",
                table: "RecipeIngredient");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeIngredient_Unit_UnitId",
                table: "RecipeIngredient");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecipeIngredient",
                table: "RecipeIngredient");

            migrationBuilder.DropIndex(
                name: "IX_RecipeIngredient_IngredientId",
                table: "RecipeIngredient");

            migrationBuilder.DropIndex(
                name: "IX_RecipeIngredient_UnitId",
                table: "RecipeIngredient");

            migrationBuilder.RenameColumn(
                name: "UnitId",
                table: "RecipeIngredient",
                newName: "SortOrder");

            migrationBuilder.RenameColumn(
                name: "IngredientId",
                table: "RecipeIngredient",
                newName: "Id");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "RecipeIngredient",
                type: "decimal(10,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,4)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "RecipeIngredient",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "CanonicalIngredientId",
                table: "RecipeIngredient",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CanonicalUnitId",
                table: "RecipeIngredient",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IngredientText",
                table: "RecipeIngredient",
                type: "nvarchar(200)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsOptional",
                table: "RecipeIngredient",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MeasureText",
                table: "RecipeIngredient",
                type: "nvarchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreparationText",
                table: "RecipeIngredient",
                type: "nvarchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SectionTitle",
                table: "RecipeIngredient",
                type: "nvarchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitText",
                table: "RecipeIngredient",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecipeIngredient",
                table: "RecipeIngredient",
                column: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeIngredient_Ingredient_CanonicalIngredientId",
                table: "RecipeIngredient",
                column: "CanonicalIngredientId",
                principalTable: "Ingredient",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeIngredient_Unit_CanonicalUnitId",
                table: "RecipeIngredient",
                column: "CanonicalUnitId",
                principalTable: "Unit",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeIngredient_Ingredient_CanonicalIngredientId",
                table: "RecipeIngredient");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeIngredient_Unit_CanonicalUnitId",
                table: "RecipeIngredient");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecipeIngredient",
                table: "RecipeIngredient");

            migrationBuilder.DropIndex(
                name: "IX_RecipeIngredient_CanonicalIngredientId",
                table: "RecipeIngredient");

            migrationBuilder.DropIndex(
                name: "IX_RecipeIngredient_CanonicalUnitId",
                table: "RecipeIngredient");

            migrationBuilder.DropIndex(
                name: "IX_RecipeIngredient_RecipeId",
                table: "RecipeIngredient");

            migrationBuilder.DropColumn(
                name: "CanonicalIngredientId",
                table: "RecipeIngredient");

            migrationBuilder.DropColumn(
                name: "CanonicalUnitId",
                table: "RecipeIngredient");

            migrationBuilder.DropColumn(
                name: "IngredientText",
                table: "RecipeIngredient");

            migrationBuilder.DropColumn(
                name: "IsOptional",
                table: "RecipeIngredient");

            migrationBuilder.DropColumn(
                name: "MeasureText",
                table: "RecipeIngredient");

            migrationBuilder.DropColumn(
                name: "PreparationText",
                table: "RecipeIngredient");

            migrationBuilder.DropColumn(
                name: "SectionTitle",
                table: "RecipeIngredient");

            migrationBuilder.DropColumn(
                name: "UnitText",
                table: "RecipeIngredient");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                table: "RecipeIngredient",
                newName: "UnitId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "RecipeIngredient",
                newName: "IngredientId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "RecipeIngredient",
                type: "decimal(10,4)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IngredientId",
                table: "RecipeIngredient",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecipeIngredient",
                table: "RecipeIngredient",
                columns: new[] { "RecipeId", "IngredientId", "UnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredient_IngredientId",
                table: "RecipeIngredient",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredient_UnitId",
                table: "RecipeIngredient",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeIngredient_Ingredient_IngredientId",
                table: "RecipeIngredient",
                column: "IngredientId",
                principalTable: "Ingredient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeIngredient_Unit_UnitId",
                table: "RecipeIngredient",
                column: "UnitId",
                principalTable: "Unit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
