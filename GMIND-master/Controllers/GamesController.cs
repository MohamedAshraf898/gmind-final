using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Gamingv1.Data;
using Gamingv1.Models;
using Gamingv1.ViewModels;
using Gamingv1.Services;
using System.Security.Cryptography;
using System.Text;

namespace Gamingv1.Controllers
{
    /// <summary>
    /// Handles game store operations, purchases, and app verification
    /// </summary>
    public class GamesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<GamesController> _logger;

        public GamesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<GamesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Display all games with filtering options
        /// </summary>
        public async Task<IActionResult> Index(int? categoryId, string? search)
        {
            var gamesQuery = _context.Games
                .Include(g => g.Category)
                .Include(g => g.GameImages)
                .Include(g => g.GameReviews)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                gamesQuery = gamesQuery.Where(g => g.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                gamesQuery = gamesQuery.Where(g => g.Title.Contains(search) || g.Description.Contains(search));
            }

            var games = await gamesQuery.ToListAsync();
            var categories = await _context.GameCategories.ToListAsync();

            var viewModel = new GameStoreViewModel
            {
                Games = games,
                Categories = categories,
                SelectedCategoryId = categoryId,
                SearchTerm = search
            };

            return View(viewModel);
        }

        /// <summary>
        /// Display game details
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var game = await _context.Games
                .Include(g => g.Category)
                .Include(g => g.GameImages)
                .Include(g => g.GameReviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(g => g.GameId == id);

            if (game == null)
            {
                return NotFound();
            }

            var viewModel = new GameDetailsViewModel
            {
                Game = game,
                UserHasPurchased = false,
                UserReview = null
            };

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    viewModel.UserHasPurchased = await _context.GamePurchases
                        .AnyAsync(p => p.UserId == user.Id && p.GameId == id && p.PaymentStatus == PaymentStatus.Paid);

                    viewModel.UserReview = await _context.GameReviews
                        .FirstOrDefaultAsync(r => r.UserId == user.Id && r.GameId == id);
                }
            }

            return View(viewModel);
        }

        /// <summary>
        /// Purchase a game
        /// </summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(int gameId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
            {
                return NotFound();
            }

            // Check if user already purchased this game
            var existingPurchase = await _context.GamePurchases
                .FirstOrDefaultAsync(p => p.UserId == user.Id && p.GameId == gameId && p.PaymentStatus == PaymentStatus.Paid);

            if (existingPurchase != null)
            {
                TempData["Error"] = "You have already purchased this game.";
                return RedirectToAction("Details", new { id = gameId });
            }

            // Create purchase record
            var purchase = new GamePurchase
            {
                UserId = user.Id,
                GameId = gameId,
                PurchaseDate = DateTime.UtcNow,
                FinalPrice = game.Price,
                PaymentStatus = PaymentStatus.Paid // In real app, this would be handled by payment gateway
            };

            _context.GamePurchases.Add(purchase);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {user.Email} purchased game {game.Title}");
            TempData["Success"] = $"Successfully purchased {game.Title}! You can now download it from the Play Store.";

            return RedirectToAction("Details", new { id = gameId });
        }

        /// <summary>
        /// API endpoint for app verification
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> VerifyApp(int gameId, string? redirectUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not authenticated");
            }

            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
            {
                return NotFound("Game not found");
            }

            // Check if user owns this game
            var purchase = await _context.GamePurchases
                .FirstOrDefaultAsync(p => p.UserId == user.Id && p.GameId == gameId && p.PaymentStatus == PaymentStatus.Paid);

            if (purchase == null)
            {
                return BadRequest("User does not own this game");
            }

            // Generate or retrieve existing valid token
            var existingToken = await _context.GameTokens
                .FirstOrDefaultAsync(t => t.UserId == user.Id && t.GameId == gameId && t.ExpiryDate > DateTime.UtcNow);

            GameToken token;

            if (existingToken != null)
            {
                token = existingToken;
            }
            else
            {
                // Create new token
                var tokenValue = GenerateSecureToken(user.Id, gameId);
                token = new GameToken
                {
                    UserId = user.Id,
                    GameId = gameId,
                    TokenValue = tokenValue,
                    ExpiryDate = DateTime.UtcNow.AddDays(30), // Token valid for 30 days
                    CreatedAt = DateTime.UtcNow
                };

                _context.GameTokens.Add(token);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"Generated app verification token for user {user.Email} and game {game.Title}");

            // If redirectUrl is provided, redirect back to app with token
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                var separator = redirectUrl.Contains('?') ? "&" : "?";
                return Redirect($"{redirectUrl}{separator}token={token.TokenValue}");
            }

            // Otherwise return JSON response
            return Json(new { success = true, token = token.TokenValue, expiryDate = token.ExpiryDate });
        }

        /// <summary>
        /// Add or update a game review
        /// </summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int gameId, int rating, string? comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // Check if user owns this game
            var purchase = await _context.GamePurchases
                .FirstOrDefaultAsync(p => p.UserId == user.Id && p.GameId == gameId && p.PaymentStatus == PaymentStatus.Paid);

            if (purchase == null)
            {
                TempData["Error"] = "You must purchase the game before reviewing it.";
                return RedirectToAction("Details", new { id = gameId });
            }

            // Check for existing review
            var existingReview = await _context.GameReviews
                .FirstOrDefaultAsync(r => r.UserId == user.Id && r.GameId == gameId);

            if (existingReview != null)
            {
                // Update existing review
                existingReview.Rating = rating;
                existingReview.Comment = comment;
                existingReview.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new review
                var review = new GameReview
                {
                    UserId = user.Id,
                    GameId = gameId,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.UtcNow
                };

                _context.GameReviews.Add(review);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Review submitted successfully!";

            return RedirectToAction("Details", new { id = gameId });
        }

        /// <summary>
        /// Generate a secure token for app verification
        /// </summary>
        private string GenerateSecureToken(string userId, int gameId)
        {
            var data = $"{userId}:{gameId}:{DateTime.UtcNow.Ticks}";
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }
    }
}
