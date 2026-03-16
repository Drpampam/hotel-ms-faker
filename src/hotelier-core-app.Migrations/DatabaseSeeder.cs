using hotelier_core_app.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace hotelier_core_app.Migrations
{
    /// <summary>
    /// Provides methods for seeding the database with initial data and migrations.
    /// </summary>
    public class DatabaseSeeder
    {
        /// <summary>
        /// Seeds the database with initial data and applies pending migrations.
        /// </summary>
        /// <param name="provider">The service provider for dependency resolution.</param>
        public async static Task Seeder(IServiceProvider provider)
        {
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (context.Database.GetPendingMigrations().Any())
            {
                await context.Database.MigrateAsync();
            }

            List<Permission> permissions = new List<Permission>()
            {
                new Permission
                {
                    Name = "View",
                    Description = "Can view records but cannot make changes",
                    CreatedBy = "System Generated",
                    CreationDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                },
                new Permission
                {
                    Name = "Create",
                    Description = "Can add new records in the system",
                    CreatedBy = "System Generated",
                    CreationDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                },
                new Permission
                {
                    Name = "Edit",
                    Description = "Can modify existing records",
                    CreatedBy = "System Generated",
                    CreationDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                },
                new Permission
                {
                    Name = "Delete",
                    Description = "Can soft remove records from the system",
                    CreatedBy = "System Generated",
                    CreationDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                },
                new Permission
                {
                    Name = "Approve",
                    Description = "Can approve reservations, payments, or other user requests",
                    CreatedBy = "System Generated",
                    CreationDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                },
                new Permission
                {
                    Name = "Export",
                    Description = "Can export data or reports from the system",
                    CreatedBy = "System Generated",
                    CreationDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                },
                new Permission
                {
                    Name = "Manage",
                    Description = "Full control over a module, including all permissions",
                    CreatedBy = "System Generated",
                    CreationDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                }
            };

            List<SubscriptionPlan> subscriptions = new List<SubscriptionPlan>
            {
                new SubscriptionPlan
                {
                    Name = "Free",
                    Description = "Free plan",
                    Price = 0,
                    CreatedBy = "System",
                    CreationDate = DateTime.UtcNow
                },
                new SubscriptionPlan
                {
                    Name = "Standard",
                    Description = "Standard plan",
                    Price = 1000,
                    CreatedBy = "System",
                    CreationDate = DateTime.UtcNow
                },
                new SubscriptionPlan
                {
                    Name = "Premium",
                    Description = "Premium plan",
                    Price = 5000,
                    CreatedBy = "System",
                    CreationDate = DateTime.UtcNow
                }
            };

            if (!context.Permission.Any())
            {
                await context.Permission.AddRangeAsync(permissions);
            }

            if (!context.SubscriptionPlans.Any())
            {
                await context.AddRangeAsync(subscriptions);
            }

            await context.SaveChangesAsync();

            return;
        }
    }
}
