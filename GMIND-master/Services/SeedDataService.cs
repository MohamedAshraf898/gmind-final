// using Microsoft.AspNetCore.Identity;
// using Microsoft.EntityFrameworkCore;
// using Gamingv1.Models;
// using Gamingv1.Data;

// namespace Gamingv1.Services
// {
//     /// <summary>
//     /// Service to seed initial data including roles for the Educational Game Store
//     /// </summary>
//     public static class SeedDataService
//     {
//         /// <summary>
//         /// Seeds comprehensive initial data including roles and users
//         /// </summary>
//         public static async Task SeedAsync(IServiceProvider serviceProvider)
//         {
//             using var scope = serviceProvider.CreateScope();
//             var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
//             var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//             var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//             var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

//             try
//             {
//                 // 1. FIRST: Create Identity Roles (This is critical!)
//                 await SeedRolesAsync(roleManager, logger);

//                 // 2. Create Users with proper role assignments
//                 await SeedUsersAsync(userManager, logger);

//                 // 3. Seed other data...
//                 await SeedGameCategoriesAsync(context);
//                 await SeedGamesAsync(context, userManager);
//                 await SeedClassesAsync(context, userManager);
//                 await SeedParentStudentRelationshipsAsync(context, userManager);
//                 await SeedEventsAsync(context);
//                 await SeedDynamicContentAsync(context);
//                 await SeedGamePurchasesAsync(context, userManager);
//                 await SeedEventRegistrationsAsync(context, userManager);

//                 await context.SaveChangesAsync();
//                 logger.LogInformation("Database seeding completed successfully");
//             }
//             catch (Exception ex)
//             {
//                 logger.LogError(ex, "An error occurred while seeding the database");
//                 throw;
//             }
//         }

//         /// <summary>
//         /// Seeds Identity roles for the application
//         /// </summary>
//         private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
//         {
//             logger.LogInformation("Starting role seeding...");

//             foreach (var roleName in ApplicationRoles.GetAllRoles())
//             {
//                 if (!await roleManager.RoleExistsAsync(roleName))
//                 {
//                     var role = new IdentityRole(roleName)
//                     {
//                         NormalizedName = roleName.ToUpper()
//                     };

//                     var result = await roleManager.CreateAsync(role);
                    
//                     if (result.Succeeded)
//                     {
//                         logger.LogInformation("Created role: {RoleName}", roleName);
//                     }
//                     else
//                     {
//                         logger.LogError("Failed to create role {RoleName}: {Errors}", 
//                             roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
//                     }
//                 }
//                 else
//                 {
//                     logger.LogInformation("Role already exists: {RoleName}", roleName);
//                 }
//             }

//             logger.LogInformation("Role seeding completed");
//         }

//         /// <summary>
//         /// Seeds initial users for all roles with proper role assignments
//         /// </summary>
//         private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
//         {
//             logger.LogInformation("Starting user seeding...");

//             // Admin User
//             await CreateUserWithRoleAsync(userManager, new ApplicationUser
//             {
//                 UserName = "admin@gamingstore.com",
//                 Email = "admin@gamingstore.com",
//                 Name = "System Administrator",
//                 Role = ApplicationRoles.Admin,
//                 StudentCode = "ADMIN001",
//                 Phone = "+1234567890",
//                 CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
//                 EmailConfirmed = true
//             }, "Admin123!", ApplicationRoles.Admin, logger);

//             // Teachers
//             var teachers = new[]
//             {
//                 new { Email = "teacher1@school.com", Name = "Sarah Johnson", Code = "TEACH001" },
//                 new { Email = "teacher2@school.com", Name = "Michael Chen", Code = "TEACH002" },
//                 new { Email = "teacher3@school.com", Name = "Emily Rodriguez", Code = "TEACH003" }
//             };

