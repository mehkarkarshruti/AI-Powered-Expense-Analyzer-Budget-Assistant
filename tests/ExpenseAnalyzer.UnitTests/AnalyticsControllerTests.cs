using ExpenseAnalyzer.API.Controllers;
using ExpenseAnalyzer.Core.DTOs.Analytics;
using ExpenseAnalyzer.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ExpenseAnalyzer.UnitTests;

public class AnalyticsControllerTests
{
    [Fact]
    public async Task GetMonthlySummary_InvalidUserId_ReturnsBadRequest()
    {
        // Arrange
        var mockService = new Mock<IAnalyticsService>();
        var mockLogger = new Mock<ILogger<AnalyticsController>>();
        var controller = new AnalyticsController(mockService.Object, mockLogger.Object);

        // Act
        var result = await controller.GetMonthlySummary(userId: 0, month: "2026-08");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetMonthlySummary_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        int userId = 1;
        string month = "2026-08";
        var expectedDto = new MonthlyAnalyticsDto
        {
            Month = month,
            TotalExpenses = 1500m,
            TotalTransactions = 2,
            AverageExpense = 750m
        };

        var mockService = new Mock<IAnalyticsService>();
        mockService.Setup(s => s.GetMonthlySummaryAsync(userId, month))
                   .ReturnsAsync(expectedDto);

        var mockLogger = new Mock<ILogger<AnalyticsController>>();
        var controller = new AnalyticsController(mockService.Object, mockLogger.Object);

        // Act
        var actionResult = await controller.GetMonthlySummary(userId, month);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedDto = Assert.IsType<MonthlyAnalyticsDto>(okResult.Value);
        Assert.Equal(month, returnedDto.Month);
        Assert.Equal(1500m, returnedDto.TotalExpenses);
    }

    [Fact]
    public async Task GetCategorySpendingSummary_InvalidUserId_ReturnsBadRequest()
    {
        // Arrange
        var mockService = new Mock<IAnalyticsService>();
        var mockLogger = new Mock<ILogger<AnalyticsController>>();
        var controller = new AnalyticsController(mockService.Object, mockLogger.Object);

        // Act
        var result = await controller.GetCategorySpendingSummary(userId: -5, month: "2026-08");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetCategorySpendingSummary_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        int userId = 1;
        string month = "2026-08";
        var expectedDtoList = new List<CategorySpendDto>
        {
            new CategorySpendDto { CategoryName = "Rent", TotalAmount = 3000m, Percentage = 75m },
            new CategorySpendDto { CategoryName = "Groceries", TotalAmount = 1000m, Percentage = 25m }
        };

        var mockService = new Mock<IAnalyticsService>();
        mockService.Setup(s => s.GetCategorySpendingSummaryAsync(userId, month))
                   .ReturnsAsync(expectedDtoList);

        var mockLogger = new Mock<ILogger<AnalyticsController>>();
        var controller = new AnalyticsController(mockService.Object, mockLogger.Object);

        // Act
        var actionResult = await controller.GetCategorySpendingSummary(userId, month);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedDtoList = Assert.IsType<List<CategorySpendDto>>(okResult.Value);
        Assert.Equal(2, returnedDtoList.Count);
        Assert.Equal("Rent", returnedDtoList[0].CategoryName);
    }
}
