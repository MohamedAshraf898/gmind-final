using Gamingv1.Models;

namespace Gamingv1.ViewModels
{
    /// <summary>
    /// View model for the game store page
    /// </summary>
    public class GameStoreViewModel
    {
        public IEnumerable<Game> Games { get; set; } = new List<Game>();
        public IEnumerable<GameCategory> Categories { get; set; } = new List<GameCategory>();
        public int? SelectedCategoryId { get; set; }
        public string? SearchTerm { get; set; }
    }

    /// <summary>
    /// View model for game details page
    /// </summary>
    public class GameDetailsViewModel
    {
        public Game Game { get; set; } = null!;
        public bool UserHasPurchased { get; set; }
        public GameReview? UserReview { get; set; }
    }

    /// <summary>
    /// View model for creating/editing games (Admin)
    /// </summary>
    public class GameFormViewModel
    {
        public int GameId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string DownloadLink { get; set; } = string.Empty;
        public IEnumerable<GameCategory> Categories { get; set; } = new List<GameCategory>();
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
