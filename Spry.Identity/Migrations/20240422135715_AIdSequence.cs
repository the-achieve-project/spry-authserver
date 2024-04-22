using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spry.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AIdSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "AIdNumbers");

            migrationBuilder.AddColumn<string>(
                name: "AchieveId",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SequenceId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: false,
                defaultValueSql: "nextval('\"AIdNumbers\"')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AchieveId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SequenceId",
                table: "AspNetUsers");

            migrationBuilder.DropSequence(
                name: "AIdNumbers");
        }
    }
}
