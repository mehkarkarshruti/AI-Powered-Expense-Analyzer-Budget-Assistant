using ExpenseAnalyzer.API.Models;

namespace ExpenseAnalyzer.API.Services;

public interface IExpenseService
{
    Task<Expense?> GetByIdAsync(int id, int userId);
    Task<IEnumerable<Expense>> GetAllByUserIdAsync(int userId);
    Task<Expense> CreateExpenseAsync(Expense expense);
    Task UpdateExpenseAsync(Expense expense);
    Task DeleteExpenseAsync(Expense expense);
    Task<decimal> GetTotalSpentAsync(int userId, int? categoryId, int month, int year);
}
