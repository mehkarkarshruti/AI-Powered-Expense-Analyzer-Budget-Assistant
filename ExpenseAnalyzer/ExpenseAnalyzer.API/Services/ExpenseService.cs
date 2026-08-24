using ExpenseAnalyzer.API.Models;
using ExpenseAnalyzer.API.Repositories;

namespace ExpenseAnalyzer.API.Services;

public class ExpenseService(IExpenseRepository expenseRepo, IBudgetService budgetService) : IExpenseService
{
    public Task<Expense?> GetByIdAsync(int id, int userId)
    {
        return expenseRepo.GetByIdAsync(id, userId);
    }

    public Task<IEnumerable<Expense>> GetAllByUserIdAsync(int userId)
    {
        return expenseRepo.GetAllByUserIdAsync(userId);
    }

    public async Task<Expense> CreateExpenseAsync(Expense expense)
    {
        var created = await expenseRepo.AddAsync(expense);
        
        // Immediately trigger check against budgets!
        await budgetService.CheckBudgetThresholdsAsync(expense.UserId, expense.CategoryId, expense.ExpenseDate);
        
        return created;
    }

    public async Task UpdateExpenseAsync(Expense expense)
    {
        await expenseRepo.UpdateAsync(expense);
        
        // Re-evaluate budget thresholds after update.
        await budgetService.CheckBudgetThresholdsAsync(expense.UserId, expense.CategoryId, expense.ExpenseDate);
    }

    public async Task DeleteExpenseAsync(Expense expense)
    {
        await expenseRepo.DeleteAsync(expense);
    }

    public Task<decimal> GetTotalSpentAsync(int userId, int? categoryId, int month, int year)
    {
        return expenseRepo.GetTotalSpentAsync(userId, categoryId, month, year);
    }
}
