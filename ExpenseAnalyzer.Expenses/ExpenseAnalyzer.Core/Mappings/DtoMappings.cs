using ExpenseAnalyzer.Core.DTOs;
using ExpenseAnalyzer.Core.Entities;

namespace ExpenseAnalyzer.Core.Mappings;

public static class DtoMappings
{
    public static ExpenseResponseDto ToResponse(this Expense expense)
    {
        return new ExpenseResponseDto
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

    public static CategoryResponseDto ToResponse(this Category category)
    {
        return new CategoryResponseDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            IsActive = category.IsActive
        };
    }

    public static Expense ToEntity(this CreateExpenseDto request, int userId)
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
