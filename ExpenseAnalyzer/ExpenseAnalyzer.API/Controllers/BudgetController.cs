using ExpenseAnalyzer.API.DTOs;
using ExpenseAnalyzer.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseAnalyzer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BudgetController(IBudgetService budgetService) : ControllerBase
{
    private int? GetUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (idClaim != null && int.TryParse(idClaim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }

    [HttpPost]
    public async Task<IActionResult> SetBudget([FromBody] CreateBudgetRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        int? userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await budgetService.CreateOrUpdateBudgetAsync(userId.Value, request);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetBudget([FromQuery] int? categoryId, [FromQuery] byte? month, [FromQuery] short? year)
    {
        int? userId = GetUserId();
        if (userId == null) return Unauthorized();

        var targetMonth = month ?? (byte)DateTime.UtcNow.Month;
        var targetYear = year ?? (short)DateTime.UtcNow.Year;

        var budget = await budgetService.GetBudgetAsync(userId.Value, categoryId, targetMonth, targetYear);
        if (budget == null) return NotFound(new { message = "Budget not found for this criteria." });

        return Ok(budget);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllBudgets()
    {
        int? userId = GetUserId();
        if (userId == null) return Unauthorized();

        var budgets = await budgetService.GetAllBudgetsAsync(userId.Value);
        return Ok(budgets);
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetBudgetStatusAndAlerts()
    {
        int? userId = GetUserId();
        if (userId == null) return Unauthorized();

        var status = await budgetService.GetBudgetStatusAndCheckAlertsAsync(userId.Value);
        return Ok(status);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBudget(int id)
    {
        int? userId = GetUserId();
        if (userId == null) return Unauthorized();

        await budgetService.DeleteBudgetAsync(id, userId.Value);
        return NoContent();
    }
}
