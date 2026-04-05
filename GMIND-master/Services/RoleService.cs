using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Gamingv1.Data;
using Gamingv1.Models;

namespace Gamingv1.Services
{
    /// <summary>
    /// Service for managing application roles and user role assignments
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoleService> _logger;

        /// <summary>
        /// Initializes a new instance of the RoleService
        /// </summary>
        public RoleService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<RoleService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Creates default application roles if they don't exist
        /// </summary>
        public async Task InitializeRoles()
        {
            string[] roleNames = { "Admin", "Teacher", "Parent", "Student" };
            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                    _logger.LogInformation("Created role: {Role}", roleName);
                }
            }
        }

        /// <summary>
        /// Assigns a role to a user and syncs with ApplicationUser.Role
        /// </summary>
        public async Task AssignRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found.");
            }

            // Check if role exists
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
                _logger.LogInformation("Created new role: {Role}", role);
            }

            // Remove existing roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                _logger.LogInformation("Removed existing roles from user {UserId}", userId);
            }

            // Assign new role
            var addResult = await _userManager.AddToRoleAsync(user, role);
            if (!addResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to assign role {role} to user {userId}: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
            }

            // Update OAuthAccounts if they exist
            var oauthAccount = await _context.OAuthAccounts
                .FirstOrDefaultAsync(o => o.UserId == userId);
            if (oauthAccount != null)
            {
                oauthAccount.AccountType = role;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated OAuth account type for user {UserId}", userId);
            }

            _logger.LogInformation("Assigned role {Role} to user {UserId}", role, userId);
        }

        /// <summary>
        /// Handles OAuth login and assigns/syncs role
        /// </summary>
        public async Task HandleOAuthLoginAsync(ExternalLoginInfo info, string defaultRole = "Student")
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var provider = info.LoginProvider; // e.g., "Google"
            var providerUserId = info.ProviderKey;
            var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email not found in external login info");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Create new user
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Name = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? email,
                    CreatedAt = DateTime.UtcNow,
                    StudentCode = defaultRole == "Student" ? await GenerateUniqueStudentCodeAsync() : null,
                    EmailConfirmed = true
                };
                
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    throw new Exception("Failed to create user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                
                await _userManager.AddToRoleAsync(user, defaultRole);
                _logger.LogInformation("Created new user {Email} with role {Role} from {Provider}", email, defaultRole, provider);
            }

            // Add OAuth login if not already present
            var existingLogin = await _userManager.FindByLoginAsync(provider, providerUserId);
            if (existingLogin == null)
            {
                await _userManager.AddLoginAsync(user, info);
                _logger.LogInformation("Added {Provider} login for user {Email}", provider, email);
            }

            // Store in OAuthAccounts if not already present
            var existingAccount = await _context.OAuthAccounts
                .FirstOrDefaultAsync(o => o.UserId == user.Id && o.Provider == provider);
                
            if (existingAccount == null)
            {
                var userRole = await GetUserRoleAsync(user.Id);
                
                var oauthAccount = new OAuthAccount
                {
                    UserId = user.Id,
                    Provider = provider,
                    ProviderUserId = providerUserId,
                    Token = info.AuthenticationTokens?.FirstOrDefault()?.Value,
                    AccountType = userRole,
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.OAuthAccounts.Add(oauthAccount);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created OAuth account record for user {Email} with {Provider}", email, provider);
            }
        }

        /// <summary>
        /// Gets a user's role
        /// </summary>
        public async Task<string> GetUserRoleAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found.");
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault() ?? string.Empty;
        }

        /// <summary>
        /// Validates if a user is in a specific role
        /// </summary>
        public async Task<bool> IsUserInRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }
            
            return await _userManager.IsInRoleAsync(user, role);
        }

        /// <summary>
        /// Generates a unique student code
        /// </summary>
        private async Task<string> GenerateUniqueStudentCodeAsync()
        {
            string code;
            do
            {
                code = "STU" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            } while (await _context.Users.AnyAsync(u => u.StudentCode == code));
            
            return code;
        }
    }
}