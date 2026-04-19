using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace hotelier_core_app.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddActivationCodeAndTenantPlanType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlanType",
                schema: "public",
                table: "Tenant",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActivationCode",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CodeHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PlanType = table.Column<int>(type: "integer", nullable: false),
                    BoundToEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    UsedByTenantId = table.Column<long>(type: "bigint", nullable: true),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GeneratedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivationCode", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivationCode",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "PlanType",
                schema: "public",
                table: "Tenant");
        }
    }
}
