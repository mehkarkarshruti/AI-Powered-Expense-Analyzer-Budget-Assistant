using ExpenseAnalyzer.API.Models;
using ExpenseAnalyzer.API.Repositories;
using ExpenseAnalyzer.API.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace ExpenseAnalyzer.UnitTests;

public class BudgetServiceTests
{
    private readonly IBudgetRepository _budgetRepo;
    private readonly IExpenseRepository _expenseRepo;
    private readonly IAlertService _alertService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BudgetService> _logger;
    private readonly BudgetService _sut; // System Under Test

    public BudgetServiceTests()
    {
        _budgetRepo = Substitute.For<IBudgetRepository>();
        _expenseRepo = Substitute.For<IExpenseRepository>();
        _alertService = Substitute.For<IAlertService>();
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _logger = Substitute.For<ILogger<BudgetService>>();

        _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient());

        _sut = new BudgetService(
            _budgetRepo,
            _expenseRepo,
            _alertService,
            _httpClientFactory,
            _logger
        );
    }

    [Fact]
    public async Task CheckBudgetThresholdsAsync_ShouldNotTriggerAlert_WhenBelow80Percent()
    {
        // Arrange
        int userId = 1;
        var budget = new Budget { BudgetId = 10, BudgetAmount = 1000m };
        _budgetRepo.GetBudgetAsync(userId, null, Arg.Any<byte>(), Arg.Any<short>())
            .Returns(budget);

        _expenseRepo.GetTotalSpentAsync(userId, null, Arg.Any<int>(), Arg.Any<int>())
            .Returns(500m); // 50%

        // Act
        await _sut.CheckBudgetThresholdsAsync(userId, null, DateOnly.FromDateTime(DateTime.Now));

        // Assert
        await _alertService.DidNotReceive().ValidateAndCreateAlertAsync(
            Arg.Any<int>(), Arg.Any<int?>(), Arg.Any<AlertType>(), Arg.Any<string>());
    }

    [Fact]
    public async Task CheckBudgetThresholdsAsync_ShouldTriggerWARNING80_WhenUtilizationIsBetween80And99Percent()
    {
        // Arrange
        int userId = 1;
        var budget = new Budget { BudgetId = 10, BudgetAmount = 1000m };
        _budgetRepo.GetBudgetAsync(userId, null, Arg.Any<byte>(), Arg.Any<short>())
            .Returns(budget);

        _expenseRepo.GetTotalSpentAsync(userId, null, Arg.Any<int>(), Arg.Any<int>())
            .Returns(850m); // 85%

        // Act
        await _sut.CheckBudgetThresholdsAsync(userId, null, DateOnly.FromDateTime(DateTime.Now));

        // Assert
        await _alertService.Received(1).ValidateAndCreateAlertAsync(
            userId,
            budget.BudgetId,
            AlertType.WARNING_80,
            Arg.Is<string>(msg => msg.Contains("Warning: You have utilized 85.00%"))
        );
    }

    [Fact]
    public async Task CheckBudgetThresholdsAsync_ShouldTriggerEXCEEDED100_WhenUtilizationIs100PercentOrMore()
    {
        // Arrange
        int userId = 1;
        var budget = new Budget { BudgetId = 10, BudgetAmount = 1000m };
        _budgetRepo.GetBudgetAsync(userId, null, Arg.Any<byte>(), Arg.Any<short>())
            .Returns(budget);

        _expenseRepo.GetTotalSpentAsync(userId, null, Arg.Any<int>(), Arg.Any<int>())
            .Returns(1050m); // 105%

        // Act
        await _sut.CheckBudgetThresholdsAsync(userId, null, DateOnly.FromDateTime(DateTime.Now));

        // Assert
        await _alertService.Received(1).ValidateAndCreateAlertAsync(
            userId,
            budget.BudgetId,
            AlertType.EXCEEDED_100,
            Arg.Is<string>(msg => msg.Contains("Critical: You have breached your monthly overall budget"))
        );
    }
}
