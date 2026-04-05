using Microsoft.AspNetCore.Identity;
using Gamingv1.Models;

namespace Gamingv1.Services
{
    /// <summary>
    /// Interface for role management operations
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// Creates default application roles if they don't exist
        /// </summary>
        Task InitializeRoles();

        /// <summary>
        /// Assigns a role to a user and syncs with ApplicationUser.Role
        /// </summary>
        /// <param name="userId">User ID to assign role to</param>
        /// <param name="role">Role to assign</param>
        Task AssignRoleAsync(string userId, string role);

        /// <summary>
        /// Handles OAuth login and assigns/syncs role
        /// </summary>
        /// <param name="info">External login information</param>
        /// <param name="defaultRole">Default role to assign if new user</param>
        Task HandleOAuthLoginAsync(ExternalLoginInfo info, string defaultRole = "Student");

        /// <summary>
        /// Gets a user's role
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>The user's role</returns>
        Task<string> GetUserRoleAsync(string userId);

        /// <summary>
        /// Validates if a user is in a specific role
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="role">Role to check</param>
        /// <returns>True if the user is in the role</returns>
        Task<bool> IsUserInRoleAsync(string userId, string role);
    }
}