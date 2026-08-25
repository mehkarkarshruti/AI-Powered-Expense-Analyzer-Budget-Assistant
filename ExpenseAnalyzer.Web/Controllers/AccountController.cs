using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseAnalyzer.Web.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Please enter your email and password.";
            return View();
        }

        try
        {
            var client = _httpClientFactory.CreateClient();

            var request = new
            {
                email,
                password
            };

            var response = await client.PostAsJsonAsync(
                "http://localhost:5001/api/Auth/login",
                request);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            var result =
                await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (result == null || string.IsNullOrEmpty(result.Token))
            {
                ViewBag.Error = "Login failed. No token received.";
                return View();
            }

            HttpContext.Session.SetString("JwtToken", result.Token);
            HttpContext.Session.SetString("UserId", result.UserId.ToString());
            HttpContext.Session.SetString("UserName", result.Name);
            HttpContext.Session.SetString("UserEmail", result.Email);

            return RedirectToAction("Index", "Dashboard");
        }
        catch
        {
            ViewBag.Error =
                "Unable to connect to the authentication server.";
            return View();
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(
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

        try
        {
            var client = _httpClientFactory.CreateClient();

            var request = new
            {
                name,
                email,
                password
            };

            var response = await client.PostAsJsonAsync(
                "http://localhost:5001/api/Auth/register",
                request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                ViewBag.Error = response.StatusCode == System.Net.HttpStatusCode.Conflict
                    ? "A user with this email already exists."
                    : "Registration failed.";

                return View();
            }

            var result =
                await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (result == null || string.IsNullOrEmpty(result.Token))
            {
                ViewBag.Error = "Registration failed. No token received.";
                return View();
            }

            HttpContext.Session.SetString("JwtToken", result.Token);
            HttpContext.Session.SetString("UserId", result.UserId.ToString());
            HttpContext.Session.SetString("UserName", result.Name);
            HttpContext.Session.SetString("UserEmail", result.Email);

            return RedirectToAction("Index", "Dashboard");
        }
        catch
        {
            ViewBag.Error =
                "Unable to connect to the authentication server.";
            return View();
        }
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Account");
    }
}

public class LoginResponse
{
    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;
}
