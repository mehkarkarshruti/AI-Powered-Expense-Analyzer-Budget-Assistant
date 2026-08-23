using ExpenseAnalyzer.API.Data;
using ExpenseAnalyzer.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.API.Repositories
{
    public class BudgetRepository : IBudgetRepository
    {
        private readonly AppDbContext _context;

        public BudgetRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Budget?> GetByIdAsync(int id, int userId)
        {
            return await _context.Budgets
                .FirstOrDefaultAsync(b => b.BudgetId == id && b.UserId == userId);
        }

        public async Task<Budget?> GetByUserAndMonthAsync(int userId, byte month, short year)
        {
            return await _context.Budgets
                .FirstOrDefaultAsync(b => b.UserId == userId && b.Month == month && b.Year == year);
        }

        public async Task<IEnumerable<Budget>> GetAllByUserIdAsync(int userId)
        {
            return await _context.Budgets
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .ToListAsync();
        }

        public async Task<Budget> AddAsync(Budget budget)
        {
            budget.CreatedAt = DateTime.UtcNow;
            budget.UpdatedAt = DateTime.UtcNow;
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
            return budget;
        }

        public async Task UpdateAsync(Budget budget)
        {
            budget.UpdatedAt = DateTime.UtcNow;
            _context.Budgets.Update(budget);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Budget budget)
        {
            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
        }
    }
}