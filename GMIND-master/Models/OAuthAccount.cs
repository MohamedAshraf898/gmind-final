using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gamingv1.Models
{
    /// <summary>
    /// OAuth accounts linked to users (Google, Facebook, etc.)
    /// </summary>
    public class OAuthAccount
    {
        [Key]
        public int OAuthId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Provider { get; set; } = string.Empty; // Google, Facebook

        [Required]
        public string ProviderUserId { get; set; } = string.Empty;

        public string? Token { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountType { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
