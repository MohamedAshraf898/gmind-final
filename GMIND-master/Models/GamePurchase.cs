using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gamingv1.Models
{
    /// <summary>
    /// Game purchases made by users
    /// </summary>
    public class GamePurchase
    {
        [Key]
        public int PurchaseId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int GameId { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        public int? CouponId { get; set; } // Nullable for discount coupons

        [Column(TypeName = "decimal(18,2)")]
        public decimal FinalPrice { get; set; }

        [Required]
        public PaymentStatus PaymentStatus { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("GameId")]
        public virtual Game Game { get; set; } = null!;
    }

    /// <summary>
    /// Tokens for app verification
    /// </summary>
    public class GameToken
    {
        [Key]
        public int TokenId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int GameId { get; set; }

        [Required]
        public string TokenValue { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("GameId")]
        public virtual Game Game { get; set; } = null!;
    }

    /// <summary>
    /// Payment status enumeration
    /// </summary>
    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Refunded
    }
}
