using ExpenseAnalyzer.Core.Entities;

namespace ExpenseAnalyzer.Core.Interfaces;

public interface IExpenseRepository
{
    Task<IEnumerable<Expense>> GetByUserIdAsync(int userId);
    Task<Expense?> GetByIdAsync(int expenseId);
    Task AddAsync(Expense expense);
    Task UpdateAsync(Expense expense);
    Task DeleteAsync(Expense expense);
    Task<decimal> GetTotalSpentInMonthAsync(int userId, int year, int month);
}
