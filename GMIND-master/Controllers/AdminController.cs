using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Gamingv1.Data;
using Gamingv1.Models;
using Gamingv1.ViewModels;
using Gamingv1.Services;

namespace Gamingv1.Controllers
{
    /// <summary>
    /// Admin controller for managing games, content, and system administration
    /// </summary>
    [Authorize(Policy = "RequireAdmin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;
        private readonly FileService _fileService;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<AdminController> logger,
            FileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _fileService = fileService;
        }

        /// <summary>
        /// Admin dashboard
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalGames = await _context.Games.CountAsync(),
                TotalPurchases = await _context.GamePurchases.Where(p => p.PaymentStatus == PaymentStatus.Paid).CountAsync(),
                TotalRevenue = await _context.GamePurchases.Where(p => p.PaymentStatus == PaymentStatus.Paid).SumAsync(p => p.FinalPrice),
                RecentPurchases = await _context.GamePurchases
                    .Include(p => p.User)
                    .Include(p => p.Game)
                    .OrderByDescending(p => p.PurchaseDate)
                    .Take(10)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        #region Game Management

        /// <summary>
        /// Manage games
        /// </summary>
        public async Task<IActionResult> Games()
        {
            var games = await _context.Games
                .Include(g => g.Category)
                .Include(g => g.GameImages)
                .ToListAsync();

            return View(games);
        }

        /// <summary>
        /// Create new game
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreateGame()
        {
            var viewModel = new GameUploadViewModel
            {
                Categories = await _context.GameCategories.ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGame(GameUploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                var game = new Game
                {
                    Title = model.Title,
                    Description = model.Description,
                    Price = model.Price,
                    CategoryId = model.CategoryId,
                    DownloadLink = model.DownloadLink,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Games.Add(game);
                await _context.SaveChangesAsync();

                // Process uploaded images
                if (model.ImageFiles != null && model.ImageFiles.Any())
                {
                    foreach (var imageFile in model.ImageFiles)
                    {
                        var imageUrl = await _fileService.UploadGameImageAsync(imageFile);
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            var gameImage = new GameImage
                            {
                                GameId = game.GameId,
                                ImageUrl = imageUrl
                            };
                            _context.GameImages.Add(gameImage);
                        }
                    }
                }

                // Add image URLs if provided
                if (model.ImageUrls != null)
                {
                    foreach (var imageUrl in model.ImageUrls.Where(url => !string.IsNullOrEmpty(url)))
                    {
                        var gameImage = new GameImage
                        {
                            GameId = game.GameId,
                            ImageUrl = imageUrl
                        };
                        _context.GameImages.Add(gameImage);
                    }
                }

                await _context.SaveChangesAsync();

                var user = await _userManager.GetUserAsync(User);
                _logger.LogInformation($"Admin {user?.Email} created game {game.Title}");
                TempData["Success"] = "Game created successfully!";

                return RedirectToAction("Games");
            }

            model.Categories = await _context.GameCategories.ToListAsync();
            return View(model);
        }

        /// <summary>
        /// Edit existing game
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditGame(int id)
        {
            var game = await _context.Games
                .Include(g => g.GameImages)
                .FirstOrDefaultAsync(g => g.GameId == id);

            if (game == null)
            {
                return NotFound();
            }

            var viewModel = new GameUploadViewModel
            {
                GameId = game.GameId,
                Title = game.Title,
                Description = game.Description,
                Price = game.Price,
                CategoryId = game.CategoryId,
                DownloadLink = game.DownloadLink,
                Categories = await _context.GameCategories.ToListAsync(),
                ImageUrls = game.GameImages.Select(img => img.ImageUrl).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGame(GameUploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                var game = await _context.Games.Include(g => g.GameImages).FirstOrDefaultAsync(g => g.GameId == model.GameId);
                if (game == null)
                {
                    return NotFound();
                }

                game.Title = model.Title;
                game.Description = model.Description;
                game.Price = model.Price;
                game.CategoryId = model.CategoryId;
                game.DownloadLink = model.DownloadLink;

                // Get existing image URLs to compare
                var existingImages = game.GameImages.Select(img => img.ImageUrl).ToList();
                var imagesToRemove = game.GameImages.Where(img => !model.ImageUrls.Contains(img.ImageUrl)).ToList();

                // Remove images that were deleted
                foreach (var image in imagesToRemove)
                {
                    // Delete the file if it's stored in our uploads folder
                    if (image.ImageUrl.StartsWith("/uploads/"))
                    {
                        _fileService.DeleteFile(image.ImageUrl);
                    }
                    _context.GameImages.Remove(image);
                }

                // Add new images from URLs
                foreach (var imageUrl in model.ImageUrls.Where(url => !string.IsNullOrEmpty(url) && !existingImages.Contains(url)))
                {
                    var gameImage = new GameImage
                    {
                        GameId = game.GameId,
                        ImageUrl = imageUrl
                    };
                    _context.GameImages.Add(gameImage);
                }

                // Process uploaded images
                if (model.ImageFiles != null && model.ImageFiles.Any())
                {
                    foreach (var imageFile in model.ImageFiles)
                    {
                        var imageUrl = await _fileService.UploadGameImageAsync(imageFile);
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            var gameImage = new GameImage
                            {
                                GameId = game.GameId,
                                ImageUrl = imageUrl
                            };
                            _context.GameImages.Add(gameImage);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                var user = await _userManager.GetUserAsync(User);
                _logger.LogInformation($"Admin {user?.Email} updated game {game.Title}");
                TempData["Success"] = "Game updated successfully!";

                return RedirectToAction("Games");
            }

            model.Categories = await _context.GameCategories.ToListAsync();
            return View(model);
        }

        /// <summary>
        /// Delete game
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGame(int id)
        {
            var game = await _context.Games
                .Include(g => g.GameImages)
                .Include(g => g.GameReviews)
                .Include(g => g.GamePurchases)
                .Include(g => g.GameTokens)
                .FirstOrDefaultAsync(g => g.GameId == id);

            if (game == null)
            {
                return NotFound();
            }

            // Remove related entities
            _context.GameImages.RemoveRange(game.GameImages);
            _context.GameReviews.RemoveRange(game.GameReviews);
            _context.GamePurchases.RemoveRange(game.GamePurchases);
            _context.GameTokens.RemoveRange(game.GameTokens);
            
            // Remove the game
            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);
            _logger.LogInformation($"Admin {user?.Email} deleted game {game.Title}");
            TempData["Success"] = "Game deleted successfully!";

            return RedirectToAction("Games");
        }

        /// <summary>
        /// Delete a game image
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DeleteGameImage(int gameId, string imageUrl)
        {
            var game = await _context.Games
                .Include(g => g.GameImages)
                .FirstOrDefaultAsync(g => g.GameId == gameId);

            if (game == null)
            {
                TempData["Error"] = "Game not found.";
                return RedirectToAction("Games");
            }

            var gameImage = game.GameImages.FirstOrDefault(i => i.ImageUrl == imageUrl);
            if (gameImage != null)
            {
                _context.GameImages.Remove(gameImage);
                await _context.SaveChangesAsync();

                // Delete physical file if it's stored in uploads folder
                if (imageUrl.Contains("/uploads/"))
                {
                    try
                    {
                        string fileName = Path.GetFileName(imageUrl);
                        string relativePath = imageUrl.Substring(imageUrl.IndexOf("/uploads/"));
                        await _fileService.DeleteFileAsync(relativePath);
                        
                        TempData["Success"] = "Image deleted successfully.";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deleting image file: {0}", imageUrl);
                        TempData["Warning"] = "Image removed from database but file deletion failed.";
                    }
                }
                else
                {
                    TempData["Success"] = "Image reference removed successfully.";
                }
            }
            else
            {
                TempData["Warning"] = "Image not found.";
            }

            return RedirectToAction("EditGame", new { id = gameId });
        }

        #endregion

        #region Dynamic Content Management

        /// <summary>
        /// Manage dynamic sections
        /// </summary>
        public async Task<IActionResult> DynamicSections()
        {
            var sections = await _context.DynamicSections
                .Include(s => s.DynamicTopics)
                .ToListAsync();

            return View(sections);
        }

        /// <summary>
        /// Create dynamic section
        /// </summary>
        [HttpGet]
        public IActionResult CreateSection()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSection(DynamicSectionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var section = new DynamicSection
                {
                    Name = model.Name,
                    Description = model.Description,
                    CreatedAt = DateTime.UtcNow
                };

                _context.DynamicSections.Add(section);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Section created successfully!";
                return RedirectToAction("DynamicSections");
            }

            return View(model);
        }

        /// <summary>
        /// Manage topics within a section
        /// </summary>
        public async Task<IActionResult> Topics(int sectionId)
        {
            var section = await _context.DynamicSections
                .Include(s => s.DynamicTopics)
                .FirstOrDefaultAsync(s => s.SectionId == sectionId);

            if (section == null)
            {
                return NotFound();
            }

            return View(section);
        }

        /// <summary>
        /// Create dynamic topic
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreateTopic(int sectionId)
        {
            var section = await _context.DynamicSections.FindAsync(sectionId);
            if (section == null)
            {
                return NotFound();
            }

            var viewModel = new DynamicTopicViewModel
            {
                SectionId = sectionId,
                SectionName = section.Name
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTopic(DynamicTopicViewModel model)
        {
            if (ModelState.IsValid)
            {
                var topic = new DynamicTopic
                {
                    SectionId = model.SectionId,
                    Title = model.Title,
                    Content = model.Content,
                    ImageUrl = model.ImageUrl,
                    CreatedAt = DateTime.UtcNow
                };

                _context.DynamicTopics.Add(topic);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Topic created successfully!";
                return RedirectToAction("Topics", new { sectionId = model.SectionId });
            }

            var section = await _context.DynamicSections.FindAsync(model.SectionId);
            model.SectionName = section?.Name ?? "";

            return View(model);
        }

        /// <summary>
        /// Edit dynamic section
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditSection(int id)
        {
            var section = await _context.DynamicSections.FindAsync(id);
            if (section == null)
            {
                return NotFound();
            }

            var model = new DynamicSectionViewModel
            {
                SectionId = section.SectionId,
                Name = section.Name,
                Description = section.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSection(DynamicSectionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var section = await _context.DynamicSections.FindAsync(model.SectionId);
                if (section == null)
                {
                    return NotFound();
                }

                section.Name = model.Name;
                section.Description = model.Description;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Section updated successfully!";

                return RedirectToAction("DynamicSections");
            }

            return View(model);
        }

        /// <summary>
        /// Delete dynamic section
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSection(int id)
        {
            var section = await _context.DynamicSections
                .Include(s => s.DynamicTopics)
                .FirstOrDefaultAsync(s => s.SectionId == id);

            if (section == null)
            {
                return NotFound();
            }

            // Remove all topics first
            _context.DynamicTopics.RemoveRange(section.DynamicTopics);
            _context.DynamicSections.Remove(section);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Section deleted successfully!";
            return RedirectToAction("DynamicSections");
        }

        /// <summary>
        /// Edit dynamic topic
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditTopic(int id)
        {
            var topic = await _context.DynamicTopics
                .Include(t => t.Section)
                .FirstOrDefaultAsync(t => t.TopicId == id);

            if (topic == null)
            {
                return NotFound();
            }

            var model = new DynamicTopicViewModel
            {
                TopicId = topic.TopicId,
                SectionId = topic.SectionId,
                SectionName = topic.Section.Name,
                Title = topic.Title,
                Content = topic.Content,
                ImageUrl = topic.ImageUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTopic(DynamicTopicViewModel model)
        {
            if (ModelState.IsValid)
            {
                var topic = await _context.DynamicTopics.FindAsync(model.TopicId);
                if (topic == null)
                {
                    return NotFound();
                }

                topic.Title = model.Title;
                topic.Content = model.Content;
                topic.ImageUrl = model.ImageUrl;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Topic updated successfully!";

                return RedirectToAction("Topics", new { sectionId = model.SectionId });
            }

            return View(model);
        }

        /// <summary>
        /// Delete dynamic topic
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTopic(int id, int sectionId)
        {
            var topic = await _context.DynamicTopics.FindAsync(id);
            if (topic == null)
            {
                return NotFound();
            }

            _context.DynamicTopics.Remove(topic);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Topic deleted successfully!";
            return RedirectToAction("Topics", new { sectionId });
        }

        #endregion

        #region User Management

        /// <summary>
        /// Manage users
        /// </summary>
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        #endregion

        #region Event Management

        /// <summary>
        /// Manage all events
        /// </summary>
        public async Task<IActionResult> Events()
        {
            var events = await _context.Events
                .Include(e => e.EventRegistrations)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return View(events);
        }

        /// <summary>
        /// Create new event
        /// </summary>
        [HttpGet]
        public IActionResult CreateEvent()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(EventFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var eventEntity = new Event
                {
                    Title = model.Title,
                    Description = model.Description,
                    ImageUrl = model.ImageUrl,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Price = model.Price,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Events.Add(eventEntity);
                await _context.SaveChangesAsync();

                var user = await _userManager.GetUserAsync(User);
                _logger.LogInformation($"Admin {user?.Email} created event {eventEntity.Title}");
                TempData["Success"] = "Event created successfully!";

                return RedirectToAction("Events");
            }

            return View(model);
        }

        /// <summary>
        /// Edit existing event
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity == null)
            {
                return NotFound();
            }

            var model = new EventFormViewModel
            {
                EventId = eventEntity.EventId,
                Title = eventEntity.Title,
                Description = eventEntity.Description,
                ImageUrl = eventEntity.ImageUrl,
                StartDate = eventEntity.StartDate,
                EndDate = eventEntity.EndDate,
                Price = eventEntity.Price
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(EventFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var eventEntity = await _context.Events.FindAsync(model.EventId);
                if (eventEntity == null)
                {
                    return NotFound();
                }

                eventEntity.Title = model.Title;
                eventEntity.Description = model.Description;
                eventEntity.ImageUrl = model.ImageUrl;
                eventEntity.StartDate = model.StartDate;
                eventEntity.EndDate = model.EndDate;
                eventEntity.Price = model.Price;

                await _context.SaveChangesAsync();

                var user = await _userManager.GetUserAsync(User);
                _logger.LogInformation($"Admin {user?.Email} updated event {eventEntity.Title}");
                TempData["Success"] = "Event updated successfully!";

                return RedirectToAction("Events");
            }

            return View(model);
        }

        /// <summary>
        /// Delete event
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventEntity = await _context.Events
                .Include(e => e.EventRegistrations)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventEntity == null)
            {
                return NotFound();
            }

            // Remove registrations first
            _context.EventRegistrations.RemoveRange(eventEntity.EventRegistrations);
            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);
            _logger.LogInformation($"Admin {user?.Email} deleted event {eventEntity.Title}");
            TempData["Success"] = "Event deleted successfully!";

            return RedirectToAction("Events");
        }

        /// <summary>
        /// View event registrations
        /// </summary>
        public async Task<IActionResult> EventRegistrations(int? eventId)
        {
            var registrationsQuery = _context.EventRegistrations
                .Include(er => er.Event)
                .Include(er => er.User)
                .AsQueryable();

            if (eventId.HasValue)
            {
                registrationsQuery = registrationsQuery.Where(er => er.EventId == eventId.Value);
            }

            var registrations = await registrationsQuery
                .OrderByDescending(er => er.RegistrationDate)
                .ToListAsync();

            ViewBag.Events = await _context.Events.ToListAsync();
            ViewBag.SelectedEventId = eventId;

            return View(registrations);
        }

        /// <summary>
        /// Delete event registration
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEventRegistration(int id)
        {
            var registration = await _context.EventRegistrations
                .Include(er => er.Event)
                .FirstOrDefaultAsync(er => er.EventRegistrationId == id);

            if (registration == null)
            {
                return NotFound();
            }

            var eventId = registration.EventId;
            _context.EventRegistrations.Remove(registration);
            await _context.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);
            _logger.LogInformation($"Admin {user?.Email} deleted registration for event {registration.Event.Title}");
            TempData["Success"] = "Registration deleted successfully!";

            return RedirectToAction("EventRegistrations", new { eventId });
        }

        /// <summary>
        /// Initialize event registration amount paid values
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> InitializeEventRegistrationAmounts()
        {
            try
            {
                // Get all registrations that need to be updated
                var registrations = await _context.EventRegistrations
                    .Include(er => er.Event)
                    .ToListAsync();

                int count = 0;
                foreach (var registration in registrations)
                {
                    try
                    {
                        // Set AmountPaid equal to the event price
                        registration.AmountPaid = registration.Event.Price;
                        count++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error updating registration {registration.EventRegistrationId}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Successfully updated {count} event registrations.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error initializing event registration amounts: {ex.Message}");
                TempData["Error"] = "An error occurred initializing event registration amounts.";
            }

            return RedirectToAction("EventRegistrations");
        }

        #endregion

        #region Game Category Management

        /// <summary>
        /// Manage game categories
        /// </summary>
        public async Task<IActionResult> GameCategories()
        {
            var categories = await _context.GameCategories
                .Include(c => c.Games)
                .ToListAsync();

            return View(categories);
        }

        /// <summary>
        /// Create new game category
        /// </summary>
        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(GameCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = new GameCategory
                {
                    Name = model.Name,
                    Description = model.Description
                };

                _context.GameCategories.Add(category);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Category created successfully!";
                return RedirectToAction("GameCategories");
            }

            return View(model);
        }

        /// <summary>
        /// Edit game category
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.GameCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var model = new GameCategoryViewModel
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(GameCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = await _context.GameCategories.FindAsync(model.CategoryId);
                if (category == null)
                {
                    return NotFound();
                }

                category.Name = model.Name;
                category.Description = model.Description;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Category updated successfully!";

                return RedirectToAction("GameCategories");
            }

            return View(model);
        }

        /// <summary>
        /// Delete game category
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.GameCategories
                .Include(c => c.Games)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }

            // Check if category has any games
            if (category.Games.Any())
            {
                TempData["Error"] = "Cannot delete category with games. Please move or delete the games first.";
                return RedirectToAction("GameCategories");
            }

            _context.GameCategories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category deleted successfully!";
            return RedirectToAction("GameCategories");
        }

        #endregion

        #region Advanced User Management

        /// <summary>
        /// Enhanced user management with roles
        /// </summary>
        public async Task<IActionResult> UsersWithRoles()
        {
            var users = await _context.Users.ToListAsync();
            var userViewModels = new List<UserWithRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserWithRoleViewModel
                {
                    User = user,
                    Roles = roles.ToList(),
                    PrimaryRole = roles.FirstOrDefault() ?? "No Role"
                });
            }

