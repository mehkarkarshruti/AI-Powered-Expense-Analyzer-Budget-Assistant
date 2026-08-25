using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Globalization;
using System.Text.Json.Serialization;

namespace ExpenseAnalyzer.Web.Controllers;

[Route("Dashboard")]
public class DashboardController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DashboardController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public sealed class ExpenseDto
    {
        [JsonPropertyName("expenseId")] public int ExpenseId { get; set; }
        [JsonPropertyName("amount")] public decimal Amount { get; set; }
        [JsonPropertyName("categoryName")] public string CategoryName { get; set; } = "";
        [JsonPropertyName("expenseDate")] public string ExpenseDate { get; set; } = "";
        [JsonPropertyName("description")] public string? Description { get; set; }
    }

    private sealed class PredictionDto
    {
        [JsonPropertyName("predictedMonthlySpending")] public decimal PredictedMonthlySpending { get; set; }
        [JsonPropertyName("monthlyBudget")] public decimal? MonthlyBudget { get; set; }
        [JsonPropertyName("isBudgetLikelyToBeExceeded")] public bool IsBudgetLikelyToBeExceeded { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
    }

    private sealed class StatusDto
    {
        [JsonPropertyName("activeAlerts")] public List<string> ActiveAlerts { get; set; } = new();
        [JsonPropertyName("prediction")] public PredictionDto? Prediction { get; set; }
    }

    public sealed class DashboardData
    {
        public List<ExpenseDto> Expenses { get; set; } = new();
        public decimal TotalSpent { get; set; }
        public int Count { get; set; }
        public decimal Budget { get; set; }
        public decimal DailyAverage { get; set; }
        public string TopCategory { get; set; } = "-";
        public List<(string Name, decimal Total)> CategoryTotals { get; set; } = new();
        public List<string> Alerts { get; set; } = new();
        public decimal? PredictedSpending { get; set; }
        public bool PredictionExceedsBudget { get; set; }
        public string? PredictionMessage { get; set; }

        public string Inr(decimal value) =>
            "₹" + value.ToString("N0", CultureInfo.InvariantCulture);

        public string MonthLabel =>
            DateTime.UtcNow.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
    }

    private bool IsAuthenticated()
    {
        return !string.IsNullOrEmpty(HttpContext.Session.GetString("Token"));
    }

    private HttpClient? ApiClient()
    {
        var token = HttpContext.Session.GetString("Token");

        if (string.IsNullOrEmpty(token)) return null;

        var client = _httpClientFactory.CreateClient("Api");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private async Task<DashboardData> LoadDataAsync()
    {
        var data = new DashboardData();

        var client = ApiClient();

        if (client is null) return data;

        data.Expenses = await client.GetFromJsonAsync<List<ExpenseDto>>("expenses")
                        ?? new List<ExpenseDto>();

        data.TotalSpent = data.Expenses.Sum(e => e.Amount);
        data.Count = data.Expenses.Count;
        data.TopCategory = data.Expenses
            .GroupBy(e => e.CategoryName)
            .OrderByDescending(g => g.Sum(e => e.Amount))
            .FirstOrDefault()?.Key ?? "-";

        var daysElapsed = DateTime.UtcNow.Day;
        data.DailyAverage = daysElapsed > 0
            ? Math.Round(data.TotalSpent / daysElapsed, 2)
            : 0;

        data.CategoryTotals = data.Expenses
            .GroupBy(e => e.CategoryName)
            .Select(g => (g.Key, g.Sum(e => e.Amount)))
            .OrderByDescending(t => t.Item2)
            .ToList();

        var budgetResponse = await client.GetAsync("budget");

        if (budgetResponse.IsSuccessStatusCode)
        {
            try
            {
                using var doc = await System.Text.Json.JsonDocument.ParseAsync(
                    await budgetResponse.Content.ReadAsStreamAsync());

                if (doc.RootElement.TryGetProperty("budgetAmount", out var ba) &&
                    ba.TryGetDecimal(out var amt))
                {
                    data.Budget = amt;
                }
            }
            catch
            {
                // leave default 0 -> view shows an unset-budget state
            }
        }

        var statusResponse = await client.GetAsync("budget/status");

        if (statusResponse.IsSuccessStatusCode)
        {
            try
            {
                var status = await statusResponse.Content.ReadFromJsonAsync<StatusDto>();
                data.Alerts = status?.ActiveAlerts ?? new List<string>();

                if (status?.Prediction is not null)
                {
                    data.PredictedSpending = status.Prediction.PredictedMonthlySpending;
                    data.PredictionExceedsBudget = status.Prediction.IsBudgetLikelyToBeExceeded;
                    data.PredictionMessage = status.Prediction.Message;
                }
            }
            catch
            {
                // alerts/prediction stay empty -> views show zero states
            }
        }

        return data;
    }

    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");

        var d = await LoadDataAsync();

        ViewBag.TotalSpent = d.Inr(d.TotalSpent);
        ViewBag.TotalSpentRaw = d.TotalSpent;
        ViewBag.BudgetAmount = d.Inr(d.Budget);
        ViewBag.BudgetRaw = d.Budget;
        ViewBag.Remaining = d.Inr(Math.Max(d.Budget - d.TotalSpent, 0));
        ViewBag.DailyAverage = d.Inr(d.DailyAverage);
        ViewBag.TransactionCount = d.Count;
        ViewBag.TopCategory = d.TopCategory;
        ViewBag.Breakdown = d.CategoryTotals
            .Select(t => (Name: t.Name,
                          Amount: d.Inr(t.Total),
                          Pct: d.TotalSpent > 0 ? (int)Math.Round(t.Total / d.TotalSpent * 100) : 0))
            .ToList();
        ViewBag.HasPrediction = d.PredictedSpending.HasValue;
        ViewBag.Predicted = d.PredictedSpending.HasValue ? d.Inr(d.PredictedSpending.Value) : "—";
        ViewBag.PredictionDifference = d.PredictedSpending.HasValue
            ? d.Inr(Math.Abs(d.PredictedSpending.Value - d.Budget))
            : "—";
        ViewBag.PredictionExceeds = d.PredictionExceedsBudget;

        return View();
    }

    [HttpGet("Budget")]
    public async Task<IActionResult> Budget()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");

        var d = await LoadDataAsync();

        ViewBag.BudgetFormatted = d.Inr(d.Budget);
        ViewBag.SpentFormatted = d.Inr(d.TotalSpent);
        ViewBag.SpentRaw = d.TotalSpent;
        ViewBag.BudgetRaw = d.Budget;
        ViewBag.RemainingFormatted = d.Inr(Math.Max(d.Budget - d.TotalSpent, 0));
        ViewBag.MonthLabel = d.MonthLabel;
        var percentage = d.Budget > 0 ? (int)Math.Round(d.TotalSpent / d.Budget * 100) : 0;
        ViewBag.Percentage = percentage;
        ViewBag.ProgressClass = percentage >= 100 ? "danger-alert"
            : percentage >= 80 ? "warning-alert" : "success-alert";

        return View();
    }

    [HttpGet("Analytics")]
    public async Task<IActionResult> Analytics()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");

        var d = await LoadDataAsync();

        var daysInMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
        var perDay = d.Expenses
            .Where(e => System.DateOnly.TryParse(e.ExpenseDate, out var _) &&
                        System.DateOnly.Parse(e.ExpenseDate).Year == DateTime.UtcNow.Year &&
                        System.DateOnly.Parse(e.ExpenseDate).Month == DateTime.UtcNow.Month)
            .GroupBy(e => System.DateOnly.Parse(e.ExpenseDate).Day)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

        var max = perDay.Values.DefaultIfEmpty(0).Max();

        ViewBag.DayBars = Enumerable.Range(1, daysInMonth)
            .Select(day => (Day: day,
                            Total: perDay.TryGetValue(day, out var t) ? t : 0m,
                            Height: max > 0 && perDay.TryGetValue(day, out var t2)
                                ? (int)Math.Round(t2 / max * 100)
                                : 0))
            .ToList();

        ViewBag.CategoryTotals = d.CategoryTotals
            .Select(t => (Name: t.Name,
                          Amount: d.Inr(t.Total),
                          Pct: d.TotalSpent > 0 ? (int)Math.Round(t.Total / d.TotalSpent * 100) : 0))
            .ToList();

        ViewBag.HasExpenses = d.Count > 0;
        ViewBag.MonthLabel = d.MonthLabel;

        return View();
    }

    [HttpGet("Prediction")]
    public async Task<IActionResult> Prediction()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");

        var d = await LoadDataAsync();

        ViewBag.CurrentSpent = d.Inr(d.TotalSpent);
        ViewBag.BudgetFormatted = d.Inr(d.Budget);
        ViewBag.HasPrediction = d.PredictedSpending.HasValue;
        ViewBag.Predicted = d.PredictedSpending.HasValue
            ? d.Inr(d.PredictedSpending.Value)
            : "—";
        ViewBag.PredictionMessage = d.PredictionMessage
            ?? "No prediction is available yet.";
        ViewBag.Exceeds = d.PredictionExceedsBudget;

        return View();
    }

    [HttpGet("Alerts")]
    public async Task<IActionResult> Alerts()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");

        var d = await LoadDataAsync();

        ViewBag.Alerts = d.Alerts;
        ViewBag.HasExpenses = d.Count > 0;
        ViewBag.MonthLabel = d.MonthLabel;
        ViewBag.PredictionWarning = d.PredictedSpending.HasValue && d.PredictionExceedsBudget
            ? $"Your predicted monthly spending is {d.Inr(d.PredictedSpending.Value)}, which is above your budget."
            : null;

        return View();
    }

    [HttpGet("Settings")]
    public async Task<IActionResult> Settings()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");

        var client = ApiClient();

        ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? "User";
        ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail") ?? "";
        ViewBag.BudgetAmount = client is null ? 0m : await LoadBudgetValue(client);

        return View();
    }

    [HttpPost("SetBudget")]
    public async Task<IActionResult> SetBudget([FromBody] BudgetInput input)
    {
        var client = ApiClient();
        if (client is null) return Unauthorized();

        if (input.budgetAmount <= 0)
        {
            return BadRequest(new { message = "Budget must be positive." });
        }

        var response = await client.PostAsJsonAsync("budget", new
        {
            categoryId = (int?)null,
            month = DateTime.UtcNow.Month,
            year = DateTime.UtcNow.Year,
            budgetAmount = input.budgetAmount
        });

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode,
                new { message = await response.Content.ReadAsStringAsync() });
        }

        return Ok(new { saved = true, budgetAmount = input.budgetAmount });
    }

    public sealed class BudgetInput { public decimal budgetAmount { get; set; } }

    private static async Task<decimal> LoadBudgetValue(HttpClient client)
    {
        try
        {
            var response = await client.GetAsync("budget");

            if (!response.IsSuccessStatusCode) return 0m;

            using var doc = await System.Text.Json.JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync());

            return doc.RootElement.TryGetProperty("budgetAmount", out var ba) &&
                   ba.TryGetDecimal(out var amt)
                ? amt
                : 0m;
        }
        catch
        {
            return 0m;
        }
    }

}
