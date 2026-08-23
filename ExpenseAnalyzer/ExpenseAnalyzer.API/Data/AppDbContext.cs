using ExpenseAnalyzer.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<Budget> Budgets => Set<Budget>();
        public DbSet<Alert> Alerts => Set<Alert>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Name).IsUnique();
            });

            // Expense configuration
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.Property(e => e.Amount).HasPrecision(10, 2);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Expenses)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Expenses)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.ExpenseDate);
            });

            // Budget configuration
            modelBuilder.Entity<Budget>(entity =>
            {
                entity.Property(b => b.BudgetAmount).HasPrecision(10, 2);

                entity.HasOne(b => b.User)
                    .WithMany(u => u.Budgets)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Enforces one budget per user per month/year
                entity.HasIndex(b => new { b.UserId, b.Month, b.Year }).IsUnique();
            });

            // Alert configuration
            modelBuilder.Entity<Alert>(entity =>
            {
                entity.Property(a => a.AlertType)
                    .HasConversion<string>();

                entity.HasOne(a => a.User)
                    .WithMany(u => u.Alerts)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Budget)
                    .WithMany(b => b.Alerts)
                    .HasForeignKey(a => a.BudgetId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(a => a.UserId);
            });
        }
    }
}