//             foreach (var teacher in teachers)
//             {
//                 await CreateUserWithRoleAsync(userManager, new ApplicationUser
//                 {
//                     UserName = teacher.Email,
//                     Email = teacher.Email,
//                     Name = teacher.Name,
//                     Role = ApplicationRoles.Teacher,
//                     StudentCode = teacher.Code,
//                     Phone = "+1234567891",
//                     CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
//                     EmailConfirmed = true
//                 }, "Teacher123!", ApplicationRoles.Teacher, logger);
//             }

//             // Parents
//             var parents = new[]
//             {
//                 new { Email = "parent1@email.com", Name = "David Wilson", Code = "PARENT01" },
//                 new { Email = "parent2@email.com", Name = "Lisa Anderson", Code = "PARENT02" },
//                 new { Email = "parent3@email.com", Name = "James Brown", Code = "PARENT03" }
//             };

//             foreach (var parent in parents)
//             {
//                 await CreateUserWithRoleAsync(userManager, new ApplicationUser
//                 {
//                     UserName = parent.Email,
//                     Email = parent.Email,
//                     Name = parent.Name,
//                     Role = ApplicationRoles.Parent,
//                     StudentCode = parent.Code,
//                     Phone = "+1234567892",
//                     CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
//                     EmailConfirmed = true
//                 }, "Parent123!", ApplicationRoles.Parent, logger);
//             }

//             // Students
//             var students = new[]
//             {
//                 new { Email = "student1@email.com", Name = "Alex Thompson", Code = "STU001" },
//                 new { Email = "student2@email.com", Name = "Emma Davis", Code = "STU002" },
//                 new { Email = "student3@email.com", Name = "Noah Garcia", Code = "STU003" },
//                 new { Email = "student4@email.com", Name = "Olivia Martinez", Code = "STU004" },
//                 new { Email = "student5@email.com", Name = "Liam Johnson", Code = "STU005" },
//                 new { Email = "student6@email.com", Name = "Ava Williams", Code = "STU006" }
//             };

//             foreach (var student in students)
//             {
//                 await CreateUserWithRoleAsync(userManager, new ApplicationUser
//                 {
//                     UserName = student.Email,
//                     Email = student.Email,
//                     Name = student.Name,
//                     Role = ApplicationRoles.Student,
//                     StudentCode = student.Code,
//                     Phone = "+1234567893",
//                     CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
//                     EmailConfirmed = true
//                 }, "Student123!", ApplicationRoles.Student, logger);
//             }

//             logger.LogInformation("User seeding completed");
//         }

//         /// <summary>
//         /// Helper method to create user with proper role assignment
//         /// </summary>
//         private static async Task CreateUserWithRoleAsync(
//             UserManager<ApplicationUser> userManager, 
//             ApplicationUser user, 
//             string password, 
//             string role, 
//             ILogger logger)
//         {
//             var existingUser = await userManager.FindByEmailAsync(user.Email);
//             if (existingUser == null)
//             {
//                 var result = await userManager.CreateAsync(user, password);
//                 if (result.Succeeded)
//                 {
//                     var roleResult = await userManager.AddToRoleAsync(user, role);
//                     if (roleResult.Succeeded)
//                     {
//                         logger.LogInformation("Created user {Email} with role {Role}", user.Email, role);
//                     }
//                     else
//                     {
//                         logger.LogError("Failed to assign role {Role} to user {Email}: {Errors}", 
//                             role, user.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
//                     }
//                 }
//                 else
//                 {
//                     logger.LogError("Failed to create user {Email}: {Errors}", 
//                         user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
//                 }
//             }
//             else
//             {
//                 // Ensure existing user has the correct role
//                 if (!await userManager.IsInRoleAsync(existingUser, role))
//                 {
//                     await userManager.AddToRoleAsync(existingUser, role);
//                     logger.LogInformation("Added role {Role} to existing user {Email}", role, user.Email);
//                 }
//             }
//         }

//         // ... (rest of the existing seed methods remain the same)
//         private static async Task SeedGameCategoriesAsync(ApplicationDbContext context)
//         {
//             // ... existing implementation
//         }

//         // Add other existing seed methods here...
//     }
// }