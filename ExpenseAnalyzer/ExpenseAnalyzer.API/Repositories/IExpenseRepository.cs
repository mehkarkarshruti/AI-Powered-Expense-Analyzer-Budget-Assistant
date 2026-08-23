using ExpenseAnalyzer.API.Models;

namespace ExpenseAnalyzer.API.Repositories
{
    public interface IExpenseRepository
    {
        Task<Expense?> GetByIdAsync(int id, int userId);
        Task<IEnumerable<Expense>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Expense>> GetAllByUserIdAsync(int userId);
        Task<IEnumerable<Expense>> GetByDateRangeAsync(int userId, DateOnly startDate, DateOnly endDate);
        Task<IEnumerable<Expense>> GetByCategoryIdAsync(int userId, int categoryId);
        Task<Expense> AddAsync(Expense expense);
        Task UpdateAsync(Expense expense);
        Task DeleteAsync(Expense expense);
        Task<decimal> GetTotalSpentAsync(int userId, int month, int year);
    }
}
