using ExpenseAnalyzer.API.DTOs;

namespace ExpenseAnalyzer.API.Services;

public interface IBudgetService
{
    Task<BudgetResponse> CreateOrUpdateBudgetAsync(int userId, CreateBudgetRequest request);
    Task<BudgetResponse?> GetBudgetAsync(int userId, int? categoryId, byte month, short year);
    Task<IEnumerable<BudgetResponse>> GetAllBudgetsAsync(int userId);
    Task DeleteBudgetAsync(int budgetId, int userId);
    Task CheckBudgetThresholdsAsync(int userId, int? categoryId, DateOnly expenseDate);
    Task<BudgetStatusDto> GetBudgetStatusAndCheckAlertsAsync(int userId);
}
