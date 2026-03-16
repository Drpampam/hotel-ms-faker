using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hotelier_core_app.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedSubscriptionPlanTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "SubscriptionPlan");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "SubscriptionPlan");

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionEndDate",
                table: "Tenant",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionStartDate",
                table: "Tenant",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriptionEndDate",
                table: "Tenant");

            migrationBuilder.DropColumn(
                name: "SubscriptionStartDate",
                table: "Tenant");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "SubscriptionPlan",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "SubscriptionPlan",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
