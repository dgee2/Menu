using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuDB.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeTitleOwnershipAndMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Recipe_Name",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Recipe");

            migrationBuilder.AddColumn<string>(
                name: "AccessScope",
                table: "Recipe",
                type: "nvarchar(30)",
                nullable: false,
                defaultValue: "Private");

            migrationBuilder.AddColumn<int>(
                name: "CookTimeMinutes",
                table: "Recipe",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Recipe",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerUserId",
                table: "Recipe",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrepTimeMinutes",
                table: "Recipe",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Servings",
                table: "Recipe",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Recipe",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Recipe",
                type: "nvarchar(200)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TotalTimeMinutes",
                table: "Recipe",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Recipe",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YieldText",
                table: "Recipe",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_Recipe_OwnerUserId_Title",
                table: "Recipe",
                columns: new[] { "OwnerUserId", "Title" },
                unique: true,
                filter: "[OwnerUserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipe_ToMenuUser",
                table: "Recipe",
                column: "OwnerUserId",
                principalSchema: "identity",
                principalTable: "MenuUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipe_ToMenuUser",
                table: "Recipe");

            migrationBuilder.DropIndex(
                name: "UX_Recipe_OwnerUserId_Title",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "AccessScope",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "CookTimeMinutes",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "PrepTimeMinutes",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "Servings",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "TotalTimeMinutes",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "YieldText",
                table: "Recipe");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Recipe",
                type: "varchar(500)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "UX_Recipe_Name",
                table: "Recipe",
                column: "Name",
                unique: true);
        }
    }
}
