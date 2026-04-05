using System.ComponentModel.DataAnnotations;

namespace Gamingv1.ViewModels
{
    /// <summary>
    /// View model for game category creation/editing
    /// </summary>
    public class GameCategoryViewModel
    {
        public int CategoryId { get; set; }
        
        [Required]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}
