using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Gamingv1.ViewModels
{
    /// <summary>
    /// View model for game uploads
    /// </summary>
    public class GameUploadViewModel : GameFormViewModel
    {
        [Display(Name = "Game Images")]
        public List<IFormFile> ImageFiles { get; set; } = new List<IFormFile>();
    }

    /// <summary>
    /// View model for event uploads
    /// </summary>
    public class EventUploadViewModel : EventFormViewModel
    {
        [Display(Name = "Event Image")]
        public IFormFile ImageFile { get; set; }
    }

    /// <summary>
    /// View model for topic uploads
    /// </summary>
    public class TopicUploadViewModel : DynamicTopicViewModel
    {
        [Display(Name = "Topic Image")]
        public IFormFile ImageFile { get; set; }
    }
}
