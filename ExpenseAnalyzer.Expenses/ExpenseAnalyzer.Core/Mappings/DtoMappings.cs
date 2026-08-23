using ExpenseAnalyzer.Core.DTOs;
using ExpenseAnalyzer.Core.Entities;

namespace ExpenseAnalyzer.Core.Mappings;

public static class DtoMappings
{
    public static ExpenseResponse ToResponse(this Expense expense)
    {
        return new ExpenseResponse
        {
            ExpenseId = expense.ExpenseId,
            UserId = expense.UserId,
            CategoryId = expense.CategoryId,
            CategoryName = expense.Category?.Name ?? string.Empty,
            Amount = expense.Amount,
            ExpenseDate = expense.ExpenseDate,
            Description = expense.Description,
            CreatedAt = expense.CreatedAt
        };
    }

    public static CategoryResponse ToResponse(this Category category)
    {
        return new CategoryResponse
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            IsActive = category.IsActive
        };
    }

    public static Expense ToEntity(this CreateExpenseRequest request, int userId)
    {
        var now = DateTime.UtcNow;

        return new Expense
        {
            UserId = userId,
            CategoryId = request.CategoryId,
            Amount = request.Amount,
            ExpenseDate = request.ExpenseDate,
            Description = request.Description,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
