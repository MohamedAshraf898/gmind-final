using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gamingv1.Data;
using Gamingv1.Models;
using Gamingv1.ViewModels;

namespace Gamingv1.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Homepage with dynamic content and featured games
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var viewModel = new HomeIndexViewModel();

        // Get dynamic sections and topics
        viewModel.DynamicSections = await _context.DynamicSections
            .Include(s => s.DynamicTopics)
            .OrderBy(s => s.Name)
            .ToListAsync();

        // Get featured games (latest 6 games)
        viewModel.FeaturedGames = await _context.Games
            .Include(g => g.Category)
            .Include(g => g.GameImages)
            .Include(g => g.GameReviews)
            .OrderByDescending(g => g.CreatedAt)
            .Take(6)
            .ToListAsync();

        // Get latest events (next 3 upcoming events)
        viewModel.UpcomingEvents = await _context.Events
            .Where(e => e.StartDate > DateTime.UtcNow)
            .OrderBy(e => e.StartDate)
            .Take(3)
            .ToListAsync();

        // Get statistics
        viewModel.TotalGames = await _context.Games.CountAsync();
        viewModel.TotalUsers = await _context.Users.CountAsync();
        viewModel.TotalClasses = await _context.Classes.CountAsync();

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// About page
    /// </summary>
    public IActionResult About()
    {
        return View();
    }

    /// <summary>
    /// Contact page
    /// </summary>
    public IActionResult Contact()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
