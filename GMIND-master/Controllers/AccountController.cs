using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Gamingv1.Data;
using Gamingv1.Models;
using Gamingv1.Services;
using Gamingv1.ViewModels;
using System.Security.Claims;

namespace Gamingv1.Controllers
{
    /// <summary>
    /// Handles user authentication, registration, and account management
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRoleService _roleService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IRoleService roleService,
            ApplicationDbContext context,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleService = roleService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Display registration form with available roles
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            var model = new RegisterViewModel
            {
                AvailableRoles = ApplicationRoles.GetPublicRoles()
                    .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = ApplicationRoles.GetDisplayName(r),
                        Value = r
                    }).ToList()
            };
            return View(model);
        }

        /// <summary>
        /// Handle user registration with proper validation and role assignment
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = ApplicationRoles.GetPublicRoles()
                    .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = ApplicationRoles.GetDisplayName(r),
                        Value = r
                    }).ToList();
                return View(model);
            }

            try
            {
                // Validate role selection
                if (!ApplicationRoles.IsValidRole(model.Role) || model.Role == ApplicationRoles.Admin)
                {
                    ModelState.AddModelError(nameof(model.Role), "Invalid role selection.");
                    model.AvailableRoles = ApplicationRoles.GetPublicRoles()
                        .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                        {
                            Text = ApplicationRoles.GetDisplayName(r),
                            Value = r
                        }).ToList();
                    return View(model);
                }

                // Generate student code if role is Student
                string? studentCode = null;
                if (model.Role == ApplicationRoles.Student)
                {
                    studentCode = await GenerateUniqueStudentCodeAsync();
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Name = model.Name,
                    StudentCode = studentCode,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Use the role service to assign the role
                    await _roleService.AssignRoleAsync(user.Id, model.Role);
                    
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User {Email} registered successfully with role {Role}", user.Email, model.Role);
                    
                    TempData["Success"] = $"Welcome! Your account has been created successfully as a {ApplicationRoles.GetDisplayName(model.Role)}.";
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
            }

            // Re-populate the roles dropdown on error
            model.AvailableRoles = ApplicationRoles.GetPublicRoles()
                .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = ApplicationRoles.GetDisplayName(r),
                    Value = r
                }).ToList();
                
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} logged in successfully", model.Email);
                    return RedirectToLocal(returnUrl);
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View("Login");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {Provider} provider", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }

            // If the user does not have an account, then ask the user to create an account
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = info.LoginProvider;

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);

            var model = new ExternalLoginViewModel
            {
                Email = email ?? "",
                Name = name ?? "",
                AvailableRoles = ApplicationRoles.GetPublicRoles()
                    .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = ApplicationRoles.GetDisplayName(r),
                        Value = r
                    }).ToList()
            };

            return View("ExternalLoginConfirmation", model);
        }

        /// <summary>
        /// Handle external login confirmation with proper validation
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = ApplicationRoles.GetPublicRoles()
                    .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = ApplicationRoles.GetDisplayName(r),
                        Value = r
                    }).ToList();
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            try
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    throw new ApplicationException("Error loading external login information during confirmation.");
                }

                // Validate role selection
                if (!ApplicationRoles.IsValidRole(model.Role) || model.Role == ApplicationRoles.Admin)
                {
                    ModelState.AddModelError(nameof(model.Role), "Invalid role selection.");
                    model.AvailableRoles = ApplicationRoles.GetPublicRoles()
                        .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                        {
                            Text = ApplicationRoles.GetDisplayName(r),
                            Value = r
                        }).ToList();
                    ViewData["ReturnUrl"] = returnUrl;
                    return View(model);
                }

                // Let the role service handle the OAuth login
                await _roleService.HandleOAuthLoginAsync(info, model.Role);
                
                // Find the user that was just created/updated
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User created an account using {Provider} provider with role {Role}", 
                        info.LoginProvider, model.Role);
                    
                    TempData["Success"] = $"Welcome! Your account has been linked successfully as a {ApplicationRoles.GetDisplayName(model.Role)}.";
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    throw new ApplicationException("Failed to create or find user after external login.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OAuth account creation for {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "An error occurred during account creation: " + ex.Message);
            }

            // Re-populate the roles dropdown on error
            model.AvailableRoles = ApplicationRoles.GetPublicRoles()
                .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = ApplicationRoles.GetDisplayName(r),
                    Value = r
                }).ToList();

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        /// <summary>
        /// Display user profile with role information
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User not found when accessing profile");
                return NotFound("User not found");
            }

            try
            {
                // Get user role using role service
                string userRole = await _roleService.GetUserRoleAsync(user.Id);

                var model = new ProfileViewModel
                {
                    Name = user.Name,
                    Email = user.Email ?? "",
                    Phone = user.Phone,
                    Role = ApplicationRoles.GetDisplayName(userRole),
                    StudentCode = user.StudentCode,
                    ProfileImage = user.ProfileImage
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile for user {UserId}", user.Id);
                TempData["Error"] = "Error loading your profile. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Update user profile information
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User not found when updating profile");
                return NotFound("User not found");
            }

            if (!ModelState.IsValid)
            {
                // Re-populate role information for display
                try
                {
                    string userRole = await _roleService.GetUserRoleAsync(user.Id);
                    model.Role = ApplicationRoles.GetDisplayName(userRole);
                    model.Email = user.Email ?? "";
                    model.StudentCode = user.StudentCode;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reloading profile for user {UserId}", user.Id);
                }
                return View(model);
            }

            try
            {
                // Update user information
                user.Name = model.Name?.Trim() ?? "";
                user.Phone = model.Phone?.Trim();
                user.ProfileImage = model.ProfileImage?.Trim();

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} updated profile successfully", user.Id);
                    TempData["Success"] = "Profile updated successfully!";
                    return RedirectToAction("Profile");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", user.Id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating your profile. Please try again.");
            }

            // Re-populate role information on error
            try
            {
                string userRole = await _roleService.GetUserRoleAsync(user.Id);
                model.Role = ApplicationRoles.GetDisplayName(userRole);
                model.Email = user.Email ?? "";
                model.StudentCode = user.StudentCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading profile for user {UserId}", user.Id);
            }

            return View(model);
        }
        
        /// <summary>
        /// Change user role (Admin only) - follows the same validation pattern as other methods
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "RequireAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserRole(string userId, string newRole)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(newRole))
            {
                _logger.LogWarning("Invalid parameters for role change: UserId={UserId}, Role={Role}", userId, newRole);
                TempData["Error"] = "User ID and role are required.";
                return RedirectToAction("Users", "Admin");
            }
            
            // Validate role
            if (!ApplicationRoles.IsValidRole(newRole))
            {
                _logger.LogWarning("Invalid role attempted: {Role} for user {UserId}", newRole, userId);
                TempData["Error"] = $"Invalid role selected: {newRole}";
                return RedirectToAction("Users", "Admin");
            }
            
            try
            {
                // Verify user exists
                var targetUser = await _userManager.FindByIdAsync(userId);
                if (targetUser == null)
                {
                    _logger.LogWarning("User not found for role change: {UserId}", userId);
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Users", "Admin");
                }

                // Get current user (admin) for logging
                var currentUser = await _userManager.GetUserAsync(User);
                var currentUserRole = await _roleService.GetUserRoleAsync(targetUser.Id);
                
                // Use role service to assign role
                await _roleService.AssignRoleAsync(userId, newRole);
                
                _logger.LogInformation("Admin {AdminEmail} changed user {UserEmail} role from {OldRole} to {NewRole}", 
                    currentUser?.Email, targetUser.Email, currentUserRole, newRole);
                
                TempData["Success"] = $"User role updated successfully to {ApplicationRoles.GetDisplayName(newRole)}.";
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "ArgumentException changing role for user {UserId} to {Role}", userId, newRole);
                TempData["Error"] = "User not found: " + ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing role for user {UserId} to {Role}", userId, newRole);
                TempData["Error"] = "Failed to update user role. Please try again.";
            }
            
            return RedirectToAction("Users", "Admin");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Generates a unique student code that doesn't exist in the database
        /// </summary>
        private async Task<string> GenerateUniqueStudentCodeAsync()
        {
            string code;
            do
            {
                code = "STU" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            } while (await _context.Users.AnyAsync(u => u.StudentCode == code));
            
            return code;
        }
    }
}
