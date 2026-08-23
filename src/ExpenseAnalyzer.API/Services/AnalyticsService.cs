using ExpenseAnalyzer.Core.DTOs.Analytics;
using ExpenseAnalyzer.Core.Interfaces;
using ExpenseAnalyzer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.API.Services;

/// <summary>
/// Service implementation providing expense analytics calculations.
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly AppDbContext _dbContext;

    public AnalyticsService(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc />
    public async Task<MonthlyAnalyticsDto> GetMonthlySummaryAsync(int userId, string month)
    {
        if (!DateTime.TryParseExact(month, "yyyy-MM", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedMonth))
        {
            parsedMonth = DateTime.UtcNow;
        }

        int year = parsedMonth.Year;
        int selectMonth = parsedMonth.Month;

        var expensesQuery = _dbContext.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Date.Year == year && e.Date.Month == selectMonth);

        decimal totalExpenses = 0m;
        int totalTransactions = 0;
        decimal averageExpense = 0m;

        if (await expensesQuery.AnyAsync())
        {
            totalExpenses = await expensesQuery.SumAsync(e => e.Amount);
            totalTransactions = await expensesQuery.CountAsync();
            if (totalTransactions > 0)
            {
                averageExpense = totalExpenses / totalTransactions;
            }
        }

        return new MonthlyAnalyticsDto
        {
            Month = parsedMonth.ToString("yyyy-MM"),
            TotalExpenses = Math.Round(totalExpenses, 2),
            TotalTransactions = totalTransactions,
            AverageExpense = Math.Round(averageExpense, 2)
        };
    }

    /// <inheritdoc />
    public async Task<List<CategorySpendDto>> GetCategorySpendingSummaryAsync(int userId, string month)
    {
        if (!DateTime.TryParseExact(month, "yyyy-MM", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedMonth))
        {
            parsedMonth = DateTime.UtcNow;
        }

        int year = parsedMonth.Year;
        int selectMonth = parsedMonth.Month;

        var expenses = await _dbContext.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Date.Year == year && e.Date.Month == selectMonth)
            .Include(e => e.Category)
            .ToListAsync();

        if (expenses.Count == 0)
        {
            return new List<CategorySpendDto>();
        }

        decimal totalMonthSpend = expenses.Sum(e => e.Amount);

        var categorySpendingList = expenses
            .GroupBy(e => e.Category?.Name ?? "General")
            .Select(g => {
                decimal categoryTotal = g.Sum(e => e.Amount);
                return new CategorySpendDto
                {
                    CategoryName = g.Key,
                    TotalAmount = Math.Round(categoryTotal, 2),
                    Percentage = totalMonthSpend > 0 
                        ? Math.Round((categoryTotal / totalMonthSpend) * 100m, 2) 
                        : 0m
                };
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();

        return categorySpendingList;
    }
}
