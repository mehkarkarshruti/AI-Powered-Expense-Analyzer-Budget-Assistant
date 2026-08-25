using System.ComponentModel.DataAnnotations;

namespace ExpenseAnalyzer.API.DTOs;

public class CreateBudgetRequest
{
    public int? CategoryId { get; set; }

    [Required]
    [Range(1, 12)]
    public byte Month { get; set; }

    [Required]
    [Range(2000, 2100)]
    public short Year { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Budget amount must be positive")]
    public decimal BudgetAmount { get; set; }
}
