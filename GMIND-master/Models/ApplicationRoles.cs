using System.ComponentModel.DataAnnotations;

namespace Gamingv1.Models
{
    /// <summary>
    /// Constants for application roles used throughout the system
    /// </summary>
    public static class ApplicationRoles
    {
        public const string Admin = "Admin";
        public const string Teacher = "Teacher";
        public const string Parent = "Parent";
        public const string Student = "Student";

        /// <summary>
        /// Gets all available roles
        /// </summary>
        public static string[] GetAllRoles()
        {
            return new[] { Admin, Teacher, Parent, Student };
        }

        /// <summary>
        /// Checks if a role is valid
        /// </summary>
        public static bool IsValidRole(string role)
        {
            return GetAllRoles().Contains(role);
        }

        /// <summary>
        /// Gets all roles except Admin (for public registration)
        /// </summary>
        public static string[] GetPublicRoles()
        {
            return new[] { Teacher, Parent, Student };
        }

        /// <summary>
        /// Gets role display name
        /// </summary>
        public static string GetDisplayName(string role)
        {
            return role switch
            {
                Admin => "Administrator",
                Teacher => "Teacher",
                Parent => "Parent",
                Student => "Student",
                _ => role
            };
        }
    }
}
