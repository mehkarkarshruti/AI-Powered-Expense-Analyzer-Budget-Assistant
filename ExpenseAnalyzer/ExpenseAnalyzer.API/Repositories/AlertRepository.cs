using ExpenseAnalyzer.API.Data;
using ExpenseAnalyzer.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.API.Repositories
{
    public class AlertRepository : IAlertRepository
    {
        private readonly AppDbContext _context;

        public AlertRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Alert>> GetUnreadByUserIdAsync(int userId)
        {
            return await _context.Alerts
                .Where(a => a.UserId == userId && !a.IsRead)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Alert>> GetAllByUserIdAsync(int userId)
        {
            return await _context.Alerts
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Alert?> GetByIdAsync(int id, int userId)
        {
            return await _context.Alerts
                .FirstOrDefaultAsync(a => a.AlertId == id && a.UserId == userId);
        }

        public async Task<Alert> AddAsync(Alert alert)
        {
            alert.CreatedAt = DateTime.UtcNow;
            _context.Alerts.Add(alert);
            await _context.SaveChangesAsync();
            return alert;
        }

        public async Task MarkAsReadAsync(int alertId, int userId)
        {
            var alert = await _context.Alerts
                .FirstOrDefaultAsync(a => a.AlertId == alertId && a.UserId == userId);

            if (alert != null)
            {
                alert.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}