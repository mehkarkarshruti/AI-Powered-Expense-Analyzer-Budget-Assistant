using System.ComponentModel.DataAnnotations;

namespace ExpenseAnalyzer.API.DTOs
{
    public class UpdateExpenseRequest
    {
        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be a valid category.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Expense date is required.")]
        public DateOnly ExpenseDate { get; set; }

        [MaxLength(255, ErrorMessage = "Description cannot exceed 255 characters.")]
        public string? Description { get; set; }
    }
}
