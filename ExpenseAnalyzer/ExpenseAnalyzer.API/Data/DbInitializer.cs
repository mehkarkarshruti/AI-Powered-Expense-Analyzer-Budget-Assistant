using ExpenseAnalyzer.API.Models;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.API.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var isSqlite = context.Database.ProviderName?.Contains("Sqlite") == true;

            if (isSqlite)
            {
                // Ensure the database folder exists (e.g. /data on Render disks,
                // or the app folder when running without a mounted disk).
                var connectionString = context.Database.GetConnectionString() ?? "";
                const string prefix = "Data Source=";
                if (connectionString.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    var dbPath = connectionString[prefix.Length..].Trim();
                    var directory = Path.GetDirectoryName(dbPath);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                }

                // SQLite (cloud deployments): migrations are SQL Server-specific,
                // so create the schema directly from the current model instead.
                await context.Database.EnsureCreatedAsync();
            }
            else
            {
                // SQL Server (local development): apply pending migrations.
                await context.Database.MigrateAsync();
            }

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