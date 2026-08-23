namespace ExpenseAnalyzer.Core.DTOs.Analytics;

/// <summary>
/// DTO representing monthly spending analytics including total expenses, transaction counts, and averages.
/// </summary>
public class MonthlyAnalyticsDto
{
    /// <summary>
    /// The target month (Format: YYYY-MM, e.g. 2026-08).
    /// </summary>
    public string Month { get; set; } = string.Empty;

    /// <summary>
    /// The total expense amount spent during the month.
    /// </summary>
    public decimal TotalExpenses { get; set; }

    /// <summary>
    /// The number of expense transaction records during the month.
    /// </summary>
    public int TotalTransactions { get; set; }

    /// <summary>
    /// The average expense amount spent per transaction during the month.
    /// </summary>
    public decimal AverageExpense { get; set; }
}
