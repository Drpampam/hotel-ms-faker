using hotelier_core_app.Model.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace hotelier_core_app.Migrations
{
    /// <summary>
    /// Entity Framework Core database context for hotel-ms, supporting multi-tenancy.
    /// </summary>
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, long, ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, IdentityRoleClaim<long>, ApplicationUserToken>
    {
        private ITenantProvider _tenantProvider;

        public DbSet<Address> Addresses { get; set; }
        public DbSet<ApplicationUserPolicyGroup> UserPolicyGroups { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<LoyaltyProgram> LoyaltyPrograms { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<ModuleGroup> ModuleGroups { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Permission> Permission { get; set; }
        public DbSet<PolicyGroup> PolicyGroups { get; set; }
        public DbSet<PolicyModulePermission> PolicyModulePermissions { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public string CurrentSchema => _tenantProvider?.GetSchema() ?? "public";

        /// <summary>
        /// Initializes a new instance of the <see cref="AppDbContext"/> class.
        /// </summary>
        /// <param name="options">The database context options.</param>
        /// <param name="tenantProvider">The tenant provider for multi-tenancy.</param>
        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
            : base(options)
        {
            _tenantProvider = tenantProvider;
        }

        /// <summary>
        /// Configures the model and sets the schema for multi-tenancy.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var schema = CurrentSchema;
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Set schema for all entities
                entityType.SetSchema(schema);
            }

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.RowVersion)
                .IsRequired()
                .IsRowVersion()
                .HasColumnType("bytea")
                .HasDefaultValueSql("gen_random_bytes(8)");

            modelBuilder.Entity<ApplicationUserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });
        }
    }
}
