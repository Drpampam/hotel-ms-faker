using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using hotelier_core_app.Migrations;

#nullable disable

namespace hotelier_core_app.Migrations.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260514000000_EnsureAllTenantSchemaTables")]
    public partial class EnsureAllTenantSchemaTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent repair: for every tenant whose schema is missing tables
            // (because AddGuestProfileAndReservationState's RenameTable calls moved
            // them all to public before that schema had any data), recreate each
            // table from public using LIKE INCLUDING ALL.
            //
            // LIKE INCLUDING ALL copies: columns, identity sequences, defaults,
            // PKs, unique constraints, check constraints, and indexes.
            // FK constraints are intentionally NOT copied by PostgreSQL LIKE.
            // Each operation is in its own BEGIN/EXCEPTION block so a single
            // failure cannot abort the remaining tables/tenants.
            migrationBuilder.Sql(@"
DO $$
DECLARE
    tid  bigint;
    s    text;
    tbl  text;
    tbls text[] := ARRAY[
        'Address', 'AuditLog', 'Discount', 'GuestProfile',
        'HousekeepingTask', 'Invoice', 'InvoiceLineItem',
        'LoyaltyProgram', 'Module', 'ModuleGroup', 'Payment',
        'Permission', 'PolicyGroup', 'PolicyModulePermission',
        'Property', 'Reservation', 'ReservationExpense', 'Role',
        'RoleClaims', 'Room', 'ServiceRequest', 'SubscriptionPlan',
        'Tenant', 'User', 'UserClaim', 'UserLogin',
        'UserPolicyGroup', 'UserRole', 'UserToken'
    ];
BEGIN
    FOR tid IN
        SELECT ""Id"" FROM public.""Tenant"" WHERE ""IsDeleted"" = false
    LOOP
        s := 'tenant_' || tid::text;

        -- Ensure the schema exists (safe no-op if already present)
        BEGIN
            EXECUTE format('CREATE SCHEMA IF NOT EXISTS %I', s);
        EXCEPTION WHEN OTHERS THEN
            RAISE WARNING 'EnsureAllTenantTables: could not create schema %: %', s, SQLERRM;
        END;

        -- Create every table that must live in the tenant schema
        FOREACH tbl IN ARRAY tbls LOOP
            BEGIN
                EXECUTE format(
                    'CREATE TABLE IF NOT EXISTS %I.%I (LIKE public.%I INCLUDING ALL)',
                    s, tbl, tbl
                );
            EXCEPTION WHEN OTHERS THEN
                RAISE WARNING 'EnsureAllTenantTables: %.% – %', s, tbl, SQLERRM;
            END;
        END LOOP;

        -- Column guards: add columns introduced by post-initial migrations
        -- to tenant schemas that were provisioned before those migrations ran.
        BEGIN EXECUTE format('ALTER TABLE %I.""User""           ADD COLUMN IF NOT EXISTS ""Shift""               varchar(50)',        s); EXCEPTION WHEN OTHERS THEN NULL; END;
        BEGIN EXECUTE format('ALTER TABLE %I.""User""           ADD COLUMN IF NOT EXISTS ""Department""           varchar(100)',       s); EXCEPTION WHEN OTHERS THEN NULL; END;
        BEGIN EXECUTE format('ALTER TABLE %I.""User""           ADD COLUMN IF NOT EXISTS ""MustChangePassword""   boolean NOT NULL DEFAULT false', s); EXCEPTION WHEN OTHERS THEN NULL; END;
        BEGIN EXECUTE format('ALTER TABLE %I.""Reservation""    ADD COLUMN IF NOT EXISTS ""ActualCheckInDate""    timestamptz',       s); EXCEPTION WHEN OTHERS THEN NULL; END;
        BEGIN EXECUTE format('ALTER TABLE %I.""Reservation""    ADD COLUMN IF NOT EXISTS ""ActualCheckOutDate""   timestamptz',       s); EXCEPTION WHEN OTHERS THEN NULL; END;
        BEGIN EXECUTE format('ALTER TABLE %I.""Reservation""    ADD COLUMN IF NOT EXISTS ""SpecialRequests""      varchar(500)',       s); EXCEPTION WHEN OTHERS THEN NULL; END;
        BEGIN EXECUTE format('ALTER TABLE %I.""ServiceRequest"" ADD COLUMN IF NOT EXISTS ""Notes""               varchar(500)',       s); EXCEPTION WHEN OTHERS THEN NULL; END;
        BEGIN EXECUTE format('ALTER TABLE %I.""ServiceRequest"" ADD COLUMN IF NOT EXISTS ""ServiceRequestState"" integer NOT NULL DEFAULT 0', s); EXCEPTION WHEN OTHERS THEN NULL; END;
        BEGIN EXECUTE format('ALTER TABLE %I.""Room""           ADD COLUMN IF NOT EXISTS ""RoomState""           integer NOT NULL DEFAULT 0', s); EXCEPTION WHEN OTHERS THEN NULL; END;
        BEGIN EXECUTE format('ALTER TABLE %I.""Payment""        ADD COLUMN IF NOT EXISTS ""PaymentState""        integer NOT NULL DEFAULT 0', s); EXCEPTION WHEN OTHERS THEN NULL; END;
        BEGIN EXECUTE format('ALTER TABLE %I.""GuestProfile""   ADD COLUMN IF NOT EXISTS ""FullName""            varchar(200)',       s); EXCEPTION WHEN OTHERS THEN NULL; END;
        BEGIN EXECUTE format('ALTER TABLE %I.""GuestProfile""   ADD COLUMN IF NOT EXISTS ""Email""              varchar(200)',       s); EXCEPTION WHEN OTHERS THEN NULL; END;
        BEGIN EXECUTE format('ALTER TABLE %I.""GuestProfile""   ADD COLUMN IF NOT EXISTS ""PhoneNumber""         varchar(50)',        s); EXCEPTION WHEN OTHERS THEN NULL; END;

        RAISE NOTICE 'EnsureAllTenantTables: % done', s;
    END LOOP;
END $$;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No rollback — table creation is idempotent and cannot be safely reversed.
        }
    }
}
