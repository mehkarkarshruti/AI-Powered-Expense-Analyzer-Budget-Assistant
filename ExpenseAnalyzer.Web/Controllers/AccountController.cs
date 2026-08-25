using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace ExpenseAnalyzer.Web.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public record AuthResponse(
        [property: JsonPropertyName("userId")] int UserId,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("token")] string Token);

    [HttpGet]
    public IActionResult Login()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Token")))
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Please enter your email and password.";
            return View();
        }

        var client = _httpClientFactory.CreateClient("Api");

        var response = await client.PostAsJsonAsync("auth/login", new { email, password });

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

        StoreAuthSession(auth!);

        return RedirectToAction("Index", "Dashboard");
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

        var client = _httpClientFactory.CreateClient("Api");

        var response = await client.PostAsJsonAsync("auth/register", new { name, email, password });

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            ViewBag.Error =
                ExtractMessage(body) ?? "Registration failed. The email may already be in use.";
            return View();
        }

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

        StoreAuthSession(auth!);

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        return RedirectToAction("Login");
    }

    private void StoreAuthSession(AuthResponse auth)
    {
        HttpContext.Session.SetString("UserId", auth.UserId.ToString());
        HttpContext.Session.SetString("UserName", auth.Name);
        HttpContext.Session.SetString("UserEmail", auth.Email);
        HttpContext.Session.SetString("Token", auth.Token);
    }

    private static string? ExtractMessage(string json)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() : null;
        }
        catch
        {
            return null;
        }
    }
}
