using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace hotelier_core_app.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddUserShiftAndDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuestProfile_User_UserId",
                schema: "public",
                table: "GuestProfile");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                schema: "public",
                table: "User",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Shift",
                schema: "public",
                table: "User",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReservationExpense",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReservationId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationExpense", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservationExpense_Reservation_ReservationId",
                        column: x => x.ReservationId,
                        principalSchema: "public",
                        principalTable: "Reservation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationExpense_ReservationId",
                schema: "public",
                table: "ReservationExpense",
                column: "ReservationId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuestProfile_User_UserId",
                schema: "public",
                table: "GuestProfile",
                column: "UserId",
                principalSchema: "public",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuestProfile_User_UserId",
                schema: "public",
                table: "GuestProfile");

            migrationBuilder.DropTable(
                name: "ReservationExpense",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "Department",
                schema: "public",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Shift",
                schema: "public",
                table: "User");

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
    }
}
