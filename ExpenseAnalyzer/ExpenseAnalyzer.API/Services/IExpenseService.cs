using ExpenseAnalyzer.API.DTOs;

namespace ExpenseAnalyzer.API.Services
{
    public interface IExpenseService
    {
        Task<List<ExpenseResponse>> GetUserExpensesAsync(int userId);
        Task<ExpenseResponse?> GetExpenseAsync(int expenseId, int userId);
        Task<ExpenseResponse> CreateExpenseAsync(int userId, CreateExpenseRequest request);
        Task<ExpenseResponse> UpdateExpenseAsync(int expenseId, int userId, UpdateExpenseRequest request);
        Task DeleteExpenseAsync(int expenseId, int userId);
    }
}
