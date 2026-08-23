using ExpenseAnalyzer.API.Models;

namespace ExpenseAnalyzer.API.Repositories
{
    public interface IAlertRepository
    {
        Task<IEnumerable<Alert>> GetUnreadByUserIdAsync(int userId);
        Task<IEnumerable<Alert>> GetAllByUserIdAsync(int userId);
        Task<Alert?> GetByIdAsync(int id, int userId);
        Task<Alert> AddAsync(Alert alert);
        Task MarkAsReadAsync(int alertId, int userId);
    }
}