using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using hotelier_core_app.Migrations;

#nullable disable

namespace hotelier_core_app.Migrations.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260402000000_GuestProfileWalkInSupport")]
    public partial class GuestProfileWalkInSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the mandatory FK constraint on UserId
            migrationBuilder.DropForeignKey(
                name: "FK_GuestProfile_User_UserId",
                schema: "public",
                table: "GuestProfile");

            // Drop the non-null index on UserId
            migrationBuilder.DropIndex(
                name: "IX_GuestProfile_UserId",
                schema: "public",
                table: "GuestProfile");

            // Add new columns for walk-in guest data
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                schema: "public",
                table: "GuestProfile",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "public",
                table: "GuestProfile",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "public",
                table: "GuestProfile",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            // Make UserId nullable
            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                schema: "public",
                table: "GuestProfile",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            // Re-create index on UserId (now nullable)
            migrationBuilder.CreateIndex(
                name: "IX_GuestProfile_UserId",
                schema: "public",
                table: "GuestProfile",
                column: "UserId");

            // Re-add FK as optional (no cascade, set null)
            migrationBuilder.AddForeignKey(
                name: "FK_GuestProfile_User_UserId",
                schema: "public",
                table: "GuestProfile",
                column: "UserId",
                principalSchema: "public",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuestProfile_User_UserId",
                schema: "public",
                table: "GuestProfile");

            migrationBuilder.DropIndex(
                name: "IX_GuestProfile_UserId",
                schema: "public",
                table: "GuestProfile");

            migrationBuilder.DropColumn(
                name: "FullName",
                schema: "public",
                table: "GuestProfile");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "public",
                table: "GuestProfile");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "public",
                table: "GuestProfile");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                schema: "public",
                table: "GuestProfile",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuestProfile_UserId",
                schema: "public",
                table: "GuestProfile",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuestProfile_User_UserId",
                schema: "public",
                table: "GuestProfile",
                column: "UserId",
                principalSchema: "public",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
