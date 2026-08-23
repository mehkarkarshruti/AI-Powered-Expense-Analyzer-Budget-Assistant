using ExpenseAnalyzer.API.DTOs;
using ExpenseAnalyzer.API.Mappings;
using ExpenseAnalyzer.API.Models;
using ExpenseAnalyzer.API.Repositories;

namespace ExpenseAnalyzer.API.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepo;
        private readonly ICategoryRepository _categoryRepo;

        public ExpenseService(IExpenseRepository expenseRepo, ICategoryRepository categoryRepo)
        {
            _expenseRepo = expenseRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task<List<ExpenseResponse>> GetUserExpensesAsync(int userId)
        {
            var expenses = await _expenseRepo.GetAllByUserIdAsync(userId);

            return expenses.Select(e => e.ToResponse()).ToList();
        }

        public async Task<ExpenseResponse?> GetExpenseAsync(int expenseId, int userId)
        {
            var expense = await _expenseRepo.GetByIdAsync(expenseId, userId);

            return expense?.ToResponse();
        }

        public async Task<ExpenseResponse> CreateExpenseAsync(int userId, CreateExpenseRequest request)
        {
            var category = await _categoryRepo.GetByIdAsync(request.CategoryId);
            if (category == null || !category.IsActive)
            {
                throw new ArgumentException("The selected category does not exist or is inactive.");
            }

            var expense = request.ToEntity(userId);
            expense.Category = category;

            await _expenseRepo.AddAsync(expense);

            return expense.ToResponse();
        }

        public async Task<ExpenseResponse> UpdateExpenseAsync(int expenseId, int userId, UpdateExpenseRequest request)
        {
            var expense = await _expenseRepo.GetByIdAsync(expenseId, userId);
            if (expense == null)
            {
                throw new KeyNotFoundException($"Expense '{expenseId}' was not found.");
            }

            var category = await _categoryRepo.GetByIdAsync(request.CategoryId);
            if (category == null || !category.IsActive)
            {
                throw new ArgumentException("The selected category does not exist or is inactive.");
            }

            expense.CategoryId = request.CategoryId;
            expense.Amount = request.Amount;
            expense.ExpenseDate = request.ExpenseDate;
            expense.Description = request.Description;
            expense.UpdatedAt = DateTime.UtcNow;

            await _expenseRepo.UpdateAsync(expense);

            return expense.ToResponse();
        }

        public async Task DeleteExpenseAsync(int expenseId, int userId)
        {
            var expense = await _expenseRepo.GetByIdAsync(expenseId, userId);
            if (expense == null)
            {
                throw new KeyNotFoundException($"Expense '{expenseId}' was not found.");
            }

            await _expenseRepo.DeleteAsync(expense);
        }
    }
}
