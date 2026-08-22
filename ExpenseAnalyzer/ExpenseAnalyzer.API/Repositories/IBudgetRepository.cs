using ExpenseAnalyzer.API.Models;

namespace ExpenseAnalyzer.API.Repositories
{
    public interface IBudgetRepository
    {
        Task<Budget?> GetByIdAsync(int id, int userId);
        Task<Budget?> GetByUserAndMonthAsync(int userId, byte month, short year);
        Task<IEnumerable<Budget>> GetAllByUserIdAsync(int userId);
        Task<Budget> AddAsync(Budget budget);
        Task UpdateAsync(Budget budget);
        Task DeleteAsync(Budget budget);
    }
}