using Gamingv1.Models;

namespace Gamingv1.ViewModels
{
    /// <summary>
    /// View model for the home page
    /// </summary>
    public class HomeIndexViewModel
    {
        public IEnumerable<DynamicSection> DynamicSections { get; set; } = new List<DynamicSection>();
        public IEnumerable<Game> FeaturedGames { get; set; } = new List<Game>();
        public IEnumerable<Event> UpcomingEvents { get; set; } = new List<Event>();
        public int TotalGames { get; set; }
        public int TotalUsers { get; set; }
        public int TotalClasses { get; set; }
    }
}
