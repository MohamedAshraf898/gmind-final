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
    /// Handles class management for teachers and students
    /// </summary>
    [Authorize]
    public class ClassesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ClassesController> _logger;

        public ClassesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ClassesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Display classes based on user role
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var viewModel = new ClassIndexViewModel();
            var userRoles = await _userManager.GetRolesAsync(user);
            
            if (userRoles.Contains("Teacher"))
            {
                viewModel.TeacherClasses = await _context.Classes
                    .Where(c => c.TeacherId == user.Id)
                    .Include(c => c.ClassStudents)
                        .ThenInclude(cs => cs.Student)
                    .ToListAsync();
            }
            else if (userRoles.Contains("Student"))
            {
                viewModel.StudentClasses = await _context.ClassStudents
                    .Where(cs => cs.StudentId == user.Id)
                    .Include(cs => cs.Class)
                        .ThenInclude(c => c.Teacher)
                    .Select(cs => cs.Class)
                    .ToListAsync();
            }
            else if (userRoles.Contains("Parent"))
            {
                // Get children's classes
                var childrenIds = await _context.ParentStudents
                    .Where(ps => ps.ParentId == user.Id)
                    .Select(ps => ps.StudentId)
                    .ToListAsync();

                viewModel.ChildrenClasses = await _context.ClassStudents
                    .Where(cs => childrenIds.Contains(cs.StudentId))
                    .Include(cs => cs.Class)
                        .ThenInclude(c => c.Teacher)
                    .Include(cs => cs.Student)
                    .GroupBy(cs => cs.Class)
                    .ToDictionaryAsync(g => g.Key, g => g.Select(cs => cs.Student).ToList());
            }

            return View(viewModel);
        }

        /// <summary>
        /// Create a new class (Teachers only)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "RequireTeacher")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireTeacher")]
        public async Task<IActionResult> Create(CreateClassViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                var joinCode = await GenerateUniqueJoinCode();

                var newClass = new Class
                {
                    ClassName = model.ClassName,
                    Description = model.Description,
                    TeacherId = user.Id,
                    JoinCode = joinCode,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Classes.Add(newClass);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Teacher {user.Email} created class {newClass.ClassName} with join code {joinCode}");
                TempData["Success"] = $"Class created successfully! Join Code: {joinCode}";

                return RedirectToAction("Index");
            }

            return View(model);
        }

        /// <summary>
        /// Join a class using join code (Students only)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "RequireStudent")]
        public IActionResult Join()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireStudent")]
        public async Task<IActionResult> Join(JoinClassViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                var classToJoin = await _context.Classes
                    .FirstOrDefaultAsync(c => c.JoinCode == model.JoinCode);

                if (classToJoin == null)
                {
                    ModelState.AddModelError("JoinCode", "Invalid join code.");
                    return View(model);
                }

                // Check if student is already in this class
                var existingEnrollment = await _context.ClassStudents
                    .FirstOrDefaultAsync(cs => cs.ClassId == classToJoin.ClassId && cs.StudentId == user.Id);

                if (existingEnrollment != null)
                {
                    TempData["Error"] = "You are already enrolled in this class.";
                    return RedirectToAction("Index");
                }

                var classStudent = new ClassStudent
                {
                    ClassId = classToJoin.ClassId,
                    StudentId = user.Id,
                    JoinedAt = DateTime.UtcNow
                };

                _context.ClassStudents.Add(classStudent);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Student {user.Email} joined class {classToJoin.ClassName}");
                TempData["Success"] = $"Successfully joined {classToJoin.ClassName}!";

                return RedirectToAction("Index");
            }

            return View(model);
        }

        /// <summary>
        /// View class details
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var classEntity = await _context.Classes
                .Include(c => c.Teacher)
                .Include(c => c.ClassStudents)
                    .ThenInclude(cs => cs.Student)
                .FirstOrDefaultAsync(c => c.ClassId == id);

            if (classEntity == null)
            {
                return NotFound();
            }

            // Check if user has access to this class
            bool hasAccess = false;
            if (await _userManager.IsInRoleAsync(user, "Teacher"))
            {
                hasAccess = classEntity.TeacherId == user.Id;
            }
            else if (await _userManager.IsInRoleAsync(user, "Student"))
            {
                hasAccess = classEntity.ClassStudents.Any(cs => cs.StudentId == user.Id);
            }
            else if (await _userManager.IsInRoleAsync(user, "Parent"))
            {
                var childrenIds = await _context.ParentStudents
                    .Where(ps => ps.ParentId == user.Id)
                    .Select(ps => ps.StudentId)
                    .ToListAsync();
                hasAccess = classEntity.ClassStudents.Any(cs => childrenIds.Contains(cs.StudentId));
            }

            if (!hasAccess)
            {
                return Forbid();
            }

            return View(classEntity);
        }

        /// <summary>
        /// Remove student from class (Teachers only)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireTeacher")]
        public async Task<IActionResult> RemoveStudent(int classId, string studentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var classEntity = await _context.Classes.FindAsync(classId);
            if (classEntity == null || classEntity.TeacherId != user.Id)
            {
                return Forbid();
            }

            var classStudent = await _context.ClassStudents
                .FirstOrDefaultAsync(cs => cs.ClassId == classId && cs.StudentId == studentId);

            if (classStudent != null)
            {
                _context.ClassStudents.Remove(classStudent);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Student removed from class successfully.";
            }

            return RedirectToAction("Details", new { id = classId });
        }

        /// <summary>
        /// Generate unique join code for classes
        /// </summary>
        private async Task<string> GenerateUniqueJoinCode()
        {
            string code;
            bool isUnique;

            do
            {
                code = GenerateRandomCode();
                isUnique = !await _context.Classes.AnyAsync(c => c.JoinCode == code);
            } while (!isUnique);

            return code;
        }

        private string GenerateRandomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
