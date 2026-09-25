using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuDB.Migrations
{
    /// <inheritdoc />
    public partial class RequireRecipeOwner : Migration
    {
        private const string LegacyOwnerId = "01a0da16-bebb-7890-9b45-5d7a9ceebc8a";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipe_ToMenuUser",
                table: "Recipe");

            // Older recipes have no caller identity to recover. Keep their content under a
            // reserved account that the provisioning middleware never authenticates. The
            // account uses a fixed UUIDv7 identifier so generated scripts are reproducible.
            migrationBuilder.Sql(
                $"""
                IF EXISTS (SELECT 1 FROM [Recipe] WHERE [OwnerUserId] IS NULL)
                BEGIN
                    IF EXISTS (SELECT 1 FROM [identity].[MenuUser] WHERE [AuthSubject] = N'menu:legacy-recipe-owner:1195')
                        THROW 51001, 'The reserved legacy recipe owner already exists; review it before applying this migration.', 1;

                    DECLARE @legacyOwnerId uniqueidentifier = '{LegacyOwnerId}';
                    DECLARE @now datetime2 = GETUTCDATE();

                    INSERT INTO [identity].[MenuUser]
                        ([Id], [AuthSubject], [DisplayName], [Email], [AvatarUrl], [CreatedAtUtc], [LastSeenAtUtc])
                    VALUES
                        (@legacyOwnerId, N'menu:legacy-recipe-owner:1195', N'Legacy recipe owner', NULL, NULL, @now, @now);

                    UPDATE [Recipe]
                    SET [OwnerUserId] = @legacyOwnerId
                    WHERE [OwnerUserId] IS NULL;
                END
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerUserId",
                table: "Recipe",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Recipe_ToMenuUser",
                table: "Recipe",
                column: "OwnerUserId",
                principalSchema: "identity",
                principalTable: "MenuUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipe_ToMenuUser",
                table: "Recipe");

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerUserId",
                table: "Recipe",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql(
                $"""
                IF EXISTS (
                    SELECT 1 FROM [identity].[MenuUser]
                    WHERE [Id] = '{LegacyOwnerId}' AND [AuthSubject] = N'menu:legacy-recipe-owner:1195'
                )
                BEGIN
                    UPDATE [Recipe]
                    SET [OwnerUserId] = NULL
                    WHERE [OwnerUserId] = '{LegacyOwnerId}';

                    DELETE FROM [identity].[MenuUser]
                    WHERE [Id] = '{LegacyOwnerId}' AND [AuthSubject] = N'menu:legacy-recipe-owner:1195';
                END
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Recipe_ToMenuUser",
                table: "Recipe",
                column: "OwnerUserId",
                principalSchema: "identity",
                principalTable: "MenuUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
