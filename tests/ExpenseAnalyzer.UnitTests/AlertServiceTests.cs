using ExpenseAnalyzer.API.Models;
using ExpenseAnalyzer.API.Repositories;
using ExpenseAnalyzer.API.Services;
using NSubstitute;
using Xunit;

namespace ExpenseAnalyzer.UnitTests;

public class AlertServiceTests
{
    private readonly IAlertRepository _alertRepo;
    private readonly AlertService _sut;

    public AlertServiceTests()
    {
        _alertRepo = Substitute.For<IAlertRepository>();
        _sut = new AlertService(_alertRepo);
    }

    [Fact]
    public async Task ValidateAndCreateAlertAsync_ShouldNotAddAlert_WhenRecentUnreadAlertOfSameTypeExists()
    {
        // Arrange
        int userId = 1;
        int budgetId = 10;
        var type = AlertType.WARNING_80;

        _alertRepo.HasRecentUnreadAlertAsync(userId, budgetId, type).Returns(true);

        // Act
        await _sut.ValidateAndCreateAlertAsync(userId, budgetId, type, "Test message");

        // Assert - verify AddAsync is NOT called
        await _alertRepo.DidNotReceive().AddAsync(Arg.Any<Alert>());
    }

    [Fact]
    public async Task ValidateAndCreateAlertAsync_ShouldAddAlert_WhenNoRecentUnreadAlertExists()
    {
        // Arrange
        int userId = 1;
        int budgetId = 10;
        var type = AlertType.WARNING_80;

        _alertRepo.HasRecentUnreadAlertAsync(userId, budgetId, type).Returns(false);

        // Act
        await _sut.ValidateAndCreateAlertAsync(userId, budgetId, type, "Test message");

        // Assert - verify AddAsync IS called
        await _alertRepo.Received(1).AddAsync(Arg.Is<Alert>(a => 
            a.UserId == userId && 
            a.BudgetId == budgetId && 
            a.AlertType == type && 
            a.Message == "Test message"
        ));
    }
}
