using ExpenseAnalyzer.API.Mappings;
using ExpenseAnalyzer.API.DTOs;
using ExpenseAnalyzer.API.Repositories;
using ExpenseAnalyzer.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseAnalyzer.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ExpensesController(IExpenseRepository expenseRepository, ICategoryRepository categoryRepository)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseResponse>>> GetExpenses()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "The token does not contain a valid user id." });
        }

        var expenses = await _expenseRepository.GetByUserIdAsync(userId.Value);

        return Ok(expenses.Select(e => e.ToResponse()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseResponse>> GetExpense(int id)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "The token does not contain a valid user id." });
        }

        var expense = await _expenseRepository.GetByIdAsync(id, userId.Value);
        if (expense is null || expense.UserId != userId)
        {
            return NotFound(new { message = $"Expense '{id}' was not found." });
        }

        return Ok(expense.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseResponse>> CreateExpense(CreateExpenseRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "The token does not contain a valid user id." });
        }

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category is null || !category.IsActive)
        {
            return BadRequest(new { message = "The selected category does not exist or is inactive." });
        }

        var expense = request.ToEntity(userId.Value);
        expense.Category = category;

        await _expenseRepository.AddAsync(expense);

        return CreatedAtAction(nameof(GetExpense), new { id = expense.ExpenseId }, expense.ToResponse());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ExpenseResponse>> UpdateExpense(int id, UpdateExpenseRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "The token does not contain a valid user id." });
        }

        var expense = await _expenseRepository.GetByIdAsync(id, userId.Value);
        if (expense is null || expense.UserId != userId)
        {
            return NotFound(new { message = $"Expense '{id}' was not found." });
        }

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category is null || !category.IsActive)
        {
            return BadRequest(new { message = "The selected category does not exist or is inactive." });
        }

        expense.CategoryId = request.CategoryId;
        expense.Amount = request.Amount;
        expense.ExpenseDate = request.ExpenseDate;
        expense.Description = request.Description;
        expense.UpdatedAt = DateTime.UtcNow;

        await _expenseRepository.UpdateAsync(expense);

        return Ok(expense.ToResponse());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "The token does not contain a valid user id." });
        }

        var expense = await _expenseRepository.GetByIdAsync(id, userId.Value);
        if (expense is null || expense.UserId != userId)
        {
            return NotFound(new { message = $"Expense '{id}' was not found." });
        }

        await _expenseRepository.DeleteAsync(expense);

        return NoContent();
    }
}
