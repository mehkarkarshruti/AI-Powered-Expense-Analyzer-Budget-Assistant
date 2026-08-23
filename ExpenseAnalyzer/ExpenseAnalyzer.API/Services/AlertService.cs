using ExpenseAnalyzer.API.DTOs;
using ExpenseAnalyzer.API.Models;
using ExpenseAnalyzer.API.Repositories;

namespace ExpenseAnalyzer.API.Services;

public class AlertService(IAlertRepository alertRepo) : IAlertService
{
    public async Task<IEnumerable<AlertResponse>> GetUnreadAlertsAsync(int userId)
    {
        var alerts = await alertRepo.GetUnreadByUserIdAsync(userId);
        return alerts.Select(a => new AlertResponse
        {
            AlertId = a.AlertId,
            BudgetId = a.BudgetId,
            AlertType = a.AlertType.ToString(),
            Message = a.Message,
            IsRead = a.IsRead,
            CreatedAt = a.CreatedAt
        });
    }

    public async Task MarkAlertAsReadAsync(int alertId, int userId)
    {
        await alertRepo.MarkAsReadAsync(alertId, userId);
    }

    public async Task ValidateAndCreateAlertAsync(int userId, int? budgetId, AlertType type, string message)
    {
        // Avoid duplicate unread alerts for same budget cycle
        if (!await alertRepo.HasRecentUnreadAlertAsync(userId, budgetId, type))
        {
            await alertRepo.AddAsync(new Alert
            {
                UserId = userId,
                BudgetId = budgetId,
                AlertType = type,
                Message = message
            });
        }
    }
}
