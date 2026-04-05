using System.ComponentModel.DataAnnotations;
using Gamingv1.Models;

namespace Gamingv1.ViewModels
{
    /// <summary>
    /// User details view model
    /// </summary>
    public class UserDetailsViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public List<string> Roles { get; set; } = new List<string>();
        public List<string> AllRoles { get; set; } = new List<string>();

        // Activity data
        public List<GamePurchase> GamePurchases { get; set; } = new List<GamePurchase>();
        public List<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();
        public List<Class> Classes { get; set; } = new List<Class>();

        // Relationships (for parents/students)
        public List<ApplicationUser> Children { get; set; } = new List<ApplicationUser>();
        public List<ApplicationUser> Parents { get; set; } = new List<ApplicationUser>();
    }

    /// <summary>
    /// User edit view model
    /// </summary>
    public class UserEditViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    }

    /// <summary>
    /// Link parent and student view model
    /// </summary>
    public class LinkParentStudentViewModel
    {
        [Required]
        [Display(Name = "Parent")]
        public string ParentId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Student")]
        public string StudentId { get; set; } = string.Empty;

        // Lists for dropdowns
        public List<ApplicationUser> Parents { get; set; } = new List<ApplicationUser>();
        public List<ApplicationUser> Students { get; set; } = new List<ApplicationUser>();
        
        // List of existing relationships
        public List<ParentStudentRelationship> ExistingRelationships { get; set; } = new List<ParentStudentRelationship>();
    }

    /// <summary>
    /// User profile image view model
    /// </summary>
    public class UserProfileImageViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Profile Image")]
        [Required(ErrorMessage = "Please select an image to upload")]
        public IFormFile? ProfileImage { get; set; }

        public string? CurrentImageUrl { get; set; }
    }
}
