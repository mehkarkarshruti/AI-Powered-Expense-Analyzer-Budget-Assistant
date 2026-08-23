using ExpenseAnalyzer.API.Services;
using ExpenseAnalyzer.Core.Entities;
using ExpenseAnalyzer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ExpenseAnalyzer.UnitTests;

public class AnalyticsServiceTests
{
    private AppDbContext GetInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_EmptyExpenses_ReturnsZeroSummary()
    {
        // Arrange
        using var dbContext = GetInMemoryDbContext(Guid.NewGuid().ToString());
        var service = new AnalyticsService(dbContext);
        int userId = 1;
        string month = "2026-08";

        // Act
        var result = await service.GetMonthlySummaryAsync(userId, month);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(month, result.Month);
        Assert.Equal(0m, result.TotalExpenses);
        Assert.Equal(0, result.TotalTransactions);
        Assert.Equal(0m, result.AverageExpense);
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_MultipleExpensesSameMonth_ReturnsCorrectSummary()
    {
        // Arrange
        using var dbContext = GetInMemoryDbContext(Guid.NewGuid().ToString());
        int userId = 1;
        string month = "2026-08";
        var date = new DateTime(2026, 8, 15);

        dbContext.Expenses.Add(new Expense { ExpenseId = 1, UserId = userId, CategoryId = 1, Amount = 1000m, Date = date });
        dbContext.Expenses.Add(new Expense { ExpenseId = 2, UserId = userId, CategoryId = 2, Amount = 500m, Date = date.AddDays(2) });
        // Add expense for a different month/user to ensure filtering works
        dbContext.Expenses.Add(new Expense { ExpenseId = 3, UserId = userId, CategoryId = 1, Amount = 2000m, Date = new DateTime(2026, 9, 1) });
        dbContext.Expenses.Add(new Expense { ExpenseId = 4, UserId = 2, CategoryId = 1, Amount = 9999m, Date = date });
        
        await dbContext.SaveChangesAsync();

        var service = new AnalyticsService(dbContext);

        // Act
        var result = await service.GetMonthlySummaryAsync(userId, month);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(month, result.Month);
        Assert.Equal(1500m, result.TotalExpenses);
        Assert.Equal(2, result.TotalTransactions);
        Assert.Equal(750m, result.AverageExpense);
    }

    [Fact]
    public async Task GetCategorySpendingSummaryAsync_EmptyExpenses_ReturnsEmptyList()
    {
        // Arrange
        using var dbContext = GetInMemoryDbContext(Guid.NewGuid().ToString());
        var service = new AnalyticsService(dbContext);
        int userId = 1;
        string month = "2026-08";

        // Act
        var result = await service.GetCategorySpendingSummaryAsync(userId, month);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCategorySpendingSummaryAsync_MultipleCategories_CalculatesCorrectPercentages()
    {
        // Arrange
        using var dbContext = GetInMemoryDbContext(Guid.NewGuid().ToString());
        int userId = 1;
        string month = "2026-08";
        var date = new DateTime(2026, 8, 10);

        var rentCategory = new Category { CategoryId = 1, Name = "Rent" };
        var foodCategory = new Category { CategoryId = 2, Name = "Groceries" };
        dbContext.Categories.AddRange(rentCategory, foodCategory);

        dbContext.Expenses.Add(new Expense { ExpenseId = 1, UserId = userId, CategoryId = 1, Amount = 3000m, Date = date, Category = rentCategory });
        dbContext.Expenses.Add(new Expense { ExpenseId = 2, UserId = userId, CategoryId = 2, Amount = 1000m, Date = date.AddDays(1), Category = foodCategory });
        
        await dbContext.SaveChangesAsync();

        var service = new AnalyticsService(dbContext);

        // Act
        var result = await service.GetCategorySpendingSummaryAsync(userId, month);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        // Rent (Highest spending first, due to OrderByDescending)
        Assert.Equal("Rent", result[0].CategoryName);
        Assert.Equal(3000m, result[0].TotalAmount);
        Assert.Equal(75m, result[0].Percentage);

        // Groceries
        Assert.Equal("Groceries", result[1].CategoryName);
        Assert.Equal(1000m, result[1].TotalAmount);
        Assert.Equal(25m, result[1].Percentage);
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_InvalidMonthFormat_FallsBackToCurrentMonth()
    {
        // Arrange
        using var dbContext = GetInMemoryDbContext(Guid.NewGuid().ToString());
        int userId = 1;
        var date = DateTime.UtcNow;

        dbContext.Expenses.Add(new Expense { ExpenseId = 1, UserId = userId, CategoryId = 1, Amount = 1200m, Date = date });
        await dbContext.SaveChangesAsync();

        var service = new AnalyticsService(dbContext);

        // Act
        var result = await service.GetMonthlySummaryAsync(userId, "invalid-format");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(date.ToString("yyyy-MM"), result.Month);
        Assert.Equal(1200m, result.TotalExpenses);
        Assert.Equal(1, result.TotalTransactions);
    }
}
