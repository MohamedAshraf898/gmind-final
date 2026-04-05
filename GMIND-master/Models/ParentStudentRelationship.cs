using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gamingv1.Models
{
    /// <summary>
    /// Represents the relationship between parents and students
    /// </summary>
    public class ParentStudentRelationship
    {
        [Key]
        public int RelationshipId { get; set; }

        [Required]
        public string ParentId { get; set; } = string.Empty;

        [Required]
        public string StudentId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ParentId")]
        public virtual ApplicationUser Parent { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual ApplicationUser Student { get; set; } = null!;
    }
}
