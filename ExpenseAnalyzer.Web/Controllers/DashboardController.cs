using ExpenseAnalyzer.Web.Models.Analytics;
using ExpenseAnalyzer.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseAnalyzer.Web.Controllers;

public class DashboardController : Controller
{
    private readonly ApiService _apiService;

    public DashboardController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        return View();
    }

    public async Task<IActionResult> Budget()
    {
        return View();
    }

    public async Task<IActionResult> Analytics()
    {
        try
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                userId = "1";
            }

            var monthly = await _apiService.GetAsync<MonthlyAnalytics>(
                $"/api/Analytics/monthly?userId={userId}");

            var categories = await _apiService.GetAsync<List<CategorySpend>>(
                $"/api/Analytics/category?userId={userId}");

            ViewBag.Monthly = monthly;
            ViewBag.Categories = categories;
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Unable to load analytics data.";
            ViewBag.Monthly = null;
            ViewBag.Categories = null;
        }

        return View();
    }

    public IActionResult Prediction()
    {
        return View();
    }

    public IActionResult Alerts()
    {
        return View();
    }

    public IActionResult Settings()
    {
        return View();
    }
}
