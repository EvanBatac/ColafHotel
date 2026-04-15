using ColafHotel.Data;
using ColafHotel.Helpers;
using ColafHotel.Models;
using ColafHotel.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ColafHotel.Controllers;

public class RoomController(AppDbContext context, IWebHostEnvironment environment) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? searchTerm, string? roomType, decimal? minPrice, decimal? maxPrice)
    {
        var query = context.Rooms.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var trimmed = searchTerm.Trim();
            query = query.Where(room =>
                room.RoomName.Contains(trimmed) ||
                room.RoomType.Contains(trimmed) ||
                (room.Description != null && room.Description.Contains(trimmed)));
        }

        if (!string.IsNullOrWhiteSpace(roomType))
        {
            query = query.Where(room => room.RoomType == roomType);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(room => room.PricePerNight >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(room => room.PricePerNight <= maxPrice.Value);
        }

        var rooms = await query.ToListAsync();

        var viewModel = new RoomIndexViewModel
        {
            SearchTerm = searchTerm,
            RoomType = roomType,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Rooms = rooms
                .OrderBy(room => room.PricePerNight)
                .ToList()
        };

        return View(viewModel);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var room = await context.Rooms.FirstOrDefaultAsync(r => r.RoomId == id);
        if (room is null)
        {
            return NotFound();
        }

        return View(room);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new RoomFormViewModel());
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoomFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string? imagePath = null;
        try
        {
            imagePath = await FileUploadHelper.SaveImageAsync(
                model.ImageFile,
                environment.WebRootPath,
                Path.Combine("uploads", "rooms"));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
            return View(model);
        }

        var room = new Room
        {
            RoomName = model.RoomName.Trim(),
            RoomType = model.RoomType,
            Description = model.Description?.Trim(),
            PricePerNight = model.PricePerNight,
            ImagePath = imagePath,
            IsAvailable = model.IsAvailable
        };

        context.Rooms.Add(room);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Room created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var room = await context.Rooms.FindAsync(id);
        if (room is null)
        {
            return NotFound();
        }

        return View(new RoomFormViewModel
        {
            RoomId = room.RoomId,
            RoomName = room.RoomName,
            RoomType = room.RoomType,
            Description = room.Description,
            PricePerNight = room.PricePerNight,
            ExistingImagePath = room.ImagePath,
            IsAvailable = room.IsAvailable
        });
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RoomFormViewModel model)
    {
        if (id != model.RoomId)
        {
            return BadRequest();
        }

        var room = await context.Rooms.FindAsync(id);
        if (room is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.ExistingImagePath = room.ImagePath;
            return View(model);
        }

        try
        {
            room.ImagePath = await FileUploadHelper.SaveImageAsync(
                model.ImageFile,
                environment.WebRootPath,
                Path.Combine("uploads", "rooms"),
                room.ImagePath);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
            model.ExistingImagePath = room.ImagePath;
            return View(model);
        }

        room.RoomName = model.RoomName.Trim();
        room.RoomType = model.RoomType;
        room.Description = model.Description?.Trim();
        room.PricePerNight = model.PricePerNight;
        room.IsAvailable = model.IsAvailable;

        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Room updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var room = await context.Rooms.FindAsync(id);
        if (room is null)
        {
            return NotFound();
        }

        context.Rooms.Remove(room);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Room deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
