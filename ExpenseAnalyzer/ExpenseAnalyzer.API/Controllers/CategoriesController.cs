using ExpenseAnalyzer.API.DTOs;
using ExpenseAnalyzer.API.Extensions;
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
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetCategories()
        {
            var categories = await _categoryRepository.GetAllActiveAsync();

            return Ok(categories.Select(c => c.ToResponse()));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<CategoryResponse>> CreateCategory(CreateCategoryRequest request)
        {
            var name = request.Name.Trim();

            var activeCategories = await _categoryRepository.GetAllActiveAsync();
            if (activeCategories.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { message = $"A category named '{name}' already exists." });
            }

            var category = await _categoryRepository.AddAsync(new Category
            {
                Name = name,
                IsActive = true
            });

            return Created($"/api/categories/{category.CategoryId}", category.ToResponse());
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category is null || !category.IsActive)
            {
                return NotFound(new { message = $"Category '{id}' was not found." });
            }

            category.IsActive = false;
            await _categoryRepository.UpdateAsync(category);

            return NoContent();
        }
    }
}
