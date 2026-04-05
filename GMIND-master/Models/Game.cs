using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gamingv1.Models
{
    /// <summary>
    /// Game category for organizing games
    /// </summary>
    public class GameCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Game> Games { get; set; } = new List<Game>();
    }

    /// <summary>
    /// Educational games available for purchase
    /// </summary>
    public class Game
    {
        [Key]
        public int GameId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int CategoryId { get; set; }

        [Required]
        public string DownloadLink { get; set; } = string.Empty; // Google Play Store URL

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual GameCategory Category { get; set; } = null!;
        public virtual ICollection<GameImage> GameImages { get; set; } = new List<GameImage>();
        public virtual ICollection<GameReview> GameReviews { get; set; } = new List<GameReview>();
        public virtual ICollection<GamePurchase> GamePurchases { get; set; } = new List<GamePurchase>();
        public virtual ICollection<GameToken> GameTokens { get; set; } = new List<GameToken>();
    }

    /// <summary>
    /// Images for games
    /// </summary>
    public class GameImage
    {
        [Key]
        public int ImageId { get; set; }

        public int GameId { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("GameId")]
        public virtual Game Game { get; set; } = null!;
    }

    /// <summary>
    /// User reviews for games
    /// </summary>
    public class GameReview
    {
        [Key]
        public int ReviewId { get; set; }

        public int GameId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("GameId")]
        public virtual Game Game { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