            ViewBag.AllRoles = ApplicationRoles.GetAllRoles();
            return View(userViewModels);
        }

        /// <summary>
        /// Change user role
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserRole(string userId, string newRole)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(newRole))
            {
                TempData["Error"] = "Invalid user ID or role.";
                return RedirectToAction("UsersWithRoles");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("UsersWithRoles");
            }

            // Remove all existing roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            // Add new role
            var result = await _userManager.AddToRoleAsync(user, newRole);
            if (result.Succeeded)
            {
                TempData["Success"] = $"User role changed to {newRole} successfully!";
                _logger.LogInformation($"Admin changed user {user.Email} role to {newRole}");
            }
            else
            {
                TempData["Error"] = "Failed to change user role: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("UsersWithRoles");
        }

        /// <summary>
        /// Delete user account
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("UsersWithRoles");
            }

            // Don't allow deleting admin users (safety measure)
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains(ApplicationRoles.Admin))
            {
                TempData["Error"] = "Cannot delete admin users.";
                return RedirectToAction("UsersWithRoles");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "User deleted successfully!";
                _logger.LogInformation($"Admin deleted user {user.Email}");
            }
            else
            {
                TempData["Error"] = "Failed to delete user: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("UsersWithRoles");
        }

        /// <summary>
        /// User details and edit
        /// </summary>
        public async Task<IActionResult> UserDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var model = new UserDetailsViewModel
            {
                User = user,
                Roles = roles.ToList(),
                AllRoles = ApplicationRoles.GetAllRoles().ToList()
            };

            // Get user's activity
            model.GamePurchases = await _context.GamePurchases
                .Include(p => p.Game)
                .Where(p => p.UserId == id)
                .OrderByDescending(p => p.PurchaseDate)
                .Take(5)
                .ToListAsync();

            model.EventRegistrations = await _context.EventRegistrations
                .Include(er => er.Event)
                .Where(er => er.UserId == id)
                .OrderByDescending(er => er.RegistrationDate)
                .Take(5)
                .ToListAsync();

            // Get class information if teacher or student
            if (roles.Contains(ApplicationRoles.Teacher))
            {
                model.Classes = await _context.Classes
                    .Where(c => c.TeacherId == id)
                    .ToListAsync();
            }
            else if (roles.Contains(ApplicationRoles.Student))
            {
                model.Classes = await _context.Classes
                    .Where(c => c.ClassStudents.Any(cs => cs.StudentId == id))
                    .ToListAsync();
            }

            // Get parent-student relationships
            if (roles.Contains(ApplicationRoles.Parent))
            {
                model.Children = await _context.ParentStudentRelationships
                    .Include(ps => ps.Student)
                    .Where(ps => ps.ParentId == id)
                    .Select(ps => ps.Student)
                    .ToListAsync();
            }
            else if (roles.Contains(ApplicationRoles.Student))
            {
                model.Parents = await _context.ParentStudentRelationships
                    .Include(ps => ps.Parent)
                    .Where(ps => ps.StudentId == id)
                    .Select(ps => ps.Parent)
                    .ToListAsync();
            }

            return View(model);
        }

        /// <summary>
        /// Edit user details
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid data submitted.";
                return RedirectToAction("UserDetails", new { id = model.Id });
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("UsersWithRoles");
            }

            // Update user fields
            user.Name = model.Name;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            // Save changes
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Failed to update user: " + string.Join(", ", result.Errors.Select(e => e.Description));
                return RedirectToAction("UserDetails", new { id = model.Id });
            }

            TempData["Success"] = "User updated successfully!";
            return RedirectToAction("UserDetails", new { id = model.Id });
        }

        /// <summary>
        /// Unlock a user account
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("UsersWithRoles");
            }

            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
            TempData["Success"] = "User account unlocked successfully!";
            return RedirectToAction("UserDetails", new { id });
        }

        /// <summary>
        /// Reset user password
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("UsersWithRoles");
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Generate a temporary password (in a real app, you'd email this to the user)
            string tempPassword = GenerateRandomPassword();
            
            var result = await _userManager.ResetPasswordAsync(user, token, tempPassword);
            if (result.Succeeded)
            {
                TempData["Success"] = $"Password reset successfully! Temporary password: {tempPassword}";
                return RedirectToAction("UserDetails", new { id });
            }
            else
            {
                TempData["Error"] = "Failed to reset password: " + string.Join(", ", result.Errors.Select(e => e.Description));
                return RedirectToAction("UserDetails", new { id });
            }
        }

        /// <summary>
        /// Generate a random password
        /// </summary>
        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #endregion

        #region Reports and Analytics

        /// <summary>
        /// Analytics dashboard
        /// </summary>
        public async Task<IActionResult> Analytics()
        {
            var analytics = new AdminAnalyticsViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalGames = await _context.Games.CountAsync(),
                TotalEvents = await _context.Events.CountAsync(),
                TotalClasses = await _context.Classes.CountAsync(),
                
                AdminCount = await _context.Users.Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == ApplicationRoles.Admin))).CountAsync(),
                TeacherCount = await _context.Users.Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == ApplicationRoles.Teacher))).CountAsync(),
                ParentCount = await _context.Users.Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == ApplicationRoles.Parent))).CountAsync(),
                StudentCount = await _context.Users.Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == ApplicationRoles.Student))).CountAsync(),

                TotalGamePurchases = await _context.GamePurchases.Where(p => p.PaymentStatus == PaymentStatus.Paid).CountAsync(),
                TotalRevenue = await _context.GamePurchases.Where(p => p.PaymentStatus == PaymentStatus.Paid).SumAsync(p => p.FinalPrice),
                TotalEventRegistrations = await _context.EventRegistrations.CountAsync(),

                RecentUsers = await _context.Users.OrderByDescending(u => u.CreatedAt).Take(5).ToListAsync(),
                RecentPurchases = await _context.GamePurchases
                    .Include(p => p.User)
                    .Include(p => p.Game)
                    .Where(p => p.PaymentStatus == PaymentStatus.Paid)
                    .OrderByDescending(p => p.PurchaseDate)
                    .Take(10)
                    .ToListAsync(),
                RecentEventRegistrations = await _context.EventRegistrations
                    .Include(er => er.User)
                    .Include(er => er.Event)
                    .OrderByDescending(er => er.RegistrationDate)
                    .Take(10)
                    .ToListAsync()
            };

            return View(analytics);
        }

        #endregion

        #region Parent-Student Link Management

        /// <summary>
        /// Link parent and student accounts
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> LinkParentStudent()
        {
            var parents = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Parent);
            var students = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Student);
            var relationships = await _context.ParentStudentRelationships
                .Include(r => r.Parent)
                .Include(r => r.Student)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var model = new LinkParentStudentViewModel
            {
                Parents = parents.ToList(),
                Students = students.ToList(),
                ExistingRelationships = relationships
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkParentStudent(LinkParentStudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var parents = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Parent);
                var students = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Student);
                model.Parents = parents.ToList();
                model.Students = students.ToList();
                return View(model);
            }

            // Check if relationship already exists
            var existingRelationship = await _context.ParentStudentRelationships
                .FirstOrDefaultAsync(ps => ps.ParentId == model.ParentId && ps.StudentId == model.StudentId);

            if (existingRelationship != null)
            {
                TempData["Error"] = "This parent-student relationship already exists.";
                return RedirectToAction("UsersWithRoles");
            }

            // Create new relationship
            var relationship = new ParentStudentRelationship
            {
                ParentId = model.ParentId,
                StudentId = model.StudentId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ParentStudentRelationships.Add(relationship);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Parent and student linked successfully!";
            return RedirectToAction("UsersWithRoles");
        }

        /// <summary>
        /// Delete a parent-student relationship
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DeleteParentStudentLink(int relationshipId)
        {
            var relationship = await _context.ParentStudentRelationships
                .Include(r => r.Parent)
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.RelationshipId == relationshipId);

            if (relationship == null)
            {
                TempData["Error"] = "Relationship not found.";
                return RedirectToAction("LinkParentStudent");
            }

            try
            {
                _context.ParentStudentRelationships.Remove(relationship);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Relationship between {relationship.Parent.Name} and {relationship.Student.Name} removed successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing parent-student relationship: {0}", relationshipId);
                TempData["Error"] = "An error occurred while removing the relationship.";
            }

            return RedirectToAction("LinkParentStudent");
        }

        #endregion

        #region User Profile Image Management

        /// <summary>
        /// Show profile image upload form
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> UploadUserProfileImage(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("UsersWithRoles");
            }

            var model = new UserProfileImageViewModel
            {
                UserId = id,
                CurrentImageUrl = user.ProfileImage
            };

            return View(model);
        }

        /// <summary>
        /// Upload user profile image
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadUserProfileImage(UserProfileImageViewModel model)
        {
            if (!ModelState.IsValid || model.ProfileImage == null)
            {
                TempData["Error"] = "Please select an image to upload.";
                return RedirectToAction("UploadUserProfileImage", new { id = model.UserId });
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("UsersWithRoles");
            }

            // If user already has a profile image, delete it
            if (!string.IsNullOrEmpty(user.ProfileImage) && user.ProfileImage.Contains("/uploads/"))
            {
                try
                {
                    string relativePath = user.ProfileImage.Substring(user.ProfileImage.IndexOf("/uploads/"));
                    await _fileService.DeleteFileAsync(relativePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete previous profile image: {0}", user.ProfileImage);
                }
            }

            // Upload the new image
            try
            {
                string imageUrl = await _fileService.UploadUserImageAsync(model.ProfileImage);
                
                // Update user profile
                user.ProfileImage = imageUrl;
                await _userManager.UpdateAsync(user);
                
                TempData["Success"] = "Profile image uploaded successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload profile image for user: {0}", model.UserId);
                TempData["Error"] = "Failed to upload profile image. Please try again.";
            }

            return RedirectToAction("UserDetails", new { id = model.UserId });
        }

        /// <summary>
        /// Delete user profile image
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DeleteUserProfileImage(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("UsersWithRoles");
            }

            if (string.IsNullOrEmpty(user.ProfileImage))
            {
                TempData["Warning"] = "User does not have a profile image.";
                return RedirectToAction("UserDetails", new { id = id });
            }

            // Delete the image file if it's stored in uploads
            if (user.ProfileImage.Contains("/uploads/"))
            {
                try
                {
                    string relativePath = user.ProfileImage.Substring(user.ProfileImage.IndexOf("/uploads/"));
                    await _fileService.DeleteFileAsync(relativePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete profile image: {0}", user.ProfileImage);
                    TempData["Warning"] = "Failed to delete image file, but reference was removed.";
                }
            }

            // Clear the profile image URL
            user.ProfileImage = null;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Profile image deleted successfully.";
            return RedirectToAction("UserDetails", new { id = id });
        }

        #endregion

        #region User Edit Management

        /// <summary>
        /// Edit user form
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("UsersWithRoles");
            }

            var model = new UserEditViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        #endregion
    }
}
