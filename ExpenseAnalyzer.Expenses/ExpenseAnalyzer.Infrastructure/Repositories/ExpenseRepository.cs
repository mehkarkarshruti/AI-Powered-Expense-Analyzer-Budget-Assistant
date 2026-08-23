using ExpenseAnalyzer.Core.Entities;
using ExpenseAnalyzer.Core.Interfaces;
using ExpenseAnalyzer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.Infrastructure.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly AppDbContext _context;

    public ExpenseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Expense>> GetByUserIdAsync(int userId)
    {
        return await _context.Expenses
            .AsNoTracking()
            .Include(e => e.Category)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.ExpenseDate)
            .ThenByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<Expense?> GetByIdAsync(int expenseId)
    {
        return await _context.Expenses
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.ExpenseId == expenseId);
    }

    public async Task AddAsync(Expense expense)
    {
        await _context.Expenses.AddAsync(expense);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Expense expense)
    {
        _context.Expenses.Update(expense);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Expense expense)
    {
        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalSpentInMonthAsync(int userId, int year, int month)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1);

        return await _context.Expenses
            .Where(e => e.UserId == userId && e.ExpenseDate >= start && e.ExpenseDate < end)
            .SumAsync(e => e.Amount);
    }
}
