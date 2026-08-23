using ExpenseAnalyzer.API.Controllers;
using ExpenseAnalyzer.Core.DTOs.Prediction;
using ExpenseAnalyzer.Core.Entities;
using ExpenseAnalyzer.Core.Interfaces;
using ExpenseAnalyzer.Infrastructure.Data;
using ExpenseAnalyzer.ML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ExpenseAnalyzer.Tests;

public class PredictionServiceTests
{
    private AppDbContext GetInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task PredictMonthlySpending_UserWithNoExpenses_ReturnsInsufficientDataStatus()
    {
        // Arrange
        using var dbContext = GetInMemoryDbContext(Guid.NewGuid().ToString());
        var loggerMock = new Mock<ILogger<PredictionService>>();
        var service = new PredictionService(dbContext, loggerMock.Object);

        // Act
        var result = await service.PredictMonthlySpendingAsync(userId: 99);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(99, result.UserId);
        Assert.Equal("InsufficientData", result.PredictionStatus);
        Assert.Equal(0m, result.CurrentMonthSpending);
        Assert.Equal(0.0, result.ConfidenceScore);
        Assert.True(result.IsFallback);
    }

    [Fact]
    public async Task PredictMonthlySpending_UserWithBudget_PredictsOverBudget_SetsFlagTrue()
    {
        // Arrange
        using var dbContext = GetInMemoryDbContext(Guid.NewGuid().ToString());
        string currentMonth = DateTime.UtcNow.ToString("yyyy-MM");
        int userId = 1;

        dbContext.Budgets.Add(new Budget { BudgetId = 1, UserId = userId, Month = currentMonth, Amount = 10000m });

        DateTime now = DateTime.UtcNow;
        // User has already spent 12,000 in current month (exceeds budget of 10,000)
        dbContext.Expenses.Add(new Expense { ExpenseId = 1, UserId = userId, CategoryId = 1, Amount = 12000m, Date = now.AddDays(-1), Description = "Shopping" });
        await dbContext.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<PredictionService>>();
        var service = new PredictionService(dbContext, loggerMock.Object);

        // Act
        var result = await service.PredictMonthlySpendingAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.True(result.IsBudgetLikelyToBeExceeded);
        Assert.Equal("LikelyToExceed", result.PredictionStatus);
        Assert.True(result.PredictedMonthlySpending >= 12000m);
    }

    [Fact]
    public async Task PredictMonthlySpending_UserUnderBudget_SetsFlagFalse()
    {
        // Arrange
        using var dbContext = GetInMemoryDbContext(Guid.NewGuid().ToString());
        string currentMonth = DateTime.UtcNow.ToString("yyyy-MM");
        int userId = 2;

        dbContext.Budgets.Add(new Budget { BudgetId = 2, UserId = userId, Month = currentMonth, Amount = 50000m });

        DateTime now = DateTime.UtcNow;
        dbContext.Expenses.Add(new Expense { ExpenseId = 2, UserId = userId, CategoryId = 1, Amount = 2000m, Date = now.AddDays(-2), Description = "Groceries" });
        
        // Add past month expenses
        DateTime pastMonth = now.AddMonths(-1);
        dbContext.Expenses.Add(new Expense { ExpenseId = 3, UserId = userId, CategoryId = 1, Amount = 5000m, Date = pastMonth, Description = "Past Rent" });
        dbContext.Expenses.Add(new Expense { ExpenseId = 4, UserId = userId, CategoryId = 2, Amount = 3000m, Date = pastMonth, Description = "Past Utilities" });
        await dbContext.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<PredictionService>>();
        var service = new PredictionService(dbContext, loggerMock.Object);

        // Act
        var result = await service.PredictMonthlySpendingAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsBudgetLikelyToBeExceeded);
        Assert.Equal("UnderBudget", result.PredictionStatus);
    }

    [Fact]
    public async Task PredictionController_InvalidUserId_ReturnsBadRequest()
    {
        // Arrange
        var mockEngine = new Mock<IPredictionEngine>();
        var mockLogger = new Mock<ILogger<PredictionController>>();
        var controller = new PredictionController(mockEngine.Object, mockLogger.Object);

        // Act
        var result = await controller.GetMonthlyPrediction(0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task PredictionController_ValidUserId_ReturnsOkResult()
    {
        // Arrange
        var expectedDto = new SpendingPredictionDto
        {
            UserId = 1,
            CurrentMonth = "2026-08",
            HistoricalAverage = 10000m,
            CurrentMonthSpending = 4000m,
            PredictedMonthlySpending = 9500m,
            MonthlyBudget = 12000m,
            RemainingBudget = 2500m,
            PredictionStatus = "UnderBudget",
            ConfidenceScore = 0.85,
            IsBudgetLikelyToBeExceeded = false,
            Message = "Good job!",
            IsFallback = false
        };

        var mockEngine = new Mock<IPredictionEngine>();
        mockEngine.Setup(e => e.PredictMonthlySpendingAsync(1))
                  .ReturnsAsync(expectedDto);

        var mockLogger = new Mock<ILogger<PredictionController>>();
        var controller = new PredictionController(mockEngine.Object, mockLogger.Object);

        // Act
        var actionResult = await controller.GetMonthlyPrediction(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedDto = Assert.IsType<SpendingPredictionDto>(okResult.Value);
        Assert.Equal(1, returnedDto.UserId);
        Assert.Equal(9500m, returnedDto.PredictedMonthlySpending);
    }
}
