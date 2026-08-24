using ExpenseAnalyzer.API.Data;
using ExpenseAnalyzer.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.API.Repositories;

public class AlertRepository(AppDbContext context) : IAlertRepository
{
    public async Task<IEnumerable<Alert>> GetUnreadByUserIdAsync(int userId)
    {
        return await context.Alerts
            .Where(a => a.UserId == userId && !a.IsRead)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Alert>> GetAllByUserIdAsync(int userId)
    {
        return await context.Alerts
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Alert?> GetByIdAsync(int id, int userId)
    {
        return await context.Alerts
            .FirstOrDefaultAsync(a => a.AlertId == id && a.UserId == userId);
    }

    public async Task<Alert> AddAsync(Alert alert)
    {
        alert.CreatedAt = DateTime.UtcNow;
        context.Alerts.Add(alert);
        await context.SaveChangesAsync();
        return alert;
    }

    public async Task MarkAsReadAsync(int alertId, int userId)
    {
        var alert = await context.Alerts
            .FirstOrDefaultAsync(a => a.AlertId == alertId && a.UserId == userId);

        if (alert != null)
        {
            alert.IsRead = true;
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> HasRecentUnreadAlertAsync(int userId, int? budgetId, AlertType type)
    {
        // Ensures no duplicate unread alert of the same type for the same budget cycle
        return await context.Alerts
            .AnyAsync(a => a.UserId == userId &&
                           a.BudgetId == budgetId &&
                           a.AlertType == type &&
                           !a.IsRead);
    }
}