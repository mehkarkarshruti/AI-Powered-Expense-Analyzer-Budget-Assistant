using Microsoft.AspNetCore.Mvc;

namespace ExpenseAnalyzer.Web.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        // Temporary frontend-only login.
        // This will later call the team's Auth API.

        if (!string.IsNullOrWhiteSpace(email) &&
            !string.IsNullOrWhiteSpace(password))
        {
            return RedirectToAction("Index", "Dashboard");
        }

        ViewBag.Error = "Please enter your email and password.";
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(
        string name,
        string email,
        string password,
        string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Please fill in all required fields.";
            return View();
        }

        if (password != confirmPassword)
        {
            ViewBag.Error = "Passwords do not match.";
            return View();
        }

        // Temporary frontend-only registration.
        // This will later call the team's Auth API.

        return RedirectToAction("Login");
    }
}
