using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Gamingv1.Models;
using Gamingv1.Services;

namespace Gamingv1.Data
{
    /// <summary>
    /// Handles database initialization and seeding
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Initialize database with required roles and admin user
        /// </summary>
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Apply migrations
                logger.LogInformation("Applying migrations...");
                await context.Database.MigrateAsync();

                // Create roles
                logger.LogInformation("Creating roles...");
                await roleService.InitializeRoles();

                // Create admin user
                logger.LogInformation("Creating admin user...");
                await SeedAdminUser(userManager, roleService);

                // Seed basic data
                logger.LogInformation("Seeding initial data...");
                await SeedInitialData(context);

                logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database initialization");
                throw;
            }
        }

        /// <summary>
        /// Seeds the administrator user
        /// </summary>
        private static async Task SeedAdminUser(UserManager<ApplicationUser> userManager, IRoleService roleService)
        {
            var adminEmail = "admin@gamingstore.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Name = "System Administrator",
                    StudentCode = "ADMIN001",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");

                if (result.Succeeded)
                {
                    await roleService.AssignRoleAsync(adminUser.Id, "Admin");
                }
            }
            else 
            {
                // Ensure admin is in the Admin role
                var isAdmin = await roleService.IsUserInRoleAsync(adminUser.Id, "Admin");
                if (!isAdmin)
                {
                    await roleService.AssignRoleAsync(adminUser.Id, "Admin");
                }
            }
        }

        /// <summary>
        /// Seeds initial data for testing
        /// </summary>
        private static async Task SeedInitialData(ApplicationDbContext context)
        {
            // Seed game categories if none exist
            if (!await context.GameCategories.AnyAsync())
            {
                var categories = new List<GameCategory>
                {
                    new GameCategory { Name = "Math", Description = "Educational math games", CreatedAt = new DateTime(2024, 7, 2) },
                    new GameCategory { Name = "Science", Description = "Science learning games", CreatedAt = new DateTime(2024, 7, 2) },
                    new GameCategory { Name = "Language", Description = "Language learning games", CreatedAt = new DateTime(2024, 7, 2) },
                    new GameCategory { Name = "History", Description = "Historical educational games", CreatedAt = new DateTime(2024, 7, 2) },
                    new GameCategory { Name = "Geography", Description = "Geography learning games", CreatedAt = new DateTime(2024, 7, 2) }
                };
                context.GameCategories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // Seed dynamic sections if none exist
            if (!await context.DynamicSections.AnyAsync())
            {
                var sections = new List<DynamicSection>
                {
                    new DynamicSection { Name = "Latest Trends", Description = "Latest educational trends", CreatedAt = new DateTime(2024, 7, 2) },
                    new DynamicSection { Name = "Featured Games", Description = "Featured educational games", CreatedAt = new DateTime(2024, 7, 2) },
                    new DynamicSection { Name = "News", Description = "Educational news and updates", CreatedAt = new DateTime(2024, 7, 2) }
                };
                context.DynamicSections.AddRange(sections);
                await context.SaveChangesAsync();
            }
        }
    }
}