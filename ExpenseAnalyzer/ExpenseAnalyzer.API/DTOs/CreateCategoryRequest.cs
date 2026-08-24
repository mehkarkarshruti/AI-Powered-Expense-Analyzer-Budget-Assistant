using System.ComponentModel.DataAnnotations;

namespace ExpenseAnalyzer.API.DTOs;

public class CreateCategoryRequest
{
    [Required(ErrorMessage = "Category name is required.")]
    [MaxLength(50, ErrorMessage = "Category name cannot exceed 50 characters.")]
    [MinLength(2, ErrorMessage = "Category name must be at least 2 characters.")]
    public string Name { get; set; } = string.Empty;
}
