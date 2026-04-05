using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Gamingv1.Data;
using Gamingv1.Models;
using Gamingv1.ViewModels;

namespace Gamingv1.Controllers
{
    /// <summary>
    /// Handles event management and registration
    /// </summary>
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EventsController> _logger;

        public EventsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<EventsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Display all available events
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Include(e => e.EventRegistrations)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return View(events);
        }

        /// <summary>
        /// Display event details
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var eventEntity = await _context.Events
                .Include(e => e.EventRegistrations)
                    .ThenInclude(er => er.User)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventEntity == null)
            {
                return NotFound();
            }

            var viewModel = new EventDetailsViewModel
            {
                Event = eventEntity,
                UserIsRegistered = false
            };

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    viewModel.UserIsRegistered = await _context.EventRegistrations
                        .AnyAsync(er => er.EventId == id && er.UserId == user.Id);
                }
            }

            return View(viewModel);
        }

        /// <summary>
        /// Register for an event
        /// </summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(int eventId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var eventEntity = await _context.Events.FindAsync(eventId);
            if (eventEntity == null)
            {
                return NotFound();
            }

            // Check if already registered
            var existingRegistration = await _context.EventRegistrations
                .FirstOrDefaultAsync(er => er.EventId == eventId && er.UserId == user.Id);

            if (existingRegistration != null)
            {
                TempData["Error"] = "You are already registered for this event.";
                return RedirectToAction("Details", new { id = eventId });
            }

            // Check if event has passed
            if (eventEntity.EndDate < DateTime.UtcNow)
            {
                TempData["Error"] = "This event has already ended.";
                return RedirectToAction("Details", new { id = eventId });
            }

            var registration = new EventRegistration
            {
                UserId = user.Id,
                EventId = eventId,
                RegistrationDate = DateTime.UtcNow,
                PaymentStatus = PaymentStatus.Paid // In real app, handle payment processing
            };

            // Set the AmountPaid property safely
            try
            {
                registration.AmountPaid = eventEntity.Price;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error setting AmountPaid: {ex.Message}");
                // Continue without setting AmountPaid (will be fixed by migration later)
            }

            _context.EventRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {user.Email} registered for event {eventEntity.Title}");
            TempData["Success"] = $"Successfully registered for {eventEntity.Title}!";

            return RedirectToAction("Details", new { id = eventId });
        }

        /// <summary>
        /// Create new event (Admin only)
        /// </summary>
        [Authorize(Policy = "RequireAdmin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Policy = "RequireAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventFormViewModel model)
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

                return RedirectToAction("Index");
            }

            return View(model);
        }

        /// <summary>
        /// View user's registered events
        /// </summary>
        [Authorize]
        public async Task<IActionResult> MyEvents()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var registrations = await _context.EventRegistrations
                .Where(er => er.UserId == user.Id)
                .Include(er => er.Event)
                .OrderByDescending(er => er.RegistrationDate)
                .ToListAsync();

            return View(registrations);
        }
    }

    /// <summary>
    /// View model for event details
    /// </summary>
    public class EventDetailsViewModel
    {
        public Event Event { get; set; } = null!;
        public bool UserIsRegistered { get; set; }
    }
}
