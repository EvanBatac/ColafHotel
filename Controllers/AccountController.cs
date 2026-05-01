using Microsoft.AspNetCore.Mvc;

namespace ColafHotel.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Profile));
        }

        return View();
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    public IActionResult Profile()
    {
        return View();
    }

    [HttpPost]
    public IActionResult EditProfile()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Logout()
    {
        return RedirectToAction("Index", "Home");
    }
}