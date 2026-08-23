using Microsoft.AspNetCore.Mvc;

namespace ExpenseAnalyzer.Web.Controllers;

public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Budget()
    {
        return View();
    }

    public IActionResult Analytics()
    {
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
