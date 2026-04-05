using System.ComponentModel.DataAnnotations;

namespace Gamingv1.Models
{
    /// <summary>
    /// Dynamic sections for homepage content management
    /// </summary>
    public class DynamicSection
    {
        [Key]
        public int SectionId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<DynamicTopic> DynamicTopics { get; set; } = new List<DynamicTopic>();
    }

    /// <summary>
    /// Dynamic topics within sections
    /// </summary>
    public class DynamicTopic
    {
        [Key]
        public int TopicId { get; set; }

        public int SectionId { get; set; }

        [Required]
        [StringLength(300)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(5000)]
        public string Content { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual DynamicSection Section { get; set; } = null!;
    }
}
