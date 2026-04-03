using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using hotelier_core_app.Migrations;

#nullable disable

namespace hotelier_core_app.Migrations.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260403000000_ReservationGuestIdToGuestProfile")]
    public partial class ReservationGuestIdToGuestProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old FK that pointed Reservation.GuestId → User.Id
            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_User_GuestId",
                schema: "public",
                table: "Reservation");

            // Re-add FK pointing Reservation.GuestId → GuestProfile.Id
            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_GuestProfile_GuestId",
                schema: "public",
                table: "Reservation",
                column: "GuestId",
                principalSchema: "public",
                principalTable: "GuestProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_GuestProfile_GuestId",
                schema: "public",
                table: "Reservation");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_User_GuestId",
                schema: "public",
                table: "Reservation",
                column: "GuestId",
                principalSchema: "public",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
