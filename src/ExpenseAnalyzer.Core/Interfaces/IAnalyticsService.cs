using ExpenseAnalyzer.Core.DTOs.Analytics;

namespace ExpenseAnalyzer.Core.Interfaces;

/// <summary>
/// Service interface exposing core analytics functionality.
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Computes the monthly spending summary for a specified user and month.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="month">The month in YYYY-MM format.</param>
    /// <returns>A MonthlyAnalyticsDto with totals and averages.</returns>
    Task<MonthlyAnalyticsDto> GetMonthlySummaryAsync(int userId, string month);

    /// <summary>
    /// Computes category-wise spending distribution for a specified user and month.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="month">The month in YYYY-MM format.</param>
    /// <returns>A list of CategorySpendDto representing category breakdowns.</returns>
    Task<List<CategorySpendDto>> GetCategorySpendingSummaryAsync(int userId, string month);
}
