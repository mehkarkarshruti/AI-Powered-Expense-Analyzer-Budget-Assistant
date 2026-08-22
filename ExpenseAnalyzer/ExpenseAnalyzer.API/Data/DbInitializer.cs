using ExpenseAnalyzer.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.API.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Automatically applies any pending migrations
            await context.Database.MigrateAsync();

            // Seeds default categories if the table is empty
            if (!await context.Categories.AnyAsync())
            {
                var defaultCategories = new List<Category>
                {
                    new() { Name = "Food & Dining", IsActive = true },
                    new() { Name = "Transportation", IsActive = true },
                    new() { Name = "Housing & Rent", IsActive = true },
                    new() { Name = "Utilities", IsActive = true },
                    new() { Name = "Entertainment", IsActive = true },
                    new() { Name = "Healthcare", IsActive = true },
                    new() { Name = "Shopping", IsActive = true },
                    new() { Name = "Miscellaneous", IsActive = true }
                };

                await context.Categories.AddRangeAsync(defaultCategories);
                await context.SaveChangesAsync();
            }
        }
    }
}