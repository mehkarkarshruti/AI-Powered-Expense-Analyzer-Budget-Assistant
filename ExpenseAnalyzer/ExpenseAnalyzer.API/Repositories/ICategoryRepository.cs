using ExpenseAnalyzer.API.Models;

namespace ExpenseAnalyzer.API.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllActiveAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<bool> NameExistsAsync(string name);
        Task<Category> AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeactivateAsync(Category category);
    }
}
