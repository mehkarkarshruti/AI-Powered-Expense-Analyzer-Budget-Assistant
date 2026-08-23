using ExpenseAnalyzer.API.DTOs;
using ExpenseAnalyzer.API.Models;

namespace ExpenseAnalyzer.API.Services;

public interface IAlertService
{
    Task<IEnumerable<AlertResponse>> GetUnreadAlertsAsync(int userId);
    Task MarkAlertAsReadAsync(int alertId, int userId);
    Task ValidateAndCreateAlertAsync(int userId, int? budgetId, AlertType type, string message);
}
