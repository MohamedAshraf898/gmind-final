using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gamingv1.Models
{
    /// <summary>
    /// Classes created by teachers
    /// </summary>
    public class Class
    {
        [Key]
        public int ClassId { get; set; }

        [Required]
        [StringLength(200)]
        public string ClassName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string TeacherId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string JoinCode { get; set; } = string.Empty; // Unique code to join class

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("TeacherId")]
        public virtual ApplicationUser Teacher { get; set; } = null!;
        public virtual ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();
    }

    /// <summary>
    /// Students enrolled in classes
    /// </summary>
    public class ClassStudent
    {
        [Key]
        public int Id { get; set; }

        public int ClassId { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual ApplicationUser Student { get; set; } = null!;
    }

    /// <summary>
    /// Parent-student relationships
    /// </summary>
    public class ParentStudent
    {
        [Key]
        public int Id { get; set; }

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
