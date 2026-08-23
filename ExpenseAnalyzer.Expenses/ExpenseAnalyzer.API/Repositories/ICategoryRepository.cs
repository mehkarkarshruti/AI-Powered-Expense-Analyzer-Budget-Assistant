using ExpenseAnalyzer.API.Models;

namespace ExpenseAnalyzer.API.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllActiveAsync();
    Task<Category?> GetByIdAsync(int categoryId);
    Task<bool> NameExistsAsync(string name);
    Task AddAsync(Category category);
    Task DeactivateAsync(Category category);
}
