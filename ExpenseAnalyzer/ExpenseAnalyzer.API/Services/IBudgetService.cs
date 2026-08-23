using ExpenseAnalyzer.API.DTOs;

namespace ExpenseAnalyzer.API.Services
{
    public interface IBudgetService
    {
        Task<BudgetResponseDto> SetBudgetAsync(int userId, SetBudgetDto dto);
        Task<BudgetResponseDto?> GetBudgetAsync(int userId, byte month, short year);
        Task<BudgetStatusDto> GetBudgetStatusAndCheckAlertsAsync(int userId);
    }
}
