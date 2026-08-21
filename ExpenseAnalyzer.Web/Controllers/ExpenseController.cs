using Microsoft.AspNetCore.Mvc;

namespace ExpenseAnalyzer.Web.Controllers;

public class ExpenseController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
