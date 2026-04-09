using hotelier_core_app.Model.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace hotelier_core_app.Migrations
{
    public class DatabaseSeeder
    {
        public async static Task Seeder(IServiceProvider provider)
        {
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            var pending = context.Database.GetPendingMigrations().ToList();
            if (pending.Any())
            {
                try
                {
                    await context.Database.MigrateAsync();
                }
                catch (Exception ex) when (ex.GetBaseException().Message.Contains("already exists"))
                {
                    // The physical schema is ahead of __EFMigrationsHistory (e.g. a previous
                    // partially-applied migration or a manual schema change).  Stamp every
                    // still-pending migration as applied so future deploys don't re-run DDL.
                    Console.WriteLine($"[Seeder] MigrateAsync failed ({ex.GetBaseException().Message}). " +
                                      "Stamping pending migrations as applied and continuing…");
                    using var stampScope = provider.CreateScope();
                    var stampCtx = stampScope.ServiceProvider.GetRequiredService<AppDbContext>();
                    foreach (var mig in stampCtx.Database.GetPendingMigrations())
                    {
                        await stampCtx.Database.ExecuteSqlRawAsync(
                            "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") " +
                            "VALUES ({0}, {1}) ON CONFLICT DO NOTHING",
                            mig, "8.0.0");
                    }
                }
            }

            // Idempotent schema guards — run every startup so columns added by migrations that
            // were stamped-but-not-applied (due to 42P07 recovery) are always present.
            await context.Database.ExecuteSqlRawAsync(@"
                ALTER TABLE ""User"" ADD COLUMN IF NOT EXISTS ""Shift""      varchar(50);
                ALTER TABLE ""User"" ADD COLUMN IF NOT EXISTS ""Department"" varchar(100);
            ");

            // ── Permissions ──────────────────────────────────────────────────────
            if (!context.Permission.Any())
            {
                var permissions = new List<Permission>
                {
                    new Permission { Name = "View",   Description = "Can view records but cannot make changes",            CreatedBy = "System", CreationDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                    new Permission { Name = "Create", Description = "Can add new records in the system",                   CreatedBy = "System", CreationDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                    new Permission { Name = "Edit",   Description = "Can modify existing records",                         CreatedBy = "System", CreationDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                    new Permission { Name = "Delete", Description = "Can soft-remove records from the system",             CreatedBy = "System", CreationDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                    new Permission { Name = "Approve",Description = "Can approve reservations, payments, or requests",     CreatedBy = "System", CreationDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                    new Permission { Name = "Export", Description = "Can export data or reports from the system",          CreatedBy = "System", CreationDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                    new Permission { Name = "Manage", Description = "Full control over a module, including all permissions",CreatedBy = "System", CreationDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow }
                };
                await context.Permission.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
            }

            // ── Subscription Plans ───────────────────────────────────────────────
            if (!context.SubscriptionPlans.Any())
            {
                var subscriptions = new List<SubscriptionPlan>
                {
                    new SubscriptionPlan { Name = "Free",     Description = "Free plan with basic features",  Price = 0,    CreatedBy = "System", CreationDate = DateTime.UtcNow },
                    new SubscriptionPlan { Name = "Standard", Description = "Standard plan for small hotels", Price = 1000, CreatedBy = "System", CreationDate = DateTime.UtcNow },
                    new SubscriptionPlan { Name = "Premium",  Description = "Premium plan for hotel chains",  Price = 5000, CreatedBy = "System", CreationDate = DateTime.UtcNow }
                };
                await context.SubscriptionPlans.AddRangeAsync(subscriptions);
                await context.SaveChangesAsync();
            }

            // ── Default Tenant ───────────────────────────────────────────────────
            if (!context.Tenants.Any())
            {
                var freePlan = await context.SubscriptionPlans.FirstOrDefaultAsync(s => s.Name == "Free");
                var tenant = new Tenant
                {
                    Name = "Hotelier Default",
                    Description = "Default system tenant",
                    CreatedBy = "System",
                    CreationDate = DateTime.UtcNow,
                    SubscriptionPlanId = freePlan?.Id,
                    SubscriptionStartDate = DateTime.UtcNow,
                    SubscriptionEndDate = DateTime.UtcNow.AddYears(1)
                };
                await context.Tenants.AddAsync(tenant);
                await context.SaveChangesAsync();
            }

            // ── Roles ────────────────────────────────────────────────────────────
            var roleNames = new[] { "SuperAdmin", "Admin", "FrontDesk", "Housekeeping", "Guest", "Developer" };
            var roleTenant = await context.Tenants.FirstOrDefaultAsync();
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = roleName,
                        TenantId = roleTenant?.Id,
                        CreationDate = DateTime.UtcNow,
                        CreatedBy = "System"
                    });
                }
            }

            // ── Default Admin User ───────────────────────────────────────────────
            var adminEmail = "admin@hotelier.io";
            var defaultTenant = await context.Tenants.FirstOrDefaultAsync();
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    IsActive = true,
                    Status = "Active",
                    TenantId = defaultTenant?.Id,
                    CreatedBy = "System",
                    CreationDate = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(adminUser, "P@ssw0rd!");
                if (result.Succeeded)
                {
                    existingAdmin = adminUser;
                }
            }

            // Ensure admin has SuperAdmin role regardless of when the user was created
            if (existingAdmin != null)
            {
                var superAdminRole = await roleManager.FindByNameAsync("SuperAdmin");
                var hasRole = superAdminRole != null && await context.UserRoles
                    .AnyAsync(ur => ur.UserId == existingAdmin.Id && ur.RoleId == superAdminRole.Id);

                if (superAdminRole != null && !hasRole)
                {
                    if (defaultTenant != null)
                    {
                        await context.UserRoles.AddAsync(new ApplicationUserRole
                        {
                            UserId = existingAdmin.Id,
                            RoleId = superAdminRole.Id,
                            TenantId = defaultTenant!.Id
                        });
                        await context.SaveChangesAsync();
                    }
                }
            }

            // ── Module Groups ────────────────────────────────────────────────────
            if (!context.ModuleGroups.Any())
            {
                var moduleGroups = new List<ModuleGroup>
                {
                    new ModuleGroup { Name = "Operations",     Description = "Front desk, reservations and room management", CreatedBy = "System", CreationDate = DateTime.UtcNow },
                    new ModuleGroup { Name = "Finance",        Description = "Payments, billing and discounts",              CreatedBy = "System", CreationDate = DateTime.UtcNow },
                    new ModuleGroup { Name = "Housekeeping",   Description = "Room cleaning and maintenance tasks",          CreatedBy = "System", CreationDate = DateTime.UtcNow },
                    new ModuleGroup { Name = "Administration", Description = "Users, roles, settings and subscriptions",     CreatedBy = "System", CreationDate = DateTime.UtcNow },
                    new ModuleGroup { Name = "Reporting",      Description = "Analytics, reports and exports",               CreatedBy = "System", CreationDate = DateTime.UtcNow }
                };
                await context.ModuleGroups.AddRangeAsync(moduleGroups);
                await context.SaveChangesAsync();
            }

            // ── Modules ──────────────────────────────────────────────────────────
            if (!context.Modules.Any())
            {
                var ops    = await context.ModuleGroups.FirstOrDefaultAsync(g => g.Name == "Operations");
                var fin    = await context.ModuleGroups.FirstOrDefaultAsync(g => g.Name == "Finance");
                var hk     = await context.ModuleGroups.FirstOrDefaultAsync(g => g.Name == "Housekeeping");
                var admin  = await context.ModuleGroups.FirstOrDefaultAsync(g => g.Name == "Administration");
                var report = await context.ModuleGroups.FirstOrDefaultAsync(g => g.Name == "Reporting");

                var modules = new List<Module>
                {
                    new Module { Name = "Reservations",      Description = "Create and manage guest bookings",         Url = "/api/v1/reservations",    ModuleGroupId = ops?.Id ?? 0,    CreatedBy = "System", CreationDate = DateTime.UtcNow },
                    new Module { Name = "Rooms",             Description = "Room inventory and state management",      Url = "/api/v1/rooms",           ModuleGroupId = ops?.Id ?? 0,    CreatedBy = "System", CreationDate = DateTime.UtcNow },
                    new Module { Name = "Properties",        Description = "Hotel property management",                Url = "/api/v1/properties",      ModuleGroupId = ops?.Id ?? 0,    CreatedBy = "System", CreationDate = DateTime.UtcNow },
                    new Module { Name = "Guests",            Description = "Guest profiles and history",               Url = "/api/v1/guests",          ModuleGroupId = ops?.Id ?? 0,    CreatedBy = "System", CreationDate = DateTime.UtcNow },
                    new Module { Name = "Service Requests",  Description = "In-stay guest service requests",           Url = "/api/v1/service-requests",ModuleGroupId = ops?.Id ?? 0,    CreatedBy = "System", CreationDate = DateTime.UtcNow },
                    new Module { Name = "Payments",          Description = "Payment processing and history",           Url = "/api/v1/payments",        ModuleGroupId = fin?.Id ?? 0,    CreatedBy = "System", CreationDate = DateTime.UtcNow },
                    new Module { Name = "Discounts",         Description = "Discount codes and promotional offers",    Url = "/api/v1/discounts",       ModuleGroupId = fin?.Id ?? 0,    CreatedBy = "System", CreationDate = DateTime.UtcNow },
                    new Module { Name = "Housekeeping",      Description = "Housekeeping task scheduling",             Url = "/api/v1/housekeeping",    ModuleGroupId = hk?.Id ?? 0,     CreatedBy = "System", CreationDate = DateTime.UtcNow },
                    new Module { Name = "Users",             Description = "User account management",                  Url = "/api/v1/user",            ModuleGroupId = admin?.Id ?? 0,  CreatedBy = "System", CreationDate = DateTime.UtcNow },
                    new Module { Name = "Roles",             Description = "Role and access management",               Url = "/api/v1/role",            ModuleGroupId = admin?.Id ?? 0,  CreatedBy = "System", CreationDate = DateTime.UtcNow },
                    new Module { Name = "Subscriptions",     Description = "Subscription plan management",             Url = "/api/v1/subscription",    ModuleGroupId = admin?.Id ?? 0,  CreatedBy = "System", CreationDate = DateTime.UtcNow },
                    new Module { Name = "Policy Groups",     Description = "Permission and policy management",         Url = "/api/v1/policy-groups",   ModuleGroupId = admin?.Id ?? 0,  CreatedBy = "System", CreationDate = DateTime.UtcNow },
                    new Module { Name = "Reports",           Description = "Occupancy, revenue and operational reports",Url = "/api/v1/reports",        ModuleGroupId = report?.Id ?? 0, CreatedBy = "System", CreationDate = DateTime.UtcNow }
                };
                await context.Modules.AddRangeAsync(modules);
                await context.SaveChangesAsync();
            }
        }
    }
}
