using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spry.AuthServer.Migrations
{
    /// <inheritdoc />
    public partial class UserDeviceLogins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserDeviceLogins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Device = table.Column<string>(type: "text", nullable: true),
                    Request = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDeviceLogins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDeviceLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDeviceLogins_UserId",
                table: "UserDeviceLogins",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDeviceLogins");
        }
    }
}
