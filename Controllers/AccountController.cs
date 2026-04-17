using System.Security.Claims;
using ColafHotel.Data;
using ColafHotel.Helpers;
using ColafHotel.Models;
using ColafHotel.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ColafHotel.Controllers;

public class AccountController(AppDbContext context, IWebHostEnvironment environment) : Controller
{
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Profile));
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        var normalizedEmail = model.Email.Trim().ToLowerInvariant();

        if (await context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
        {
            ModelState.AddModelError(nameof(model.Email), "That email is already registered.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new User
        {
            FullName = model.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = PasswordHasher.HashPassword(model.Password),
            Role = Roles.Guest,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        await SignInUserAsync(user);
        TempData["SuccessMessage"] = "Your account has been created successfully.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
        if (user is null || !PasswordHasher.VerifyPassword(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        await SignInUserAsync(user);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return user.Role == Roles.Admin
            ? RedirectToAction("Dashboard", "Home")
            : RedirectToAction("Index", "Room");
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        return View(user);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        return View(new EditProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            ExistingProfilePicturePath = user.ProfilePicturePath
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();

        if (await context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail && u.UserId != user.UserId))
        {
            ModelState.AddModelError(nameof(model.Email), "That email is already in use.");
        }

        if (!ModelState.IsValid)
        {
            model.ExistingProfilePicturePath = user.ProfilePicturePath;
            return View(model);
        }

        try
        {
            user.ProfilePicturePath = await FileUploadHelper.SaveImageAsync(
                model.ProfilePicture,
                environment.WebRootPath,
                Path.Combine("uploads", "profiles"),
                user.ProfilePicturePath);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.ProfilePicture), ex.Message);
            model.ExistingProfilePicturePath = user.ProfilePicturePath;
            return View(model);
        }

        user.FullName = model.FullName.Trim();
        user.Email = normalizedEmail;

        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            user.PasswordHash = PasswordHasher.HashPassword(model.NewPassword);
        }

        await context.SaveChangesAsync();
        await SignInUserAsync(user);

        TempData["SuccessMessage"] = "Your profile has been updated.";
        return RedirectToAction(nameof(Profile));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private async Task<User?> GetCurrentUserAsync()
    {
        var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
        if (userId.HasValue)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);
        }

        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(claimValue, out var claimUserId))
        {
            return await context.Users.FirstOrDefaultAsync(u => u.UserId == claimUserId);
        }

        return null;
    }

    private async Task SignInUserAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });

        HttpContext.Session.SetInt32(SessionKeys.UserId, user.UserId);
        HttpContext.Session.SetString(SessionKeys.Role, user.Role);
        HttpContext.Session.SetString(SessionKeys.FullName, user.FullName);
        HttpContext.Session.SetString(SessionKeys.ProfilePicturePath, user.ProfilePicturePath ?? string.Empty);
    }
}
