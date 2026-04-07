using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace hotelier_core_app.Migrations.Migrations
{
    [Microsoft.EntityFrameworkCore.DbContext(typeof(AppDbContext))]
    [Migration("20260407000000_AddReservationExpense")]
    public partial class AddReservationExpense : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ReservationExpense", schema: "public");
        }
    }
}
