using ExpenseAnalyzer.API.Data;
using ExpenseAnalyzer.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.API.Repositories;

public class BudgetRepository(AppDbContext context) : IBudgetRepository
{
    public async Task<Budget?> GetByIdAsync(int id, int userId)
    {
        return await context.Budgets
            .FirstOrDefaultAsync(b => b.BudgetId == id && b.UserId == userId);
    }

    public async Task<Budget?> GetBudgetAsync(int userId, int? categoryId, byte month, short year)
    {
        return await context.Budgets
            .FirstOrDefaultAsync(b => b.UserId == userId && b.CategoryId == categoryId && b.Month == month && b.Year == year);
    }

    public async Task<IEnumerable<Budget>> GetAllByUserIdAsync(int userId)
    {
        return await context.Budgets
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.Year)
            .ThenByDescending(b => b.Month)
            .ToListAsync();
    }

    public async Task<Budget> AddAsync(Budget budget)
    {
        budget.CreatedAt = DateTime.UtcNow;
        budget.UpdatedAt = DateTime.UtcNow;
        context.Budgets.Add(budget);
        await context.SaveChangesAsync();
        return budget;
    }

    public async Task UpdateAsync(Budget budget)
    {
        budget.UpdatedAt = DateTime.UtcNow;
        context.Budgets.Update(budget);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Budget budget)
    {
        context.Budgets.Remove(budget);
        await context.SaveChangesAsync();
    }
}