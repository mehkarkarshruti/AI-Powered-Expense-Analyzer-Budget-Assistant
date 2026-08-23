using ExpenseAnalyzer.API.Models;
using ExpenseAnalyzer.API.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.API.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllActiveAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int categoryId)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
    }

    public async Task<bool> NameExistsAsync(string name)
    {
        return await _context.Categories
            .AnyAsync(c => c.Name == name);
    }

    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeactivateAsync(Category category)
    {
        category.IsActive = false;
        await _context.SaveChangesAsync();
    }
}
