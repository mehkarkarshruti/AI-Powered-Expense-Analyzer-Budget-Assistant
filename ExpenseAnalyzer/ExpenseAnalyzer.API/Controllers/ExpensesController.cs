using ExpenseAnalyzer.API.DTOs;
using ExpenseAnalyzer.API.Extensions;
using ExpenseAnalyzer.API.Services;
using ExpenseAnalyzer.API.Mappings;
using ExpenseAnalyzer.API.Models;
using ExpenseAnalyzer.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseAnalyzer.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;

        public ExpensesController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseResponse>>> GetExpenses()
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                return Unauthorized(new { message = "The token does not contain a valid user id." });
            }

            var expenses = await _expenseService.GetUserExpensesAsync(userId.Value);

            return Ok(expenses);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ExpenseResponse>> GetExpense(int id)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                return Unauthorized(new { message = "The token does not contain a valid user id." });
            }

            var expense = await _expenseService.GetExpenseAsync(id, userId.Value);
            if (expense is null)
            {
                return NotFound(new { message = $"Expense '{id}' was not found." });
            }

            return Ok(expense);
        }

        [HttpPost]
        public async Task<ActionResult<ExpenseResponse>> CreateExpense(CreateExpenseRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                return Unauthorized(new { message = "The token does not contain a valid user id." });
            }

            try
            {
                var expense = await _expenseService.CreateExpenseAsync(userId.Value, request);

                return CreatedAtAction(nameof(GetExpense), new { id = expense.ExpenseId }, expense);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ExpenseResponse>> UpdateExpense(int id, UpdateExpenseRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                return Unauthorized(new { message = "The token does not contain a valid user id." });
            }

            try
            {
                var expense = await _expenseService.UpdateExpenseAsync(id, userId.Value, request);

                return Ok(expense);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                return Unauthorized(new { message = "The token does not contain a valid user id." });
            }

            try
            {
                await _expenseService.DeleteExpenseAsync(id, userId.Value);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
