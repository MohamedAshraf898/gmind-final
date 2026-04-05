using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Gamingv1.Models
{
    /// <summary>
    /// Represents a user in the educational game store system
    /// Extends IdentityUser to include custom fields
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Phone]
        public string? Phone { get; set; }

        [StringLength(20)]
        public string? StudentCode { get; set; }

        [StringLength(255)]
        public string? ProfileImage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<OAuthAccount> OAuthAccounts { get; set; } = new List<OAuthAccount>();
        public virtual ICollection<GamePurchase> GamePurchases { get; set; } = new List<GamePurchase>();
        public virtual ICollection<GameToken> GameTokens { get; set; } = new List<GameToken>();
        public virtual ICollection<GameReview> GameReviews { get; set; } = new List<GameReview>();
        public virtual ICollection<Class> TaughtClasses { get; set; } = new List<Class>();
        public virtual ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();
        public virtual ICollection<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
        public virtual ICollection<ParentStudent> StudentParents { get; set; } = new List<ParentStudent>();
        public virtual ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();

        /// <summary>
        /// Helper method to get the user's role as a string
        /// </summary>
        /// <param name="userManager">The UserManager instance</param>
        /// <returns>The first role the user belongs to, or empty string if none</returns>
        public async Task<string> GetRoleAsync(UserManager<ApplicationUser> userManager)
        {
            var roles = await userManager.GetRolesAsync(this);
            return roles.FirstOrDefault() ?? string.Empty;
        }
    }
}
