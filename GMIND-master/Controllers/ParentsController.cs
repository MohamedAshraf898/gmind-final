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
    /// Handles parent-student relationship management
    /// </summary>
    [Authorize(Policy = "RequireParent")]
    public class ParentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRoleService _roleService;
        private readonly ILogger<ParentsController> _logger;

        public ParentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IRoleService roleService,
            ILogger<ParentsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleService = roleService;
            _logger = logger;
        }

        /// <summary>
        /// Parent dashboard showing children and their activities
        /// </summary>
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Parent"))
            {
                return Forbid();
            }

            var children = await _context.ParentStudents
                .Where(ps => ps.ParentId == user.Id)
                .Include(ps => ps.Student)
                .Select(ps => ps.Student)
                .ToListAsync();

            var viewModel = new ParentDashboardViewModel
            {
                Children = children
            };

            // Get children's classes
            foreach (var child in children)
            {
                var childClasses = await _context.ClassStudents
                    .Where(cs => cs.StudentId == child.Id)
                    .Include(cs => cs.Class)
                        .ThenInclude(c => c.Teacher)
                    .Select(cs => cs.Class)
                    .ToListAsync();

                viewModel.ChildrenClasses[child] = childClasses;

                // Get children's purchases
                var childPurchases = await _context.GamePurchases
                    .Where(p => p.UserId == child.Id && p.PaymentStatus == PaymentStatus.Paid)
                    .Include(p => p.Game)
                    .ToListAsync();

                viewModel.ChildrenPurchases[child] = childPurchases;
            }

            return View(viewModel);
        }

        /// <summary>
        /// Add a child using their student code
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddChild()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Parent"))
            {
                return Forbid();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddChild(AddChildViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Parent"))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                var student = await _context.Users
                    .Where(u => u.StudentCode == model.StudentCode)
                    .Join(
                        _context.UserRoles,
                        u => u.Id,
                        ur => ur.UserId,
                        (u, ur) => new { User = u, RoleId = ur.RoleId }
                    )
                    .Join(
                        _context.Roles.Where(r => r.Name == "Student"),
                        combined => combined.RoleId,
                        r => r.Id,
                        (combined, r) => combined.User
                    )
                    .FirstOrDefaultAsync();

                if (student == null)
                {
                    ModelState.AddModelError("StudentCode", "Invalid student code.");
                    return View(model);
                }

                // Check if relationship already exists
                var existingRelationship = await _context.ParentStudents
                    .FirstOrDefaultAsync(ps => ps.ParentId == user.Id && ps.StudentId == student.Id);

                if (existingRelationship != null)
                {
                    TempData["Error"] = "This student is already linked to your account.";
                    return RedirectToAction("Dashboard");
                }

                var parentStudent = new ParentStudent
                {
                    ParentId = user.Id,
                    StudentId = student.Id,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ParentStudents.Add(parentStudent);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Parent {user.Email} linked to student {student.Email}");
                TempData["Success"] = $"Successfully linked to student {student.Name}!";

                return RedirectToAction("Dashboard");
            }

            return View(model);
        }

        /// <summary>
        /// Remove a child from parent's account
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveChild(string studentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Parent"))
            {
                return Forbid();
            }

            var parentStudent = await _context.ParentStudents
                .FirstOrDefaultAsync(ps => ps.ParentId == user.Id && ps.StudentId == studentId);

            if (parentStudent != null)
            {
                _context.ParentStudents.Remove(parentStudent);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Parent {user.Email} unlinked from student {studentId}");
                TempData["Success"] = "Student removed from your account.";
            }

            return RedirectToAction("Dashboard");
        }

        /// <summary>
        /// View detailed information about a specific child
        /// </summary>
        public async Task<IActionResult> ChildDetails(string studentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Parent"))
            {
                return Forbid();
            }

            // Verify parent-student relationship
            var relationship = await _context.ParentStudents
                .FirstOrDefaultAsync(ps => ps.ParentId == user.Id && ps.StudentId == studentId);

            if (relationship == null)
            {
                return Forbid();
            }

            var student = await _context.Users.FindAsync(studentId);
            if (student == null)
            {
                return NotFound();
            }

            // Get student's classes
            var studentClasses = await _context.ClassStudents
                .Where(cs => cs.StudentId == studentId)
                .Include(cs => cs.Class)
                    .ThenInclude(c => c.Teacher)
                .Select(cs => cs.Class)
                .ToListAsync();

            // Get student's purchases
            var studentPurchases = await _context.GamePurchases
                .Where(p => p.UserId == studentId && p.PaymentStatus == PaymentStatus.Paid)
                .Include(p => p.Game)
                    .ThenInclude(g => g.Category)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();

            // Get student's reviews
            var studentReviews = await _context.GameReviews
                .Where(r => r.UserId == studentId)
                .Include(r => r.Game)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var viewModel = new ChildDetailsViewModel
            {
                Student = student,
                Classes = studentClasses,
                Purchases = studentPurchases,
                Reviews = studentReviews
            };

            return View(viewModel);
        }
    }

    /// <summary>
    /// View model for child details page
    /// </summary>
    public class ChildDetailsViewModel
    {
        public ApplicationUser Student { get; set; } = null!;
        public List<Class> Classes { get; set; } = new List<Class>();
        public List<GamePurchase> Purchases { get; set; } = new List<GamePurchase>();
        public List<GameReview> Reviews { get; set; } = new List<GameReview>();
    }
}
