using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MenuDB.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.CreateTable(
                name: "MenuUser",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthSubject = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(512)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuUser", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UX_MenuUser_AuthSubject",
                schema: "identity",
                table: "MenuUser",
                column: "AuthSubject",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuUser",
                schema: "identity");
        }
    }
}
