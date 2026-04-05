using System.ComponentModel.DataAnnotations;
using Gamingv1.Models;

namespace Gamingv1.ViewModels
{
    /// <summary>
    /// Admin dashboard view model
    /// </summary>
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalGames { get; set; }
        public int TotalPurchases { get; set; }
        public decimal TotalRevenue { get; set; }
        public IEnumerable<GamePurchase> RecentPurchases { get; set; } = new List<GamePurchase>();
    }

    /// <summary>
    /// Dynamic section view model
    /// </summary>
    public class DynamicSectionViewModel
    {
        public int SectionId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Section Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Dynamic topic view model
    /// </summary>
    public class DynamicTopicViewModel
    {
        public int TopicId { get; set; }

        public int SectionId { get; set; }

        [Display(Name = "Section")]
        public string SectionName { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(5000)]
        [Display(Name = "Content")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }
    }

    /// <summary>
    /// Event form view model
    /// </summary>
    public class EventFormViewModel
    {
        public int EventId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Event Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; } = DateTime.Now.AddHours(2);

        [Required]
        [Display(Name = "Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
        public decimal Price { get; set; }
    }

    /// <summary>
    /// User with role view model
    /// </summary>
    public class UserWithRoleViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public List<string> Roles { get; set; } = new List<string>();
        public string PrimaryRole { get; set; } = string.Empty;
    }

    /// <summary>
    /// Admin analytics view model
    /// </summary>
    public class AdminAnalyticsViewModel
    {
        // User Statistics
        public int TotalUsers { get; set; }
        public int AdminCount { get; set; }
        public int TeacherCount { get; set; }
        public int ParentCount { get; set; }
        public int StudentCount { get; set; }

        // Content Statistics
        public int TotalGames { get; set; }
        public int TotalEvents { get; set; }
        public int TotalClasses { get; set; }

        // Financial Statistics
        public int TotalGamePurchases { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalEventRegistrations { get; set; }

        // Recent Activity
        public IEnumerable<ApplicationUser> RecentUsers { get; set; } = new List<ApplicationUser>();
        public IEnumerable<GamePurchase> RecentPurchases { get; set; } = new List<GamePurchase>();
        public IEnumerable<EventRegistration> RecentEventRegistrations { get; set; } = new List<EventRegistration>();
    }
}
