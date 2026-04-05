using System.ComponentModel.DataAnnotations;
using Gamingv1.Models;

namespace Gamingv1.ViewModels
{
    /// <summary>
    /// View model for class index page
    /// </summary>
    public class ClassIndexViewModel
    {
        public IEnumerable<Class> TeacherClasses { get; set; } = new List<Class>();
        public IEnumerable<Class> StudentClasses { get; set; } = new List<Class>();
        public Dictionary<Class, List<ApplicationUser>> ChildrenClasses { get; set; } = new Dictionary<Class, List<ApplicationUser>>();
    }

    /// <summary>
    /// View model for creating a new class
    /// </summary>
    public class CreateClassViewModel
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Class Name")]
        public string ClassName { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// View model for joining a class
    /// </summary>
    public class JoinClassViewModel
    {
        [Required]
        [StringLength(20)]
        [Display(Name = "Join Code")]
        public string JoinCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// View model for managing parent-student relationships
    /// </summary>
    public class AddChildViewModel
    {
        [Required]
        [StringLength(20)]
        [Display(Name = "Student Code")]
        public string StudentCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// View model for parent dashboard
    /// </summary>
    public class ParentDashboardViewModel
    {
        public IEnumerable<ApplicationUser> Children { get; set; } = new List<ApplicationUser>();
        public Dictionary<ApplicationUser, List<Class>> ChildrenClasses { get; set; } = new Dictionary<ApplicationUser, List<Class>>();
        public Dictionary<ApplicationUser, List<GamePurchase>> ChildrenPurchases { get; set; } = new Dictionary<ApplicationUser, List<GamePurchase>>();
    }
}
