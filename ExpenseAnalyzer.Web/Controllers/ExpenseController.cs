using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace ExpenseAnalyzer.Web.Controllers;

[Route("Expense")]
public class ExpenseController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ExpenseController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public record ExpenseDto(
        [property: JsonPropertyName("expenseId")] int ExpenseId,
        [property: JsonPropertyName("userId")] int UserId,
        [property: JsonPropertyName("categoryId")] int CategoryId,
        [property: JsonPropertyName("categoryName")] string CategoryName,
        [property: JsonPropertyName("amount")] decimal Amount,
        [property: JsonPropertyName("expenseDate")] string ExpenseDate,
        [property: JsonPropertyName("description")] string? Description);

    public record CategoryDto(
        [property: JsonPropertyName("categoryId")] int CategoryId,
        [property: JsonPropertyName("name")] string Name);

    public sealed class ExpenseView
    {
        public int id { get; set; }
        public string date { get; set; } = "";
        public string category { get; set; } = "";
        public int categoryId { get; set; }
        public string description { get; set; } = "";
        public decimal amount { get; set; }
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        if (!IsAuthenticated())
        {
            return RedirectToAction("Login", "Account");
        }

        return View();
    }

    [HttpGet("List")]
    public async Task<IActionResult> List()
    {
        var client = ApiClient();
        if (client is null)
        {
            return Unauthorized();
        }

        var response = await client.GetAsync("expenses");

        if (HandleAuthFailure(response))
        {
            return Unauthorized();
        }

        var expenses = await response.Content.ReadFromJsonAsync<List<ExpenseDto>>() ?? new();

        return Json(expenses.Select(ToView));
    }

    [HttpGet("Categories")]
    public async Task<IActionResult> Categories()
    {
        var client = ApiClient();
        if (client is null)
        {
            return Unauthorized();
        }

        var categories = await client.GetFromJsonAsync<List<CategoryDto>>("categories") ?? new();

        return Json(categories);
    }

    public sealed class ExpenseInput
    {
        public int categoryId { get; set; }
        public decimal amount { get; set; }
        public string date { get; set; } = "";
        public string? description { get; set; }
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] ExpenseInput input)
    {
        var client = ApiClient();
        if (client is null)
        {
            return Unauthorized();
        }

        if (input.amount <= 0 || input.categoryId <= 0 || string.IsNullOrWhiteSpace(input.date))
        {
            return BadRequest(new { message = "Amount, category and date are required." });
        }

        var response = await client.PostAsJsonAsync("expenses", new
        {
            categoryId = input.categoryId,
            amount = input.amount,
            expenseDate = input.date,
            description = input.description
        });

        if (HandleAuthFailure(response))
        {
            return Unauthorized();
        }

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode,
                new { message = await response.Content.ReadAsStringAsync() });
        }

        var created = await response.Content.ReadFromJsonAsync<ExpenseDto>();

        return Created("", ToView(created!));
    }

    [HttpPut("Update/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ExpenseInput input)
    {
        var client = ApiClient();
        if (client is null)
        {
            return Unauthorized();
        }

        var response = await client.PutAsJsonAsync($"expenses/{id}", new
        {
            categoryId = input.categoryId,
            amount = input.amount,
            expenseDate = input.date,
            description = input.description
        });

        if (HandleAuthFailure(response))
        {
            return Unauthorized();
        }

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode,
                new { message = await response.Content.ReadAsStringAsync() });
        }

        var updated = await response.Content.ReadFromJsonAsync<ExpenseDto>();

        return Json(ToView(updated!));
    }

    [HttpDelete("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var client = ApiClient();
        if (client is null)
        {
            return Unauthorized();
        }

        var response = await client.DeleteAsync($"expenses/{id}");

        if (HandleAuthFailure(response))
        {
            return Unauthorized();
        }

        if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
        {
            return StatusCode((int)response.StatusCode);
        }

        return NoContent();
    }

    private bool IsAuthenticated()
    {
        return !string.IsNullOrEmpty(HttpContext.Session.GetString("Token"));
    }

    private HttpClient? ApiClient()
    {
        var token = HttpContext.Session.GetString("Token");

        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        var client = _httpClientFactory.CreateClient("Api");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private bool HandleAuthFailure(HttpResponseMessage response)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            HttpContext.Session.Clear();
            return true;
        }

        return false;
    }

    private static ExpenseView ToView(ExpenseDto e)
    {
        return new ExpenseView
        {
            id = e.ExpenseId,
            date = e.ExpenseDate,
            category = e.CategoryName,
            categoryId = e.CategoryId,
            description = e.Description ?? "",
            amount = e.Amount
        };
    }
}
