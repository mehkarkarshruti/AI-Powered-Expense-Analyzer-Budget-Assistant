using ExpenseAnalyzer.API.DTOs;
using ExpenseAnalyzer.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseAnalyzer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BudgetController : ControllerBase
    {
        private readonly IBudgetService _budgetService;

        public BudgetController(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        private int GetUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim != null && int.TryParse(idClaim.Value, out int userId))
            {
                return userId;
            }
            return 1; // Fallback to 1 for testing if claim is absent
        }

        [HttpPost]
        public async Task<IActionResult> SetBudget([FromBody] SetBudgetDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            int userId = GetUserId();
            var result = await _budgetService.SetBudgetAsync(userId, dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetBudget([FromQuery] byte? month, [FromQuery] short? year)
        {
            int userId = GetUserId();
            var targetMonth = month ?? (byte)DateTime.UtcNow.Month;
            var targetYear = year ?? (short)DateTime.UtcNow.Year;

            var budget = await _budgetService.GetBudgetAsync(userId, targetMonth, targetYear);
            if (budget == null) return NotFound(new { message = "Budget not found for this month." });

            return Ok(budget);
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetBudgetStatusAndAlerts()
        {
            int userId = GetUserId();
            var status = await _budgetService.GetBudgetStatusAndCheckAlertsAsync(userId);
            return Ok(status);
        }
    }
}
