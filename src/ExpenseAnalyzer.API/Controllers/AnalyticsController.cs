using ExpenseAnalyzer.Core.DTOs.Analytics;
using ExpenseAnalyzer.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExpenseAnalyzer.API.Controllers;

/// <summary>
/// Web API Controller providing HTTP REST endpoints for retrieving expense analytics,
/// including monthly spending summaries and category-wise spending distributions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves monthly spending details including totals, averages, and transaction counts.
    /// </summary>
    /// <param name="userId">The unique identifier of the user (must be a positive integer).</param>
    /// <param name="month">Optional target month in YYYY-MM format. Defaults to current month if not specified.</param>
    /// <returns>MonthlyAnalyticsDto representing the month's spending status.</returns>
    [HttpGet("monthly")]
    [ProducesResponseType(typeof(MonthlyAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MonthlyAnalyticsDto>> GetMonthlySummary([FromQuery] int userId, [FromQuery] string? month)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("Invalid userId parameter provided to /api/analytics/monthly: {UserId}", userId);
            return BadRequest(new { message = "UserId must be a positive integer." });
        }

        try
        {
            // Default to current month string if not provided
            string targetMonth = string.IsNullOrWhiteSpace(month) ? DateTime.UtcNow.ToString("yyyy-MM") : month;
            
            var result = await _analyticsService.GetMonthlySummaryAsync(userId, targetMonth);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred retrieving monthly summary for UserId {UserId} and Month {Month}", userId, month);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while generating monthly spending summary." });
        }
    }

    /// <summary>
    /// Retrieves category-wise spending totals and percentages for the user and month.
    /// </summary>
    /// <param name="userId">The unique identifier of the user (must be a positive integer).</param>
    /// <param name="month">Optional target month in YYYY-MM format. Defaults to current month if not specified.</param>
    /// <returns>A list of CategorySpendDto representing the categorical breakdown.</returns>
    [HttpGet("category")]
    [ProducesResponseType(typeof(List<CategorySpendDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<CategorySpendDto>>> GetCategorySpendingSummary([FromQuery] int userId, [FromQuery] string? month)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("Invalid userId parameter provided to /api/analytics/category: {UserId}", userId);
            return BadRequest(new { message = "UserId must be a positive integer." });
        }

        try
        {
            string targetMonth = string.IsNullOrWhiteSpace(month) ? DateTime.UtcNow.ToString("yyyy-MM") : month;

            var result = await _analyticsService.GetCategorySpendingSummaryAsync(userId, targetMonth);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred retrieving category summary for UserId {UserId} and Month {Month}", userId, month);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while generating category spending summary." });
        }
    }
}
