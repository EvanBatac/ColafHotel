using ColafHotel.Data;
using ColafHotel.Models;
using ColafHotel.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ColafHotel.Controllers;

public class HomeController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var featuredRooms = await context.Rooms
            .Where(room => room.IsAvailable)
            .Take(12)
            .ToListAsync();

        var viewModel = new HomeIndexViewModel
        {
            FeaturedRooms = featuredRooms
                .OrderBy(room => room.PricePerNight)
                .Take(3)
                .ToList()
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
